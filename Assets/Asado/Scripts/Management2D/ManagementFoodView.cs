using System;
using System.Collections;
using System.Collections.Generic;
using Asadito.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Asadito
{
    /// <summary>A visual reference only. Inventory identity and all rules remain in ManagementService.</summary>
    public sealed class ManagementFoodTarget : MonoBehaviour
    {
        public string FoodId;
        public int UnitId;
        public Freshness Freshness;
        public bool Selected;
        public bool CanBuy = true;
        public int SelectionSlot = -1;
        public Vector3 Home;
        public Vector2 DisplayUV;
        public Vector3 HomeScale = Vector3.one;
        public Image Visual;
        public RectTransform Rect => Visual.rectTransform;
        internal int DrawOrder;
    }

    /// <summary>Keeps quantity taps native while forwarding card-button drags to the shop gesture owner.</summary>
    public sealed class ManagementShopGestureRelay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, ICancelHandler
    {
        public ManagementFoodView View;
        public void OnPointerDown(PointerEventData e) => View.OnPointerDown(e);
        public void OnPointerUp(PointerEventData e) => View.OnPointerUp(e);
        public void OnBeginDrag(PointerEventData e) => View.OnBeginDrag(e);
        public void OnDrag(PointerEventData e) => View.OnDrag(e);
        public void OnEndDrag(PointerEventData e) => View.OnEndDrag(e);
        public void OnCancel(BaseEventData e) => View.OnCancel(e);
    }

    /// <summary>Canvas-only food, painted depth and UI pointer ownership; no parallel inventory.</summary>
    public sealed class ManagementFoodView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler,
        IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, ICancelHandler
    {
        const int NoPointer = int.MinValue;
        public Image Viewport { get; private set; }
        public bool IsFridge { get; private set; }
        public int CurrentLevel { get; private set; } = 1;
        public readonly List<ManagementFoodTarget> Targets = new List<ManagementFoodTarget>();
        public RectTransform CartDropZone { get; set; }
        public bool IsDragging => dragTarget != null || shopGesture != ShopGesture.None;
        sealed class PriceSign { public RectTransform Root; public Text Product, Price, Stock, Quantity; public Button Minus, Plus; }
        readonly Dictionary<string, PriceSign> tickets = new Dictionary<string, PriceSign>();
        readonly Dictionary<ManagementFoodTarget, Coroutine> motions = new Dictionary<ManagementFoodTarget, Coroutine>();
        readonly List<RaycastResult> hits = new List<RaycastResult>(16);
        ManagementService service;
        Action<ManagementFoodTarget> select;
        Func<string, Sprite> foodSprite;
        Func<string, Vector2> foodSize;
        Font font;
        RectTransform foodLayer;
        ManagementFoodTarget pressedTarget, dragTarget;
        Image dragPreview;
        int pointer = NoPointer, page, pageCount = 1;
        bool suppressClick, pointerReleased;
        PointerEventData pressedEvent;
        enum ShopGesture { None, Pending, Cart, Swipe, Ignored }
        ShopGesture shopGesture;
        Action<int> swipePage;
        public void SetShopSwipe(Action<int> navigate) => swipePage = navigate;
        Coroutine cartMotion;
        Vector2 lastSize;
        public int PageIndex => page;
        public int PageCount => pageCount;
        public const int ProductsPerPage = 4;
        Func<string,int> quantity;
        Action<string,int> adjust;
        Func<string,Transform,Color,float,float,float,float,Image> makePanel;
        Func<string,Transform,float,float,float,float,Action,Button> makeButton;
        Font titleFont;
        readonly Dictionary<string,ProductEconomy> productsById = new Dictionary<string,ProductEconomy>();
        readonly List<FoodDefinition> catalogue = new List<FoodDefinition>();
        public void SetShopPresentation(Func<string,int> quantity, Action<string,int> adjust,
            Func<string,Transform,Color,float,float,float,float,Image> panel,
            Func<string,Transform,float,float,float,float,Action,Button> button, Font title)
        { this.quantity=quantity;this.adjust=adjust;makePanel=panel;makeButton=button;titleFont=title; }

        static readonly Color Cream = new Color32(255,244,219,255);

        public static ManagementFoodView Create(Transform parent, Vector2 min, Vector2 max, Font font,
            ManagementService service, bool fridge, Action<ManagementFoodTarget> select,
            Func<string, Sprite> foodSprite, Func<string, Vector2> foodSize)
        {
            var go = new GameObject(fridge ? "Heladera 2D" : "Mostrador 2D", typeof(RectTransform), typeof(Image), typeof(ManagementFoodView));
            var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var view = go.GetComponent<ManagementFoodView>(); view.Viewport = go.GetComponent<Image>();
            view.Viewport.color = Color.clear; view.Viewport.raycastTarget = true;
            view.font = font; view.service = service; view.IsFridge = fridge;
            view.select = select; view.foodSprite = foodSprite; view.foodSize = foodSize;
            view.foodLayer = new GameObject("Food layers", typeof(RectTransform)).GetComponent<RectTransform>();
            view.foodLayer.SetParent(rect, false); view.foodLayer.anchorMin = Vector2.zero; view.foodLayer.anchorMax = Vector2.one;
            view.foodLayer.offsetMin = view.foodLayer.offsetMax = Vector2.zero;
            Canvas.ForceUpdateCanvases();
            if (fridge) view.BuildFridge();
            return view;
        }

        ManagementFoodTarget Food(string id, int unitId, Vector2 uv)
        {
            var go = new GameObject(unitId == 0 ? "Comprar " + id + " pieza " + Targets.Count : "Inventory unit " + unitId,
                typeof(RectTransform), typeof(Image), typeof(ManagementFoodTarget));
            go.transform.SetParent(foodLayer, false);
            var item = go.GetComponent<ManagementFoodTarget>(); item.FoodId = id; item.UnitId = unitId;
            item.DisplayUV = uv; item.DrawOrder = Targets.Count; item.Visual = go.GetComponent<Image>();
            item.Visual.sprite = foodSprite(id); item.Visual.preserveAspect = true; item.Visual.raycastTarget = false;
            item.Rect.anchorMin = item.Rect.anchorMax = new Vector2(.5f,.5f);
            item.Rect.sizeDelta = foodSize(id); Targets.Add(item); return item;
        }
        public void ConfigureShop(int level)
        {
            CurrentLevel = level; page = 0; BuildShop();
        }
        public void ChangePage() => ChangePage(1);
        public void ChangePage(int delta) { int next=Mathf.Clamp(page+delta,0,pageCount-1);if(next==page)return;page=next;BuildShop(); }
        void ClearShop()
        {
            CancelInteraction(); StopAllCoroutines(); motions.Clear(); cartMotion = null;
            if(CartDropZone!=null)CartDropZone.localScale=Vector3.one;
            foreach (var t in Targets) if (t != null) { t.gameObject.SetActive(false); Destroy(t.gameObject); }
            Targets.Clear();
            foreach (var s in tickets.Values) { s.Root.gameObject.SetActive(false); Destroy(s.Root.gameObject); }
            tickets.Clear();
        }
        void BuildShop()
        {
            ClearShop(); productsById.Clear();catalogue.Clear();
            foreach(var p in service.Config.Products) productsById[p.FoodId]=p;
            var all=FoodCatalog.GetAll();
            // First page mixes familiar meat and achura; every subsequent item remains data driven.
            foreach(var id in new[]{"chorizo","tira","vacio","chinchulines"})
                foreach(var food in all)if(food.Id==id)catalogue.Add(food);
            foreach(var food in all)if(!catalogue.Exists(f=>f.Id==food.Id))catalogue.Add(food);
            pageCount=Math.Max(1,(catalogue.Count+ProductsPerPage-1)/ProductsPerPage);
            page=Mathf.Clamp(page,0,pageCount-1);
            for(int slot=0;slot<ProductsPerPage;slot++)
            {
                int index=page*ProductsPerPage+slot;if(index>=catalogue.Count)break;
                var food=catalogue[index];float left=slot%2*.5f,bottom=slot<2?.51f:.01f;
                var image=makePanel("Corte "+food.Id,Viewport.transform,Cream,.5f,.5f,1,1);
                var card=image.rectTransform;card.anchorMin=new Vector2(left+.006f,bottom);
                card.anchorMax=new Vector2(left+.494f,bottom+.48f);card.offsetMin=card.offsetMax=Vector2.zero;
                image.raycastTarget=false;card.SetAsFirstSibling();
                var metal=makePanel("Bandeja "+food.Id,card,new Color32(181,176,154,240),.5f,.82f,1,1);
                metal.rectTransform.anchorMin=new Vector2(.025f,.68f);metal.rectTransform.anchorMax=new Vector2(.975f,.975f);
                metal.rectTransform.offsetMin=metal.rectTransform.offsetMax=Vector2.zero;metal.raycastTarget=false;
                var band=makePanel("Nombre corte "+food.Id,card,new Color32(34,61,45,255),.5f,.565f,1,1);
                band.rectTransform.anchorMin=new Vector2(.025f,.475f);band.rectTransform.anchorMax=new Vector2(.975f,.655f);
                band.rectTransform.offsetMin=band.rectTransform.offsetMax=Vector2.zero;band.raycastTarget=false;
                var target=Food(food.Id,0,new Vector2(left+.25f,bottom+.48f*.83f));
                target.CanBuy=productsById.TryGetValue(food.Id,out var product)&&product.UnlockLevel<=CurrentLevel;
                var sign=new PriceSign{Root=card};
                sign.Product=CardText(card,"Producto "+food.Id,.5f,.565f,.90f,.18f,32,Color.white,titleFont);
                sign.Product.text=food.DisplayName.ToUpper();
                sign.Price=CardText(card,"Precio "+food.Id,.27f,.40f,.49f,.14f,60,new Color32(178,38,23,255),titleFont);
                sign.Stock=CardText(card,"Stock "+food.Id,.77f,.40f,.43f,.14f,25,new Color32(34,61,45,255),font);
                sign.Quantity=CardText(card,"Cantidad "+food.Id,.5f,.16f,.24f,.22f,46,new Color32(34,61,45,255),titleFont);
                var id=food.Id;
                sign.Minus=makeButton("−",card,.19f,.16f,132,132,()=>adjust(id,-1));sign.Minus.name="Restar corte "+id;
                sign.Plus=makeButton("+",card,.81f,.16f,132,132,()=>adjust(id,1));sign.Plus.name="Sumar corte "+id;
                sign.Minus.gameObject.AddComponent<ManagementWidthFit>().Configure(card,132,12,.19f);
                sign.Plus.gameObject.AddComponent<ManagementWidthFit>().Configure(card,132,12,.19f);
                sign.Minus.gameObject.AddComponent<ManagementShopGestureRelay>().View = this;
                sign.Plus.gameObject.AddComponent<ManagementShopGestureRelay>().View = this;
                tickets[id]=sign;UpdateTicket(id,quantity(id));
            }
            Canvas.ForceUpdateCanvases();RefreshLayout();
        }
        Text CardText(Transform card,string name,float x,float y,float w,float h,int size,Color color,Font typeface)
        {
            var text=SignText(card,name,Vector2.zero,Vector2.zero,size,color);text.font=typeface??font;
            var rect=text.rectTransform;rect.anchorMin=new Vector2(x-w*.5f,y-h*.5f);rect.anchorMax=new Vector2(x+w*.5f,y+h*.5f);
            rect.offsetMin=rect.offsetMax=Vector2.zero;text.verticalOverflow=VerticalWrapMode.Overflow;
            return text;
        }
        void BuildFridge()
        {
            int index = 0;
            foreach (var unit in service.State.Inventory)
            {
                int shelf = index / 3, col = index % 3;
                var item = Food(unit.FoodId,unit.Id,new Vector2(.365f + col * .215f,.723f - shelf * .166f));
                item.Freshness = unit.FreshnessAt(service.State.FreshnessCycle,service.Config.Product(unit.FoodId).FreshCycles);
                item.Visual.color = RestingTint(item); index++;
            }
            RefreshLayout();
        }
        Vector2 PrepUV(int slot) => new Vector2(.32f + slot % 2 * .32f,.195f - slot / 2 * .038f);
        Vector3 Position(ManagementFoodTarget target, Vector2 uv)
        {
            var area = Viewport.rectTransform.rect; var half = target.Rect.sizeDelta * .5f;
            float x = Mathf.Clamp((uv.x-.5f)*area.width,area.xMin+half.x+4,area.xMax-half.x-4);
            float y = Mathf.Clamp((uv.y-.5f)*area.height,area.yMin+half.y+4,area.yMax-half.y-4);
            return new Vector3(x,y,0);
        }
        public void RefreshLayout()
        {
            if (Viewport == null || foodLayer == null) return;
            lastSize = Viewport.rectTransform.rect.size;
            foreach (var target in Targets)
            {
                StopMotion(target);
                if(!IsFridge)
                {
                    Vector2 original=foodSize(target.FoodId), cell=Viewport.rectTransform.rect.size;
                    float scale=Mathf.Min(cell.x*.43f/original.x,cell.y*.48f*.28f/original.y);
                    target.Rect.sizeDelta=original*Mathf.Max(.01f,scale);
                }
                target.Home = Position(target,target.DisplayUV);
                target.Rect.localPosition = target.Selected ? Position(target,PrepUV(target.SelectionSlot)) : target.Home;
                target.Rect.localScale = target.HomeScale; target.Visual.color = RestingTint(target);
            }
        }
        void LateUpdate()
        {
            if (Viewport.rectTransform.rect.size != lastSize) RefreshLayout();
            if (pointerReleased && !IsDragging) { pointer = NoPointer; pressedTarget = null; pressedEvent = null; pointerReleased = false; }
        }
        void Update()
        {
            if (pointer == NoPointer) return;
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                int fingers = 0;
                foreach (var touch in touchscreen.touches) if (touch.press.isPressed) fingers++;
                if (fingers > 1) CancelInteraction();
            }
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) CancelInteraction();
        }
        static Color RestingTint(ManagementFoodTarget t) => t.Freshness == Freshness.Spoiled ? new Color(.55f,.60f,.48f) : Color.white;
        public Vector2 ScreenPoint(ManagementFoodTarget target, Camera eventCamera = null)
            => RectTransformUtility.WorldToScreenPoint(EventCamera(eventCamera),target.Rect.position);

        Camera EventCamera(Camera camera)
        {
            if (camera != null) return camera;
            var canvas = GetComponentInParent<Canvas>();
            return canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        }

        /// <summary>The same Canvas order as rendering, including opaque signs and later UI overlays.</summary>
        public ManagementFoodTarget Raycast(Vector2 point, Camera eventCamera = null)
        {
            eventCamera = EventCamera(eventCamera);
            if (!RectTransformUtility.RectangleContainsScreenPoint(Viewport.rectTransform,point,eventCamera)) return null;
            if (EventSystem.current != null)
            {
                hits.Clear(); var data = new PointerEventData(EventSystem.current) { position = point };
                EventSystem.current.RaycastAll(data,hits);
                if (hits.Count > 0 && hits[0].gameObject != gameObject) return null;
            }
            // Selection can promote an ID to the foreground; actual sibling order is authoritative.
            for (int i = foodLayer.childCount-1; i >= 0; i--)
            {
                var target = foodLayer.GetChild(i).GetComponent<ManagementFoodTarget>();
                if (target == null || !target.isActiveAndEnabled || (!IsFridge && !target.CanBuy)) continue;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(target.Rect,point,eventCamera,out var local)) continue;
                var rect = target.Rect.rect; if (!rect.Contains(local)) continue;
                var uv = new Vector2((local.x-rect.xMin)/rect.width,(local.y-rect.yMin)/rect.height);
                if (FoodSilhouette.Contains(target.FoodId,uv)) return target;
            }
            return null;
        }
        public void OnPointerDown(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left) return;
            if (pointer != NoPointer && pointer != e.pointerId) { e.eligibleForClick = false; CancelInteraction(); return; }
            if (IsDragging) return;
            pointer = e.pointerId; pressedEvent = e; pressedTarget = Raycast(e.position,e.pressEventCamera);
            suppressClick = false; pointerReleased = false;
        }
        public void OnPointerUp(PointerEventData e)
        {
            if (pointer != e.pointerId) { CancelInteraction(); return; }
            pointerReleased = true; // EventSystem's click/end-drag follows pointer-up in this frame.
        }
        public void OnPointerClick(PointerEventData e)
        {
            if (pointer != e.pointerId || suppressClick || IsDragging) return;
            var target = Raycast(e.position,e.pressEventCamera);
            if (target != null && target == pressedTarget) select?.Invoke(target);
            pointer = NoPointer; pressedTarget = null; pressedEvent = null; pointerReleased = false;
        }
        public void OnBeginDrag(PointerEventData e)
        {
            // Quantity press and drag share the same GameObject: even a canceled drag must suppress Button clicks.
            if (!IsFridge) e.eligibleForClick = false;
            if (IsFridge || CartDropZone == null || pointer != e.pointerId || suppressClick) return;
            shopGesture = ShopGesture.Pending;
            suppressClick = true;
            e.eligibleForClick = false; // A drag over +/− must never become a quantity click on release.
            if (pressedTarget != null)
            {
                dragTarget = pressedTarget; StopMotion(dragTarget);
                dragTarget.Rect.localScale = dragTarget.HomeScale;
                dragTarget.Visual.color = new Color(1,.92f,.76f);
                var go = new GameObject("Dragged food preview",typeof(RectTransform),typeof(Image));
                go.transform.SetParent(Viewport.transform.parent,false); dragPreview = go.GetComponent<Image>();
                dragPreview.sprite = dragTarget.Visual.sprite; dragPreview.preserveAspect = true; dragPreview.raycastTarget = false;
                dragPreview.rectTransform.sizeDelta = dragTarget.Rect.sizeDelta; dragPreview.rectTransform.localScale = Vector3.one * 1.06f;
            }
            OnDrag(e);
        }
        Vector2 GestureDelta(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport.rectTransform,e.pressPosition,e.pressEventCamera,out var start);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport.rectTransform,e.position,e.pressEventCamera,out var current);
            return current-start;
        }
        void ResolveGesture(PointerEventData e)
        {
            if (shopGesture != ShopGesture.Pending) return;
            var delta = GestureDelta(e);
            if (Mathf.Abs(delta.x) >= 12f && Mathf.Abs(delta.x) > Mathf.Abs(delta.y)*1.2f)
            {
                shopGesture = ShopGesture.Swipe;
                ClearDragPreview(); // Browsing is never a food drop, even when the finger later crosses the cart.
            }
            else if (Mathf.Abs(delta.y) >= 12f && Mathf.Abs(delta.y) > Mathf.Abs(delta.x)*1.2f)
                shopGesture = dragTarget != null ? ShopGesture.Cart : ShopGesture.Ignored;
        }
        public void OnDrag(PointerEventData e)
        {
            if (!IsDragging) return;
            if (pointer != e.pointerId) { CancelInteraction(); return; }
            ResolveGesture(e);
            if (dragPreview == null) return;
            var parent = (RectTransform)dragPreview.transform.parent;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,e.position,e.pressEventCamera,out var local))
                dragPreview.rectTransform.localPosition = new Vector3(local.x,local.y+28,0);
            var image = CartDropZone.GetComponent<Image>();
            if (image != null) image.color = shopGesture == ShopGesture.Cart && ValidDrop(e) ? new Color32(255,225,154,255) : Cream;
        }
        bool ValidDrop(PointerEventData e)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(CartDropZone,e.position,e.pressEventCamera)) return false;
            hits.Clear(); EventSystem.current.RaycastAll(e,hits);
            return hits.Count == 0 || hits[0].gameObject == CartDropZone.gameObject;
        }
        public void OnEndDrag(PointerEventData e)
        {
            if (!IsDragging) return;
            if (pointer != e.pointerId) { CancelInteraction(); return; }
            ResolveGesture(e);
            var delta = GestureDelta(e);
            float threshold = Mathf.Max(44f,Viewport.rectTransform.rect.width*.10f);
            bool browse = shopGesture == ShopGesture.Swipe && Mathf.Abs(delta.x) >= threshold;
            var target = dragTarget;
            bool valid = shopGesture == ShopGesture.Cart && target != null && ValidDrop(e);
            CancelInteraction(); // Clear ownership before a page rebuild or selection callback.
            if (browse) swipePage?.Invoke(delta.x < 0f ? 1 : -1);
            else if (valid) select?.Invoke(target); // The caller revalidates QuoteCart; no debit here.
        }
        public void OnCancel(BaseEventData e) => CancelInteraction();
        void ClearDragPreview()
        {
            if (dragTarget != null) { dragTarget.Visual.color = RestingTint(dragTarget); dragTarget.Rect.localScale = dragTarget.HomeScale; }
            if (dragPreview != null) { dragPreview.gameObject.SetActive(false); Destroy(dragPreview.gameObject); }
            dragPreview = null; dragTarget = null;
        }
        void CancelInteraction()
        {
            if (pressedEvent != null) pressedEvent.eligibleForClick = false;
            pressedEvent = null;
            ClearDragPreview();
            shopGesture = ShopGesture.None; pressedTarget = null; pointer = NoPointer;
            suppressClick = true; pointerReleased = false;
            if (CartDropZone != null) { var image = CartDropZone.GetComponent<Image>(); if (image != null) image.color = Cream; }
        }
        void StopMotion(ManagementFoodTarget target)
        {
            if (motions.TryGetValue(target,out var old)) { if (old != null) StopCoroutine(old); motions.Remove(target); }
        }
        public void Feedback(ManagementFoodTarget target)
        {
            StopMotion(target); motions[target] = StartCoroutine(Pulse(target));
            if (cartMotion != null) StopCoroutine(cartMotion);
            if (CartDropZone != null) cartMotion = StartCoroutine(CartPulse());
        }
        IEnumerator Pulse(ManagementFoodTarget target)
        {
            float t = 0; target.Visual.color = new Color(1,.94f,.8f);
            while (t < .18f) { t += Time.unscaledDeltaTime; target.Rect.localScale = target.HomeScale * (1+.06f*Mathf.Sin(Mathf.Clamp01(t/.18f)*Mathf.PI)); yield return null; }
            target.Rect.localScale = target.HomeScale; target.Visual.color = RestingTint(target); motions.Remove(target);
        }
        IEnumerator CartPulse()
        {
            float t = 0;
            while (t < .18f) { t += Time.unscaledDeltaTime; CartDropZone.localScale = Vector3.one * (1+.025f*Mathf.Sin(Mathf.Clamp01(t/.18f)*Mathf.PI)); yield return null; }
            CartDropZone.localScale = Vector3.one; cartMotion = null;
        }
        public void MoveSelection(ManagementFoodTarget target,bool chosen,int slot)
        {
            target.Selected = chosen; target.SelectionSlot = chosen ? slot : -1; StopMotion(target);
            if (chosen) target.Rect.SetAsLastSibling();
            else target.Rect.SetSiblingIndex(Mathf.Min(target.DrawOrder,foodLayer.childCount-1));
            motions[target] = StartCoroutine(MoveUnit(target,chosen ? Position(target,PrepUV(slot)) : target.Home));
        }
        IEnumerator MoveUnit(ManagementFoodTarget target,Vector3 to)
        {
            var start = target.Rect.localPosition; float t = 0;
            target.Visual.color = target.Selected ? new Color(1,.96f,.82f) : RestingTint(target);
            while (t < .24f)
            {
                t += Time.unscaledDeltaTime; float progress = Mathf.Clamp01(t/.24f), eased = Mathf.SmoothStep(0,1,progress);
                target.Rect.localPosition = Vector3.Lerp(start,to,eased) + Vector3.up*(12*Mathf.Sin(progress*Mathf.PI));
                target.Rect.localScale = target.HomeScale*(1+.04f*Mathf.Sin(progress*Mathf.PI)); yield return null;
            }
            target.Rect.localPosition = to; target.Rect.localScale = target.HomeScale;
            target.Visual.color = RestingTint(target); motions.Remove(target);
        }
        void OnApplicationFocus(bool focused) { if (!focused) CancelInteraction(); }
        void OnApplicationPause(bool paused) { if (paused) CancelInteraction(); }
        void OnDisable()
        {
            CancelInteraction(); StopAllCoroutines(); motions.Clear(); cartMotion = null;
            if (CartDropZone != null) CartDropZone.localScale = Vector3.one;
        }
        void OnDestroy() { Targets.Clear(); tickets.Clear(); }

        Text SignText(Transform parent,string name,Vector2 pos,Vector2 size,int fontSize,Color color)
        {
            var text = new GameObject(name,typeof(RectTransform),typeof(Text)).GetComponent<Text>(); text.transform.SetParent(parent,false);
            text.rectTransform.anchoredPosition = pos; text.rectTransform.sizeDelta = size;
            text.font = font; text.fontSize = fontSize; text.color = color; text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false; text.verticalOverflow = VerticalWrapMode.Overflow; return text;
        }
        public void UpdateTicket(string id,int selected)
        {
            if(!tickets.TryGetValue(id,out var sign))return;
            bool configured=productsById.TryGetValue(id,out var product),unlocked=configured&&product.UnlockLevel<=CurrentLevel;
            sign.Price.text=configured?"$"+product.Price:"A definir";
            sign.Price.fontSize=configured?60:28;
            sign.Stock.text=!configured?"Sin precio":!unlocked?"Nivel "+product.UnlockLevel:service.Stock(id)==0?"AGOTADO":"Stock "+service.Stock(id);
            sign.Quantity.text=selected.ToString();sign.Minus.interactable=selected>0;
            sign.Plus.interactable=unlocked&&service.Stock(id)>selected;
        }
    }
}

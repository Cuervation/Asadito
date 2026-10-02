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
        public int SelectionSlot = -1;
        public Vector3 Home;
        public Vector2 DisplayUV;
        public Vector3 HomeScale = Vector3.one;
        public Image Visual;
        public RectTransform Rect => Visual.rectTransform;
        internal int DrawOrder;
    }

    /// <summary>Canvas-only food, painted depth and UI pointer ownership; no parallel inventory.</summary>
    public sealed class ManagementFoodView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler,
        IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, ICancelHandler
    {
        public const int ShopDisplayLimit = 24;
        const int NoPointer = int.MinValue;
        public Image Viewport { get; private set; }
        public bool IsFridge { get; private set; }
        public int CurrentLevel { get; private set; } = 1;
        public readonly List<ManagementFoodTarget> Targets = new List<ManagementFoodTarget>();
        public RectTransform CartDropZone { get; set; }
        public bool IsDragging => dragTarget != null;
        sealed class PriceSign { public RectTransform Root; public Text Product, Price, Stock; }
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
        Coroutine cartMotion;
        Vector2 lastSize;
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
        public void ChangePage() { page = (page + 1) % pageCount; BuildShop(); }
        void ClearShop()
        {
            CancelInteraction(); StopAllCoroutines(); motions.Clear(); cartMotion = null;
            foreach (var t in Targets) if (t != null) { t.gameObject.SetActive(false); Destroy(t.gameObject); }
            Targets.Clear();
            foreach (var s in tickets.Values) { s.Root.gameObject.SetActive(false); Destroy(s.Root.gameObject); }
            tickets.Clear();
        }
        void BuildShop()
        {
            ClearShop(); var products = Array.FindAll(service.Config.Products, p => p.UnlockLevel <= CurrentLevel);
            pageCount = Math.Max(1, (products.Length + 1) / 2);
            for (int group = 0; group < 2; group++)
            {
                int index = page * 2 + group; if (index >= products.Length) break;
                var product = products[index]; int count = Math.Min(ShopDisplayLimit, service.Stock(product.FoodId));
                // Full grill footprints, two overlapping layers, back to front. Stock never controls scale.
                for (int i = 0; i < count; i++)
                {
                    int layer = i / 12, row = i % 12 / 2, col = i % 2;
                    float x = (group == 0 ? .14f : .61f) + col * .20f + (row % 2) * .008f + layer * .015f;
                    float y = .595f - row * .048f - layer * .016f;
                    Food(product.FoodId, 0, new Vector2(x,y));
                }
                tickets[product.FoodId] = Ticket(product.FoodId, new Vector2(group == 0 ? .26f : .74f,.785f));
                UpdateTicket(product.FoodId,0);
            }
            RefreshLayout();
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
                StopMotion(target); target.Home = Position(target,target.DisplayUV);
                target.Rect.localPosition = target.Selected ? Position(target,PrepUV(target.SelectionSlot)) : target.Home;
                target.Rect.localScale = target.HomeScale; target.Visual.color = RestingTint(target);
            }
        }
        void LateUpdate()
        {
            if (Viewport.rectTransform.rect.size != lastSize) RefreshLayout();
            if (pointerReleased && !IsDragging) { pointer = NoPointer; pressedTarget = null; pointerReleased = false; }
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
                if (target == null || !target.isActiveAndEnabled) continue;
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
            if (pointer != NoPointer && pointer != e.pointerId) { CancelInteraction(); return; }
            if (IsDragging) return;
            pointer = e.pointerId; pressedTarget = Raycast(e.position,e.pressEventCamera);
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
            pointer = NoPointer; pressedTarget = null; pointerReleased = false;
        }
        public void OnBeginDrag(PointerEventData e)
        {
            if (IsFridge || CartDropZone == null || pointer != e.pointerId || pressedTarget == null || suppressClick) return;
            dragTarget = pressedTarget; suppressClick = true; StopMotion(dragTarget);
            dragTarget.Rect.localScale = dragTarget.HomeScale;
            dragTarget.Visual.color = new Color(1,.92f,.76f);
            var go = new GameObject("Dragged food preview",typeof(RectTransform),typeof(Image));
            go.transform.SetParent(Viewport.transform.parent,false); dragPreview = go.GetComponent<Image>();
            dragPreview.sprite = dragTarget.Visual.sprite; dragPreview.preserveAspect = true; dragPreview.raycastTarget = false;
            dragPreview.rectTransform.sizeDelta = dragTarget.Rect.sizeDelta; dragPreview.rectTransform.localScale = Vector3.one * 1.06f;
            OnDrag(e);
        }
        public void OnDrag(PointerEventData e)
        {
            if (!IsDragging) return;
            if (pointer != e.pointerId) { CancelInteraction(); return; }
            var parent = (RectTransform)dragPreview.transform.parent;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,e.position,e.pressEventCamera,out var local))
                dragPreview.rectTransform.localPosition = new Vector3(local.x,local.y+28,0);
            var image = CartDropZone.GetComponent<Image>();
            if (image != null) image.color = ValidDrop(e) ? new Color32(255,225,154,255) : Cream;
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
            var target = dragTarget; bool valid = ValidDrop(e); CancelInteraction();
            if (valid) select?.Invoke(target); // The caller revalidates QuoteCart; no debit here.
        }
        public void OnCancel(BaseEventData e) => CancelInteraction();
        void CancelInteraction()
        {
            if (dragTarget != null) { dragTarget.Visual.color = RestingTint(dragTarget); dragTarget.Rect.localScale = dragTarget.HomeScale; }
            if (dragPreview != null) { dragPreview.gameObject.SetActive(false); Destroy(dragPreview.gameObject); }
            dragPreview = null; dragTarget = null; pressedTarget = null; pointer = NoPointer;
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

        PriceSign Ticket(string id,Vector2 uv)
        {
            var root = new GameObject("Precio " + id,typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();
            root.SetParent(Viewport.transform,false); root.anchorMin = root.anchorMax = uv; root.sizeDelta = new Vector2(360,280);
            root.localScale = Vector3.one * .78f;
            root.GetComponent<Image>().color = new Color32(25,61,46,255); root.GetComponent<Image>().raycastTarget = true;
            Panel(root,"Metal stem",new Vector2(0,-170),new Vector2(12,75),new Color32(145,166,162,255));
            Panel(root,"Metal foot",new Vector2(0,-208),new Vector2(64,12),new Color32(80,110,103,255));
            Panel(root,"White price card",Vector2.zero,new Vector2(350,270),new Color32(255,250,237,255));
            Panel(root,"Product band",new Vector2(0,103),new Vector2(330,48),new Color32(25,61,46,255));
            var sign = new PriceSign { Root = root };
            sign.Product = SignText(root,"Product name",new Vector2(0,103),new Vector2(322,80),32,Color.white);
            sign.Price = SignText(root,"Large red price",new Vector2(0,20),new Vector2(328,275),110,new Color32(178,25,19,255));
            sign.Price.fontStyle = FontStyle.Bold;
            var outline = sign.Price.gameObject.AddComponent<Outline>(); outline.effectColor = new Color32(31,14,6,255); outline.effectDistance = new Vector2(1.3f,-1.3f);
            SignText(root,"Price unit",new Vector2(0,-67),new Vector2(324,60),24,new Color32(30,56,41,255)).text = "POR PIEZA";
            sign.Stock = SignText(root,"Stock and cart",new Vector2(0,-108),new Vector2(326,65),26,new Color32(30,56,41,255));
            Panel(root,"Metal card clip",new Vector2(0,-137),new Vector2(48,15),new Color32(115,135,130,255));
            return sign;
        }
        static void Panel(Transform parent,string name,Vector2 pos,Vector2 size,Color color)
        {
            var image = new GameObject(name,typeof(RectTransform),typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent,false); image.rectTransform.anchoredPosition = pos; image.rectTransform.sizeDelta = size;
            image.color = color; image.raycastTarget = true;
        }
        Text SignText(Transform parent,string name,Vector2 pos,Vector2 size,int fontSize,Color color)
        {
            var text = new GameObject(name,typeof(RectTransform),typeof(Text)).GetComponent<Text>(); text.transform.SetParent(parent,false);
            text.rectTransform.anchoredPosition = pos; text.rectTransform.sizeDelta = size;
            text.font = font; text.fontSize = fontSize; text.color = color; text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false; text.verticalOverflow = VerticalWrapMode.Overflow; return text;
        }
        public void UpdateTicket(string id,int quantity)
        {
            if (!tickets.TryGetValue(id,out var sign)) return;
            sign.Product.text = FoodCatalog.Get(id).DisplayName.ToUpper(); sign.Price.text = "$"+service.Config.Product(id).Price;
            sign.Stock.text = service.Stock(id) == 0 ? "AGOTADO" : "Stock "+service.Stock(id)+" · llevás "+quantity;
        }
    }
}

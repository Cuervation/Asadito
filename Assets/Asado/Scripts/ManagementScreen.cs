using System;
using System.Collections.Generic;
using System.Collections;
using Asadito.Runtime;
using UnityEngine;
using UnityEngine.UI;
namespace Asadito
{
    /// <summary>Event-driven mobile views; cooking remains owned by AsaditoGame.</summary>
    public sealed class ManagementScreen
    {
        private readonly AsaditoGame game;
        private readonly Transform parent;
        private readonly ManagementService service;
        private readonly ManagementArtLibrary art;
        private GameObject root;
        public bool IsOpen => root != null;
        private MvpLevelDefinition order;
        private GuestProfile[] guests;
        private Action<string[]> begin;
        private Action back;
        private string pendingDiscard;
        private readonly Dictionary<string, int> cart = new Dictionary<string, int>();
        private readonly Dictionary<string,Text> cartRows=new Dictionary<string,Text>();
        private readonly Dictionary<string,Button> cartRemoves=new Dictionary<string,Button>();
        private Text cartTotal,shopNotice,cartHeading,fridgeNotice,shopPageLabel;
        readonly Dictionary<string,Image> cartIcons=new Dictionary<string,Image>();
        private Button checkoutButton,emptyCartButton,prepareButton,discardButton;
        private ManagementFoodView foodView;
        private GameObject cartDetails;
        private bool cartExpanded;
        private bool showLearningTip;
        private bool Guided => order.Number == 1 && !service.State.ManagementTutorialCompleted;
        private int Required(string id) => Array.FindAll(order.FoodIds, food => food == id).Length;
        private int Missing(string id) => Math.Max(0, Required(id) - service.Available(id));
        private readonly List<int> selection = new List<int>();
        private float ContentWidth => ((RectTransform)root.transform).rect.width;
        private float FitWidth(float width) => Mathf.Min(width, Mathf.Max(1,ContentWidth - 32));
        private void Fit(RectTransform rect,float width,float margin=32,float x=-1,RectTransform bounds=null,float fraction=1)
            => rect.gameObject.AddComponent<ManagementWidthFit>().Configure(bounds ?? (RectTransform)root.transform,width,margin,x,fraction);
        private static readonly Color Ink = new Color32(34, 61, 45, 255);
        private static readonly Color Cream = new Color32(255, 244, 219, 255);
        public ManagementScreen(AsaditoGame game, Transform parent, ManagementService service)
        {
            this.game = game; this.parent = parent; this.service = service;
            art = Resources.Load<ManagementArtLibrary>("ManagementArt");
        }
        public void Close()
        {
            if (root != null) { root.SetActive(false); UnityEngine.Object.Destroy(root); }
            root = null; foodView = null;
        }
        private void Page(string title, string backdrop, bool scene = false, bool header = true)
        {
            Close();
            root = new GameObject("Management " + title, typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var bg = game.MakeImage("Management backdrop", rect, art != null && backdrop != null ? art.Get(backdrop) : null,
                scene ? new Color32(35,58,45,255) : new Color32(150, 150, 150, 255), Vector2.zero, Vector2.one, Vector2.zero);
            bg.rectTransform.offsetMin = bg.rectTransform.offsetMax = Vector2.zero;
            game.MakeImage("Management shade", rect, null, new Color32(20, 31, 24, (byte)(scene ? 35 : 145)), Vector2.zero, Vector2.one, Vector2.zero).raycastTarget = true;
            if(!header)return;
            Label(title, scene ? .945f : .90f, 52, Cream);
            Icon("AsaditoCoin_Hud", .26f, scene ? .888f : .835f, 58, 58);
            Label("" + service.Wallet.Balance + " MONEDAS", scene ? .888f : .835f, 34, Cream);
        }
        private Text Label(string text, float y, int size = 32, Color? color = null, float x = .5f, float width = 880, float height = 64)
        {
            var label = game.MakeText(text, root.transform, text, size, color ?? Ink, TextAnchor.MiddleCenter,
                x, y, FitWidth(width), Mathf.Max(height, size * 2.5f), (color ?? Ink) != Ink);
            if ((color ?? Ink) == Ink && game.ManagementBodyFont != null) label.font = game.ManagementBodyFont;
            // Dynamic font ascenders can extend beyond preferredHeight on mobile/camera render.
            label.verticalOverflow = VerticalWrapMode.Overflow;
            Fit(label.rectTransform,width);
            return label;
        }
        private void Panel(string name, float y, float h)
        { var panel=game.MakePanel(name, root.transform, new Color32(255, 245, 224, 246), .5f, y, FitWidth(940), h); Fit(panel.rectTransform,940); }
        private void Icon(string name, float x, float y, float w, float h)
        {
            if (art == null || art.Get(name) == null) return;
            var image = game.MakeImage(name, root.transform, art.Get(name), Color.white, new Vector2(x,y), new Vector2(x,y), new Vector2(w,h));
            image.preserveAspect = true; image.raycastTarget = false;
        }
        private Button Button(string label, float x, float y, Action action, float width = 400, Transform destination = null, float height = 88)
        {
            var button=game.MakeButton(label,destination ?? root.transform,x,y,width,height,new Color32(199,139,54,255),()=>action());
            // The existing arcade face/border/extrusion had fixed widths. Stretch only these
            // painted layers with the responsive button; retain their caps, heights and fonts.
            foreach(Transform child in button.transform)
            {
                if(!child.name.StartsWith("Cara boton ")&&!child.name.StartsWith("Borde boton ")&&!child.name.StartsWith("Relieve inferior "))continue;
                var layer=(RectTransform)child;float extra=layer.sizeDelta.x-width;
                layer.anchorMin=new Vector2(0,layer.anchorMin.y);layer.anchorMax=new Vector2(1,layer.anchorMax.y);
                layer.sizeDelta=new Vector2(extra,layer.sizeDelta.y);
            }
            if(destination==null)Fit(button.GetComponent<RectTransform>(),width,24,x);
            return button;
        }
        private void Save() { game.PersistManagement(); game.ManagementFeedback(); }
        public void Open(MvpLevelDefinition order, GuestProfile[] guests, Action<string[]> begin, Action back)
        {
            this.order = order; this.guests = guests; this.begin = begin; this.back = back;
            selection.Clear(); cart.Clear(); pendingDiscard = null;
            int bit = 1 << (order.Number - 1);
            showLearningTip = order.Number < 6 && (service.State.LearningTipsSeen & bit) == 0;
            service.State.LearningTipsSeen |= bit;
            game.PersistManagement(); Planning();
        }
        private void Planning()
        {
            Page("PRÓXIMO ASADO", "Patio_Management_Base");
            Panel("Order card", .69f, 390);
            Label("NIVEL " + order.Number + " · " + order.Title, .77f, 38);
            Label(guests.Length + " COMENSALES · " + order.FoodIds.Length + " PIEZAS", .715f, 32);
            Label(OrderSummary(), .66f, 30);
            Label(Guided ? (service.State.Inventory.Count>0 ? "Ya tenés stock. Tocá PREPARAR ASADO y elegí las piezas." : "Comprá carne, elegila en la heladera y llevála a la parrilla.") :
                showLearningTip ? order.LearningGoal : "Revisá tus invitados y lo que ya tenés", .60f, 27, height:110);
            for (int i=0;i<guests.Length;i++)
            {
                float y = .525f - i*.06f;
                string preference = "Gustos variados";
                foreach (var product in service.Config.Products)
                    if (guests[i].FavoriteFoods.Contains(product.FoodId)) preference = "Le encanta " + FoodCatalog.Get(product.FoodId).DisplayName;
                var portrait = game.MakeImage("Invitado " + guests[i].Id, root.transform, game.ManagementGuestSprite(guests[i].Id),
                    Color.white, new Vector2(.12f,y), new Vector2(.12f,y), new Vector2(84,84));
                portrait.preserveAspect = true;
                string request = FoodCatalog.Get(guests[i].RequestedFoodId).DisplayName.ToUpper();
                Label(guests[i].Name + " · PIDE " + request + "\n" + guests[i].PreferredDoneness.ToSpanish() +
                    "\n" + preference,
                    y, 22, Cream, x:.57f, width:770, height:110);
            }
            // Preparation is an explicit route, not a hidden consequence of opening storage.
            Button("PREPARAR ASADO", .5f, .225f, () => Fridge(), 880, height:120);
            Button("CARNICERÍA", .28f, .14f, () => Shop(), 430);
            Button("HELADERA", .72f, .14f, () => Fridge(), 430);
            Button("VOLVER", .5f, .07f, () => { Close(); back(); });
        }
        private string OrderSummary()
        {
            var ids = new Dictionary<string,int>();
            foreach (var id in order.FoodIds) { if (!ids.ContainsKey(id)) ids[id]=0; ids[id]++; }
            var lines=new List<string>(); foreach(var pair in ids) lines.Add(FoodCatalog.Get(pair.Key).DisplayName + " ×" + pair.Value);
            return string.Join(" · ", lines);
        }
        private int InCart(string id) => cart.TryGetValue(id,out int quantity) ? quantity : 0;
        private void Shop(string notice = "")
        {
            selection.Clear();Page("CARNICERÍA",null,true,false);
            var backdrop=new GameObject("Carnicería ilustrada",typeof(RectTransform),typeof(RawImage));
            var backdropRect=backdrop.GetComponent<RectTransform>();backdropRect.SetParent(root.transform,false);
            backdropRect.anchorMin=Vector2.zero;backdropRect.anchorMax=Vector2.one;backdropRect.offsetMin=backdropRect.offsetMax=Vector2.zero;
            backdrop.GetComponent<RawImage>().texture=art.Get("ButcherShop_CounterV2").texture;backdrop.GetComponent<RawImage>().raycastTarget=false;
            var header=game.MakePanel("Cartel carnicería",root.transform,new Color32(100,57,29,245),.5f,.948f,FitWidth(1040),148);Fit(header.rectTransform,1040);
            Button("‹",.075f,.948f,()=>{cart.Clear();Planning();},132,height:132).name="Volver carnicería";
            var title=Label("CARNICERÍA",.948f,62,Cream,x:.455f,width:600,height:130);
            title.rectTransform.GetComponent<ManagementWidthFit>().Configure((RectTransform)root.transform,600,0,-1,.57f);
            title.resizeTextForBestFit=true;title.resizeTextMinSize=34;title.resizeTextMaxSize=62;
            var wallet=game.MakePanel("Saldo carnicería",root.transform,Ink,.875f,.948f,240,115);Fit(wallet.rectTransform,240,12,.875f);
            Icon("AsaditoCoin_Hud",.80f,.948f,64,64);
            var balance=Label(service.Wallet.Balance.ToString("N0"),.948f,44,Cream,x:.89f,width:154,height:100);
            balance.rectTransform.GetComponent<ManagementWidthFit>().Configure((RectTransform)root.transform,154,0,-1,.16f);
            Icon("Butcher_Neutral",.16f,.834f,300,300);
            var bubble=game.MakePanel("Consejo carnicera",root.transform,Cream,.61f,.856f,FitWidth(640),164);Fit(bubble.rectTransform,640,12,.61f);
            var greeting=Label("¿Qué llevamos al asado?",.856f,38,Ink,x:.61f,width:600,height:140);
            Fit(greeting.rectTransform,600,32,.61f);
            var orderBand=game.MakePanel("Pedido orientativo",root.transform,Ink,.61f,.779f,FitWidth(660),84);Fit(orderBand.rectTransform,660,12,.61f);
            var orderHint=Label("PARA TU ASADO: "+OrderSummary(),.779f,27,Cream,x:.61f,width:640,height:75);
            Fit(orderHint.rectTransform,640,32,.61f);
            foodView=ManagementFoodView.Create(root.transform,new Vector2(.025f,.31f),new Vector2(.975f,.75f),game.ManagementBodyFont,service,false,
                target=>AddToCart(target.FoodId,target),game.ManagementFoodSprite,game.ManagementFoodSize);
            var displayFont=Resources.Load<Font>("Fonts/LilitaOne-Regular");
            foodView.SetShopPresentation(InCart,(id,delta)=>{if(delta>0)AddToCart(id);else RemoveFromCart(id);},game.MakePanel,
                (text,destination,x,y,width,height,action)=>Button(text,x,y,action,width,destination,height),displayFont);
            foodView.ConfigureShop(order.Number);
            foodView.SetShopSwipe(ChangeShopPage);
            // Keep the old pager band as breathing room above the unchanged cart.
            var swipeHint=game.MakePanel("Pista de deslizamiento",root.transform,new Color32(34,61,45,185),.5f,.286f,FitWidth(1000),58);
            swipeHint.raycastTarget=false;Fit(swipeHint.rectTransform,1000);
            shopPageLabel=Label("",.286f,25,Cream,width:940,height:65);
            shopPageLabel.font=game.ManagementBodyFont;shopPageLabel.raycastTarget=false;
            shopPageLabel.gameObject.name="Paginación carnicería";
            cartRows.Clear();cartRemoves.Clear();cartExpanded=false;
            var products=Array.FindAll(service.Config.Products,p=>p.UnlockLevel<=order.Number);
            cartDetails=new GameObject("Detalle carrito",typeof(RectTransform));cartDetails.transform.SetParent(root.transform,false);
            float cartWidth=FitWidth(960), rowsWidth=cartWidth-30;
            var details=cartDetails.GetComponent<RectTransform>();details.anchorMin=details.anchorMax=new Vector2(.5f,.50f);details.sizeDelta=new Vector2(cartWidth,354);Fit(details,960);
            var detailsPanel=game.MakePanel("Carrito de compra",details,Cream,.5f,.5f,cartWidth,354);detailsPanel.raycastTarget=true;Fit(detailsPanel.rectTransform,960,0,bounds:details);
            var viewport=new GameObject("Resumen carrito",typeof(RectTransform),typeof(RectMask2D),typeof(Image),typeof(ScrollRect)).GetComponent<RectTransform>();
            viewport.SetParent(details,false);viewport.anchorMin=viewport.anchorMax=new Vector2(.5f,.5f);viewport.sizeDelta=new Vector2(rowsWidth,320);Fit(viewport,930,30,bounds:details);
            viewport.GetComponent<Image>().color=Color.clear;viewport.GetComponent<Image>().raycastTarget=true;
            var content=new GameObject("Filas carrito",typeof(RectTransform)).GetComponent<RectTransform>();content.SetParent(viewport,false);
            content.anchorMin=content.anchorMax=content.pivot=new Vector2(0,1);content.sizeDelta=new Vector2(rowsWidth,Math.Max(320,products.Length*132));Fit(content,930,0,bounds:viewport);
            var scroll=viewport.GetComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;
            for(int i=0;i<products.Length;i++)
            {
                var p=products[i];var row=new GameObject("Fila carrito "+p.FoodId,typeof(RectTransform)).GetComponent<RectTransform>();row.SetParent(content,false);
                row.anchorMin=row.anchorMax=row.pivot=new Vector2(0,1);row.sizeDelta=new Vector2(rowsWidth,132);Fit(row,930,0,bounds:content);row.anchoredPosition=new Vector2(0,-i*132);
                cartRows[p.FoodId]=DisplayLabel("Resumen "+p.FoodId,"",row,new Vector2(0,-66),29,rowsWidth*.586f,76,true);
                var rowLabel=cartRows[p.FoodId].rectTransform;rowLabel.anchorMin=rowLabel.anchorMax=new Vector2(.302f,1);Fit(rowLabel,545,0,bounds:row,fraction:.586f);
                var remove=Button("−",.70f,.5f,()=>RemoveFromCart(p.FoodId),100,row,height:132);remove.name="Restar carrito "+p.FoodId;cartRemoves[p.FoodId]=remove;
                Button("+",.85f,.5f,()=>AddToCart(p.FoodId),100,row,height:132).name="Sumar carrito "+p.FoodId;
            }
            cartDetails.SetActive(false);
            var drop=game.MakePanel("Changuito de compras",root.transform,Cream,.5f,.175f,FitWidth(1020),224);drop.raycastTarget=true;Fit(drop.rectTransform,1020);
            foodView.CartDropZone=drop.rectTransform;DrawCart(drop.rectTransform);
            drop.transform.Find("Changuito").localScale=Vector3.one*.72f;
            cartHeading=Label("",.187f,30,Ink,x:.36f,width:320,height:95);
            cartTotal=Label("",.147f,40,Ink,x:.36f,width:340,height:85);
            Button("DETALLE",.18f,.175f,()=>ToggleCartDetails(!cartExpanded),220,height:150);
            // The basket itself is the detail affordance; keep its art unobscured.
            var detailButton=root.transform.Find("DETALLE").GetComponent<Button>();
            foreach(var image in detailButton.GetComponentsInChildren<Image>())image.color=Color.clear;
            foreach(var text in detailButton.GetComponentsInChildren<Text>())text.enabled=false;
            detailButton.transition=Selectable.Transition.None;
            Label("DETALLE",.13f,20,Ink,x:.16f,width:160,height:40);
            checkoutButton=Button("PAGAR Y SALIR",.75f,.175f,Checkout,450,height:160);
            emptyCartButton=Button("VACIAR",.22f,.049f,()=>{cart.Clear();ToggleCartDetails(false);RefreshCart("Carrito vacío. No se gastaron monedas.");},320,height:132);
            Button("HELADERA",.70f,.049f,()=>{cart.Clear();Fridge();},560,height:132);
            var noticeBand=game.MakePanel("Aviso de vitrina",root.transform,new Color32(24,43,33,240),.5f,.242f,FitWidth(710),60);noticeBand.raycastTarget=false;Fit(noticeBand.rectTransform,710,fraction:.62f);
            shopNotice=Label("",.242f,24,Cream,width:690,height:70);
            Fit(shopNotice.rectTransform,690,fraction:.60f);
            // Overlays are always above the card controls, not just above their food images.
            cartDetails.transform.SetAsLastSibling();
            RefreshCart(notice);
        }
        private void ChangeShopPage(int delta)
        { ToggleCartDetails(false);foodView.ChangePage(delta);RefreshCart(); }
        private void ToggleCartDetails(bool expanded)
        {
            cartExpanded=expanded;cartDetails.SetActive(expanded);float y=expanded?.617f:.242f;
            shopNotice.rectTransform.anchorMin=shopNotice.rectTransform.anchorMax=new Vector2(.5f,y);
            var band=root.transform.Find("Aviso de vitrina").GetComponent<RectTransform>();band.anchorMin=band.anchorMax=new Vector2(.5f,y);
        }
        private void DrawCart(RectTransform parent)
        {
            cartIcons.Clear();var basket=new GameObject("Changuito",typeof(RectTransform)).GetComponent<RectTransform>();basket.SetParent(parent,false);
            basket.anchorMin=basket.anchorMax=new Vector2(.14f,.54f);basket.sizeDelta=new Vector2(230,155);
            var steel=new Color32(68,94,84,255);
            void Stroke(string name,float x,float y,float w,float h,float angle=0)
            {var line=game.MakePanel(name,basket,steel,.5f,.5f,w,h);line.raycastTarget=false;line.rectTransform.anchoredPosition=new Vector2(x,y);line.rectTransform.localRotation=Quaternion.Euler(0,0,angle);}
            Stroke("Manija",-85,62,52,10);Stroke("Barra del mango",-59,29,10,78,10);
            Stroke("Borde superior",27,43,174,10);Stroke("Base canasto",26,-30,148,10);
            Stroke("Lateral",105,6,10,77,-12);Stroke("Ruedas soporte",20,-51,170,9);
            Stroke("Rueda izquierda",-25,-67,25,25);Stroke("Rueda derecha",79,-67,25,25);
            for(int i=0;i<4;i++)Stroke("Rejilla vertical",-27+i*36,8,5,62);
            Stroke("Rejilla horizontal",26,7,155,5);
            foreach(string id in new[]{"tira","chorizo"})
            {float x=id=="tira"?-7:62;var icon=game.MakeImage("Contenido changuito "+id,basket,game.ManagementFoodSprite(id),Color.white,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(100,70));
                icon.rectTransform.anchoredPosition=new Vector2(x,8);icon.preserveAspect=true;icon.raycastTarget=false;cartIcons[id]=icon;}
        }
        private Text DisplayLabel(string name,string value,Transform holder,Vector2 position,int size,float width,float height,bool top=false)
        {
            var label=game.MakeText(name,holder,value,size,Ink,TextAnchor.MiddleCenter,0,top ? 1 : .5f,width,Mathf.Max(height,size*2.5f),false);
            label.rectTransform.anchoredPosition=position;label.raycastTarget=false;label.verticalOverflow=VerticalWrapMode.Overflow;
            if(game.ManagementBodyFont!=null)label.font=game.ManagementBodyFont;return label;
        }
        private void RefreshCart(string notice="")
        {
            var quote=service.QuoteCart(cart,order.Number);
            int visibleRows=0;RectTransform rows=null;
            foreach(var pair in cartRows)
            {
                var p=service.Config.Product(pair.Key);int count=InCart(pair.Key);
                pair.Value.text=FoodCatalog.Get(pair.Key).DisplayName+" ×"+count+" · $"+p.Price*count;
                var row=(RectTransform)pair.Value.transform.parent;rows=(RectTransform)row.parent;
                row.gameObject.SetActive(count>0);
                if(count>0)row.anchoredPosition=new Vector2(0,-visibleRows++*132);
                cartRemoves[pair.Key].interactable=count>0;foodView.UpdateTicket(pair.Key,count);
            }
            if(rows!=null)rows.sizeDelta=new Vector2(rows.sizeDelta.x,Math.Max(320,visibleRows*132));
            cartHeading.text="TU CARRITO\n"+quote.Quantity+" "+(quote.Quantity==1?"pieza":"piezas");
            foreach(var pair in cartIcons){pair.Value.gameObject.SetActive(InCart(pair.Key)>0);}
            cartTotal.text="TOTAL $"+quote.Total;cartTotal.name=cartTotal.text;
            checkoutButton.interactable=quote.CanBuy;emptyCartButton.interactable=cart.Count>0;
            if(cart.Count>0&&quote.Error!=null)notice=quote.Error;
            shopNotice.text=string.IsNullOrEmpty(notice) ? cart.Count==0 ? "Tocá un corte o usá + para stockear" : "Revisá tu carrito y pagá una sola vez" : notice;
            shopPageLabel.text="Deslizá a los lados para ver más cortes · "+(foodView.PageIndex+1)+" / "+foodView.PageCount;
            foreach(var target in foodView.Targets)foodView.UpdateTicket(target.FoodId,InCart(target.FoodId));
        }
        private void AddToCart(string id,ManagementFoodTarget target=null)
        {
            var proposed=new Dictionary<string,int>(cart){[id]=InCart(id)+1};var quote=service.QuoteCart(proposed,order.Number);
            if(!quote.CanBuy){RefreshCart(quote.Error);return;}
            cart[id]=InCart(id)+1;game.ManagementFeedback();RefreshCart("+1 "+FoodCatalog.Get(id).DisplayName+" al carrito");
            if(target!=null)foodView.Feedback(target);
        }
        private void RemoveFromCart(string id)
        {
            int count=InCart(id);if(count<=1)cart.Remove(id);else cart[id]=count-1;game.ManagementFeedback();RefreshCart();
        }
        private void Checkout()
        {
            string error=service.BuyCart(cart,order.Number);if(error!=null){RefreshCart(error);return;}
            int count=0;foreach(var q in cart.Values)count+=q;cart.Clear();Save();
            Fridge("✓ Compra guardada: "+count+" piezas.\nTocá "+order.FoodIds.Length+" para este asado; el resto queda guardado.");
        }
        private void Fridge(string notice="")
        {
            selection.Clear();pendingDiscard=null;Page("HELADERA",null,true);
            Label(service.State.Inventory.Count+" / "+service.Config.FridgeCapacity+" GUARDADAS · "+OrderSummary(),.838f,27,Cream,height:80);
            var hint=Label("Elegí "+order.FoodIds.Length+" piezas para este asado. El resto queda guardado.\nPodés cambiar los cortes; se evaluará lo que sirvas.",.787f,25,Cream,height:86);
            hint.name="Fridge preparation hint";
            var scene=new GameObject("Fridge scene 2D",typeof(RectTransform));
            var sceneRect=scene.GetComponent<RectTransform>();sceneRect.SetParent(root.transform,false);
            sceneRect.anchorMin=new Vector2(.015f,.295f);sceneRect.anchorMax=new Vector2(.985f,.77f);sceneRect.offsetMin=sceneRect.offsetMax=Vector2.zero;
            var fridgeArt=art.Get("Fridge_Hybrid_OpenEmptyV2");
            var frame=new GameObject("Fridge art frame",typeof(RectTransform),typeof(AspectRatioFitter));
            var frameRect=frame.GetComponent<RectTransform>();frameRect.SetParent(sceneRect,false);
            frame.GetComponent<AspectRatioFitter>().aspectMode=AspectRatioFitter.AspectMode.FitInParent;
            frame.GetComponent<AspectRatioFitter>().aspectRatio=(float)fridgeArt.texture.width/fridgeArt.texture.height;
            var backdrop=new GameObject("Heladera ilustrada",typeof(RectTransform),typeof(RawImage));
            var backdropRect=backdrop.GetComponent<RectTransform>();backdropRect.SetParent(frameRect,false);
            backdropRect.anchorMin=Vector2.zero;backdropRect.anchorMax=Vector2.one;backdropRect.offsetMin=backdropRect.offsetMax=Vector2.zero;
            var image=backdrop.GetComponent<RawImage>();image.texture=fridgeArt.texture;image.raycastTarget=false;
            Canvas.ForceUpdateCanvases();
            foodView=ManagementFoodView.Create(frameRect,Vector2.zero,Vector2.one,game.ManagementBodyFont,service,true,SelectUnit,game.ManagementFoodSprite,game.ManagementFoodSize);
            fridgeNotice=Label("",.262f,26,Cream,height:105);fridgeNotice.name="Fridge preparation status";
            var selectionBand=game.MakePanel("Fridge selection band",root.transform,new Color32(24,43,33,230),.5f,.262f,FitWidth(990),106);selectionBand.transform.SetSiblingIndex(fridgeNotice.transform.GetSiblingIndex());Fit(selectionBand.rectTransform,990);
            prepareButton=Button("IR A LA PARRILLA",.5f,.193f,ConfirmPreparation,850,height:132);
            // Stable control identity for input/tests; its face communicates the next action.
            prepareButton.name="PREPARAR";
            if(service.CanRecover(order.FoodIds))Button("CAJA DEL ASADOR",Guided?.5f:.26f,.119f,()=>{
                if(!service.Recover(order.FoodIds))return;service.State.ActiveRun.Level=order.Number;Save();Close();begin(order.FoodIds);
            },Guided?650:440,height:132);
            else Button("COMPRAR",Guided?.5f:.26f,.119f,()=>Shop(),Guided?650:440,height:132);
            Button("VOLVER",.25f,.043f,()=>{selection.Clear();Planning();},440,height:132);
            Button("CANCELAR",.75f,.043f,CancelSelection,440,height:132);
            if(!Guided)discardButton=Button("DESCARTAR",.74f,.119f,DiscardSelection,440,height:132);
            else discardButton=null;
            if(service.State.Inventory.Count==0)Label("Heladera vacía\nComprá carne para tu asado",.58f,34,Ink,width:540,height:160);
            RefreshPreparation(notice);
        }
        private void SelectUnit(ManagementFoodTarget target)
        {
            if(selection.Contains(target.UnitId)){selection.Remove(target.UnitId);foodView.MoveSelection(target,false,0);RepositionSelection();RefreshPreparation();return;}
            if(target.Freshness==Freshness.Spoiled){RefreshPreparation("Esta pieza está podrida: no se puede cocinar");return;}
            // The order fixes batch size, not the paid player's choice of cuts (including L1).
            if(selection.Count>=order.FoodIds.Length){RefreshPreparation("Ya elegiste las "+order.FoodIds.Length+" piezas de este asado.\nTocá una de la tabla para cambiarla.");return;}
            selection.Add(target.UnitId);game.ManagementFeedback();foodView.MoveSelection(target,true,selection.Count-1);pendingDiscard=null;RefreshPreparation();
        }
        private void RepositionSelection(){for(int i=0;i<selection.Count;i++){var t=foodView.Targets.Find(x=>x.UnitId==selection[i]);if(t!=null)foodView.MoveSelection(t,true,i);}}
        private void CancelSelection()
        {
            foreach(int id in selection){var t=foodView.Targets.Find(x=>x.UnitId==id);if(t!=null)foodView.MoveSelection(t,false,0);}
            selection.Clear();pendingDiscard=null;RefreshPreparation("Selección cancelada: la carne sigue guardada");
        }
        private void RefreshPreparation(string notice="")
        {
            prepareButton.interactable=selection.Count==order.FoodIds.Length&&service.CanPrepareUnits(order.Number,selection);
            int remaining=Math.Max(0,order.FoodIds.Length-selection.Count);
            var action=prepareButton.GetComponentInChildren<Text>();
            action.text=prepareButton.interactable ? "IR A LA PARRILLA" : selection.Count==0 ? "ELEGÍ "+order.FoodIds.Length+" PIEZAS" : remaining>0 ? "FALTA"+(remaining==1 ? " 1 PIEZA" : "N "+remaining+" PIEZAS") : "REVISÁ LAS PIEZAS";
            if(discardButton!=null)discardButton.interactable=selection.Count>0;
            var counts=new Dictionary<string,int>();foreach(int id in selection){var u=service.State.Inventory.Find(x=>x.Id==id);if(u==null)continue;if(!counts.ContainsKey(u.FoodId))counts[u.FoodId]=0;counts[u.FoodId]++;}
            var names=new List<string>();foreach(var pair in counts)names.Add(FoodCatalog.Get(pair.Key).DisplayName+" ×"+pair.Value);
            string status=selection.Count==0 ? "Tocá "+order.FoodIds.Length+" piezas de los estantes para llevarlas a la tabla.\nDespués tocá IR A LA PARRILLA." :
                selection.Count+" / "+order.FoodIds.Length+" en la tabla · "+(remaining>0 ? "Falta"+(remaining==1 ? " 1 pieza" : "n "+remaining+" piezas") : "Listas para la parrilla")+"\n"+string.Join(" · ",names);
            fridgeNotice.text=string.IsNullOrEmpty(notice) ? status : notice;
        }
        private void ConfirmPreparation()
        {
            if(selection.Count!=order.FoodIds.Length||!service.PrepareUnits(order.Number,selection)){RefreshPreparation("No se pudo preparar: revisá las piezas");return;}
            var selected=service.State.ActiveRun.Units.ConvertAll(x=>x.FoodId).ToArray();Save();Close();begin(selected);
        }
        private void DiscardSelection()
        {
            if(Guided||selection.Count==0)return;
            if(pendingDiscard!="units"){pendingDiscard="units";RefreshPreparation("Descartar pierde lo pagado. Tocá DESCARTAR de nuevo");return;}
            foreach(int id in selection)service.Discard(id);Save();Fridge("Descarte registrado como pérdida");
        }
        private static string Coins(int amount)=>"$"+amount.ToString("N0",System.Globalization.CultureInfo.GetCultureInfo("es-AR"));
        private static string SignedCoins(int amount)=>(amount<0?"−":"+")+"$"+Math.Abs((long)amount).ToString("N0",System.Globalization.CultureInfo.GetCultureInfo("es-AR"));
        private Text ResultText(Transform parent,string name,string value,float x,float y,float width,float height,int size,Color color,bool display=false)
        {
            var text=game.MakeText(name,parent,value,size,color,TextAnchor.MiddleCenter,x,y,width,Mathf.Max(height,size*2.5f),false);
            text.font=display?game.ManagementDisplayFont:game.ManagementBodyFont;
            return text;
        }
        private Image ResultCard(Transform parent,string name,float x,float y,float width,float height,Color color)
            =>game.MakePanel(name,parent,color,x,y,width,height);
        private RectTransform ResultSheet(Transform parent,string name,float height=1660)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.gameObject.AddComponent<ManagementResultLayout>().Configure((RectTransform)parent,960,height);return rect;
        }
        private Button ResultLink(Transform parent,string label,float x,float y,Action action)
        {
            var button=Button(label,x,y,action,250,parent,72);
            foreach(var image in button.GetComponentsInChildren<Image>())image.color=Color.clear;
            var text=button.GetComponentInChildren<Text>();text.font=game.ManagementBodyFont;text.fontSize=25;text.color=Ink;
            foreach(var effect in text.GetComponents<BaseMeshEffect>())effect.enabled=false;
            button.targetGraphic=text;var colors=button.colors;colors.normalColor=Ink;colors.highlightedColor=new Color32(113,70,37,255);
            colors.pressedColor=new Color32(160,86,28,255);button.colors=colors;return button;
        }
        public void Results(EconomicResult result,Action retry,Action next,bool hasNext,string detail)
        {
            // Presentation only: no Complete(), purchases, save writes or progress mutation here.
            Page("RESULTADOS","Patio_Management_Base",false,false);root.name="Management ¡ASADO COMPLETADO!";
            var sheet=ResultSheet(root.transform,"Result sheet");
            string headline=!result.Passed?"¡LA PRÓXIMA SALE!":result.Stars>=3?"¡TE LUCISTE!":"¡BUEN ASADO!";
            ResultText(sheet,"Result headline",headline,.5f,.962f,920,90,56,Cream,true);
            int level=order!=null?order.Number:game.ManagementLevelNumber;
            string subtitle=result.Recovery?"CAJA DEL ASADOR · RECOMPENSA REDUCIDA":hasNext&&result.Passed?"NIVEL "+level+" · SIGUIENTE DESBLOQUEADO":
                "NIVEL "+level+(service.State.Inventory.Count>0?" · "+service.State.Inventory.Count+" PIEZAS SIGUEN GUARDADAS":" · CADA ASADO CUENTA");
            if(result.Order!=null&&!result.Order.IsComplete)subtitle="NIVEL "+level+" · PEDIDO INCORRECTO";
            ResultText(sheet,"Result context",subtitle,.5f,.913f,920,38,22,Cream);
            var board=game.MakeImage("Illustrated result board",sheet,art.Get("Frame_ResultPanel"),Color.white,new Vector2(.5f,.705f),new Vector2(.5f,.705f),new Vector2(940,628));
            board.preserveAspect=true;
            // The painted wooden slots are decoration; the actual earned/unearned stars are live UI.
            for(int i=0;i<3;i++)
            {
                var star=game.MakeImage("Management star "+i,board.transform,AsaditoUiIcons.Get(AsaditoUiIcon.Star),
                    i<result.Stars?new Color32(247,176,37,255):new Color32(111,99,74,255),
                    new Vector2(.5f+(i-1)*.171f,i==1?.895f:.863f),new Vector2(.5f+(i-1)*.171f,i==1?.895f:.863f),new Vector2(i==1?124:108,i==1?124:108));
                star.preserveAspect=true;
            }
            ResultText(board.transform,"Result general label","RESULTADO GENERAL",.5f,.705f,700,46,25,Ink);
            ResultText(board.transform,"GENERAL "+Mathf.RoundToInt(result.Overall)+"%",Mathf.RoundToInt(result.Overall)+"%",.5f,.555f,700,136,110,Ink,true);
            ResultLink(board.transform,"VER DETALLE",.5f,.385f,()=>ShowResultDetails(result,detail));
            var people=guests??game.ManagementGuests;
            for(int i=0;i<people.Length;i++)
            {
                float span=Mathf.Min(.74f,.16f*(people.Length-1));float x=people.Length==1?.5f:.5f-span*.5f+span*i/(people.Length-1);
                var portrait=game.MakeImage("Result guest "+people[i].Id,board.transform,game.ManagementResultGuestSprite(people[i].Id),Color.white,
                    new Vector2(x,.245f),new Vector2(x,.245f),new Vector2(86,86));portrait.preserveAspect=true;
            }
            var metrics=new[]{result.Asador,result.Economy,result.Operations};var captions=new[]{"ASADOR","GESTIÓN","OPERACIÓN"};
            for(int i=0;i<3;i++)
            {
                var card=ResultCard(sheet,"Result metric "+captions[i],.185f+i*.315f,.46f,280,124,Ink);
                ResultText(card.transform,"Result caption "+captions[i],captions[i],.5f,.72f,250,34,23,Cream);
                ResultText(card.transform,"Result value "+captions[i],Mathf.RoundToInt(metrics[i])+"%",.5f,.34f,250,60,48,Cream,true);
            }
            var ledger=game.MakeImage("Illustrated result wallet",sheet,art.Get("Frame_ProductCard"),Color.white,new Vector2(.5f,.312f),new Vector2(.5f,.312f),new Vector2(900,292));
            ledger.type=Image.Type.Sliced;
            ResultText(ledger.transform,"Result balance caption","TU SALDO",.29f,.83f,280,34,22,Ink);
            ResultText(ledger.transform,"Result balance",Coins(result.Balance),.29f,.64f,340,66,48,Ink,true);
            string profitName=result.Profit<0?"PÉRDIDA DEL ASADO":"GANANCIA DEL ASADO";
            ResultText(ledger.transform,"Result profit caption",profitName,.70f,.83f,340,34,22,Ink);
            ResultText(ledger.transform,"Result profit",SignedCoins(result.Profit),.70f,.64f,350,66,44,result.Profit<0?new Color32(171,62,40,255):Ink,true);
            var moneyLabels=new[]{"INGRESO","CARNE USADA","DESPERDICIO"};var moneyValues=new[]{SignedCoins(result.Income),Coins(result.Spending),Coins(result.Waste)};
            for(int i=0;i<3;i++)
            {
                float x=.23f+i*.27f;
                ResultText(ledger.transform,"Result money label "+i,moneyLabels[i],x,.37f,225,32,19,Ink);
                ResultText(ledger.transform,"Result money value "+i,moneyValues[i],x,.22f,225,45,30,Ink,true);
            }
            var tip=ResultCard(sheet,"Result advice card",.5f,.162f,880,172,new Color32(28,48,35,240));
            ResultText(tip.transform,"Result advice title",result.Passed&&hasNext?"¡EL PRÓXIMO ASADO TE ESPERA!":"PARA EL PRÓXIMO ASADO",.5f,.79f,830,38,24,Cream);
            string advice=result.Order!=null&&!result.Order.IsComplete?"Pedido incorrecto. Serví lo que pidió cada comensal. Mirá los faltantes en VER DETALLE.":result.Advice;
            ResultText(tip.transform,"Result advice",advice,.5f,.39f,820,100,27,Cream);
            if(hasNext)
            {
                var secondary=Button("OTRO ASADO",.23f,.06f,()=>{Close();retry();},370,sheet,140);secondary.GetComponentInChildren<Text>().fontSize=38;
                var primary=Button("SIGUIENTE",.69f,.06f,()=>{Close();next();},470,sheet,140);primary.GetComponentInChildren<Text>().fontSize=44;
            }
            else
            {
                var secondary=Button("NIVELES",.20f,.06f,()=>{Close();next();},300,sheet,140);secondary.GetComponentInChildren<Text>().fontSize=38;
                var primary=Button("OTRO ASADO",.67f,.06f,()=>{Close();retry();},510,sheet,140);primary.GetComponentInChildren<Text>().fontSize=44;
            }
        }
        private void ShowResultDetails(EconomicResult result,string detail)
        {
            if(root==null||root.transform.Find("Result detail overlay")!=null)return;
            var overlay=new GameObject("Result detail overlay",typeof(RectTransform));var full=(RectTransform)overlay.transform;full.SetParent(root.transform,false);
            full.anchorMin=Vector2.zero;full.anchorMax=Vector2.one;full.offsetMin=full.offsetMax=Vector2.zero;
            var dim=game.MakeImage("Result detail dim",full,null,new Color32(16,27,20,235),Vector2.zero,Vector2.one,Vector2.zero);
            dim.rectTransform.offsetMin=dim.rectTransform.offsetMax=Vector2.zero;dim.raycastTarget=true;
            var sheet=ResultSheet(full,"Result detail sheet",1420);
            ResultCard(sheet,"Result detail paper",.5f,.5f,940,1390,Cream);
            ResultText(sheet,"Result detail title","TU ASADO, EN DETALLE",.5f,.929f,880,90,46,Ink,true);
            var viewport=new GameObject("Result detail viewport",typeof(RectTransform),typeof(Image),typeof(RectMask2D),typeof(ScrollRect));
            var area=(RectTransform)viewport.transform;area.SetParent(sheet,false);area.anchorMin=new Vector2(.075f,.17f);area.anchorMax=new Vector2(.925f,.855f);area.offsetMin=area.offsetMax=Vector2.zero;
            viewport.GetComponent<Image>().color=Color.clear;
            var content=new GameObject("Result detail content",typeof(RectTransform)).GetComponent<RectTransform>();content.SetParent(area,false);
            content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;
            string orderStatus=result.Order==null?"":("PEDIDO "+(result.Order.IsComplete?"CUMPLIDO":"INCORRECTO")+" · "+result.Order.MatchedCount+" / "+result.Order.RequiredCount+" piezas correctas\n\n");
            string orderAdvice=result.Order!=null&&!result.Order.IsComplete?result.Advice+"\n\n":"";
            string explanation=orderStatus+orderAdvice+"ASADOR "+Mathf.RoundToInt(result.Asador)+"%   ·   GESTIÓN "+Mathf.RoundToInt(result.Economy)+"%\nOPERACIÓN "+Mathf.RoundToInt(result.Operations)+"%   ·   GENERAL "+Mathf.RoundToInt(result.Overall)+"%\n\n"+
                "Carne utilizada: "+Coins(result.Spending)+"\nDesperdicio: "+Coins(result.Waste)+" (incluido en el gasto)\nIngreso: "+SignedCoins(result.Income)+"\n"+(result.Profit<0?"Pérdida: ":"Ganancia: ")+SignedCoins(result.Profit)+"\nSaldo: "+Coins(result.Balance)+"\n\nLOS INVITADOS\n\n"+detail;
            var body=ResultText(content,"Result detailed breakdown",explanation,.5f,1,790,1000,29,Ink);body.alignment=TextAnchor.UpperLeft;
            body.verticalOverflow=VerticalWrapMode.Overflow;Canvas.ForceUpdateCanvases();
            content.sizeDelta=new Vector2(0,Mathf.Max(area.rect.height,body.preferredHeight+32));
            body.rectTransform.anchorMin=Vector2.zero;body.rectTransform.anchorMax=Vector2.one;body.rectTransform.offsetMin=new Vector2(12,16);body.rectTransform.offsetMax=new Vector2(-12,-16);
            var scroll=viewport.GetComponent<ScrollRect>();scroll.viewport=area;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=42;
            Button("VOLVER AL RESUMEN",.5f,.065f,()=>{overlay.SetActive(false);UnityEngine.Object.Destroy(overlay);},820,sheet,140).GetComponentInChildren<Text>().fontSize=42;
        }
    }
}

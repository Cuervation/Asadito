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
        private Text cartTotal,shopNotice,cartHeading,fridgeNotice;
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
        private void Page(string title, string backdrop, bool scene = false)
        {
            Close();
            root = new GameObject("Management " + title, typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var bg = game.MakeImage("Management backdrop", rect, art != null && backdrop != null ? art.Get(backdrop) : null,
                scene ? new Color32(35,58,45,255) : new Color32(150, 150, 150, 255), Vector2.zero, Vector2.one, Vector2.zero);
            bg.rectTransform.offsetMin = bg.rectTransform.offsetMax = Vector2.zero;
            game.MakeImage("Management shade", rect, null, new Color32(20, 31, 24, (byte)(scene ? 35 : 145)), Vector2.zero, Vector2.one, Vector2.zero).raycastTarget = true;
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
            Label(Guided ? "1 · Mirá el pedido. Después entrá a la carnicería." :
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
                Label(guests[i].Name + " · " + guests[i].PreferredDoneness.ToSpanish() + " · " + Mathf.RoundToInt(guests[i].TargetFoodAmount*1000) + " g\n" + preference,
                    y, 28, Cream, x:.57f, width:770, height:102);
            }
            Button("CARNICERÍA", .28f, .21f, () => Shop(), 430);
            Button("HELADERA", .72f, .21f, () => Fridge(), 430).interactable = !Guided || service.CanPrepare(order.FoodIds) || service.CanRecover(order.FoodIds);
            Button("VOLVER", .5f, .12f, () => { Close(); back(); });
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
            selection.Clear();Page("CARNICERÍA",null,true);
            Label("Para tu asado: "+OrderSummary(),.835f,28,Cream,height:80);
            var backdrop=new GameObject("Mostrador ilustrado",typeof(RectTransform),typeof(RawImage));
            var backdropRect=backdrop.GetComponent<RectTransform>();backdropRect.SetParent(root.transform,false);
            backdropRect.anchorMin=new Vector2(.015f,.32f);backdropRect.anchorMax=new Vector2(.985f,.815f);backdropRect.offsetMin=backdropRect.offsetMax=Vector2.zero;
            var backdropImage=backdrop.GetComponent<RawImage>();backdropImage.raycastTarget=false;
            var counter=art.Get("ButcherShop_CounterV2");backdropImage.texture=counter.texture;backdropImage.uvRect=new Rect(0,.22f,1,.64f);
            foodView=ManagementFoodView.Create(root.transform,new Vector2(.015f,.32f),new Vector2(.985f,.815f),game.ManagementBodyFont,service,false,
                target=>AddToCart(target.FoodId,target),game.ManagementFoodSprite,game.ManagementFoodSize);
            foodView.ConfigureShop(order.Number);
            cartRows.Clear();cartRemoves.Clear();cartExpanded=false;
            var products=Array.FindAll(service.Config.Products,p=>p.UnlockLevel<=order.Number);
            cartDetails=new GameObject("Detalle carrito",typeof(RectTransform));cartDetails.transform.SetParent(root.transform,false);
            float cartWidth=FitWidth(960), rowsWidth=cartWidth-30;
            var details=cartDetails.GetComponent<RectTransform>();details.anchorMin=details.anchorMax=new Vector2(.5f,.410f);details.sizeDelta=new Vector2(cartWidth,354);Fit(details,960);
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
            var drop=game.MakePanel("Changuito de compras",root.transform,Cream,.5f,.269f,FitWidth(960),174);drop.raycastTarget=true;Fit(drop.rectTransform,960);
            foodView.CartDropZone=drop.rectTransform;DrawCart(drop.rectTransform);
            cartHeading=Label("",.269f,30,Ink,x:.48f,width:430,height:130);
            Button("DETALLE",.84f,.269f,()=>ToggleCartDetails(!cartExpanded),256,height:132);
            cartTotal=Label("",.193f,36,Cream,x:.30f,width:520,height:90);
            emptyCartButton=Button("VACIAR",.80f,.193f,()=>{cart.Clear();ToggleCartDetails(false);RefreshCart("Carrito vacío. No se gastaron monedas.");},268,height:132);
            var noticeBand=game.MakePanel("Aviso de vitrina",root.transform,new Color32(24,43,33,230),.5f,.339f,FitWidth(970),82);noticeBand.raycastTarget=false;Fit(noticeBand.rectTransform,970);
            shopNotice=Label("",.339f,27,Cream,height:90);
            checkoutButton=Button("PAGAR Y SALIR",.5f,.119f,Checkout,740,height:132);
            Button("VOLVER",.26f,.043f,()=>{cart.Clear();Planning();},440,height:132).name="Volver carnicería";
            Button("HELADERA",.74f,.043f,()=>{cart.Clear();Fridge();},440,height:132).interactable=!Guided||service.CanPrepare(order.FoodIds)||service.CanRecover(order.FoodIds);
            if(products.Length>2)Button("OTROS CORTES",.5f,.755f,()=>{foodView.ChangePage();RefreshCart();},440);
            RefreshCart(notice);
        }
        private void ToggleCartDetails(bool expanded)
        {
            cartExpanded=expanded;cartDetails.SetActive(expanded);float y=expanded?.548f:.339f;
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
            foreach(var pair in cartRows)
            {
                var p=service.Config.Product(pair.Key);int count=InCart(pair.Key);
                pair.Value.text=FoodCatalog.Get(pair.Key).DisplayName+" ×"+count+" · $"+p.Price*count;
                cartRemoves[pair.Key].interactable=count>0;foodView.UpdateTicket(pair.Key,count);
            }
            cartHeading.text="TU CHANGUITO\n"+(quote.Quantity==0?"Arrastrá la carne acá":quote.Quantity+" piezas · mirá DETALLE");
            foreach(var pair in cartIcons){pair.Value.gameObject.SetActive(InCart(pair.Key)>0);}
            cartTotal.text="TOTAL $"+quote.Total;cartTotal.name=cartTotal.text;
            checkoutButton.interactable=quote.CanBuy;emptyCartButton.interactable=cart.Count>0;
            if(cart.Count>0&&quote.Error!=null)notice=quote.Error;
            shopNotice.text=string.IsNullOrEmpty(notice) ? cart.Count==0 ? "Arrastrá la carne del mostrador al changuito" : "Revisá tu carrito y pagá una sola vez" : notice;
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
            int count=0;foreach(var q in cart.Values)count+=q;cart.Clear();Save();Fridge("✓ Compra lista: "+count+" piezas en tu heladera");
        }
        private void Fridge(string notice="")
        {
            selection.Clear();pendingDiscard=null;Page("HELADERA",null,true);
            Label(service.State.Inventory.Count+" / "+service.Config.FridgeCapacity+" ESPACIOS · "+OrderSummary(),.835f,28,Cream,height:90);
            var scene=new GameObject("Fridge scene 2D",typeof(RectTransform));
            var sceneRect=scene.GetComponent<RectTransform>();sceneRect.SetParent(root.transform,false);
            sceneRect.anchorMin=new Vector2(.015f,.295f);sceneRect.anchorMax=new Vector2(.985f,.805f);sceneRect.offsetMin=sceneRect.offsetMax=Vector2.zero;
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
            fridgeNotice=Label("",.262f,28,Cream,height:105);
            var selectionBand=game.MakePanel("Fridge selection band",root.transform,new Color32(24,43,33,230),.5f,.262f,FitWidth(990),106);selectionBand.transform.SetSiblingIndex(fridgeNotice.transform.GetSiblingIndex());Fit(selectionBand.rectTransform,990);
            prepareButton=Button("PREPARAR",.5f,.193f,ConfirmPreparation,650,height:132);
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
            int chosen=0;foreach(int id in selection)if(service.State.Inventory.Find(u=>u.Id==id)?.FoodId==target.FoodId)chosen++;
            if(selection.Count>=order.FoodIds.Length||(Guided&&chosen>=Required(target.FoodId))){RefreshPreparation("La bandeja está completa. Tocá una pieza para devolverla");return;}
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
            if(discardButton!=null)discardButton.interactable=selection.Count>0;
            var counts=new Dictionary<string,int>();foreach(int id in selection){var u=service.State.Inventory.Find(x=>x.Id==id);if(u==null)continue;if(!counts.ContainsKey(u.FoodId))counts[u.FoodId]=0;counts[u.FoodId]++;}
            var names=new List<string>();foreach(var pair in counts)names.Add(FoodCatalog.Get(pair.Key).DisplayName+" ×"+pair.Value);
            fridgeNotice.text=string.IsNullOrEmpty(notice) ? selection.Count+" / "+order.FoodIds.Length+" listas · "+(names.Count>0 ? string.Join(" · ",names) : "Tocá las piezas de los estantes") : notice;
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
        public void Results(EconomicResult result, Action retry, Action next, bool hasNext, string detail)
        {
            Page(result.Passed ? "¡ASADO COMPLETADO!" : "¡A SEGUIR PRACTICANDO!", "Patio_Management_Base");
            root.name = "Management ¡ASADO COMPLETADO!";
            Panel("Economic result card",.50f,1160);
            Label("ASADOR "+Mathf.RoundToInt(result.Asador)+"%",.75f,42);
            Label("GESTIÓN "+Mathf.RoundToInt(result.Economy)+"% · OPERACIÓN "+Mathf.RoundToInt(result.Operations)+"%",.69f,30);
            Label("GENERAL "+Mathf.RoundToInt(result.Overall)+"%",.635f,38);
            for (int i = 0; i < 3; i++)
                game.MakeImage("Management star " + i, root.transform, AsaditoUiIcons.Get(AsaditoUiIcon.Star),
                    i < result.Stars ? new Color32(229,155,38,255) : new Color32(139,119,91,100),
                    new Vector2(.44f + i*.06f,.597f), new Vector2(.44f + i*.06f,.597f), new Vector2(44,44));
            Label("Carne usada "+result.Spending+"  ·  Desperdicio "+result.Waste,.56f,32);
            Label("Ingresos +"+result.Income+"  ·  Ganancia "+(result.Profit>=0?"+":"")+result.Profit,.51f,32);
            Label("SALDO "+result.Balance,.455f,42);
            Label(result.Recovery?"Caja del Asador: recompensa reducida":result.Passed && hasNext ? "¡Desbloqueaste el próximo nivel!" : "La carne sin preparar sigue en tu heladera",.405f,27);
            Label(result.Advice,.36f,27,height:100);
            Label(detail,.275f,24,Ink,height:260);
            Button("OTRO ASADO",.28f,.17f,()=>{Close();retry();},430);
            Button(hasNext?"SIGUIENTE":"NIVELES",.72f,.17f,()=>{Close();next();},430);
        }
    }
}

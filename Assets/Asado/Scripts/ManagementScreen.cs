using System;
using System.Collections.Generic;
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
        private string pendingPurchase, pendingDiscard;
        private bool showLearningTip;
        private bool Guided => order.Number == 1 && !service.State.ManagementTutorialCompleted;
        private int Required(string id) => Array.FindAll(order.FoodIds, food => food == id).Length;
        private int Missing(string id) => Math.Max(0, Required(id) - service.Available(id));
        private readonly List<string> selection = new List<string>();
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
            root = null;
        }
        private void Page(string title, string backdrop)
        {
            Close();
            root = new GameObject("Management " + title, typeof(RectTransform));
            var rect = root.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var bg = game.MakeImage("Management backdrop", rect, art != null ? art.Get(backdrop) : null,
                new Color32(150, 150, 150, 255), Vector2.zero, Vector2.one, Vector2.zero);
            bg.rectTransform.offsetMin = bg.rectTransform.offsetMax = Vector2.zero;
            game.MakeImage("Management shade", rect, null, new Color32(20, 31, 24, 145), Vector2.zero, Vector2.one, Vector2.zero).raycastTarget = true;
            Label(title, .90f, 52, Cream);
            Icon("AsaditoCoin_Hud", .26f, .835f, 58, 58);
            Label("" + service.Wallet.Balance + " MONEDAS", .835f, 34, Cream);
        }
        private Text Label(string text, float y, int size = 32, Color? color = null, float x = .5f, float width = 880, float height = 64)
        {
            var label = game.MakeText(text, root.transform, text, size, color ?? Ink, TextAnchor.MiddleCenter,
                x, y, width, Mathf.Max(height, size * 2.5f), (color ?? Ink) != Ink);
            if ((color ?? Ink) == Ink && game.ManagementBodyFont != null) label.font = game.ManagementBodyFont;
            // Dynamic font ascenders can extend beyond preferredHeight on mobile/camera render.
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }
        private void Panel(string name, float y, float h)
            => game.MakePanel(name, root.transform, new Color32(255, 245, 224, 246), .5f, y, 940, h);
        private void Icon(string name, float x, float y, float w, float h)
        {
            if (art == null || art.Get(name) == null) return;
            var image = game.MakeImage(name, root.transform, art.Get(name), Color.white, new Vector2(x,y), new Vector2(x,y), new Vector2(w,h));
            image.preserveAspect = true; image.raycastTarget = false;
        }
        private Button Button(string label, float x, float y, Action action, float width = 400)
            => game.MakeButton(label, root.transform, x,y,width,88, new Color32(199,139,54,255), () => action());
        private void Save() { game.PersistManagement(); game.ManagementFeedback(); }
        public void Open(MvpLevelDefinition order, GuestProfile[] guests, Action<string[]> begin, Action back)
        {
            this.order = order; this.guests = guests; this.begin = begin; this.back = back;
            selection.Clear(); pendingPurchase = pendingDiscard = null;
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
        private void Shop(string notice = "")
        {
            Page("CARNICERÍA", "ButcherShop_Background");
            Icon("Butcher_Greeting", .84f,.745f,170,230);
            Label("Pedido: " + OrderSummary(), .75f, 28, Cream, x:.40f,width:720,height:130);
            for (int i=0;i<service.Config.Products.Length;i++)
            {
                var product=service.Config.Products[i]; float y=.59f-i*.215f;
                Panel("Product " + product.FoodId,y,340);
                var image=game.MakeImage("Shop food " + product.FoodId,root.transform, game.ManagementFoodSprite(product.FoodId),Color.white,new Vector2(.18f,y),new Vector2(.18f,y),new Vector2(210,130)); image.preserveAspect=true;
                Label(FoodCatalog.Get(product.FoodId).DisplayName,y+.06f,32,x:.61f,width:600);
                Label(product.Price+" monedas · "+Mathf.RoundToInt(product.PortionAmount*1000)+" g · stock "+service.Stock(product.FoodId),y+.018f,27,x:.61f,width:600);
                Label("Heladera " + service.Available(product.FoodId) + " · sugeridas " + Required(product.FoodId) + " · faltan " + Missing(product.FoodId),y-.018f,27,x:.61f,width:610);
                string action = pendingPurchase == product.FoodId ? "CONFIRMAR −" + product.Price : "COMPRAR +1";
                var button=Button(action,.61f,y-.061f,()=>Purchase(product.FoodId),460);
                button.name="Comprar " + product.FoodId;
                button.interactable=order.Number>=product.UnlockLevel && service.Stock(product.FoodId)>0 && (!Guided || Missing(product.FoodId)>0);
            }
            if (string.IsNullOrEmpty(notice)) notice = Guided ? service.CanPrepare(order.FoodIds) ? "¡Pedido comprado! Revisá tu heladera." : "2 · Comprá 1 chorizo y 1 tira. Confirmá cada compra." : "Confirmá el precio antes de comprar.";
            Label(notice,.245f,28,Cream,height:95);
            Button("HELADERA",.5f,.16f,()=> { pendingPurchase=null; Fridge(); },560).interactable=!Guided || service.CanPrepare(order.FoodIds) || service.CanRecover(order.FoodIds);
            Button("VOLVER",.5f,.08f,()=> { pendingPurchase=null; Planning(); }).name = "Volver carnicería";
        }
        private void Purchase(string id)
        {
            if (pendingPurchase != id)
            {
                pendingPurchase=id;
                Shop("Confirmá: " + FoodCatalog.Get(id).DisplayName + " por " + service.Config.Product(id).Price + " monedas.");
                return;
            }
            pendingPurchase=null;
            string error=service.Buy(id,1,order.Number);
            if(error==null) Save();
            Shop(error ?? "✓ ¡Compra lista! Carne guardada en la heladera.");
        }
        private void Fridge(string notice = "")
        {
            Page("HELADERA", "Patio_Management_Base");
            Icon("Fridge_Tier1_OpenEmpty",.18f,.73f,230,270);
            Label(service.State.Inventory.Count+" / "+service.Config.FridgeCapacity+" ESPACIOS",.745f,34,Cream,x:.61f,width:630);
            Label("Elegí "+order.FoodIds.Length+" piezas · "+selection.Count+" listas",.685f,30,Cream,x:.61f,width:630);
            for(int i=0;i<service.Config.Products.Length;i++)
            {
                var product=service.Config.Products[i]; float y=.56f-i*.18f;
                Panel("Inventory "+product.FoodId,y,270);
                var image=game.MakeImage("Fridge food "+product.FoodId,root.transform,game.ManagementFoodSprite(product.FoodId),Color.white,new Vector2(.17f,y),new Vector2(.17f,y),new Vector2(220,130)); image.preserveAspect=true;
                int chosen=selection.FindAll(x=>x==product.FoodId).Count;
                Label(FoodCatalog.Get(product.FoodId).DisplayName,y+.044f,32,x:.57f,width:630);
                Label("Tenés "+service.Available(product.FoodId)+" · elegidas "+chosen+" · sugeridas "+Required(product.FoodId),y,27,x:.57f,width:650);
                Button("−",.40f,y-.055f,()=>{ selection.Remove(product.FoodId); Fridge(); },120).interactable=chosen>0;
                var add = Button("+",.60f,y-.055f,()=>{selection.Add(product.FoodId);Fridge();},120);
                add.name = "Preparar " + product.FoodId;
                add.interactable=chosen<service.Available(product.FoodId)&&selection.Count<order.FoodIds.Length && (!Guided || chosen < Required(product.FoodId));
                var discard=Button(pendingDiscard==product.FoodId?"CONFIRMAR":"DESCARTAR",.82f,y-.055f,()=>{
                    if (pendingDiscard!=product.FoodId) { pendingDiscard=product.FoodId; Fridge("Descartar pierde lo que pagaste. Confirmá o elegí otra acción."); return; }
                    pendingDiscard=null;
                    var unit=service.State.Inventory.FindLast(x=>x.FoodId==product.FoodId);
                    if(unit!=null&&service.Discard(unit.Id)){selection.Remove(product.FoodId);Save();} Fridge("Descarte registrado como pérdida");
                },240);
                discard.interactable=!Guided && service.Available(product.FoodId)>0;
            }
            Label(string.IsNullOrEmpty(notice) && Guided ? service.CanRecover(order.FoodIds) ? "Si no podés comprar, usá la Caja del Asador para seguir." : "3 · Elegí 1 tira y 1 chorizo con +. Después prepará." : notice,.265f,28,Cream,height:110);
            bool ready=selection.Count==order.FoodIds.Length && service.CanPrepare(selection.ToArray());
            Button("PREPARAR",.5f,.205f,()=>{
                var selected=selection.ToArray(); if(!service.Prepare(order.Number,selected))return;Save();Close();begin(selected);
            },580).interactable=ready;
            if(service.CanRecover(order.FoodIds))
                Button("CAJA DEL ASADOR",.5f,.14f,()=>{
                    if(!service.Recover(order.FoodIds))return; service.State.ActiveRun.Level=order.Number;Save();Close();begin(order.FoodIds);
                },580);
            else Button("COMPRAR",.5f,.14f,()=>Shop(),580);
            Label(service.CanRecover(order.FoodIds)?"Ayuda: recompensa reducida · máximo " + service.Config.RecoveryStarCap + " estrella":"La carne que no uses queda guardada",.095f,25,Cream,height:55);
            Button("VOLVER",.5f,.045f,Planning,360);
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

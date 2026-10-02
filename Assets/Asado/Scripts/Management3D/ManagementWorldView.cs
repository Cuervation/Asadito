using System;
using System.Collections;
using System.Collections.Generic;
using Asadito.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Asadito
{
    public sealed class ManagementFoodTarget : MonoBehaviour
    {
        public string FoodId;
        public int UnitId;
        public Freshness Freshness;
        public bool Selected;
        public Vector3 Home;
        public MeshRenderer Visual;
    }

    /// <summary>Owned viewport: real meshes/colliders and fixed camera, no parallel business state.
    /// Pointer events only reach this RawImage when no overlay intercepted them.</summary>
    public sealed class ManagementWorldView : MonoBehaviour, IPointerClickHandler
    {
        const int WorldLayer=30;
        static readonly Dictionary<string,Func<Mesh>> factories=new Dictionary<string,Func<Mesh>>{
            ["chorizo"]=ManagementMeshes.Chorizo,["tira"]=ManagementMeshes.Tira};
        public static void RegisterFoodModel(string foodId,Func<Mesh> factory){if(factory==null)throw new ArgumentNullException(nameof(factory));factories[foodId]=factory;}
        public Camera WorldCamera { get; private set; }
        public RawImage Viewport { get; private set; }
        public bool IsFridge { get; private set; }
        public float DoorOpenFraction { get; private set; }
        public readonly List<ManagementFoodTarget> Targets=new List<ManagementFoodTarget>();
        readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();
        readonly Dictionary<string,Mesh> foods=new Dictionary<string,Mesh>();
        readonly Dictionary<string,Text> tickets=new Dictionary<string,Text>();
        readonly Dictionary<ManagementFoodTarget,Coroutine> motions=new Dictionary<ManagementFoodTarget,Coroutine>();
        Transform world,door;
        Material solid,glass;
        RenderTexture texture;
        Font font;
        Action<ManagementFoodTarget> select;
        Vector3 preparationSpot;
        int page, pageCount=1;
        ManagementService service;
        readonly Color wood=new Color(.42f,.22f,.105f), cream=new Color(.92f,.87f,.70f), metal=new Color(.57f,.65f,.65f), green=new Color(.18f,.33f,.25f);

        public static ManagementWorldView Create(Transform parent,Vector2 min,Vector2 max,Font font,ManagementService service,bool fridge,Action<ManagementFoodTarget> select)
        {
            var go=new GameObject(fridge ? "Heladera 3D" : "Vitrina de vidrio",typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage),typeof(ManagementWorldView));
            var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;
            var view=go.GetComponent<ManagementWorldView>();view.Viewport=go.GetComponent<RawImage>();view.Viewport.raycastTarget=true;
            view.font=font;view.service=service;view.select=select;view.IsFridge=fridge;view.Build();return view;
        }
        void Build()
        {
            world=new GameObject(IsFridge ? "Physical fridge world" : "Physical butcher world").transform;
            world.position=new Vector3(1000,1000,1000);world.gameObject.layer=WorldLayer;
            owned.Add(world.gameObject);
            solid=new Material(Resources.Load<Shader>("ManagementSurface")){name="ASADITO shared painted mobile surface"};owned.Add(solid);
            solid.SetFloat("_Gloss",.17f);
            glass=new Material(Resources.Load<Shader>("ManagementGlass")){name="ASADITO low cost glass"};owned.Add(glass);
            foreach(var pair in factories)
            {
                var mesh=Resources.Load<Mesh>("Management3D/"+pair.Key);
                if(mesh==null){mesh=pair.Value();owned.Add(mesh);}foods[pair.Key]=mesh;
            }
            var cam=new GameObject("Management isolated camera",typeof(Camera));cam.transform.SetParent(world,false);
            WorldCamera=cam.GetComponent<Camera>();WorldCamera.clearFlags=CameraClearFlags.SolidColor;
            WorldCamera.backgroundColor=new Color(.14f,.22f,.19f);WorldCamera.cullingMask=1<<WorldLayer;
            WorldCamera.orthographic=true;WorldCamera.orthographicSize=IsFridge ? 3.95f : 3.35f;
            WorldCamera.nearClipPlane=.1f;WorldCamera.farClipPlane=35;WorldCamera.allowHDR=false;WorldCamera.allowMSAA=false;
            cam.transform.localPosition=IsFridge ? new Vector3(3.0f,8.5f,12) : new Vector3(3.5f,8.8f,10.5f);
            cam.transform.LookAt(world.position+(IsFridge ? new Vector3(-.45f,2.2f,0) : new Vector3(0,.9f,0)));
            // Capped render target, resized for actual safe-area viewport rather than assumed screen ratio.
            Canvas.ForceUpdateCanvases();ResizeTexture();
            if(IsFridge)BuildFridge();else BuildShop();
            SetLayer(world);BatchStaticProps();Physics.SyncTransforms();
        }
        void BatchStaticProps()
        {
            var list=new List<CombineInstance>();
            foreach(var renderer in world.GetComponentsInChildren<MeshRenderer>())
            {
                if(renderer.sharedMaterial!=solid||renderer.GetComponentInParent<ManagementFoodTarget>()!=null||
                    (door!=null&&renderer.transform.IsChildOf(door)))continue;
                list.Add(new CombineInstance{mesh=renderer.GetComponent<MeshFilter>().sharedMesh,
                    transform=world.worldToLocalMatrix*renderer.transform.localToWorldMatrix});renderer.enabled=false;
            }
            if(list.Count==0)return;var mesh=new Mesh{name="Combined fixed shop/fridge props"};mesh.CombineMeshes(list.ToArray(),true,true);owned.Add(mesh);
            Shape("Batched static scenery",mesh,Vector3.zero);
        }
        void ResizeTexture(int captureHeight=0)
        {
            var size=Viewport.rectTransform.rect.size;
            if(size.x<1||size.y<1)return; // Layout can briefly collapse a stretched rect; never create a NaN projection.
            float aspect=size.x/size.y;
            WorldCamera.aspect=aspect;
            WorldCamera.orthographicSize=Mathf.Max(IsFridge ? 3.95f : 3.35f,(IsFridge ? 3.3f : 3.55f)/aspect);
            int h=captureHeight>0 ? captureHeight : Mathf.Min(1200,Mathf.Max(640,Screen.height));int w=Mathf.Clamp(Mathf.RoundToInt(h*aspect),512,1200);
            if(texture!=null&&texture.width==w&&texture.height==h)return;
            if(texture!=null){texture.Release();Destroy(texture);}
            texture=new RenderTexture(w,h,16,RenderTextureFormat.ARGB32){name="Management viewport (capped)",antiAliasing=2};texture.Create();
            WorldCamera.targetTexture=texture;WorldCamera.aspect=aspect;Viewport.texture=texture;
        }
        void OnRectTransformDimensionsChange(){if(WorldCamera!=null&&Viewport!=null)ResizeTexture();}
        void OnDisable(){if(WorldCamera!=null)WorldCamera.enabled=false;if(world!=null)world.gameObject.SetActive(false);}
        void OnDestroy()
        {
            if(WorldCamera!=null)WorldCamera.targetTexture=null;
            if(texture!=null){texture.Release();Destroy(texture);}
            foreach(var item in owned)if(item!=null)Destroy(item);
            owned.Clear();Targets.Clear();
        }
        static void SetLayer(Transform root){root.gameObject.layer=WorldLayer;foreach(Transform child in root)SetLayer(child);}
        GameObject Shape(string name,Mesh mesh,Vector3 position,Transform parent=null,Material material=null)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.layer=WorldLayer;go.transform.SetParent(parent ?? world,false);
            go.transform.localPosition=position;go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=material ?? solid;return go;
        }
        GameObject Box(string name,Vector3 size,Vector3 pos,Color color,float radius=.06f,Transform parent=null,Material material=null)
        {
            var mesh=ManagementMeshes.RoundedBox(size,radius,color);owned.Add(mesh);return Shape(name,mesh,pos,parent,material);
        }
        void Room()
        {
            Box("Terracotta floor",new Vector3(16,.14f,14),new Vector3(0,-.25f,0),new Color(.48f,.28f,.17f));
            Box("Warm tiled wall",new Vector3(14,7,.18f),new Vector3(0,3,-3.2f),cream);
            Box("Sage wall base",new Vector3(14,1.6f,.22f),new Vector3(0,.55f,-3.05f),green);
            for(int i=0;i<9;i++)Box("Tile seam",new Vector3(.025f,4,.035f),new Vector3(-5+i*1.25f,3.5f,-3.04f),new Color(.70f,.67f,.53f),.005f);
            for(int i=0;i<4;i++)Box("Tile grout",new Vector3(14,.022f,.035f),new Vector3(0,2+i*.9f,-3.04f),new Color(.70f,.67f,.53f),.005f);
            Box("Timber wall trim",new Vector3(14,.17f,.23f),new Vector3(0,1.4f,-2.99f),wood);
            Box("Pendant support",new Vector3(.045f,1.2f,.045f),new Vector3(-2,5,-2.2f),metal);
            Box("Warm hanging lamp",new Vector3(1.0f,.25f,.70f),new Vector3(-2,3.9f,-2.2f),new Color(.83f,.59f,.23f),.11f);
        }
        void BuildShop()
        {
            Room();
            Box("Counter timber base",new Vector3(6.2f,1.2f,3.3f),new Vector3(0,.45f,0),wood,.14f);
            for(int i=0;i<8;i++)Box("Wooden front stave",new Vector3(.72f,1.02f,.10f),new Vector3(-2.65f+i*.76f,.47f,1.65f),new Color(.50f+(i%2)*.025f,.28f,.13f));
            Box("Brushed metal counter",new Vector3(6.1f,.16f,3.15f),new Vector3(0,1.12f,0),metal,.07f);
            Box("Front brass lip",new Vector3(6.3f,.10f,.14f),new Vector3(0,1.21f,1.65f),new Color(.84f,.61f,.29f));
            for(int side=-1;side<=1;side+=2)
            {
                Box("Glass side",new Vector3(.035f,1.0f,3.0f),new Vector3(side*3.01f,1.72f,0),Color.white,.008f,null,glass);
                Box("Window rail",new Vector3(.07f,.08f,3.25f),new Vector3(side*3.07f,2.2f,0),metal);
            }
            Box("Glass front",new Vector3(6,.67f,.035f),new Vector3(0,1.55f,1.60f),Color.white,.008f,null,glass);
            Box("Back counter rail",new Vector3(6.3f,.10f,.12f),new Vector3(0,2.2f,-1.57f),metal);
            Box("Scale foot",new Vector3(.74f,.13f,.7f),new Vector3(2.20f,2.05f,-2.0f),metal);
            Box("Scale housing",new Vector3(.42f,.65f,.32f),new Vector3(2.20f,2.40f,-2.0f),cream);
            Box("Scale face",new Vector3(.30f,.30f,.025f),new Vector3(2.20f,2.5f,-1.82f),green);
            Box("Scale bowl",new Vector3(.85f,.13f,.68f),new Vector3(2.2f,2.80f,-2.0f),metal);
            var products=Array.FindAll(service.Config.Products,p=>p.UnlockLevel<=CurrentLevel);
            pageCount=Math.Max(1,(products.Length+1)/2);ShopPage(products);
        }
        public int CurrentLevel { get; set; }=1;
        // Configure level before rebuilding the groups; future catalog additions register a Mesh factory, not a new inventory.
        public void ConfigureShop(int level){CurrentLevel=level;ClearProducts();var products=Array.FindAll(service.Config.Products,p=>p.UnlockLevel<=level);pageCount=Math.Max(1,(products.Length+1)/2);ShopPage(products);}
        void ClearProducts()
        {
            foreach(var t in Targets)if(t!=null){t.gameObject.SetActive(false);Destroy(t.gameObject);}Targets.Clear();
            foreach(var text in tickets.Values)if(text!=null){text.transform.parent.gameObject.SetActive(false);Destroy(text.transform.parent.gameObject);}tickets.Clear();
            var group=world.Find("Display groups");if(group!=null){group.gameObject.SetActive(false);Destroy(group.gameObject);}
        }
        public void ChangePage(){page=(page+1)%pageCount;ClearProducts();ShopPage(Array.FindAll(service.Config.Products,p=>p.UnlockLevel<=CurrentLevel));}
        void ShopPage(ProductEconomy[] products)
        {
            var groups=new GameObject("Display groups").transform;groups.SetParent(world,false);groups.gameObject.layer=WorldLayer;
            for(int local=0;local<2;local++)
            {
                int index=page*2+local;if(index>=products.Length)break;var p=products[index];float x=local==0 ? -1.48f : 1.48f;
                Box("Exhibition tray "+p.FoodId,new Vector3(2.78f,.075f,2.8f),new Vector3(x,1.23f,0),new Color(.77f,.81f,.77f),.035f,groups);
                int count=Math.Min(8,service.Stock(p.FoodId));
                if(!foods.ContainsKey(p.FoodId))continue; // Art not registered never masquerades as another cut.
                for(int i=0;i<count;i++)
                {
                    var pos=new Vector3(x+(i%2==0 ? -.67f : .67f),1.29f,-.97f+i/2*.61f);
                    var item=Food(p.FoodId,0,pos,groups);item.transform.localScale=Vector3.one*.86f;
                    item.transform.localRotation=Quaternion.Euler(0,(i%3-1)*7,0);
                }
                Box("Price sign stand",new Vector3(.045f,1.12f,.06f),new Vector3(x,1.82f,-1.32f),metal,.012f,groups);
                Box("Price sign foot",new Vector3(.28f,.04f,.18f),new Vector3(x,1.29f,-1.32f),metal,.018f,groups);
                var text=Ticket("Precio "+p.FoodId,new Vector3(x,2.72f,-1.32f),p.FoodId);tickets[p.FoodId]=text;
                UpdateTicket(p.FoodId,0);
            }
            SetLayer(groups);Physics.SyncTransforms();
        }
        Text Ticket(string name,Vector3 position,string id)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Canvas));go.transform.SetParent(world,false);go.transform.localPosition=position;
            go.transform.rotation=WorldCamera.transform.rotation;go.transform.localScale=Vector3.one*.0047f;
            go.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;go.GetComponent<RectTransform>().sizeDelta=new Vector2(510,148);
            var bg=new GameObject("Ticket paper",typeof(RectTransform),typeof(Image));bg.transform.SetParent(go.transform,false);var rect=bg.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            bg.GetComponent<Image>().color=new Color(1,.94f,.78f);bg.GetComponent<Image>().raycastTarget=false;
            var label=new GameObject("Price and stock",typeof(RectTransform),typeof(Text));label.transform.SetParent(go.transform,false);var r=label.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(8,0);r.offsetMax=new Vector2(-8,0);
            var text=label.GetComponent<Text>();text.font=font;text.fontSize=34;text.color=new Color(.12f,.22f,.16f);text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;SetLayer(go.transform);return text;
        }
        public void UpdateTicket(string id,int quantity)
        {
            if(!tickets.TryGetValue(id,out var text))return;
            var p=service.Config.Product(id);text.text=FoodCatalog.Get(id).DisplayName.ToUpper()+" · $"+p.Price+"\n"+(service.Stock(id)==0 ? "AGOTADO" : "Stock "+service.Stock(id)+" · llevás "+quantity);
        }
        void BuildFridge()
        {
            Room();int shelves=Math.Max(2,(service.Config.FridgeCapacity+3)/4);
            float height=Mathf.Max(3.95f,1.9f+shelves*1.16f);
            Box("Fridge sage cabinet",new Vector3(3.85f,height,.18f),new Vector3(0,height/2-.1f,-1.17f),green,.16f);
            Box("Cream inner back",new Vector3(3.5f,height-.35f,.13f),new Vector3(0,height/2,-1.05f),new Color(.85f,.90f,.82f));
            foreach(int side in new[]{-1,1})Box("Interior side wall",new Vector3(.16f,height-.36f,1.35f),new Vector3(side*1.72f,height/2,-.37f),cream);
            Box("Fridge bottom liner",new Vector3(3.65f,.20f,1.45f),new Vector3(0,.48f,-.30f),cream,.05f);
            Box("Fridge top arch",new Vector3(3.85f,.24f,1.7f),new Vector3(0,height-.07f,-.36f),cream,.10f);
            for(int shelf=0;shelf<shelves;shelf++)
            {
                float y=.85f+shelf*1.16f;
                Box("Shelf "+shelf,new Vector3(3.40f,.08f,1.35f),new Vector3(0,y,-.30f),new Color(.70f,.79f,.77f),.035f);
                Box("Shelf front rail",new Vector3(3.50f,.09f,.10f),new Vector3(0,y,.40f),cream);
            }
            var hinge=new GameObject("Animated refrigerator hinge").transform;hinge.SetParent(world,false);hinge.localPosition=new Vector3(-1.90f,0,.52f);door=hinge;
            Box("Rounded fridge door",new Vector3(3.80f,height-.1f,.22f),new Vector3(1.90f,height/2,.09f),new Color(.32f,.50f,.37f),.10f,door);
            Box("Ivory door inset",new Vector3(3.30f,height-.55f,.065f),new Vector3(1.90f,height/2,-.04f),cream,.025f,door);
            Box("Brass door handle",new Vector3(.11f,1.1f,.18f),new Vector3(3.5f,height*.58f,.30f),new Color(.86f,.64f,.32f),.04f,door);
            Box("Preparation tray",new Vector3(3.6f,.14f,1.6f),new Vector3(.6f,.22f,1.58f),wood,.06f);
            preparationSpot=new Vector3(.6f,.32f,1.58f);
            int index=0;
            foreach(var unit in service.State.Inventory)
            {
                int shelf=index/4,col=index%2,row=index/2%2;
                var pos=new Vector3(col==0 ? -.85f : .85f,.91f+shelf*1.16f,row==0 ? -.63f : .04f);
                var item=Food(unit.FoodId,unit.Id,pos);item.transform.localScale=Vector3.one*.86f;
                item.Freshness=unit.FreshnessAt(service.State.FreshnessCycle,service.Config.Product(unit.FoodId).FreshCycles);
                if(item.Freshness==Freshness.Spoiled)Tint(item,new Color(.55f,.60f,.48f));index++;
            }
            SetLayer(door);StartCoroutine(OpenDoor());
        }
        IEnumerator OpenDoor()
        {
            float t=0;while(t<.55f&&door!=null){t+=Time.unscaledDeltaTime;DoorOpenFraction=Mathf.Clamp01(t/.55f);door.localRotation=Quaternion.Euler(0,-110*Mathf.SmoothStep(0,1,DoorOpenFraction),0);yield return null;}
            Physics.SyncTransforms();
        }
        ManagementFoodTarget Food(string id,int unitId,Vector3 position,Transform holder=null)
        {
            if(!foods.TryGetValue(id,out var mesh))throw new InvalidOperationException("No 3D model registered for "+id);
            var go=Shape(unitId==0 ? "Comprar "+id+" modelo "+Targets.Count : "Inventory unit "+unitId,mesh,position,holder);
            var t=go.AddComponent<ManagementFoodTarget>();t.FoodId=id;t.UnitId=unitId;t.Home=position;t.Visual=go.GetComponent<MeshRenderer>();
            var collider=go.AddComponent<BoxCollider>();collider.center=mesh.bounds.center;collider.size=mesh.bounds.size+new Vector3(.02f,.05f,.03f);
            if(!foods.TryGetValue("shadow",out var shadow)){shadow=ManagementMeshes.Disk(.63f,.29f,new Color(.31f,.32f,.29f));foods["shadow"]=shadow;owned.Add(shadow);}
            Shape("Contact shadow",shadow,new Vector3(0,.002f,0),go.transform);
            Targets.Add(t);return t;
        }
        public Vector2 ScreenPoint(ManagementFoodTarget target,Camera eventCamera=null)
        {
            var uv=WorldCamera.WorldToViewportPoint(target.transform.TransformPoint(target.GetComponent<MeshFilter>().sharedMesh.bounds.center));
            var r=Viewport.rectTransform;var local=new Vector3(r.rect.xMin+uv.x*r.rect.width,r.rect.yMin+uv.y*r.rect.height,0);
            return RectTransformUtility.WorldToScreenPoint(eventCamera,r.TransformPoint(local));
        }
        public ManagementFoodTarget Raycast(Vector2 point,Camera eventCamera=null)
        {
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport.rectTransform,point,eventCamera,out var local))return null;
            var r=Viewport.rectTransform.rect;var uv=new Vector2((local.x-r.xMin)/r.width,(local.y-r.yMin)/r.height);
            if(uv.x<0||uv.x>1||uv.y<0||uv.y>1)return null;
            Physics.SyncTransforms();var ray=WorldCamera.ViewportPointToRay(uv);
            if(Physics.Raycast(ray,out var hit,35,1<<WorldLayer))return hit.collider.GetComponent<ManagementFoodTarget>();
            // 44dp-equivalent hit envelope for slim foods, only inside this non-blocked viewport.
            ManagementFoodTarget nearest=null;float distance=Screen.width/360f*22;
            foreach(var t in Targets)
            {
                if(t==null||!t.gameObject.activeInHierarchy)continue;
                float d=Vector2.Distance(point,ScreenPoint(t,eventCamera));if(d<distance){distance=d;nearest=t;}
            }
            return nearest;
        }
        public void OnPointerClick(PointerEventData data)
        {
            if(IsFridge&&DoorOpenFraction<.9f)return;
            var target=Raycast(data.position,data.pressEventCamera);if(target!=null)select?.Invoke(target);
        }
        static void Tint(ManagementFoodTarget t,Color color)
        {
            var block=new MaterialPropertyBlock();block.SetColor("_Tint",color);t.Visual.SetPropertyBlock(block);
        }
        public void Feedback(ManagementFoodTarget target)
        {
            if(motions.TryGetValue(target,out var old)&&old!=null)StopCoroutine(old);
            motions[target]=StartCoroutine(Pulse(target));
            var mesh=target.GetComponent<MeshFilter>().sharedMesh;
            var ghost=Shape("Carne al carrito 3D",mesh,target.transform.localPosition);
            ghost.transform.position=target.transform.position;ghost.transform.localScale=target.transform.localScale;ghost.transform.rotation=target.transform.rotation;
            StartCoroutine(Flight(ghost));
        }
        IEnumerator Pulse(ManagementFoodTarget target)
        {
            Tint(target,new Color(1.25f,1.1f,.80f));float t=0;
            while(target!=null&&t<.22f){t+=Time.unscaledDeltaTime;target.transform.localScale=Vector3.one*.86f*(1+.10f*Mathf.Sin(t/.22f*Mathf.PI));yield return null;}
            if(target!=null){target.transform.localScale=Vector3.one*.86f;Tint(target,Color.white);motions.Remove(target);}
        }
        IEnumerator Flight(GameObject ghost)
        {
            Vector3 start=ghost.transform.localPosition,end=new Vector3(0,.9f,2.9f);float t=0;
            while(ghost!=null&&t<.32f){t+=Time.unscaledDeltaTime;float f=Mathf.Clamp01(t/.32f);ghost.transform.localPosition=Vector3.Lerp(start,end,1-Mathf.Pow(1-f,3))+Vector3.up*Mathf.Sin(f*Mathf.PI)*.6f;ghost.transform.localScale=Vector3.one*.86f*(1-f*.9f);yield return null;}
            if(ghost!=null)Destroy(ghost);
        }
        public void MoveSelection(ManagementFoodTarget target,bool chosen,int slot)
        {
            target.Selected=chosen;if(motions.TryGetValue(target,out var old)&&old!=null)StopCoroutine(old);
            var to=chosen ? preparationSpot+new Vector3((slot%4-1.5f)*.81f,.04f,-.31f+(slot/4)*.63f) : target.Home;
            motions[target]=StartCoroutine(MoveUnit(target,to,chosen));
        }
        IEnumerator MoveUnit(ManagementFoodTarget target,Vector3 to,bool chosen)
        {
            Tint(target,chosen ? new Color(1.12f,1.12f,.72f) : Color.white);var start=target.transform.localPosition;var scale=target.transform.localScale;float endScale=chosen ? .55f : .86f;float t=0;
            while(target!=null&&t<.25f){t+=Time.unscaledDeltaTime;float f=Mathf.Clamp01(t/.25f);target.transform.localPosition=Vector3.Lerp(start,to,Mathf.SmoothStep(0,1,f))+Vector3.up*Mathf.Sin(f*Mathf.PI)*.3f;target.transform.localScale=Vector3.Lerp(scale,Vector3.one*endScale,f);yield return null;}
            if(target!=null){target.transform.localPosition=to;target.transform.localScale=Vector3.one*endScale;motions.Remove(target);}Physics.SyncTransforms();
        }
        public void CaptureRender(){if(WorldCamera!=null){ResizeTexture(1200);WorldCamera.Render();}}
    }
}

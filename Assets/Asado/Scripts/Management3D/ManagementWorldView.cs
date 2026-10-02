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
        public int SelectionSlot=-1;
        public Vector3 Home;
        public Vector2 DisplayUV;
        public float DisplayHeight;
        public Vector3 HomeScale=Vector3.one*.86f;
        public MeshRenderer Visual;
    }

    /// <summary>Owned viewport: real meshes/colliders and fixed camera, no parallel business state.
    /// Pointer events only reach this RawImage when no overlay intercepted them.</summary>
    public sealed class ManagementWorldView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        const int WorldLayer=30;
        public const int ShopDisplayLimit=24;
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
        sealed class PriceSign
        {
            public RectTransform Root;
            public Text Product, Price, Stock;
        }
        readonly Dictionary<string,PriceSign> tickets=new Dictionary<string,PriceSign>();
        readonly Dictionary<string,Vector2> ticketPositions=new Dictionary<string,Vector2>();
        readonly Dictionary<ManagementFoodTarget,Coroutine> motions=new Dictionary<ManagementFoodTarget,Coroutine>();
        Transform world;
        Material solid;
        readonly Dictionary<string,Material> foodMaterials=new Dictionary<string,Material>();
        Func<string,Sprite> foodSprite;
        Func<string,Vector2> foodSize;
        public RectTransform CartDropZone { get; set; }
        public bool IsDragging => dragTarget!=null;
        ManagementFoodTarget dragTarget;
        Image dragPreview;
        int dragPointer;
        bool suppressClick;
        Vector3 dragHomeScale;
        RenderTexture texture;
        Font font;
        Action<ManagementFoodTarget> select;
        int page, pageCount=1;
        ManagementService service;
        readonly Color metal=new Color(.57f,.65f,.65f);

        public static ManagementWorldView Create(Transform parent,Vector2 min,Vector2 max,Font font,ManagementService service,bool fridge,Action<ManagementFoodTarget> select,Func<string,Sprite> foodSprite=null,Func<string,Vector2> foodSize=null)
        {
            var go=new GameObject(fridge ? "Heladera 3D" : "Vitrina de vidrio",typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage),typeof(ManagementWorldView));
            var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;
            var view=go.GetComponent<ManagementWorldView>();view.Viewport=go.GetComponent<RawImage>();view.Viewport.raycastTarget=true;
            view.foodSprite=foodSprite;view.foodSize=foodSize;view.font=font;view.service=service;view.select=select;view.IsFridge=fridge;view.Build();return view;
        }
        void Build()
        {
            world=new GameObject(IsFridge ? "Physical fridge world" : "Physical butcher world").transform;
            world.position=new Vector3(1000,1000,1000);world.gameObject.layer=WorldLayer;
            owned.Add(world.gameObject);
            solid=new Material(Resources.Load<Shader>("ManagementSurface")){name="ASADITO shared painted mobile surface"};owned.Add(solid);
            solid.SetFloat("_Gloss",.17f);
            foreach(var pair in factories)
            {
                var mesh=Resources.Load<Mesh>("Management3D/"+pair.Key+"-grill-relief") ?? Resources.Load<Mesh>("Management3D/"+pair.Key);
                if(mesh!=null&&mesh.name.Contains("volumetric raw")&&foodSprite!=null)
                {
                    var sprite=foodSprite(pair.Key);
                    if(sprite!=null){var mat=new Material(Resources.Load<Shader>("ManagementFoodTexture")){name="Original grill texture · "+pair.Key};mat.mainTexture=sprite.texture;foodMaterials[pair.Key]=mat;owned.Add(mat);}
                }
                if(mesh==null){mesh=pair.Value();owned.Add(mesh);}foods[pair.Key]=mesh;
            }
            var cam=new GameObject("Management isolated camera",typeof(Camera));cam.transform.SetParent(world,false);
            WorldCamera=cam.GetComponent<Camera>();WorldCamera.clearFlags=CameraClearFlags.SolidColor;
            WorldCamera.backgroundColor=Color.clear;WorldCamera.cullingMask=1<<WorldLayer;
            WorldCamera.orthographic=false;WorldCamera.fieldOfView=60;
            WorldCamera.nearClipPlane=.1f;WorldCamera.farClipPlane=35;WorldCamera.allowHDR=false;WorldCamera.allowMSAA=false;
            cam.transform.localPosition=new Vector3(0,9,-5.2f);cam.transform.LookAt(world.position);
            if(!IsFridge)cam.transform.localRotation*=Quaternion.AngleAxis(5,Vector3.forward);
            // Capped render target, resized for actual safe-area viewport rather than assumed screen ratio.
            Canvas.ForceUpdateCanvases();ResizeTexture();
            if(IsFridge)BuildFridge();else BuildShop();
            SetLayer(world);Physics.SyncTransforms();
        }
        void ResizeTexture(int captureHeight=0)
        {
            var size=Viewport.rectTransform.rect.size;
            if(size.x<1||size.y<1)return; // Layout can briefly collapse a stretched rect; never create a NaN projection.
            float aspect=size.x/size.y;
            WorldCamera.aspect=aspect;
            if(world!=null){if(IsFridge)LayoutFridge();else LayoutShop();}
            int h=captureHeight>0 ? captureHeight : Mathf.Min(1200,Mathf.Max(640,Screen.height));int w=Mathf.Clamp(Mathf.RoundToInt(h*aspect),512,1200);
            if(texture!=null&&texture.width==w&&texture.height==h)return;
            if(texture!=null){texture.Release();Destroy(texture);}
            texture=new RenderTexture(w,h,16,RenderTextureFormat.ARGB32){name="Management viewport (capped)",antiAliasing=1};texture.Create();
            WorldCamera.targetTexture=texture;WorldCamera.aspect=aspect;Viewport.texture=texture;
        }
        void OnRectTransformDimensionsChange(){if(WorldCamera!=null&&Viewport!=null)ResizeTexture();}
        void OnDisable(){CancelDrag();if(WorldCamera!=null)WorldCamera.enabled=false;if(world!=null)world.gameObject.SetActive(false);}
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
        void BuildShop()
        {
            // The illustrated empty counter owns the environment. Only saleable food is rendered here.
            var products=Array.FindAll(service.Config.Products,p=>p.UnlockLevel<=CurrentLevel);
            pageCount=Math.Max(1,(products.Length+1)/2);ShopPage(products);
        }
        Vector3 ShopPoint(float u,float v,float height=0)
        {
            // Counter base plane; a second supported layer may rest on the cuts below.
            var ray=WorldCamera.ViewportPointToRay(new Vector3(u,v,0));
            var plane=new Plane(world.up,world.TransformPoint(Vector3.up*height));
            return plane.Raycast(ray,out float distance) ? world.InverseTransformPoint(ray.GetPoint(distance)) : Vector3.zero;
        }
        Vector3 TicketPoint(Vector2 uv)=>world.InverseTransformPoint(WorldCamera.ViewportToWorldPoint(new Vector3(uv.x,uv.y,8)));
        void LayoutShop()
        {
            var center=world.TransformPoint(ShopPoint(.5f,.46f));
            var scales=new Dictionary<string,float>();
            foreach(var target in Targets)if(target!=null&&target.UnitId==0)
            {
                if(!scales.TryGetValue(target.FoodId,out float scale))
                {
                    float width=foods[target.FoodId].bounds.size.x;
                    float referenceWidth=Mathf.Abs(WorldCamera.WorldToViewportPoint(center+world.right*width*.5f).x-
                        WorldCamera.WorldToViewportPoint(center-world.right*width*.5f).x);
                    // Same UI-reference footprint as the grill. Perspective supplies natural depth,
                    // but stock count and screen aspect must never miniaturize the food.
                    Vector2 size=foodSize!=null ? foodSize(target.FoodId) : new Vector2(220,100);
                    float desiredWidth=size.x/Viewport.rectTransform.rect.width;
                    scale=desiredWidth/Mathf.Max(.001f,referenceWidth);
                    // Account for the raised mesh, tray angle and camera roll, not just a flat X line.
                    // Calibrate at the middle depth; back/front cuts retain natural perspective.
                    for(int pass=0;pass<4;pass++)
                        scale*=desiredWidth/Mathf.Max(.001f,ProjectedShopWidth(foods[target.FoodId],world.InverseTransformPoint(center),scale));
                    scales[target.FoodId]=scale;
                }
                target.HomeScale=Vector3.one*scale;
                target.Home=ShopPoint(target.DisplayUV.x,target.DisplayUV.y)+Vector3.up*(target.DisplayHeight*scale);
                target.transform.localPosition=target.Home;target.transform.localScale=target.HomeScale;
                KeepFoodInsideTray(target);
            }
            foreach(var pair in ticketPositions)if(tickets.TryGetValue(pair.Key,out var sign)&&sign.Root!=null)
            {
                sign.Root.localPosition=TicketPoint(pair.Value);
                sign.Root.localScale=Vector3.one*(244f/Viewport.rectTransform.rect.width*
                    16*Mathf.Tan(WorldCamera.fieldOfView*Mathf.Deg2Rad*.5f)*WorldCamera.aspect/360);
            }
            Physics.SyncTransforms();
        }
        float ProjectedShopWidth(Mesh mesh,Vector3 position,float scale)=>ShopProjectedBounds(mesh,
            world.localToWorldMatrix*Matrix4x4.TRS(position,Quaternion.identity,Vector3.one*scale)).width;
        Rect ShopProjectedBounds(Mesh mesh,Matrix4x4 matrix)
        {
            var bounds=mesh.bounds;Vector2 min=Vector2.one*float.MaxValue,max=Vector2.one*float.MinValue;
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
            {
                var corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z));
                var uv=WorldCamera.WorldToViewportPoint(matrix.MultiplyPoint3x4(corner));
                min=Vector2.Min(min,new Vector2(uv.x,uv.y));max=Vector2.Max(max,new Vector2(uv.x,uv.y));
            }
            return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
        }
        void KeepFoodInsideTray(ManagementFoodTarget target)
        {
            // Move an edge cut inward on short/narrow safe areas. Never shrink it to fit.
            for(int pass=0;pass<4;pass++)
            {
                var bounds=ShopProjectedBounds(foods[target.FoodId],target.transform.localToWorldMatrix);
                float dx=bounds.xMin<.035f ? .035f-bounds.xMin : bounds.xMax>.965f ? .965f-bounds.xMax : 0;
                float dy=bounds.yMin<.26f ? .26f-bounds.yMin : bounds.yMax>.65f ? .65f-bounds.yMax : 0;
                if(Mathf.Abs(dx)+Mathf.Abs(dy)<.0001f)break;
                var uv=WorldCamera.WorldToViewportPoint(target.transform.position);
                target.Home=ShopPoint(uv.x+dx,uv.y+dy,target.Home.y);
                target.transform.localPosition=target.Home;
            }
        }
        public int CurrentLevel { get; set; }=1;
        // Configure level before rebuilding the groups; future catalog additions register a Mesh factory, not a new inventory.
        public void ConfigureShop(int level){CurrentLevel=level;ClearProducts();var products=Array.FindAll(service.Config.Products,p=>p.UnlockLevel<=level);pageCount=Math.Max(1,(products.Length+1)/2);ShopPage(products);}
        void ClearProducts()
        {
            foreach(var t in Targets)if(t!=null){t.gameObject.SetActive(false);Destroy(t.gameObject);}Targets.Clear();
            foreach(var sign in tickets.Values)if(sign.Root!=null){sign.Root.gameObject.SetActive(false);Destroy(sign.Root.gameObject);}tickets.Clear();ticketPositions.Clear();
            var group=world.Find("Display groups");if(group!=null){group.gameObject.SetActive(false);Destroy(group.gameObject);}
        }
        public void ChangePage(){page=(page+1)%pageCount;ClearProducts();ShopPage(Array.FindAll(service.Config.Products,p=>p.UnlockLevel<=CurrentLevel));}
        void ShopPage(ProductEconomy[] products)
        {
            var groups=new GameObject("Display groups").transform;groups.SetParent(world,false);groups.gameObject.layer=WorldLayer;
            for(int local=0;local<2;local++)
            {
                int index=page*2+local;if(index>=products.Length)break;var p=products[index];
                int count=Math.Min(ShopDisplayLimit,service.Stock(p.FoodId));
                if(!foods.ContainsKey(p.FoodId))continue;
                for(int i=0;i<count;i++)
                {
                    // Two columns / six depth rows, plus a supported second layer.
                    // Abundance comes from full-size overlapping cuts, not shrunken tiles.
                    int layer=i/12,row=i%12/2,col=i%2;
                    float u=(local==0?.205f:.615f)+col*(local==0?.168f:.197f)-row*.004f+
                        (row%2)*.010f+layer*(col==0?-.018f:.018f);
                    float v=.588f-row*.052f-(u-.5f)*(.055f+row*.009f)-layer*.013f;
                    var item=Food(p.FoodId,0,ShopPoint(u,v),groups);item.DisplayUV=new Vector2(u,v);
                    item.DisplayHeight=layer*.20f;
                    item.transform.localRotation=Quaternion.Euler(0,(i%3-1)*4+(row%2)*2,0);
                }
                Vector2 ticketUV=new Vector2(local==0?.28f:.72f,.745f);
                tickets[p.FoodId]=Ticket("Precio "+p.FoodId,TicketPoint(ticketUV));
                ticketPositions[p.FoodId]=ticketUV;
                UpdateTicket(p.FoodId,0);
            }
            SetLayer(groups);LayoutShop();
        }
        PriceSign Ticket(string name,Vector3 position)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(BoxCollider));
            go.transform.SetParent(world,false);go.transform.localPosition=position;
            go.transform.rotation=WorldCamera.transform.rotation;
            go.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            var root=go.GetComponent<RectTransform>();root.sizeDelta=new Vector2(360,280);
            // The price card owns this space: reading/tapping a sign must not buy hidden meat.
            go.GetComponent<BoxCollider>().size=new Vector3(360,280,1);
            SignPanel(root,"Metal stand shadow",new Vector2(7,-182),new Vector2(15,108),new Color(.08f,.14f,.12f,.35f));
            SignPanel(root,"Metal stem",new Vector2(0,-182),new Vector2(12,108),metal);
            SignPanel(root,"Stem highlight",new Vector2(-3,-182),new Vector2(3,108),new Color(.91f,.95f,.92f));
            SignPanel(root,"Metal foot",new Vector2(0,-235),new Vector2(64,12),new Color(.35f,.44f,.43f));
            SignPanel(root,"Foot highlight",new Vector2(0,-231),new Vector2(58,4),metal);
            SignPanel(root,"Card shadow",new Vector2(5,-5),new Vector2(360,280),new Color(.05f,.10f,.08f,.32f));
            SignPanel(root,"Dark green frame",Vector2.zero,new Vector2(360,280),new Color(.10f,.24f,.18f));
            SignPanel(root,"White price card",Vector2.zero,new Vector2(350,270),new Color(.99f,.98f,.93f));
            SignPanel(root,"Product band",new Vector2(0,103),new Vector2(330,48),new Color(.10f,.25f,.18f));
            var sign=new PriceSign{Root=root};
            sign.Product=SignText(root,"Product name",new Vector2(0,103),new Vector2(322,46),32,Color.white);
            sign.Price=SignText(root,"Large red price",new Vector2(0,20),new Vector2(328,136),110,new Color(.70f,.10f,.075f));
            sign.Price.fontStyle=FontStyle.Bold;
            var outline=sign.Price.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.12f,.055f,.025f);outline.effectDistance=new Vector2(1.3f,-1.3f);
            SignText(root,"Price unit",new Vector2(0,-67),new Vector2(324,32),24,new Color(.12f,.22f,.16f)).text="POR PIEZA";
            sign.Stock=SignText(root,"Stock and cart",new Vector2(0,-108),new Vector2(326,36),26,new Color(.12f,.22f,.16f));
            SignPanel(root,"Metal card clip",new Vector2(0,-137),new Vector2(48,15),new Color(.45f,.53f,.51f));
            SignPanel(root,"Clip highlight",new Vector2(0,-132),new Vector2(43,4),new Color(.86f,.91f,.88f));
            SetLayer(root);return sign;
        }
        static RectTransform SignRect(Transform parent,GameObject go,Vector2 position,Vector2 size)
        {
            var rect=go.GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchoredPosition=position;rect.sizeDelta=size;return rect;
        }
        static void SignPanel(Transform parent,string name,Vector2 position,Vector2 size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));SignRect(parent,go,position,size);
            var image=go.GetComponent<Image>();image.color=color;image.raycastTarget=false;
        }
        Text SignText(Transform parent,string name,Vector2 position,Vector2 size,int fontSize,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));
            size.y=Mathf.Max(size.y,fontSize*2.5f);SignRect(parent,go,position,size);
            var text=go.GetComponent<Text>();text.font=font;text.fontSize=fontSize;text.color=color;
            text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;text.verticalOverflow=VerticalWrapMode.Overflow;return text;
        }
        public void UpdateTicket(string id,int quantity)
        {
            if(!tickets.TryGetValue(id,out var sign))return;
            sign.Product.text=FoodCatalog.Get(id).DisplayName.ToUpper();
            sign.Price.text="$"+service.Config.Product(id).Price;
            sign.Stock.text=service.Stock(id)==0 ? "AGOTADO" : "Stock "+service.Stock(id)+" · llevás "+quantity;
        }
        void BuildFridge()
        {
            // Empty illustrated cabinet is already open; only real inventory food exists in 3D.
            DoorOpenFraction=1;
            int index=0;
            foreach(var unit in service.State.Inventory)
            {
                int shelf=index/3,col=index%3;
                var item=Food(unit.FoodId,unit.Id,Vector3.zero);
                item.DisplayUV=new Vector2(.365f+col*.215f,.723f-shelf*.166f);
                item.DisplayHeight=index/9*.20f;
                item.transform.localRotation=Quaternion.Euler(0,(col-1)*2,0);
                item.Freshness=unit.FreshnessAt(service.State.FreshnessCycle,service.Config.Product(unit.FoodId).FreshCycles);
                Tint(item,RestingTint(item));index++;
            }
            LayoutFridge();
        }
        static Color RestingTint(ManagementFoodTarget item)=>item.Freshness==Freshness.Spoiled ? new Color(.55f,.60f,.48f) : Color.white;
        Vector2 PrepUV(int slot)=>new Vector2(.32f+(slot%2)*.32f,.176f-(slot/2%2)*.061f);
        void FridgePose(ManagementFoodTarget target,Vector2 uv,float layer,out Vector3 position,out Vector3 scale)
        {
            var mesh=foods[target.FoodId];Vector2 size=foodSize!=null?foodSize(target.FoodId):new Vector2(220,100);
            float desired=size.x/Viewport.rectTransform.rect.width;
            float factor=1;position=ShopPoint(uv.x,uv.y);
            for(int pass=0;pass<6;pass++)
            {
                position=ShopPoint(uv.x,uv.y,layer*factor);
                float width=ShopProjectedBounds(mesh,world.localToWorldMatrix*Matrix4x4.TRS(position,target.transform.localRotation,Vector3.one*factor)).width;
                factor*=desired/Mathf.Max(.001f,width);
            }
            scale=Vector3.one*factor;position=ShopPoint(uv.x,uv.y,layer*factor);
        }
        void LayoutFridge()
        {
            foreach(var target in Targets)if(target!=null)
            {
                if(motions.TryGetValue(target,out var motion)&&motion!=null){StopCoroutine(motion);motions.Remove(target);}
                FridgePose(target,target.DisplayUV,target.DisplayHeight,out target.Home,out target.HomeScale);
                if(target.Selected)
                {
                    FridgePose(target,PrepUV(target.SelectionSlot),target.SelectionSlot/4*.20f,out var pos,out var scale);
                    target.transform.localPosition=pos;target.transform.localScale=scale;
                }
                else {target.transform.localPosition=target.Home;target.transform.localScale=target.HomeScale;}
            }
            Physics.SyncTransforms();
        }
        ManagementFoodTarget Food(string id,int unitId,Vector3 position,Transform holder=null)
        {
            if(!foods.TryGetValue(id,out var mesh))throw new InvalidOperationException("No 3D model registered for "+id);
            var go=Shape(unitId==0 ? "Comprar "+id+" modelo "+Targets.Count : "Inventory unit "+unitId,mesh,position,holder,foodMaterials.TryGetValue(id,out var mat)?mat:null);
            var t=go.AddComponent<ManagementFoodTarget>();t.FoodId=id;t.UnitId=unitId;t.Home=position;t.Visual=go.GetComponent<MeshRenderer>();
            go.AddComponent<MeshCollider>().sharedMesh=mesh; // Silhouette collisions also respect overlapped inventory units.
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
            if(suppressClick||IsDragging||(IsFridge&&DoorOpenFraction<.9f))return;
            var target=Raycast(data.position,data.pressEventCamera);if(target!=null)select?.Invoke(target);
        }
        static void Tint(ManagementFoodTarget t,Color color)
        {
            var block=new MaterialPropertyBlock();block.SetColor("_Tint",color);t.Visual.SetPropertyBlock(block);
        }
        public void Feedback(ManagementFoodTarget target)
        {
            // No shrinking world-space flight copies: only one bounded pulse on the original mesh.
            if(motions.TryGetValue(target,out var old)&&old!=null)StopCoroutine(old);
            motions[target]=StartCoroutine(Pulse(target));
        }
        IEnumerator Pulse(ManagementFoodTarget target)
        {
            Tint(target,new Color(1.08f,1.04f,.9f));float t=0;
            while(target!=null&&t<.18f){t+=Time.unscaledDeltaTime;target.transform.localScale=target.HomeScale*(1+.06f*Mathf.Sin(t/.18f*Mathf.PI));yield return null;}
            if(target!=null){target.transform.localScale=target.HomeScale;Tint(target,Color.white);motions.Remove(target);}
        }
        public void OnPointerDown(PointerEventData data){if(!IsDragging)suppressClick=false;}
        public void OnBeginDrag(PointerEventData data)
        {
            if(IsFridge||IsDragging||CartDropZone==null)return;
            var target=Raycast(data.pressPosition,data.pressEventCamera);if(target==null)return;
            if(motions.TryGetValue(target,out var motion)&&motion!=null){StopCoroutine(motion);motions.Remove(target);}
            target.transform.localScale=target.HomeScale;
            dragTarget=target;dragPointer=data.pointerId;suppressClick=true;dragHomeScale=target.HomeScale;
            Tint(target,new Color(1.10f,1.06f,.9f));
            var go=new GameObject("Dragged food preview",typeof(RectTransform),typeof(Image));go.transform.SetParent(Viewport.transform.parent,false);
            dragPreview=go.GetComponent<Image>();dragPreview.sprite=foodSprite?.Invoke(target.FoodId);dragPreview.preserveAspect=true;dragPreview.raycastTarget=false;
            var rect=dragPreview.rectTransform;rect.sizeDelta=foodSize!=null?foodSize(target.FoodId):new Vector2(240,130);rect.SetAsLastSibling();OnDrag(data);
        }
        public void OnDrag(PointerEventData data)
        {
            if(dragTarget==null||data.pointerId!=dragPointer)return;
            var parent=(RectTransform)dragPreview.transform.parent;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,data.position,data.pressEventCamera,out var local))dragPreview.rectTransform.localPosition=new Vector3(local.x,local.y+35,0);
            bool inside=RectTransformUtility.RectangleContainsScreenPoint(CartDropZone,data.position,data.pressEventCamera);
            var image=CartDropZone.GetComponent<Image>();if(image!=null)image.color=inside?new Color32(255,225,154,255):new Color32(255,244,219,255);
        }
        public void OnEndDrag(PointerEventData data)
        {
            if(dragTarget==null||data.pointerId!=dragPointer)return;
            var target=dragTarget;bool inside=RectTransformUtility.RectangleContainsScreenPoint(CartDropZone,data.position,data.pressEventCamera);
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            // Buttons/details own their taps; dropping on a button must never add a hidden unit.
            if(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()!=null)inside=false;
            CancelDrag();if(inside)select?.Invoke(target);
        }
        void CancelDrag()
        {
            if(dragTarget!=null){Tint(dragTarget,Color.white);dragTarget.transform.localScale=dragHomeScale;}
            dragTarget=null;if(dragPreview!=null)Destroy(dragPreview.gameObject);dragPreview=null;
            if(CartDropZone!=null){var image=CartDropZone.GetComponent<Image>();if(image!=null)image.color=new Color32(255,244,219,255);}
        }
        public void MoveSelection(ManagementFoodTarget target,bool chosen,int slot)
        {
            target.Selected=chosen;target.SelectionSlot=chosen?slot:-1;
            if(motions.TryGetValue(target,out var old)&&old!=null)StopCoroutine(old);
            var to=target.Home;var scale=target.HomeScale;
            if(chosen)FridgePose(target,PrepUV(slot),slot/4*.20f,out to,out scale);
            motions[target]=StartCoroutine(MoveUnit(target,to,scale,chosen));
        }
        IEnumerator MoveUnit(ManagementFoodTarget target,Vector3 to,Vector3 endScale,bool chosen)
        {
            Tint(target,chosen ? new Color(1.10f,1.08f,.86f) : RestingTint(target));
            var start=target.transform.localPosition;var scale=target.transform.localScale;float t=0;
            while(target!=null&&t<.25f)
            {
                t+=Time.unscaledDeltaTime;float f=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.25f));
                target.transform.localPosition=Vector3.Lerp(start,to,f);target.transform.localScale=Vector3.Lerp(scale,endScale,f);
                yield return null;
            }
            if(target!=null){target.transform.localPosition=to;target.transform.localScale=endScale;motions.Remove(target);}
            Physics.SyncTransforms();
        }
        public void CaptureRender(){if(WorldCamera!=null){ResizeTexture(1200);WorldCamera.Render();}}
    }
}

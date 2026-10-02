using System.Collections.Generic;
using Asadito.Runtime;
using UnityEngine;
namespace Asadito
{
    /// <summary>Native closed relief, not a quad: silhouette, raised surface, side walls and underside.
    /// Baked from the existing raw grill atlas without modifying it or requiring CPU-readable runtime textures.</summary>
    public static class ManagementFoodRelief
    {
        public static Mesh Bake(Texture2D atlas,FoodDefinition food)
        {
            var crop=food.SpriteCrop;float x0=crop.XMin,x1=crop.XMax;
            float y0=(6-crop.YMax)/6f,y1=(6-crop.YMin)/6f;
            const int nx=36;float aspect=(x1-x0)*atlas.width/((y1-y0)*atlas.height);
            int nz=Mathf.Clamp(Mathf.RoundToInt(nx/aspect),12,24);
            var filled=new bool[nx,nz];var depth=new int[nx,nz];
            for(int z=0;z<nz;z++)for(int x=0;x<nx;x++)
            {filled[x,z]=atlas.GetPixelBilinear(Mathf.Lerp(x0,x1,(x+.5f)/nx),Mathf.Lerp(y0,y1,(z+.5f)/nz)).a>.25f;depth[x,z]=filled[x,z]?8:0;}
            for(int pass=0;pass<8;pass++)for(int z=0;z<nz;z++)for(int x=0;x<nx;x++)if(filled[x,z])
            {int d=depth[x,z];foreach(var step in new[]{new Vector2Int(-1,0),new Vector2Int(1,0),new Vector2Int(0,-1),new Vector2Int(0,1)})
                {int xx=x+step.x,zz=z+step.y;d=Mathf.Min(d,xx<0||xx>=nx||zz<0||zz>=nz?1:depth[xx,zz]+1);}depth[x,z]=d;}
            float width=food.Id=="tira"?1.50f:1.30f, length=width/aspect;
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();var colors=new List<Color>();
            Vector3 Point(int x,int z,bool top){float d=0;int count=0;for(int dz=-1;dz<=0;dz++)for(int dx=-1;dx<=0;dx++)
                {int xx=x+dx,zz=z+dz;if(xx>=0&&xx<nx&&zz>=0&&zz<nz){d+=depth[xx,zz];count++;}}
                return new Vector3((x/(float)nx-.5f)*width,top?.055f+.19f*Mathf.Clamp01(d/Mathf.Max(1,count)/4f):0,(z/(float)nz-.5f)*length);}
            Vector2 UV(int x,int z)=>new Vector2(Mathf.Lerp(x0,x1,x/(float)nx),Mathf.Lerp(y0,y1,z/(float)nz));
            void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector2 ta,Vector2 tb,Vector2 tc,Vector2 td,bool reverse,Color color)
            {int i=vertices.Count;vertices.AddRange(new[]{a,b,c,d});uv.AddRange(new[]{ta,tb,tc,td});colors.AddRange(new[]{color,color,color,color});
                triangles.AddRange(reverse?new[]{i,i+1,i+2,i+1,i+3,i+2}:new[]{i,i+2,i+1,i+1,i+2,i+3});}
            for(int z=0;z<nz;z++)for(int x=0;x<nx;x++)if(filled[x,z])
            {
                var a=Point(x,z,true);var b=Point(x+1,z,true);var c=Point(x,z+1,true);var d=Point(x+1,z+1,true);
                var aa=Point(x,z,false);var bb=Point(x+1,z,false);var cc=Point(x,z+1,false);var dd=Point(x+1,z+1,false);
                Quad(a,b,c,d,UV(x,z),UV(x+1,z),UV(x,z+1),UV(x+1,z+1),false,Color.white);
                Quad(aa,bb,cc,dd,UV(x,z),UV(x+1,z),UV(x,z+1),UV(x+1,z+1),true,new Color(.48f,.12f,.08f,0));
                var sample=(UV(x,z)+UV(x+1,z+1))*.5f;var shade=new Color(.57f,.14f,.10f,0);
                if(z==0||!filled[x,z-1])Quad(a,aa,b,bb,sample,sample,sample,sample,false,shade);
                if(z==nz-1||!filled[x,z+1])Quad(c,d,cc,dd,sample,sample,sample,sample,false,shade);
                if(x==0||!filled[x-1,z])Quad(a,c,aa,cc,sample,sample,sample,sample,false,shade);
                if(x==nx-1||!filled[x+1,z])Quad(b,bb,d,dd,sample,sample,sample,sample,false,shade);
            }
            var mesh=new Mesh{name=food.Id+" · volumetric raw grill artwork"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
    }
}

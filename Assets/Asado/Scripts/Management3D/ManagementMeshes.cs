using System.Collections.Generic;
using UnityEngine;

namespace Asadito
{
    /// <summary>Production low-poly authoring: swept casing, irregular rib strip and exposed bone.
    /// Shared by shop/inventory; no primitives or sprite billboards represent food.</summary>
    public static class ManagementMeshes
    {
        private sealed class Builder
        {
            readonly List<Vector3> vertices=new List<Vector3>();
            readonly List<int> triangles=new List<int>();
            readonly List<Color> colors=new List<Color>();
            public int Vertex(Vector3 p,Color c){vertices.Add(p);colors.Add(c.linear);return vertices.Count-1;}
            public void Quad(int a,int b,int c,int d){triangles.AddRange(new[]{a,c,b,b,c,d});}
            public Mesh Finish(string name){var m=new Mesh{name=name};m.SetVertices(vertices);m.SetColors(colors);m.SetTriangles(triangles,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
            public void Sweep(List<Vector3> path,float[] radii,Color color,int sides=10,float flatten=1)
            {
                int start=vertices.Count;
                for(int i=0;i<path.Count;i++)
                {
                    var tangent=(path[Mathf.Min(i+1,path.Count-1)]-path[Mathf.Max(0,i-1)]).normalized;
                    var side=Vector3.Cross(tangent,Vector3.up).normalized;var up=Vector3.Cross(side,tangent).normalized;
                    for(int j=0;j<=sides;j++)
                    {
                        float angle=j*Mathf.PI*2/sides;
                        Vertex(path[i]+radii[i]*(Mathf.Cos(angle)*up*flatten+Mathf.Sin(angle)*side),color*(.95f+.05f*Mathf.Cos(angle)));
                        if(i>0&&j>0){int a=start+(i-1)*(sides+1)+j-1;Quad(a,a+sides+1,a+1,a+sides+2);}
                    }
                }
            }
        }
        public static Mesh Chorizo()
        {
            var b=new Builder();var path=new List<Vector3>();var radii=new float[21];
            for(int i=0;i<21;i++){float t=i/20f;path.Add(new Vector3((t-.5f)*1.30f,.18f+.06f*Mathf.Sin(t*Mathf.PI),-.25f*Mathf.Sin(t*Mathf.PI)));radii[i]=.16f*Mathf.Pow(Mathf.Sin(t*Mathf.PI),.35f)+.006f;}
            b.Sweep(path,radii,new Color(.76f,.20f,.15f),12);
            foreach(int end in new[]{-1,1})
            {
                var knot=new List<Vector3>{new Vector3(end*.64f,.18f,0),new Vector3(end*.70f,.18f,.015f),new Vector3(end*.75f,.19f,.005f)};
                b.Sweep(knot,new[]{.035f,.043f,.003f},new Color(.65f,.12f,.10f),8);
                var stringPath=new List<Vector3>();var r=new float[9];
                for(int i=0;i<9;i++){float a=i*Mathf.PI*2/8;stringPath.Add(new Vector3(end*.675f,.18f+Mathf.Sin(a)*.036f,Mathf.Cos(a)*.036f));r[i]=.009f;}
                b.Sweep(stringPath,r,new Color(.95f,.80f,.58f),5);
            }
            // Small irregular flecks embedded in the casing, not a flat texture overlay.
            for(int k=0;k<7;k++)
            {
                float t=.16f+k*.105f;float x=(t-.5f)*1.3f,z=-.25f*Mathf.Sin(t*Mathf.PI);
                b.Sweep(new List<Vector3>{new Vector3(x-.02f,.35f,z),new Vector3(x,.37f,z+.012f),new Vector3(x+.03f,.34f,z+.028f)},new[]{.003f,.009f,.002f},new Color(.89f,.46f,.32f),5);
            }
            return b.Finish("Chorizo · casing curve, ties and flecks");
        }
        public static Mesh Tira()
        {
            var b=new Builder();var path=new List<Vector3>();var radii=new float[25];
            for(int i=0;i<25;i++)
            {
                float t=i/24f;path.Add(new Vector3((t-.5f)*1.4f,.18f+.015f*Mathf.Sin(t*11),.035f*Mathf.Sin(t*8)));
                radii[i]=.30f*Mathf.Pow(Mathf.Sin(t*Mathf.PI),.17f)*(1+.05f*Mathf.Sin(t*31))+.004f;
            }
            b.Sweep(path,radii,new Color(.66f,.16f,.17f),14,.56f);
            // Irregular ivory fat cap along the back of the rib strip.
            var fat=new List<Vector3>();var f=new float[17];
            for(int i=0;i<17;i++){float t=i/16f;fat.Add(new Vector3((t-.5f)*1.23f,.265f+.018f*Mathf.Sin(t*17),-.21f+.015f*Mathf.Sin(t*10)));f[i]=.047f*Mathf.Sin(t*Mathf.PI)+.004f;}
            b.Sweep(fat,f,new Color(.97f,.82f,.66f),7,.5f);
            for(int k=0;k<4;k++)
            {
                float x=-.47f+k*.31f;
                // Actual rib shafts and rounded exposed ends point towards the viewer.
                b.Sweep(new List<Vector3>{new Vector3(x,.11f,-.06f),new Vector3(x,.11f,.21f),new Vector3(x,.12f,.38f),new Vector3(x,.12f,.43f)},
                    new[]{.005f,.070f,.075f,.003f},new Color(.95f,.77f,.53f),10,.8f);
                b.Sweep(new List<Vector3>{new Vector3(x,.12f,.417f),new Vector3(x,.12f,.433f),new Vector3(x,.12f,.44f)},new[]{.005f,.038f,.002f},new Color(.63f,.31f,.17f),9);
                // Raised marbling ribbons follow the top meat surface in a branching pattern.
                var marble=new List<Vector3>();var r=new float[7];
                for(int j=0;j<7;j++){float t=j/6f;marble.Add(new Vector3(x+.08f*Mathf.Sin(t*5),.34f-.055f*Mathf.Abs(t-.5f)*2,-.15f+t*.30f));r[j]=.003f+.013f*Mathf.Sin(t*Mathf.PI);}
                b.Sweep(marble,r,new Color(.93f,.64f,.56f),5,.5f);
            }
            return b.Finish("Tira de asado · marbling, fat cap and four ribs");
        }
        public static Mesh RoundedBox(Vector3 size,float radius,Color color)
        {
            var b=new Builder();var half=size*.5f;radius=Mathf.Min(radius,Mathf.Min(half.x,Mathf.Min(half.y,half.z))*.95f);
            Vector3 inner=half-Vector3.one*radius;
            var normals=new[]{Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
            foreach(var n in normals)
            {
                var u=Mathf.Abs(n.y)>.5f ? Vector3.right : Vector3.Cross(Vector3.up,n);
                var v=Vector3.Cross(n,u);int[,] points=new int[5,5];
                for(int y=0;y<5;y++)for(int x=0;x<5;x++)
                {
                    var p=Vector3.Scale(n+u*(x/2f-1)+v*(y/2f-1),half);
                    var q=new Vector3(Mathf.Clamp(p.x,-inner.x,inner.x),Mathf.Clamp(p.y,-inner.y,inner.y),Mathf.Clamp(p.z,-inner.z,inner.z));
                    points[x,y]=b.Vertex(q+(p-q).normalized*radius,color);
                    if(x>0&&y>0)b.Quad(points[x-1,y-1],points[x-1,y],points[x,y-1],points[x,y]);
                }
            }
            return b.Finish("Bevelled prop");
        }
        public static Mesh Disk(float x,float z,Color color)
        {
            var b=new Builder();int center=b.Vertex(Vector3.zero,color);
            // Thin lens provides cheap contact shadow without dynamic lights.
            // Filled disk as a very thin bevelled box is less useful; use fan quads.
            int last=0;
            for(int i=0;i<=24;i++){float a=i*Mathf.PI*2/24;int cur=b.Vertex(new Vector3(Mathf.Cos(a)*x,0,Mathf.Sin(a)*z),color);if(i>0)b.Quad(center,center,cur,last);last=cur;}
            return b.Finish("Contact shadow");
        }
    }
}

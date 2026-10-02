Shader "Asadito/ManagementSurface"
{
 Properties { _Tint("Tint",Color)=(1,1,1,1) _Gloss("Soft highlight",Range(0,1))=.15 }
 SubShader {
 Tags { "RenderType"="Opaque" } Cull Back
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; fixed4 color:COLOR; };
 struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 view:TEXCOORD1; float3 local:TEXCOORD2; fixed4 color:COLOR; };
 fixed4 _Tint; half _Gloss;
 v2f vert(appdata v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.view=WorldSpaceViewDir(v.vertex);o.local=v.vertex.xyz;o.color=v.color;return o; }
 fixed4 frag(v2f i):SV_Target {
 half3 n=normalize(i.normal), l=normalize(half3(-.4,.85,.5));
 half diffuse=.53+.47*saturate(dot(n,l));
 half spec=pow(saturate(dot(n,normalize(l+normalize(i.view)))),24)*_Gloss;
 half grain=sin(i.local.x*17+i.local.z*11)*sin(i.local.y*13-i.local.x*9)*.012;
 return fixed4(i.color.rgb*_Tint.rgb*(diffuse+grain)+spec*half3(1,.88,.67),1);
 }
 ENDCG
 }
 }
}

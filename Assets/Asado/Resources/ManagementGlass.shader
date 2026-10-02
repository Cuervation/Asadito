Shader "Asadito/ManagementGlass"
{
 Properties { _Tint("Tint",Color)=(.68,.9,.9,.12) }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" } Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct a { float4 vertex:POSITION; float3 normal:NORMAL; };
 struct v { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 view:TEXCOORD1; };
 fixed4 _Tint;
 v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.normal=UnityObjectToWorldNormal(i.normal);o.view=WorldSpaceViewDir(i.vertex);return o;}
 fixed4 frag(v i):SV_Target { half edge=pow(1-abs(dot(normalize(i.normal),normalize(i.view))),3);return fixed4(_Tint.rgb,_Tint.a+edge*.16); }
 ENDCG }
 }
}

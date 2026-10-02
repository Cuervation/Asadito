Shader "Asadito/ManagementFoodTexture"
{
 Properties { _MainTex("Original grill atlas",2D)="white"{} _Tint("Tint",Color)=(1,1,1,1) }
 SubShader { Tags {"RenderType"="Opaque" "DisableBatching"="True"} Cull Back
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;float3 normal:NORMAL;fixed4 color:COLOR;};
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 n:TEXCOORD1;fixed4 color:COLOR;};
 sampler2D _MainTex;fixed4 _Tint;
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.n=UnityObjectToWorldNormal(v.normal);o.color=v.color;return o;}
 fixed4 frag(v2f i):SV_Target{
 half shade=.82+.18*saturate(dot(normalize(i.n),normalize(float3(-.3,1,.2))));
 if(i.color.a<.5)return fixed4(i.color.rgb*_Tint.rgb*shade,1);
 fixed4 c=tex2D(_MainTex,i.uv);clip(c.a-.12);return fixed4(c.rgb*_Tint.rgb*shade,1);
 }
 ENDCG }
 }
}

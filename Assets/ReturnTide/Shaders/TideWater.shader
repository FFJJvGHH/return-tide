Shader "Return Tide/Luminous Water" {
 Properties {
  _BaseColor("Deep water", Color)=(.045,.18,.21,1)
  _RippleColor("Living ripples", Color)=(.18,.46,.43,1)
  _Scale("Ripple scale",Float)=1.3
  _Speed("Flow speed",Float)=.3
 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass {
   Name "Water" Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor,_RippleColor;float _Scale,_Speed;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;};
   struct Varyings {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;half fog:TEXCOORD1;};
   Varyings vert(Attributes a){Varyings o;o.world=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.fog=ComputeFogFactor(o.positionCS.z);return o;}
   half4 frag(Varyings i):SV_Target {
    float2 p=i.world.xz*_Scale;float time=_Time.y*_Speed;
    float wave=sin(p.x*.65+sin(p.y*.7+time)*1.2-time)*sin(p.y*.8+cos(p.x*.45-time));
    float foam=smoothstep(.77,.84,wave)*.36;
    float glow=(sin(p.x*.12+p.y*.16-time)*.5+.5)*.16;
    half3 c=lerp(_BaseColor.rgb,_RippleColor.rgb,foam+glow);
    return half4(MixFog(c,i.fog),1);
   }
   ENDHLSL
  }
 }
}

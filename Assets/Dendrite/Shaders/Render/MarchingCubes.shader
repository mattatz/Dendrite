Shader "Dendrite/MarchingCubes"
{

  Properties
  {
    _Color ("Color", Color) = (0,0,0,1)
    [HDR] _Emission ("Emission", Color) = (0, 0, 0, 1)

    _Glossiness ("Smoothness", Range(0, 1)) = 0.5
    _Metallic ("Metallic", Range(0, 1)) = 0.0
    _Occlusion ("Occlusion", Range(0, 1)) = 0
  }

  CGINCLUDE

  #include "UnityCG.cginc"
  #include "UnityGBuffer.cginc"
  #include "UnityStandardUtils.cginc"

  struct Vertex
  {
    float3 position;
    float3 normal;
  };

  StructuredBuffer<Vertex> _Buffer;

  struct v2f
  {
    float4 pos : SV_POSITION;
    float3 normal : NORMAL;
    float3 worldPos : TANGENT;
    half3 ambient : TEXCOORD0;
  };

  struct v2f_shadow {
    float4 pos : SV_POSITION;
  };

  fixed4 _Color, _Emission;
  half _Glossiness, _Metallic, _Occlusion;

  v2f vert(uint id : SV_VertexID)
  {
    Vertex v = _Buffer[id];

    v2f OUT;
    float3 world = mul(unity_ObjectToWorld, float4(v.position.xyz, 1));
    OUT.pos = UnityWorldToClipPos(float4(world, 1));
    OUT.normal = UnityObjectToWorldNormal(v.normal.xyz);
    OUT.worldPos = world;
    OUT.ambient = ShadeSHPerVertex(OUT.normal, 0);
    return OUT;
  }

  void frag(
    v2f IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3,
    fixed facing : VFACE
  )
  {
    fixed3 normal = IN.normal;

    half3 c_diff, c_spec;
    half refl10;
    c_diff = DiffuseAndSpecularFromMetallic(
      _Color.rgb, _Metallic, // input
      c_spec, refl10 // output
    );

    UnityStandardData o;
    o.diffuseColor = c_diff;
    o.occlusion = _Occlusion;
    o.specularColor = c_spec;
    o.smoothness = _Glossiness;
    o.normalWorld = IN.normal;
    UnityStandardDataToGbuffer(o, outGBuffer0, outGBuffer1, outGBuffer2);

    half3 sh = ShadeSHPerPixel(IN.normal, IN.ambient, IN.worldPos);
    outEmission = _Emission * half4(sh * c_diff, 1) * _Occlusion;
  }

  ENDCG

  SubShader {
    Pass
    {
      Tags { "LightMode" = "Deferred" }
      // Cull Off

      CGPROGRAM
      #pragma target 5.0
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap
      ENDCG
    }

    Pass
    {
      Tags { "LightMode" = "ShadowCaster" }
      ZWrite On ZTest LEqual

      CGPROGRAM

      #pragma target 5.0
      #pragma vertex vert_shadow
      #pragma fragment frag_shadow
      #pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap

      v2f_shadow vert_shadow(uint id : SV_VertexID)
      {
        Vertex v = _Buffer[id];

        v2f_shadow OUT;
        float4 pos = UnityClipSpaceShadowCasterPos(float4(v.position.xyz, 1), v.normal.xyz);
        OUT.pos = UnityApplyLinearShadowBias(pos);
        return OUT;
      }

      fixed frag_shadow(v2f_shadow IN) : SV_Target {
        return 0;
      }

      ENDCG
    }

  }

}

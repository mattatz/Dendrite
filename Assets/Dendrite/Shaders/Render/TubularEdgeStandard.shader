Shader "Dendrite/TubularEdgeStandard"
{

  Properties
  {
    _Thickness ("Thickness", Range(0.01, 0.1)) = 0.1

    _Color ("Color", Color) = (1, 1, 1, 1)
    [HDR] _Emission ("Emission", Color) = (1, 1, 1, 1)

    [Space]
    _Glossiness ("Smoothness", Range(0, 1)) = 0.5
    [Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" }
    LOD 100
    Cull Back

    Pass
    {
      Tags { "LightMode" = "Deferred" }
      CGPROGRAM
      #pragma target 4.0
      #pragma vertex vert
      #pragma geometry geom
      #pragma fragment frag
      #pragma multi_compile_prepassfinal noshadowmask nodynlightmap nodirlightmap nolightmap
      #pragma multi_compile_instancing
      #pragma instancing_options procedural:setup
      #include "./TubularEdgeCommon.hlsl"
      ENDCG
    }

    Pass
    {
      Tags { "LightMode" = "ShadowCaster" }
      CGPROGRAM
      #pragma target 4.0
      #pragma vertex vert
      #pragma geometry geom
      #pragma fragment frag
      #pragma multi_compile_shadowcaster noshadowmask nodynlightmap nodirlightmap nolightmap
      #pragma multi_compile_instancing
      #pragma instancing_options procedural:setup
      #include "./TubularEdgeCommon.hlsl"
      ENDCG
    }
  }
}

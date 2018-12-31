Shader "Dendrite/Volume"
{

  Properties
  {
    _Volume ("Volume", 3D) = "" {}
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }
    LOD 100

    Pass
    {
      Cull Off
      Blend SrcAlpha One
      ZWrite On
      ZTest Always

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
      };

      struct v2f
      {
        float4 vertex : SV_POSITION;
        float3 uv : TEXCOORD0;
      };

      v2f vert(appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.normal;
        return o;
      }

      sampler3D _Volume;

      fixed4 frag(v2f i) : SV_Target
      {
        fixed4 col = tex3D(_Volume, i.uv);
        return col;
      }

      ENDCG
    }
  }
}

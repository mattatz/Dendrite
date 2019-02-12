Shader "Dendrite/Edge"
{

  Properties
  {
    _Color ("Color", Color) = (1, 1, 1, 1)
    _Gradient ("Gradient", 2D) = "" {}

    [Toggle] _Animation ("Animation", Range(0.0, 1.0)) = 1
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }
    LOD 100

    Pass
    {
      Blend One One
      ZTest Always
      ZWrite On
      Cull Off

      CGPROGRAM

      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_instancing
      #pragma instancing_options procedural:setup

      #include "UnityCG.cginc"
      #include "../Common/Node.hlsl"
      #include "../Common/Edge.hlsl"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        uint vid : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f
      {
        float4 position : SV_POSITION;
        float2 uv : TEXCOORD0;
        float2 uv2 : TEXCOORD1;
        float alpha : COLOR;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      StructuredBuffer<Node> _Nodes;
      StructuredBuffer<Edge> _Edges;
      uint _EdgesCount;

      float4 _Color;
      sampler2D _Gradient;
      half4 _Gradient_ST;

      fixed _Animation;

      float4x4 _World2Local, _Local2World;

      void setup() {
        unity_WorldToObject = _World2Local;
        unity_ObjectToWorld = _Local2World;
      }

      v2f vert(appdata IN, uint iid : SV_InstanceID)
      {
        v2f OUT;
        UNITY_SETUP_INSTANCE_ID(IN);
        UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

        Edge e = _Edges[iid];
        Node a = _Nodes[e.a];
        Node b = _Nodes[e.b];
        float3 ap = a.position;
        float3 bp = b.position;
        float3 dir = bp - ap;
        bp = ap + normalize(dir) * length(dir) * lerp(1, b.t, _Animation);
        float3 position = lerp(ap, bp, IN.vid);

        float4 vertex = float4(position, 1);
        OUT.position = UnityObjectToClipPos(vertex);
        OUT.uv = IN.uv;
        OUT.uv2 = float2(lerp(a.offset, b.offset, IN.vid), 0);
        OUT.uv2.x *= _Gradient_ST.x;

        OUT.alpha = (a.active && b.active) && (iid < _EdgesCount);

        return OUT;
      }

      fixed4 frag(v2f IN) : SV_Target
      {
        fixed4 grad = tex2D(_Gradient, IN.uv2);
        return _Color * grad * IN.alpha;
      }

      ENDCG
    }
  }
}

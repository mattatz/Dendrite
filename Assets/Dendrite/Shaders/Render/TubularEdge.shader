Shader "Dendrite/TubularEdge"
{

  Properties
  {
    _Color ("Color", Color) = (1, 1, 1, 1)
    _Thickness ("Thickness", Range(0.01, 0.1)) = 0.1
  }

  CGINCLUDE

  #include "UnityCG.cginc"
  #include "../Common/Node.hlsl"
  #include "../Common/Edge.hlsl"

  StructuredBuffer<Node> _Nodes;
  StructuredBuffer<Edge> _Edges;
  uint _EdgesCount;

  float4 _Color;
  half _Thickness;

  float4x4 _World2Local, _Local2World;

  struct appdata
  {
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    uint vid : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
  };

  struct v2g
  {
    float3 position : NORMAL;
    float3 viewDir : TANGENT;
    float2 uv : TEXCOORD0;
    float2 uv2 : TEXCOORD1;
    float alpha : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
  };

  struct g2f
  {
    float4 position : SV_POSITION;
    float3 normal : NORMAL;
    float2 uv : TEXCOORD0;
    float2 uv2 : TEXCOORD1;
  };

  void setup() {
    unity_WorldToObject = _World2Local;
    unity_ObjectToWorld = _Local2World;
  }

  v2g vert(appdata IN, uint iid : SV_InstanceID)
  {
    v2g OUT;
    UNITY_SETUP_INSTANCE_ID(IN);
    UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

    Edge e = _Edges[iid];
    Node a = _Nodes[e.a];
    Node b = _Nodes[e.b];
    float3 ap = a.position;
    float3 bp = b.position;
    float3 dir = bp - ap;
    bp = ap + normalize(dir) * length(dir) * b.t;
    float3 position = lerp(ap, bp, IN.vid);
    OUT.position = mul(unity_ObjectToWorld, float4(position, 1)).xyz;
    OUT.viewDir = WorldSpaceViewDir(float4(position, 1));
    OUT.uv = IN.uv;
    OUT.uv2 = float2(lerp(a.offset, b.offset, IN.vid), 0);
    OUT.alpha = (a.active && b.active) && (iid < _EdgesCount);

    return OUT;
  }

  [maxvertexcount(64)]
  void geom(line v2g IN[2], inout TriangleStream<g2f> OUT) {
    v2g p0 = IN[0];
    v2g p1 = IN[1];

    float alpha = p0.alpha;

    float3 t = normalize(p1.position - p0.position);
    float3 n = normalize(p0.viewDir);
    float3 bn = cross(t, n);
    n = cross(t, bn);

    float3 tp = lerp(p0.position, p1.position, alpha);
    float thickness = _Thickness * alpha;

    static const uint rows = 6, cols = 6;
    static const float rows_inv = 1.0 / rows, cols_inv = 1.0 / (cols - 1);

    g2f o0, o1;
    o0.uv = p0.uv; o0.uv2 = p0.uv2;
    o1.uv = p1.uv; o1.uv2 = p1.uv2;

    // create side
    for (uint i = 0; i < cols; i++) {
      float r = (i * cols_inv) * UNITY_TWO_PI;

      float s, c;
      sincos(r, s, c);
      float3 normal = normalize(n * c + bn * s);

      float3 w0 = p0.position + normal * thickness;
      float3 w1 = p1.position + normal * thickness;
      o0.normal = o1.normal = normal;

      o0.position = UnityWorldToClipPos(w0);
      OUT.Append(o0);

      o1.position = UnityWorldToClipPos(w1);
      OUT.Append(o1);
    }
    OUT.RestartStrip();

    // create tip
    uint row, col;
    for (row = 0; row < rows; row++)
    {
      float s0 = sin((row * rows_inv) * UNITY_HALF_PI);
      float s1 = sin(((row + 1) * rows_inv) * UNITY_HALF_PI);
      for (col = 0; col < cols; col++)
      {
        float r = (col * cols_inv) * UNITY_TWO_PI;

        float s, c;
        sincos(r, s, c);

        float3 n0 = normalize(n * c * (1.0 - s0) + bn * s * (1.0 - s0) + t * s0);
        float3 n1 = normalize(n * c * (1.0 - s1) + bn * s * (1.0 - s1) + t * s1);

        o0.position = UnityWorldToClipPos(float4(tp + n0 * thickness, 1));
        o0.normal = n0;
        OUT.Append(o0);

        o1.position = UnityWorldToClipPos(float4(tp + n1 * thickness, 1));
        o1.normal = n1;
        OUT.Append(o1);
      }
      OUT.RestartStrip();

    }

  }

  fixed4 frag(g2f IN) : SV_Target
  {
    float3 normal = IN.normal;
    fixed3 normal01 = (normal + 1.0) * 0.5;
    fixed4 color = _Color;
    color.rgb *= normal01.xyz;
    return color;
  }

  ENDCG

  SubShader
  {
    Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
    LOD 100

    Pass
    {
      ZWrite On
      Cull Back

      CGPROGRAM
      #pragma vertex vert
      #pragma geometry geom
      #pragma fragment frag
      #pragma multi_compile_instancing
      #pragma instancing_options procedural:setup
      ENDCG
    }
  }

}

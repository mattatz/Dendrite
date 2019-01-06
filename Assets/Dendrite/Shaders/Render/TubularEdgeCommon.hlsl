
#include "UnityCG.cginc"
#include "UnityGBuffer.cginc"
#include "UnityStandardUtils.cginc"

#if defined(SHADOWS_CUBE) && !defined(SHADOWS_CUBE_IN_DEPTH_TEX)
#define PASS_CUBE_SHADOWCASTER
#endif

#include "UnityCG.cginc"

#if defined(SKINNED)
#include "../Common/SkinnedNode.hlsl"
StructuredBuffer<SkinnedNode> _Nodes;
#else
#include "../Common/Node.hlsl"
StructuredBuffer<Node> _Nodes;
#endif

#include "../Common/Edge.hlsl"

StructuredBuffer<Edge> _Edges;
uint _EdgesCount;

float4 _Color, _Emission;
half _Glossiness;
half _Metallic;
half _Thickness, _Depth;

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
  float thickness : TEXCOORD2;
  float emission : TEXCOORD3;
  float alpha : COLOR;
  UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct g2f
{
  float4 position : SV_POSITION;

#if defined(PASS_CUBE_SHADOWCASTER)
  float3 shadow : TEXCOORD0;
#elif defined(UNITY_PASS_SHADOWCASTER)
#else
  float3 normal : NORMAL;
  float2 texcoord : TEXCOORD0;
  half3 ambient : TEXCOORD1;
  float3 wpos : TEXCOORD2;
  float emission : TEXCOORD3;
#endif
};

void setup()
{
  unity_WorldToObject = _World2Local;
  unity_ObjectToWorld = _Local2World;
}

v2g vert(appdata IN, uint iid : SV_InstanceID)
{
  v2g OUT;
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

  Edge e = _Edges[iid];

#if defined(SKINNED)
  SkinnedNode a = _Nodes[e.a], b = _Nodes[e.b];
  float3 ap = a.animated, bp = b.animated;
#else
  Node a = _Nodes[e.a], b = _Nodes[e.b];
  float3 ap = a.position, bp = b.position;
#endif

  float3 dir = bp - ap;
  bp = ap + normalize(dir) * length(dir) * b.t;
  float3 position = lerp(ap, bp, IN.vid);
  OUT.position = mul(unity_ObjectToWorld, float4(position, 1)).xyz;
  OUT.viewDir = WorldSpaceViewDir(float4(position, 1));
  OUT.uv = IN.uv;
  OUT.uv2 = float2(lerp(a.offset, b.offset, IN.vid) * 0.1, 0);
  OUT.alpha = (a.active && b.active) && (iid < _EdgesCount);

#if defined(THICKNESS_BY_DEPTH)
  float offset = lerp(a.offset, b.offset, IN.vid);
  OUT.thickness = OUT.alpha * max(0, (_Depth - offset) / _Depth);
#else
  OUT.thickness = OUT.alpha;
#endif

  float t = lerp(a.t, b.t, IN.vid);
  OUT.emission = smoothstep(1.0, 0.0, t);

  return OUT;
}

g2f create(float3 wpos, float3 wnrm, float2 texcoord, float emission)
{
  g2f o;
#if defined(PASS_CUBE_SHADOWCASTER)
  o.position = UnityObjectToClipPos(float4(wpos, 1));
  o.shadow = wpos - _LightPositionRange.xyz;
#elif defined(UNITY_PASS_SHADOWCASTER)
  float scos = dot(wnrm, normalize(UnityWorldSpaceLightDir(wpos)));
  wpos -= wnrm * unity_LightShadowBias.z * sqrt(1 - scos * scos);
  o.position = UnityApplyLinearShadowBias(UnityWorldToClipPos(float4(wpos, 1)));
#else
  o.position = UnityWorldToClipPos(float4(wpos, 1));
  o.normal = wnrm;
  o.texcoord = texcoord;
  o.ambient = ShadeSHPerVertex(wnrm, 0);
  o.wpos = wpos;
  o.emission = emission;
#endif

  return o;
}

void create_tip(
  inout TriangleStream<g2f> OUT, 
  float3 position, float2 uv, float thickness,
  uint rows, float rows_inv,
  uint cols, float cols_inv,
  float3 t, float3 n, float3 bn, float alpha, float emission
)
{
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

      float3 n0 = normalize(n * c * (1.0 - s0) + bn * s * (1.0 - s0) + t * s0) * alpha;
      float3 n1 = normalize(n * c * (1.0 - s1) + bn * s * (1.0 - s1) + t * s1) * alpha;

      float3 w0 = position + n0 * thickness;
      float3 w1 = position + n1 * thickness;

      g2f o0 = create(w0, n0, uv, emission);
      OUT.Append(o0);

      g2f o1 = create(w1, n1, uv, emission);
      OUT.Append(o1);
    }

    OUT.RestartStrip();
  }
}

[maxvertexcount(64)]
void geom(line v2g IN[2], inout TriangleStream<g2f> OUT)
{
  v2g p0 = IN[0];
  v2g p1 = IN[1];

  float3 t = normalize(p1.position - p0.position);
  float3 n = normalize(p0.viewDir);
  float3 bn = cross(t, n);
  n = cross(t, bn);

  float t0 = _Thickness * IN[0].thickness;
  float t1 = _Thickness * IN[1].thickness;

  float alpha = IN[0].alpha;
  float3 v0 = p0.position;
  float3 v1 = lerp(p0.position, p1.position, alpha);

  static const uint rows = 6, cols = 6;
  static const float rows_inv = 1.0 / rows, cols_inv = 1.0 / (cols - 1);

  uint i;
  for (i = 0; i < cols; i++)
  {
    float r = (i * cols_inv) * UNITY_TWO_PI;

    float s, c;
    sincos(r, s, c);
    float3 normal = normalize(n * c + bn * s) * alpha;

    float3 w0 = v0 + normal * t0;
    float3 w1 = v1 + normal * t1;

    g2f o0 = create(w0, normal, p0.uv2, p0.emission);
    OUT.Append(o0);

    g2f o1 = create(w1, normal, p1.uv2, p1.emission);
    OUT.Append(o1);
  }
  OUT.RestartStrip();

  create_tip(OUT, v1, p1.uv2, t1, rows, rows_inv, cols, cols_inv, t, n, bn, alpha, p1.emission);
};

#if defined(PASS_CUBE_SHADOWCASTER)

half4 frag(g2f input) : SV_Target
{
  float depth = length(input.shadow) + unity_LightShadowBias.x;
  return UnityEncodeCubeShadowDepth(depth * _LightPositionRange.w);
}

#elif defined(UNITY_PASS_SHADOWCASTER)

half4 frag() : SV_Target { return 0; }

#else

void frag(g2f IN, out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3)
{
  half3 albedo = _Color.rgb;

  half3 c_diff, c_spec;
  half refl10;
  c_diff = DiffuseAndSpecularFromMetallic(
    albedo, _Metallic, // input
    c_spec, refl10 // output
  );

  UnityStandardData data;
  data.diffuseColor = c_diff;
  data.occlusion = 1.0;
  data.specularColor = c_spec;
  data.smoothness = _Glossiness;
  data.normalWorld = IN.normal;
  UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

  half3 sh = ShadeSHPerPixel(data.normalWorld, IN.ambient, IN.wpos);
  outEmission = _Emission * IN.emission + half4(sh * c_diff, 1);
}

#endif

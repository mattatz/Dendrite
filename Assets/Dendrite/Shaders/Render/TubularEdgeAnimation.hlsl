
float _Offset;

v2g vert_animation(appdata IN, uint iid : SV_InstanceID)
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

  float offset = lerp(a.offset, b.offset, IN.vid);
  float shrink = saturate(max(0, _Offset - offset));
  ap = lerp(ap, bp, shrink);

  float3 dir = bp - ap;
  bp = ap + normalize(dir) * length(dir) * b.t;
  float3 position = lerp(ap, bp, IN.vid);
  OUT.position = mul(unity_ObjectToWorld, float4(position, 1)).xyz;
  OUT.viewDir = WorldSpaceViewDir(float4(position, 1));
  OUT.uv = IN.uv;
  OUT.uv2 = float2(lerp(a.offset, b.offset, IN.vid) * 0.1, 0);
  OUT.alpha = (a.active && b.active) && (iid < _EdgesCount);
  OUT.thickness = OUT.alpha * (1.0 - shrink);

  float t = lerp(a.t, b.t, IN.vid);
  // OUT.emission = smoothstep(1.0, 0.99, t);
  OUT.emission = max(smoothstep(0.0, 1.0, shrink), smoothstep(1.0, 0.999, t));

  return OUT;
}

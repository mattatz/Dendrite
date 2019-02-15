void Connect (uint3 id : SV_DispatchThreadID)
{
  uint idx = id.x;
  if (idx >= _ConnectCount)
    return;

  CANDIDATE_TYPE c = _CandidatesConsume.Consume();

  NODE_TYPE n1 = _Nodes[c.node];
  NODE_TYPE n2;
  uint idx2 = CreateNode(n2);
  n2.position = c.position;

  #ifdef SKINNED
  n2.animated = c.position;
  n2.index0 = c.bone;
  #endif

  n2.offset = n1.offset + 1.0;
  n2.mass = lerp(_MassMin, _MassMax, nrand(float2(c.node, idx2)));
  n2.from = c.node;

  _Nodes[c.node] = n1;
  _Nodes[idx2] = n2;
  CreateEdge(c.node, idx2);
}

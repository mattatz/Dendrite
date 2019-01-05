void Grow (uint3 id : SV_DispatchThreadID)
{
  uint idx = id.x;
  uint count, stride;
  _Nodes.GetDimensions(count, stride);
  if (idx >= count)
    return;

  NODE_TYPE n = _Nodes[idx];

  if (n.active)
  {
    n.t = saturate(n.t + _DT * n.mass);
    _Nodes[idx] = n;
  }
}

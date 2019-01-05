void Setup (uint3 id : SV_DispatchThreadID)
{
  uint idx = id.x;
  uint count, stride;
  _Nodes.GetDimensions(count, stride);
  if (idx >= count)
    return;

  _NodesPoolAppend.Append(idx);

  NODE_TYPE n = _Nodes[idx];
  n.active = false;
  _Nodes[idx] = n;
}

void Remove (uint3 id : SV_DispatchThreadID)
{
  uint idx = id.x;
  uint count, stride;
  _Nodes.GetDimensions(count, stride);
  if (idx >= count)
    return;

  NODE_TYPE n = _Nodes[idx];
  if (!n.active)
    return;

  _Attractions.GetDimensions(count, stride);
  for (uint i = 0; i < count; i++)
  {
    ATTRACTION_TYPE attr = _Attractions[i];
    if (attr.active)
    {
      float d = distance(attr.position, n.position);
      if (d < _KillDistance)
      {
        attr.active = false;
        _Attractions[i] = attr;
      }
    }
  }
}

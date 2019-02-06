void Remove(uint3 id : SV_DispatchThreadID)
{
  uint idx = id.x;
  uint count, stride;
  _Attractions.GetDimensions(count, stride);
  if (idx >= count)
    return;

  ATTRACTION_TYPE attr = _Attractions[idx];
  if (!attr.active)
    return;

  _Nodes.GetDimensions(count, stride);
  for (uint i = 0; i < count; i++)
  {
    NODE_TYPE n = _Nodes[i];
    if (n.active)
    {
      float d = distance(attr.position, n.position);
      if (d < _KillDistance)
      {
        attr.active = false;
        _Attractions[idx] = attr;
        return;
      }
    }
  }
}

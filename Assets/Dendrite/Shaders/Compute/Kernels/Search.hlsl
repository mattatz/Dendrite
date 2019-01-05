void Search (uint3 id : SV_DispatchThreadID)
{
  uint idx = id.x;
  uint count, stride;
  _Attractions.GetDimensions(count, stride);
  if (idx >= count)
    return;

  ATTRACTION_TYPE attr = _Attractions[idx];

  attr.found = false;
  if (attr.active)
  {
    _Nodes.GetDimensions(count, stride);

    float min_dist = _InfluenceDistance;
    uint nearest = -1;

    for (uint i = 0; i < count; i++)
    {
      NODE_TYPE n = _Nodes[i];

      if (n.active)
      {
        float3 dir = attr.position - n.position;
        float d = length(dir);
        if (d < min_dist)
        {
          min_dist = d;
          nearest = i;
          attr.found = true;
        }
      }
    }

    attr.nearest = nearest;
    _Attractions[idx] = attr;
  }
}

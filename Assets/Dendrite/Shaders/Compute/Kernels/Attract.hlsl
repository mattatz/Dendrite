void Attract (uint3 id : SV_DispatchThreadID)
{
  uint idx = id.x;
  uint count, stride;
  _Nodes.GetDimensions(count, stride);
  if (idx >= count)
    return;

  NODE_TYPE n = _Nodes[idx];
  if (n.active && n.t >= _AttractionThreshold)
  {
    float3 dir = (0.0).xxx;
    uint counter = 0;

    #ifdef SKINNED
    float dist = 1e8;
    uint nearest = -1;
    #endif

    // search neighbors in radius
    _Attractions.GetDimensions(count, stride);
    for (uint i = 0; i < count; i++)
    {
      ATTRACTION_TYPE attr = _Attractions[i];
      if (attr.active && attr.found && attr.nearest == idx)
      {
        float3 dir2 = (attr.position - n.position);
        dir += normalize(dir2);
        counter++;

        #ifdef SKINNED
        float l2 = length(dir2);
        if (l2 < dist)
        {
          dist = l2;
          nearest = i;
        }
        #endif
      }
    }

    if (counter > 0)
    {
      CANDIDATE_TYPE c;
      dir = dir / counter;
      c.position = n.position + (dir * _GrowthDistance);
      c.node = idx;

      #ifdef SKINNED
      c.bone = _Attractions[nearest].bone;
      #endif

      _CandidatesAppend.Append(c);
    }
  }

}

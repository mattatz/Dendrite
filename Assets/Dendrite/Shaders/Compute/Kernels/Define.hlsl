#ifndef __DENDRITE_DEFINE__
#define __DENDRITE_DEFINE__

StructuredBuffer<float3> _Seeds;
uint _ConnectCount;

float _InfluenceDistance, _GrowthDistance, _KillDistance;
float _GrowthLength;
float _DT;

float _MassMin, _MassMax;

AppendStructuredBuffer<Edge> _EdgesPoolAppend;

void CreateEdge(int a, int b)
{
  Edge e;
  e.a = a;
  e.b = b;
  _EdgesPoolAppend.Append(e);
}

#ifdef SKINNED

#define NODE_TYPE SkinnedNode
#define ATTRACTION_TYPE SkinnedAttraction
#define CANDIDATE_TYPE SkinnedCandidate

#else

#define NODE_TYPE Node
#define ATTRACTION_TYPE Attraction
#define CANDIDATE_TYPE Candidate

#endif

#endif
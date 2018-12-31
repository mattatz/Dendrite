Dendrite
=====================

GPU-based dendrite patterns generator with a space colonization algorithm in Unity.

![Demo](https://raw.githubusercontent.com/mattatz/Dendrite/master/Captures/DendriteSphere.gif)

Dendrite builds a branching model by feeding points as seed. 

## Rendering

Dendrite provides some rendering functions for Attraction, Node, Edge. (See sub classes of DendriteRenderingBase)

Rendering edges with volume by Marching Cubes.
![MarchingCubes](https://raw.githubusercontent.com/mattatz/Dendrite/master/Captures/DendriteSphereMarchingCubes.gif)

## Skinned Dendrite

![SkinnedDendrite](https://raw.githubusercontent.com/mattatz/Dendrite/master/Captures/DendriteSphere.gif)
SkinnedDendrite is a special type of Dendrite for a skinned mesh.
It builds a animated dendrite model with SkinnedMeshRenderer as a source.

## Compatibility

tested on Unity 2018.2.8f1, windows10 (GTX 1060).

## Sources

- Algorithmic Design with Houdini - https://vimeo.com/305061631#t=1500s 
- Modeling Trees with a Space Colonization Algorithm - http://algorithmicbotany.org/papers/colonization.egwnp2007.html
- Scrawk / Marching-Cubes-On-The-GPU - https://github.com/Scrawk/Marching-Cubes-On-The-GPU
- haolange / Unity-SeparableSubsurface - https://github.com/haolange/Unity-SeparableSubsurface

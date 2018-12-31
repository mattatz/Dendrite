using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class TextureGenBase : ScriptableObject {

    public abstract Texture2D Create(int width, int height, TextureWrapMode wrapMode = TextureWrapMode.Repeat);

}



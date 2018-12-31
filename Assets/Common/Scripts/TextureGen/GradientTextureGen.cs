using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "TextureGen/Gradient", fileName = "GradientTextureGen")]
public class GradientTextureGen : TextureGenBase {

    [SerializeField] protected Gradient grad;

    public override Texture2D Create(int width, int height, TextureWrapMode wrapMode = TextureWrapMode.Repeat)
    {
        var gradTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        gradTex.filterMode = FilterMode.Bilinear;
        gradTex.wrapMode = wrapMode;
        float inv = 1f / (width - 1);
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                var t = x * inv;
                Color col = grad.Evaluate(t);
                gradTex.SetPixel(x, y, col);
            }
        }
        gradTex.Apply();
        return gradTex;
    }

}



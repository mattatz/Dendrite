Shader "Hidden/PostProcessing/SeparableSubsurfaceScattering" {

  Properties {
    _Strength ("Strength", Float) = 5.0
  }

  HLSLINCLUDE
  #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
  #include "SeparableSubsurfaceScatteringCommon.hlsl"
  #pragma target 3.0
  ENDHLSL

  SubShader {
    ZTest Always
    ZWrite Off
    Cull Off

    /*
    Stencil {
      Ref 5
      comp equal
      pass keep
    }
    */

    Pass {
      Name "XBlur"

      HLSLPROGRAM
      #pragma vertex VertDefault
      #pragma fragment Frag
      #pragma target 3.0

      float4 Frag(VaryingsDefault i) : SV_TARGET {
          float4 SceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
          float SSSIntencity = (_Strength * _CameraDepthTexture_TexelSize.x);
          float3 XBlur = SSS(SceneColor, i.texcoord, float2(SSSIntencity, 0)).rgb;
          return float4(XBlur, SceneColor.a);

          // float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord));
          // return float4(depth.xxx, 1);
      }
      ENDHLSL
    }

    Pass {
      Name "YBlur"
      HLSLPROGRAM
      #pragma vertex VertDefault
      #pragma fragment Frag
      #pragma target 3.0

      float4 Frag(VaryingsDefault i) : COLOR {
          float4 SceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
          float SSSIntencity = (_Strength * _CameraDepthTexture_TexelSize.y);
          float3 YBlur = SSS(SceneColor, i.texcoord, float2(0, SSSIntencity)).rgb;
          return float4(YBlur, SceneColor.a);
      }
      ENDHLSL
    }

  }
}

#define DistanceToProjectionWindow 5.671281819617709 //1.0 / tan(0.5 * radians(20));
#define DPTimes300 1701.384545885313 //DistanceToProjectionWindow * 300
#define SamplerSteps 25

TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
float4 _CameraDepthTexture_TexelSize;

TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
float4 _MainTex_TexelSize;

float _Strength;
float4 _Kernel[SamplerSteps];
            
float4 SSS(float4 SceneColor, float2 UV, float2 SSSIntencity)
{
  float SceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, UV));
  float BlurLength = DistanceToProjectionWindow / SceneDepth;
  float2 UVOffset = SSSIntencity * BlurLength;
  float4 BlurSceneColor = SceneColor;
  BlurSceneColor.rgb *= _Kernel[0].rgb;

  [loop]
  for (int i = 1; i < SamplerSteps; i++)
  {
    float2 SSSUV = UV + _Kernel[i].a * UVOffset;
    float4 SSSSceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, SSSUV);
    float SSSDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, SSSUV)).r;
    float SSSScale = saturate(DPTimes300 * SSSIntencity * abs(SceneDepth - SSSDepth));
    SSSSceneColor.rgb = lerp(SSSSceneColor.rgb, SceneColor.rgb, SSSScale);
    BlurSceneColor.rgb += _Kernel[i].rgb * SSSSceneColor.rgb;
  }
  return BlurSceneColor;
}
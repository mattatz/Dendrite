Shader "Dendrite/Attraction"
{

	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
    _Size ("Size", Range(0.0, 1.0)) = 0.75
    _Intensity ("Intensity", Range(1.0, 30.0)) = 5.0
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent" }
		LOD 100

		Pass
		{
      Blend SrcAlpha One
      // ZTest Always
      Cull Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma instancing_options procedural:setup
			
			#include "UnityCG.cginc"
			#include "../Common/Attraction.hlsl"

			struct appdata
			{
				float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 position : SV_POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			StructuredBuffer<Attraction> _Attractions;
			float4 _Color;
      float _Size, _Intensity;

      float4x4 _World2Local, _Local2World;

      void setup() {
        unity_WorldToObject = _World2Local;
        unity_ObjectToWorld = _Local2World;
      }

			v2f vert (appdata IN, uint iid : SV_InstanceID)
			{
				v2f OUT;
        UNITY_SETUP_INSTANCE_ID(IN);
        UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

        Attraction p = _Attractions[iid];
        float4 vertex = IN.vertex * _Size * p.active;
        OUT.position = mul(
          UNITY_MATRIX_P, 
          float4(UnityObjectToViewPos(p.position.xyz).xyz, 1) + float4(vertex.x, vertex.y, 0, 0)
        );
        OUT.uv = IN.uv - 0.5;

				return OUT;
			}

      // square root of 2 * 0.25
      static const float SQ = 0.35355339059;
      static const float INVSQ = 1.0 / 0.35355339059;

			fixed4 frag (v2f IN) : SV_Target
			{
        float d = length(IN.uv);

        float alpha = saturate(1.0 - abs(SQ - d) * INVSQ);
        alpha = saturate(alpha * alpha * alpha - 0.1);

        float4 color = _Color;
        color.a *= saturate(saturate(0.5 - d) * _Intensity);
				return color;
			}

			ENDCG
		}
	}
}

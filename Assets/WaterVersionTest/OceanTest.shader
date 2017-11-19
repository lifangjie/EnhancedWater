Shader "Custom/OceanTest"
{
	Properties
	{
		PerlinNoise ("PerlinNoise Texture", 2D) = "white" {}
		[HideInInspector] PerlinOctave ("PerlinOctave", Vector) = (1.12, 0.59, 0.23)
		PerlinSize ("PerlinSize", float) = 1.0
		PerlinAmplitude ("PerlinAmplitude", Vector) = (35, 42, 57)
		[HideInInspector] PerlinGradient ("PerlinGradient", Vector) = (1.4, 1.6, 2.2)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				//float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D PerlinNoise;
			sampler2D DisplacementTexture;
			float2 PerlinMovement;
			float3 PerlinOctave;
			float3 PerlinAmplitude;
			float3 PerlinGradient;
			float PerlinSize;

			v2f vert (appdata v)
			{
				v2f o;

				float3 eye_vec = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
				float dist_2d = length(eye_vec.xy);
				float blend_factor = (20000 - dist_2d) / (20000 - 800);
				blend_factor = clamp(blend_factor, 0, 1);

				// Add perlin noise to distant patches
				float perlin = 0;
				blend_factor = 0;
				if (blend_factor < 1)
				{
					//float2 perlin_tc = uv_local * g_PerlinSize + g_UVBase;
					float2 uv = PerlinSize * v.uv;
					float perlin_0 = tex2Dlod(PerlinNoise, float4(uv * PerlinOctave.x + PerlinMovement, 0, 0)).w;
					float perlin_1 = tex2Dlod(PerlinNoise, float4(uv * PerlinOctave.y + PerlinMovement, 0, 0)).w;
					float perlin_2 = tex2Dlod(PerlinNoise, float4(uv * PerlinOctave.z + PerlinMovement, 0, 0)).w;
					//float perlin_0 = g_texPerlin.SampleLevel(g_samplerPerlin, perlin_tc * PerlinOctave.x + PerlinMovement, 0).w;
					//float perlin_1 = g_texPerlin.SampleLevel(g_samplerPerlin, perlin_tc * PerlinOctave.y + PerlinMovement, 0).w;
					//float perlin_2 = g_texPerlin.SampleLevel(g_samplerPerlin, perlin_tc * PerlinOctave.z + PerlinMovement, 0).w;

					perlin = perlin_0 * PerlinAmplitude.x + perlin_1 * PerlinAmplitude.y + perlin_2 * PerlinAmplitude.z;
				}

				// Displacement map
				float3 displacement = 0;
				if (blend_factor > 0)
					displacement = tex2Dlod(DisplacementTexture, float4(v.uv, 0, 0)).xzy;
				displacement = lerp(float3(0, perlin, 0), displacement, blend_factor);
				//v.vertex.xyz += displacement;
				v.vertex.xyz += float3(0, perlin, 0);

				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 col = 1;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}

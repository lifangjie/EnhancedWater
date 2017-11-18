Shader "Unlit/OceanTest"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		PerlinNoise ("PerlinNoise Texture", 2D) = "white" {}
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
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D PerlinNoise;

			v2f vert (appdata v)
			{
				v2f o;

				float3 eye_vec = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
				float dist_2d = length(eye_vec.xy);
				float blend_factor = (20000 - dist_2d) / (20000 - 800);
				blend_factor = clamp(blend_factor, 0, 1);

			// Add perlin noise to distant patches
				float perlin = 0;
				if (blend_factor < 1)
				{
					//float2 perlin_tc = uv_local * g_PerlinSize + g_UVBase;
					float perlin_0 = tex2Dlod(PerlinNoise, float4(v.uv, 0, 0)).w;
					float perlin_0 = g_texPerlin.SampleLevel(g_samplerPerlin, perlin_tc * g_PerlinOctave.x + g_PerlinMovement, 0).w;
					float perlin_1 = g_texPerlin.SampleLevel(g_samplerPerlin, perlin_tc * g_PerlinOctave.y + g_PerlinMovement, 0).w;
					float perlin_2 = g_texPerlin.SampleLevel(g_samplerPerlin, perlin_tc * g_PerlinOctave.z + g_PerlinMovement, 0).w;

					perlin = perlin_0 * g_PerlinAmplitude.x + perlin_1 * g_PerlinAmplitude.y + perlin_2 * g_PerlinAmplitude.z;
				}

			// Displacement map
				float3 displacement = 0;
				if (blend_factor > 0)
					displacement = g_texDisplacement.SampleLevel(g_samplerDisplacement, uv_local, 0).xyz;
				displacement = lerp(float3(0, 0, perlin), displacement, blend_factor);
				v.vertex.xyz += displacement;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}

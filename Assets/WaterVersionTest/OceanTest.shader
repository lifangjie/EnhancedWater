Shader "Custom/OceanTest"
{
	Properties
	{
		_Tess ("Tessellation", float) = 1.0
		minDist ("Tessellation min distance", float) = 10.0
		maxDist ("Tessellation max distance", float) = 25.0
		PerlinNoise ("PerlinNoise Texture", 2D) = "white" {}
		DisplacementTexture ("Displacement Texture", 2D) = "white" {}
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
			#pragma vertex tessvert
			#pragma fragment frag
			#pragma hull hs
			#pragma domain ds
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "Tessellation.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};
			struct InternalTessInterp_appdata {
				float4 vertex : INTERNALTESSPOS;
				float2 uv : TEXCOORD0;
			};
			InternalTessInterp_appdata tessvert (appdata v) {
				InternalTessInterp_appdata o;
				o.vertex = v.vertex;
				o.uv = v.uv;
				return o;
			}
			struct UnityTessellationFactors {
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};
			float _Tess;
			float minDist;
			float maxDist;
			float4 tessDistance (appdata v0, appdata v1, appdata v2) {
				return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
			}
			UnityTessellationFactors hsconst (InputPatch<InternalTessInterp_appdata,3> v) {
				UnityTessellationFactors o;
				float4 tf;
				appdata vi[4];
				vi[0].vertex = v[0].vertex;
				vi[0].uv = v[0].uv;
				vi[1].vertex = v[1].vertex;
				vi[1].uv = v[1].uv;
				vi[2].vertex = v[2].vertex;
				vi[2].uv = v[2].uv;
				tf = tessDistance(vi[0], vi[1], vi[2]);
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

            // tessellation hull shader
			[domain("tri")]
			[UNITY_partitioning("integer")]
			[UNITY_outputtopology("triangle_cw")]
			[UNITY_patchconstantfunc("hsconst")]
			[UNITY_outputcontrolpoints(4)]
			InternalTessInterp_appdata hs (InputPatch<InternalTessInterp_appdata,3> v, uint id : SV_OutputControlPointID) {
				return v[id];
			}


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

				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 eye_vec = o.worldPos - _WorldSpaceCameraPos;
				float dist_2d = length(eye_vec.xy);
				float blend_factor = (20000 - dist_2d) / (20000 - 800);
				blend_factor = clamp(blend_factor, 0, 1);

				// Add perlin noise to distant patches
				float perlin = 0;
				float2 uv = PerlinSize * v.uv;
				if (blend_factor < 1)
				{
					//float2 perlin_tc = uv_local * g_PerlinSize + g_UVBase;
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
					displacement = tex2Dlod(DisplacementTexture, float4(uv, 0, 0)).xzy;
				displacement = lerp(float3(0, perlin, 0), displacement, blend_factor);
				//v.vertex.xyz += displacement;

				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			[UNITY_domain("tri")]
			v2f ds (UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata,3> vi, float3 bary : SV_DomainLocation) {
				appdata v;
				v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
				v.uv = vi[0].uv*bary.x + vi[1].uv*bary.y + vi[2].uv*bary.z;
				v2f o = vert (v);
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float3 eye_vec = i.worldPos - _WorldSpaceCameraPos;
				float dist_2d = length(eye_vec.xy);
				half blend_factor = (20000 - dist_2d) / (20000 - 800);
				half4 col = half4(blend_factor, blend_factor, blend_factor, 1);
				col = mul(col, 1);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}

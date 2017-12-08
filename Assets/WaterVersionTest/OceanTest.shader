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
		DisplacementSize ("DisplacementSize", float) = 1.0
		PerlinAmplitude ("PerlinAmplitude", Vector) = (35, 42, 57)
		[HideInInspector] PerlinGradient ("PerlinGradient", Vector) = (1.4, 1.6, 2.2)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode"="Deferred"}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex tessvert
			#pragma fragment frag
			#pragma hull hs
			#pragma domain ds
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "UnityImageBasedLighting.cginc"
            #include "Lighting.cginc"
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
			struct UnityTessellationFactorsCustom {
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};
			float _Tess;
			float minDist;
			float maxDist;
			float UnityCalcDistanceTessFactorCustom (float4 vertex, float minDist, float maxDist, float tess)
			{
				float3 wpos = mul(unity_ObjectToWorld,vertex).xyz;
				float dist = distance (wpos, _WorldSpaceCameraPos);
				float f = clamp(pow(1.0 - sqrt(saturate((dist - minDist) / (maxDist - minDist))), 2), 0.01, 1.0) * tess;
				return f;
			}
			float4 tessDistance (appdata v0, appdata v1, appdata v2) {
				float3 f;
				f.x = UnityCalcDistanceTessFactorCustom (v0.vertex,minDist,maxDist,_Tess);
				f.y = UnityCalcDistanceTessFactorCustom (v1.vertex,minDist,maxDist,_Tess);
				f.z = UnityCalcDistanceTessFactorCustom (v2.vertex,minDist,maxDist,_Tess);

				return UnityCalcTriEdgeTessFactors (f);
			}
			UnityTessellationFactorsCustom hsconst (InputPatch<InternalTessInterp_appdata,3> v) {
				UnityTessellationFactorsCustom o;
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
			float DisplacementSize;

			v2f vert (appdata v)
			{
				v2f o;

				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 eye_vec = o.worldPos - _WorldSpaceCameraPos;
				float dist_2d = length(eye_vec.xy);
				float blend_factor = (1000 - dist_2d) / (1000 - 8);
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
					displacement = tex2Dlod(DisplacementTexture, float4(v.uv * DisplacementSize, 0, 0)).xzy;
				displacement = lerp(float3(0, perlin, 0), displacement, blend_factor);
				v.vertex.xyz += displacement;
				//v.vertex.xyz += float3(0, perlin, 0);
				

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
			
			void frag (v2f i,
				out half4 outGBuffer0 : SV_Target0,
				out half4 outGBuffer1 : SV_Target1,
				out half4 outGBuffer2 : SV_Target2,
				out half4 outEmission : SV_Target3
				) {
				float3 worldPos = i.worldPos;
				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				
			// Setup lighting environment
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = 0;
				gi.light.dir = half3(0,1,0);
			// Call GI (lightmaps/SH/reflections) lighting function
				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.worldViewDir = worldViewDir;
				giInput.atten = 1;
			#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				//giInput.lightmapUV = IN.lmap;
			#else
				giInput.lightmapUV = 0.0;
			#endif
			#if UNITY_SHOULD_SAMPLE_SH
				//giInput.ambient = IN.sh;
			#else
				giInput.ambient.rgb = 0.0;
			#endif
				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
			#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
				giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
			#endif
			#ifdef UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMax[0] = unity_SpecCube0_BoxMax;
				giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
				giInput.boxMax[1] = unity_SpecCube1_BoxMax;
				giInput.boxMin[1] = unity_SpecCube1_BoxMin;
				giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
			#endif
				//fixed3 normal = normalize(fixed3(s.bump.x, _BumpStrength + 1, s.bump.y));
				fixed3 normal = fixed3(0, 1, 0);
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
				gi = UnityGlobalIllumination(giInput, 1, normal);
			#else
				Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(1, worldViewDir, normal, 0.08);
				gi = UnityGlobalIllumination(giInput, 1, normal, g);
			#endif

				half fresnel = (0.02 + 0.98 * Pow5(1 - saturate(dot(normal, worldViewDir))));// * s.depth;
				UnityStandardData data;
				data.diffuseColor   = 1 - 0.9;
				data.occlusion      = 1;
				data.specularColor  = 0.08;
				data.smoothness     = 1;
				data.normalWorld    = normal;
				UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);
				//s.Albedo = lerp(1, _DeepWaterColor, );
				//outEmission = half4(1* (1 - fresnel), 1);
				//outEmission = half4(1 - fresnel, 1 - fresnel, 1 - fresnel, 1);
				outEmission = 0;

			}
			ENDCG
		}
	}
}

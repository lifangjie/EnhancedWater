Shader "Custom/WaterShaderTest" {
	Properties{
		//_Tess ("Tessellation", Range(1,32)) = 4
		_Transparency("Water transparency", Float) = 50.0
		[NoScaleOffset] _BumpMap("Normalmap ", 2D) = "bump" {}
		_WaveScale("Wave scale", Range(0.02,0.55)) = 0.118
		_ReflDistort ("Reflection distort", Range (0,1.5)) = 0.44
		_RefrDistort ("Refraction distort", Range (0,1.5)) = 0.40
		WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (9,5,-7,-4)
		_WaterColor("Simple water color", COLOR) = (.172, .463, .435, 1)
		_HorizonColor("Simple water horizon color", COLOR) = (.172, .463, .435, 1)
		[HideInInspector] _ReflectionTex("Internal Reflection", 2D) = "Blue" {}
		//[HideInInspector] _RefractionTex("Internal Refraction", 2D) = "Blue" {}

		_GerstnerIntensity("Per vertex displacement", Float) = 1.0
		_GAmplitude ("Wave Amplitude", Vector) = (0.3 ,0.35, 0.25, 0.25)
		_GFrequency ("Wave Frequency", Vector) = (1.3, 1.35, 1.25, 1.25)
		_GSteepness ("Wave Steepness", Vector) = (1.0, 1.0, 1.0, 1.0)
		_GSpeed ("Wave Speed", Vector) = (1.2, 1.375, 1.1, 1.5)
		_GDirectionAB ("Wave Direction", Vector) = (0.3 ,0.85, 0.85, 0.25)
		_GDirectionCD ("Wave Direction", Vector) = (0.1 ,0.9, 0.5, 0.5)
	}


	// -----------------------------------------------------------
	// Fragment program cards


	SubShader{

				Tags{ "WaterMode"="Refractive" "IgnoreProjector"="True" "RenderType"="Opaque" }
//		Pass {
//			Name "ShadowCaster"
//			Tags { "LightMode"="ShadowCaster" }
//
//			ZWrite On
//			ZTest LEqual
//			//ZTest Never
//			//ZWrite On
//
//			CGPROGRAM
//			#pragma target 3.0
//
//			// -------------------------------------
//
//
//			//#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
//			#pragma shader_feature _METALLICGLOSSMAP
//			#pragma shader_feature _PARALLAXMAP
//			#pragma multi_compile_shadowcaster
//			#pragma multi_compile_instancing
//			#pragma multi_compile GERSTNER_ON GERSTNER_OFF
//
//			// Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
//			//#pragma multi_compile _ LOD_FADE_CROSSFADE
//
//			#pragma vertex vert
//			#pragma fragment fragShadowCaster
//
//			#include "UnityStandardShadow.cginc"
//
//			#if defined (GERSTNER_ON)
//			#define WATER_VERTEX_DISPLACEMENT_ON
//			#include "WaterInclude.cginc"
//			uniform half4 _GAmplitude;
//			uniform half4 _GFrequency;
//			uniform half4 _GSteepness;
//			uniform half4 _GSpeed;
//			uniform half4 _GDirectionAB;
//			uniform half4 _GDirectionCD;
//			half _OffsetX, _OffsetZ, _Distance;
//			half _WaveAmplitude;
//			half _xImpact, _zImpact;
//					  #endif
//
//			void vert(VertexInput v,
//						  #ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
//				out VertexOutputShadowCaster o,
//						  #endif
//						  #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
//				out VertexOutputStereoShadowCaster os,
//						  #endif
//				out float4 opos : SV_POSITION)
//			{
//							  #if defined (GERSTNER_ON)
//				half3 worldPos = mul(unity_ObjectToWorld, v.vertex);
//				half3 vtxForAni = (worldPos).xzz;
//
//				half3 nrml;
//				half3 offsets;
//				Gerstner (
//								  offsets, nrml, v.vertex.xyz, vtxForAni,                     // offsets, nrml will be written
//								  _GAmplitude,                                                // amplitude
//								  _GFrequency,                                                // frequency
//								  _GSteepness,                                                // steepness
//								  _GSpeed,                                                    // speed
//								  _GDirectionAB,                                              // direction # 1, 2
//								  _GDirectionCD                                               // direction # 3, 4
//								  );
//
//				v.vertex.xyz += offsets;
//				v.normal = nrml;
//							  #endif
//
//				UNITY_SETUP_INSTANCE_ID(v);
//							  #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
//				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(os);
//							  #endif
//				TRANSFER_SHADOW_CASTER_NOPOS(o,opos)
//							  #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
//				o.tex = TRANSFORM_TEX(v.uv0, _MainTex);
//
//							  #ifdef _PARALLAXMAP
//				TANGENT_SPACE_ROTATION;
//				half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
//				o.tangentToWorldAndParallax[0].w = viewDirForParallax.x;
//				o.tangentToWorldAndParallax[1].w = viewDirForParallax.y;
//				o.tangentToWorldAndParallax[2].w = viewDirForParallax.z;
//							  #endif
//							  #endif
//			}
//
//
//			ENDCG
//		}
		Pass{
			Tags{"LightMode"="ForwardBase" }
			//Lighting On
			//ZWrite On
			//ZTest Off
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdadd_fullshadows
			//#pragma multi_compile_fwdbase
			#pragma multi_compile WATER_REFRACTIVE WATER_REFLECTIVE WATER_SIMPLE
			#pragma multi_compile GERSTNER_ON GERSTNER_OFF

			#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
			#define HAS_REFLECTION 1
			#endif
			#if defined (WATER_REFRACTIVE)
			#define HAS_REFRACTION 1
			#endif



			#include "UnityCG.cginc"
			//#include "UnityStandardCore.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			//#include "Tessellation.cginc"

			uniform half4 _WaveScale4;
			uniform half4 _WaveOffset;
			#if HAS_REFLECTION
			uniform float _ReflDistort;
			#endif
			#if HAS_REFRACTION
			uniform float _RefrDistort;
			#endif

			#if defined (GERSTNER_ON)
			#define WATER_VERTEX_DISPLACEMENT_ON
			#include "WaterInclude.cginc"
			uniform half4 _GAmplitude;
			uniform half4 _GFrequency;
			uniform half4 _GSteepness;
			uniform half4 _GSpeed;
			uniform half4 _GDirectionAB;
			uniform half4 _GDirectionCD;
			half _OffsetX, _OffsetZ, _Distance;
			half _WaveAmplitude;
			half _xImpact, _zImpact;
			#endif

			struct appdata {
				half4 vertex: POSITION;
			};

			struct v2f {
				half4 pos : SV_POSITION;
				half4 bumpuv : TEXCOORD0;
				half4 worldPos : TEXCOORD1;
				half3 worldNormal : TEXCOORD2;
				#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
				half4 ref : TEXCOORD3;
				#endif
				LIGHTING_COORDS(5, 6)
				//unityShadowCoord4 _LightCoord : TEXCOORD5;
				//unityShadowCoord4 _ShadowCoord : TEXCOORD6;
			};

			v2f vert(appdata v)
			{
				v2f o;

				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				#if defined (GERSTNER_ON)
				half3 vtxForAni = (o.worldPos).xzz;

				half3 nrml;
				half3 offsets;
				Gerstner (
					offsets, nrml, v.vertex.xyz, vtxForAni,                     // offsets, nrml will be written
					_GAmplitude,                                                // amplitude
					_GFrequency,                                                // frequency
					_GSteepness,                                                // steepness
					_GSpeed,                                                    // speed
					_GDirectionAB,                                              // direction # 1, 2
					_GDirectionCD                                               // direction # 3, 4
					);
				
				v.vertex.xyz += offsets;
				o.worldNormal = nrml;

				half offsetVert = v.vertex.x * v.vertex.x + v.vertex.z * v.vertex.z;
				half value = _WaveAmplitude  * 0.1 * sin(_Time.w * -2.5 + offsetVert * 0.4 + v.vertex.x * _OffsetX + v.vertex.z * _OffsetZ);
				if (sqrt(pow(o.worldPos.x - _xImpact, 2) + pow(o.worldPos.z - _zImpact, 2)) < _Distance) {
					v.vertex.y += value;
					o.worldNormal += value;
				}
				#else
				#endif

				// Finish transforming the vetex by applying the projection matrix
				o.pos = UnityObjectToClipPos(v.vertex);

				// scroll bump waves
				o.bumpuv = o.worldPos.xzxz * _WaveScale4 + _WaveOffset;

				#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
				o.ref = ComputeGrabScreenPos(o.pos);
				//TRANSFER_VERTEX_TO_FRAGMENT(o);
				o.worldPos.w = -UnityObjectToViewPos(v.vertex).z;
				#endif
				//o.projPos = o.ref;
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				//o._ShadowCoord = ComputeGrabScreenPos(o.pos);
				return o;
			}

			uniform half4 _WaterColor;

			#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
			sampler2D _ReflectionTex;
			#endif
			#if defined (WATER_REFLECTIVE) || defined (WATER_SIMPLE)
			sampler2D _ReflectiveColor;
			#endif
			#if defined (WATER_REFRACTIVE)
			uniform half _Transparency;
			sampler2D _RefractionTex;
			#endif
			#if defined (WATER_SIMPLE)
			uniform half4 _HorizonColor;
			#endif
			sampler2D _BumpMap;

			sampler2D _CameraDepthTexture;

			half4 frag(v2f i) : SV_Target
			{

				//-------------------------------------------
				//              UnityLight mainLight = MainLight();  
				//  
				//                //设置阴影的衰减系数  
				//                half atten = SHADOW_ATTENUATION(i);  
				//  
				//                //计算全局光照  
				//                half occlusion = Occlusion(i.ref.xy);  
				//                UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, mainLight);  
				//                
				//half atten = LIGHT_ATTENUATION(i);
				//UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz);
				//-------------------------------------------
				half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);

				// combine two scrolling bumpmaps into one
				half3 bump1 = UnpackNormal(tex2D(_BumpMap, i.bumpuv.xy)).rgb;
				half3 bump2 = UnpackNormal(tex2D(_BumpMap, i.bumpuv.wz)).rgb;
				half3 bump = half3(((bump1 + bump2) * 0.5).xy, 0) + i.worldNormal;
				bump = normalize(bump);

				#if HAS_REFLECTION
				half atten = LIGHT_ATTENUATION(i);
				half4 uv1 = i.ref; uv1.xy += bump.xz * _ReflDistort;
				half4 refl = half4(tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(uv1)).rgb, 0);
				half3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
				half3 halfDir = normalize(worldLightDir + viewDir);
				half3 specular = pow(max(0, dot(lerp(bump, half3(0,1,0), 0.5), halfDir)), 600) * atten;
				refl = refl + half4(specular * 4, 1);
				#endif

				#if HAS_REFRACTION
				// calculate depth
				half sceneZ = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);  
				half partZ = i.worldPos.w;
				half fade = saturate((sceneZ - partZ) / _Transparency); 

				half4 uv2 = i.ref; uv2.xy += bump.xz * _RefrDistort;
				half4 refr = half4(tex2Dproj(_RefractionTex, i.ref));
				refr = lerp(refr, _WaterColor, fade);
				#endif

				// final color is between refracted and reflected based on fresnel
				half4 color;

				half fresnel = 0.02 + 0.97*pow((1-dot(viewDir, bump)),2);
				#if defined(WATER_REFRACTIVE)
				fresnel *= fade;
				color = lerp(refr, refl, fresnel);
				#endif

				#if defined(WATER_REFLECTIVE)
				color = lerp(_WaterColor, refl, fresnel);
				#endif

				#if defined(WATER_SIMPLE)
				color = lerp(_WaterColor, _HorizonColor, fresnel);
				#endif

				#if HAS_REFLECTION
				//color = color * lerp(atten, 1, 0.98);
				return half4(atten, 0, 0, 1);
				#endif
				//return half4(fresnel, 0, 0, 1);
				//color = lerp(color, refr, 1-fade);
				//#if defined(WATER_REFRACTIVE)
				//return refr;
				//return half4(fresnel,0,0,1);
				//return half4(specular * 15, 1);
				//#endif
				
				//half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflect(-viewDir, i.worldNormal));
				// decode cubemap data into actual color
				//half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);

				//return half4(skyColor,1);
				return color;
				//return refl;
				//return half4(i.worldNormal, 1);
				//return half4(atten, 0, 0, 1);
			}
			ENDCG
		}
			      Pass
		      {
			          Tags {"LightMode"="ShadowCaster"}
			
			          ZWrite On ZTest LEqual
			
			
			          CGPROGRAM
			          #pragma vertex vert
			          #pragma fragment frag
			          #pragma target 3.0
			          #pragma multi_compile_shadowcaster
			          #pragma multi_compile_instancing // allow instanced shadow pass for most of the shaders
			          #pragma multi_compile GERSTNER_ON GERSTNER_OFF
			          #include "UnityCG.cginc"
			
			          #if defined (GERSTNER_ON)
			          #define WATER_VERTEX_DISPLACEMENT_ON
			          #include "WaterInclude.cginc"
			          uniform half4 _GAmplitude;
			          uniform half4 _GFrequency;
			          uniform half4 _GSteepness;
			          uniform half4 _GSpeed;
			          uniform half4 _GDirectionAB;
			          uniform half4 _GDirectionCD;
			          half _OffsetX, _OffsetZ, _Distance;
			          half _WaveAmplitude;
			          half _xImpact, _zImpact;
			          #endif
			
			          struct v2f {
				              V2F_SHADOW_CASTER;
				              UNITY_VERTEX_OUTPUT_STEREO
				          };
				
				          v2f vert( appdata_base v )
				          {
					              v2f o;
					              #if defined (GERSTNER_ON)
					              half3 worldPos = mul(unity_ObjectToWorld, v.vertex);
					              half3 vtxForAni = (worldPos).xzz;
					              
					              half3 nrml;
					              half3 offsets;
					              Gerstner(
					              offsets, nrml, v.vertex.xyz, vtxForAni,                     // offsets, nrml will be written
					              _GAmplitude,                                                // amplitude
					              _GFrequency,                                                // frequency
					              _GSteepness,                                                // steepness
					              _GSpeed,                                                    // speed
					              _GDirectionAB,                                              // direction # 1, 2
					              _GDirectionCD                                               // direction # 3, 4
					              );
					
					              v.vertex.xyz += offsets;
					              v.normal = nrml;
					              #endif
					              
					              UNITY_SETUP_INSTANCE_ID(v);
					              UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					              TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					              return o;
					          }
					
					          float4 frag( v2f i ) : SV_Target
					          {
						              SHADOW_CASTER_FRAGMENT(i)
						          }
						          ENDCG
						      }	
		//      UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

//						Pass {
//							Name "ShadowCaster"
//							Tags { "LightMode" = "ShadowCaster" }
//
//							ZWrite On ZTest LEqual
//							//ZWrite On
//
//							CGPROGRAM
//						#pragma target 3.0
//
//						// -------------------------------------
//
//
//						//#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
//						#pragma shader_feature _METALLICGLOSSMAP
//						#pragma shader_feature _PARALLAXMAP
//						#pragma multi_compile_shadowcaster
//						#pragma multi_compile_instancing
//						#pragma multi_compile GERSTNER_ON GERSTNER_OFF
//
//						// Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
//						//#pragma multi_compile _ LOD_FADE_CROSSFADE
//
//						#pragma vertex vert
//						#pragma fragment fragShadowCaster
//
//						#include "UnityStandardShadow.cginc"
//
//						#if defined (GERSTNER_ON)
//					  #define WATER_VERTEX_DISPLACEMENT_ON
//					  #include "WaterInclude.cginc"
//							uniform half4 _GAmplitude;
//							uniform half4 _GFrequency;
//							uniform half4 _GSteepness;
//							uniform half4 _GSpeed;
//							uniform half4 _GDirectionAB;
//							uniform half4 _GDirectionCD;
//							half _OffsetX, _OffsetZ, _Distance;
//							half _WaveAmplitude;
//							half _xImpact, _zImpact;
//					  #endif
//
//							void vert(VertexInput v,
//						  #ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
//								out VertexOutputShadowCaster o,
//						  #endif
//						  #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
//								out VertexOutputStereoShadowCaster os,
//						  #endif
//								out float4 opos : SV_POSITION)
//							{
//							  #if defined (GERSTNER_ON)
//								half3 worldPos = mul(unity_ObjectToWorld, v.vertex);
//								half3 vtxForAni = (worldPos).xzz;
//
//								half3 nrml;
//								half3 offsets;
//								Gerstner (
//								  offsets, nrml, v.vertex.xyz, vtxForAni,                     // offsets, nrml will be written
//								  _GAmplitude,                                                // amplitude
//								  _GFrequency,                                                // frequency
//								  _GSteepness,                                                // steepness
//								  _GSpeed,                                                    // speed
//								  _GDirectionAB,                                              // direction # 1, 2
//								  _GDirectionCD                                               // direction # 3, 4
//								  );
//
//								v.vertex.xyz += offsets;
//								v.normal = nrml;
//							  #endif
//
//								UNITY_SETUP_INSTANCE_ID(v);
//							  #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
//								UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(os);
//							  #endif
//								TRANSFER_SHADOW_CASTER_NOPOS(o,opos)
//							  #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
//								o.tex = TRANSFORM_TEX(v.uv0, _MainTex);
//
//							  #ifdef _PARALLAXMAP
//								TANGENT_SPACE_ROTATION;
//								half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
//								o.tangentToWorldAndParallax[0].w = viewDirForParallax.x;
//								o.tangentToWorldAndParallax[1].w = viewDirForParallax.y;
//								o.tangentToWorldAndParallax[2].w = viewDirForParallax.z;
//							  #endif
//							  #endif
//							}
//
//
//							ENDCG
//						}
					}
					//FallBack "Diffuse"
				}

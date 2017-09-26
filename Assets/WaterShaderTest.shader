Shader "Custom/WaterShaderTest" {
	Properties{
	    _Tess ("Tessellation", Range(1,32)) = 4
		_Transparency("Water transparency", Float) = 50.0
		[NoScaleOffset] _BumpMap("Normalmap ", 2D) = "bump" {}
		_WaveScale("Wave scale", Range(0.02,0.55)) = 0.118
        _ReflDistort ("Reflection distort", Range (0,1.5)) = 0.44
	    _RefrDistort ("Refraction distort", Range (0,1.5)) = 0.40
		WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (9,5,-7,-4)
		_WaterColor("Simple water color", COLOR) = (.172, .463, .435, 1)
		_HorizonColor("Simple water horizon color", COLOR) = (.172, .463, .435, 1)
		[HideInInspector] _ReflectionTex("Internal Reflection", 2D) = "Blue" {}
		[HideInInspector] _RefractionTex("Internal Refraction", 2D) = "Blue" {}

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
		Tags{ "WaterMode"="Refractive" "IgnoreProjector"="True" "RenderType"="Opaque"}// "LightMode"="ForwardBase"}
		Pass{
			Lighting On
			//ZWrite Off
			//ZTest Off
			CGPROGRAM
			#pragma target 4.6
			#pragma vertex vert tessellate:tessFixed
			#pragma fragment frag
			//#pragma tessellate:tessDistance
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
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#include "Tessellation.cginc"

			uniform fixed4 _WaveScale4;
			uniform fixed4 _WaveOffset;
            #if HAS_REFLECTION
            uniform float _ReflDistort;
            #endif
            #if HAS_REFRACTION
            uniform float _RefrDistort;
            #endif

			#if defined (GERSTNER_ON)
			#define WATER_VERTEX_DISPLACEMENT_ON
			#include "WaterInclude.cginc"
			uniform fixed4 _GAmplitude;
			uniform fixed4 _GFrequency;
			uniform fixed4 _GSteepness;
			uniform fixed4 _GSpeed;
			uniform fixed4 _GDirectionAB;
			uniform fixed4 _GDirectionCD;
            fixed _OffsetX, _OffsetZ, _Distance;
            fixed _WaveAmplitude;
            fixed _xImpact, _zImpact;
			#endif

			struct appdata {
				fixed4 vertex : POSITION;
			};

			struct v2f {
				fixed4 pos : POSITION;
				fixed4 bumpuv : TEXCOORD0;
				fixed4 worldPos : TEXCOORD1;
				fixed3 worldNormal : TEXCOORD2;
				#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
				fixed4 ref : TEXCOORD3;
				#endif
			};


            float _Tess;

            float4 tessDistance (appdata v0, appdata v1, appdata v2) {
                float minDist = 10.0;
                float maxDist = 25.0;
                return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
            }
            
            float4 tessFixed()
            {
                return _Tess;
            }


			v2f vert(appdata v)
			{
				v2f o;

				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				#if defined (GERSTNER_ON)
				fixed3 vtxForAni = (o.worldPos).xzz;

				fixed3 nrml;
				fixed3 offsets;
				Gerstner (
					offsets, nrml, v.vertex.xyz, vtxForAni,						// offsets, nrml will be written
					_GAmplitude,												// amplitude
					_GFrequency,												// frequency
					_GSteepness,												// steepness
					_GSpeed,													// speed
					_GDirectionAB,												// direction # 1, 2
					_GDirectionCD												// direction # 3, 4
					);
				
				v.vertex.xyz += offsets;
				o.worldNormal = nrml;

                fixed offsetVert = v.vertex.x * v.vertex.x + v.vertex.z * v.vertex.z;
                fixed value = _WaveAmplitude  * 0.1 * sin(_Time.w * -2.5 + offsetVert * 0.4 + v.vertex.x * _OffsetX + v.vertex.z * _OffsetZ);
                if (sqrt(pow(o.worldPos.x - _xImpact, 2) + pow(o.worldPos.z - _zImpact, 2)) < _Distance) {
                    v.vertex.y += value;
                    o.worldNormal += value;
                }
				#else
                o.worldNormal = fixed3(0,1,0);
				#endif

				// Finish transforming the vetex by applying the projection matrix
				o.pos = UnityObjectToClipPos(v.vertex);

				// scroll bump waves
				o.bumpuv = o.worldPos.xzxz * _WaveScale4 + _WaveOffset;

				#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
				o.ref = ComputeScreenPos(o.pos);
				//TRANSFER_VERTEX_TO_FRAGMENT(o);
				o.worldPos.w = -UnityObjectToViewPos(v.vertex).z;
				#endif
				//o.projPos = o.ref;
				return o;
			}

			uniform fixed4 _WaterColor;

			#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
			sampler2D _ReflectionTex;
			#endif
			#if defined (WATER_REFLECTIVE) || defined (WATER_SIMPLE)
			sampler2D _ReflectiveColor;
			#endif
			#if defined (WATER_REFRACTIVE)
			uniform fixed _Transparency;
			sampler2D _RefractionTex;
			#endif
			#if defined (WATER_SIMPLE)
			uniform fixed4 _HorizonColor;
			#endif
			sampler2D _BumpMap;

			sampler2D _CameraDepthTexture;

			fixed4 frag(v2f i) : COLOR
			{
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);

				// combine two scrolling bumpmaps into one
				fixed3 bump1 = UnpackNormal(tex2D(_BumpMap, i.bumpuv.xy)).rgb;
				fixed3 bump2 = UnpackNormal(tex2D(_BumpMap, i.bumpuv.wz)).rgb;
				fixed3 bump = fixed3(((bump1 + bump2) * 0.5).xy, 0) + i.worldNormal;
				bump = normalize(bump);

				#if HAS_REFLECTION
				fixed atten = LIGHT_ATTENUATION(i);
				fixed4 uv1 = i.ref; uv1.xy += bump.xz * _ReflDistort;
				fixed4 refl = fixed4(tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(uv1)).rgb, 0);
				fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
				fixed3 halfDir = normalize(worldLightDir + viewDir);
				fixed3 specular = pow(max(0, dot(lerp(bump, fixed3(0,1,0), 0.5), halfDir)), 600) * atten;
				refl = refl + fixed4(specular * 4, 1);
				#endif

				#if HAS_REFRACTION
				// calculate depth
				fixed4 temp = i.ref; temp.z = i.worldPos.w;
                temp.xy += bump.xz * _RefrDistort;
				fixed sceneZ = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(temp)).r);  
				//fixed sceneZ = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.ref)).r);  
				fixed partZ = i.worldPos.w;
				fixed fade = saturate((sceneZ - partZ) / _Transparency); 

				fixed4 uv2 = i.ref; uv2.xy += bump.xz * _RefrDistort;
				fixed4 refr = fixed4(tex2Dproj(_RefractionTex, UNITY_PROJ_COORD(uv2)).rgb, 1);
                //refr = refr * _WaterColor;
				refr = lerp(refr, _WaterColor, fade);
				#endif

				// final color is between refracted and reflected based on fresnel
				fixed4 color;

                fixed fresnel = 0.02 + 0.97*pow((1-dot(viewDir, bump)),2);
				//fixed fresnel = lerp(1, pow(1 - dot(viewDir, bump), 5), 0.5);// * fade;
				//fixed fresnel = pow(1 - saturate(dot(viewDir, bump)), 5);// * fade;
				#if defined(WATER_REFRACTIVE)
				//fixed fresnel = lerp(1, pow(1 - saturate(dot(viewDir, bump)), 5), 0.7);// * fade;
                fresnel *= fade;
				color = lerp(refr, refl, fresnel);
				#endif

				#if defined(WATER_REFLECTIVE)
				color = lerp(_WaterColor, refl, fresnel);
				#endif

				#if defined(WATER_SIMPLE)
				color = lerp(_WaterColor, _HorizonColor, fresnel);
				#endif

				//#if HAS_REFLECTION
				//color = color * lerp(atten, 1, 0.98);
				//#endif
				//return fixed4(fresnel, 0, 0, 1);
				//return refl;
				//color = lerp(color, refr, 1-fade);
				#if defined(WATER_REFRACTIVE)
				return fixed4(fade,0,0,1);
                //return fixed4(specular * 15, 1);
				#endif

				return color;
				//return refl;
				//return fixed4(i.worldNormal, 1);
				//return fixed4(atten, 0, 0, 1);
			}
			ENDCG

		}
	}
	//FallBack "VertexLit"
}

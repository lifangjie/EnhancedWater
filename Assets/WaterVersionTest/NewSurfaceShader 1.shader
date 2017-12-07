Shader "Custom/TestGenerate" {
	Properties {
		_Transparency("Water transparency", Float) = 50.0
		[NoScaleOffset] _BumpMap("Normalmap ", 2D) = "bump" {}
		_WaveScale("Wave scale", Range(0.02,0.55)) = 0.118
		_BumpStrength ("BumpStrength", Range(0,10)) = 3
		WaveSpeed("Wave speed (map1 x,y; map2 x,y)", Vector) = (9,5,-7,-4)
		[NoScaleOffset] _VerticesTex("Vertices Texture", 2D) = "Black" {}
		[NoScaleOffset] _RefractionTex("Internal Refraction", 2D) = "Black" {}
		[NoScaleOffset] _DepthTex("Internal Depth", 2D) = "Black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque"}
		LOD 200
		
		
	// ------------------------------------------------------------
	// Surface shader code generated out of a CGPROGRAM block:
	

	// ---- deferred shading pass:
	Pass {
		Name "DEFERRED"
		Tags { "LightMode" = "Deferred" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 5.0
#pragma exclude_renderers nomrt
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_prepassfinal
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: no
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: YES
// 0 texcoords actually used
#define UNITY_PASS_DEFERRED
#include "UnityCG.cginc"
#include "Lighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 14 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Custom noinstancing fullforwardshadows vertex:vert// tessellate:tessDistance
		////#pragma multi_compile_noinstancing
		////#pragma hull SubDToBezierHS

		//#pragma target 5.0
		//#include "UnityCG.cginc"
		//#include "UnityPBSLighting.cginc"

		struct appdata_custom {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;
		};

		struct SurfaceOutputCustom
		{
			fixed3 Albedo;  // diffuse color
			fixed3 Specular;
			fixed3 Normal;  // tangent space normal, if written
			half Emission;
			half Smoothness;
			half Occlusion; // occlusion (default 1)
			fixed Alpha;// alpha for transparencies
			fixed4 bump;
		};

//		inline half4 LightingCustom (SurfaceOutputCustom s, half3 viewDir, UnityGI gi)
//		{
//			half4 c = 0;
//			return c;
//		}

		half _BumpStrength;
		half _Transparency;
		sampler2D _RefractionTex;
		sampler2D _DepthTex;

		inline void LightingCustom_GI (
			SurfaceOutputCustom s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
			gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
			#else
			Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, s.Specular);
			gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
			#endif
		}

		uint _HeightMapSize;
		sampler2D _VerticesTex;
		sampler2D _NormalsTex;

		struct Input {
			float4 grabScreenPos;
			float viewZ;
			float4 bumpuv;
		};

		inline half4 LightingCustom_Deferred (SurfaceOutputCustom s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
		{
			half oneMinusReflectivity;
			//s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (1, 0.945, /*out*/ oneMinusReflectivity);
			
			half2 bump = (s.bump.xy + s.bump.zw) * 0.5;
			s.Normal = normalize(half3(bump.x, 1.3, bump.y) + s.Normal);

            //half grazingTerm = saturate(0.89 + (1-oneMinusReflectivity));
            //half fresnel = FresnelTerm (0.02, saturate(dot(s.Normal, viewDir)));
            half fresnel = 0.98 - 0.98 * Pow5 (1 - saturate(dot(s.Normal, viewDir)));
    
			//half4 c = UNITY_BRDF_PBS (1, 0.945, 0.055, 0.89, s.Normal, viewDir, gi.light, gi.indirect);

			UnityStandardData data;
			data.diffuseColor   = 0.11;
			data.diffuseColor   = 0;
			data.occlusion      = 1;
			data.specularColor  = 0.02;
			//data.occlusion      = 0;
			//data.specularColor  = 0;
			
            //half3 h = normalize (gi.light.dir + viewDir);
            //float nh = max (0, dot (s.Normal, h));
            //data.specularColor = pow (nh, 128.0) * 1;
			data.smoothness     = 1;
			data.normalWorld    = s.Normal;

			UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

			//half4 emission = half4(s.Emission + c.rgb, 1);

			//return emission;
			return half4(s.Albedo * fresnel, 1);
			//return half4(-log2(1-fresnel), 0, 0, 1);
			//return half4(fresnel, 0, 0, 1);
			return 0;
			
		}
		
		sampler2D _BumpMap;
		uniform half4 _WaveScale4;
		uniform half4 _WaveOffset;
		void vert(inout appdata_custom v, out Input o)
		{
			half4 uv = half4(v.texcoord.xy, 0, 0);
			float2 slope = tex2Dlod(_NormalsTex, uv).xy * _BumpStrength;
			v.normal = float3(0,1,0);
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.grabScreenPos = ComputeGrabScreenPos(UnityObjectToClipPos(v.vertex));
			o.viewZ = -UnityObjectToViewPos(v.vertex).z;
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
			o.bumpuv = worldPos.xzxz * _WaveScale4 + _WaveOffset;
		}

		void surf (Input IN, inout SurfaceOutputCustom o) {
			// Albedo comes from a texture tinted by color
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.bump.xy = UnpackNormal(tex2D(_BumpMap, IN.bumpuv.xy)).xy;
			o.bump.zw = UnpackNormal(tex2D(_BumpMap, IN.bumpuv.wz)).xy;
			fixed3 refr = tex2Dproj(_RefractionTex, IN.grabScreenPos);
			half sceneZ = LinearEyeDepth (tex2Dproj(_DepthTex, UNITY_PROJ_COORD(IN.grabScreenPos)).r);
			half depth = saturate((sceneZ - IN.viewZ) / _Transparency);
			o.Albedo = refr;
			// Metallic and smoothness come from slider variables
			o.Specular = 0.945;//fixed3(9/255, 12/255, 14/255);
			//o.Occlusion = depth;
			o.Smoothness = 0.99;
			o.Alpha = 1;
		}
		

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  half3 worldNormal : TEXCOORD0;
  float3 worldPos : TEXCOORD1;
  float4 custompack0 : TEXCOORD2; // grabScreenPos
  float custompack1 : TEXCOORD3; // viewZ
  float4 custompack2 : TEXCOORD4; // bumpuv
  float4 lmap : TEXCOORD5;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH
    half3 sh : TEXCOORD6; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD6;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_custom v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.grabScreenPos;
  o.custompack1.x = customInputData.viewZ;
  o.custompack2.xyzw = customInputData.bumpuv;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos = worldPos;
  o.worldNormal = worldNormal;
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.grabScreenPos.x = 1.0;
  surfIN.viewZ.x = 1.0;
  surfIN.bumpuv.x = 1.0;
  surfIN.grabScreenPos = IN.custompack0.xyzw;
  surfIN.viewZ = IN.custompack1.x;
  surfIN.bumpuv = IN.custompack2.xyzw;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputCustom o = (SurfaceOutputCustom)0;
  #else
  SurfaceOutputCustom o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  half atten = 1;

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
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
    giInput.ambient = IN.sh;
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
  LightingCustom_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingCustom_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, float3(0, 0, 0));
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}

ENDCG

}

	// ---- meta information extraction pass:
	Pass {
		Name "Meta"
		Tags { "LightMode" = "Meta" }
		Cull Off

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 5.0
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma skip_variants INSTANCING_ON
#pragma shader_feature EDITOR_VISUALIZATION

#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: no
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_META
#include "UnityCG.cginc"
#include "Lighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 14 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Custom noinstancing fullforwardshadows vertex:vert// tessellate:tessDistance
		////#pragma multi_compile_noinstancing
		////#pragma hull SubDToBezierHS

		//#pragma target 5.0
		//#include "UnityCG.cginc"
		//#include "UnityPBSLighting.cginc"

		struct appdata_custom {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;
		};

		struct SurfaceOutputCustom
		{
			fixed3 Albedo;  // diffuse color
			fixed3 Specular;
			fixed3 Normal;  // tangent space normal, if written
			half Emission;
			half Smoothness;
			half Occlusion; // occlusion (default 1)
			fixed Alpha;// alpha for transparencies
			fixed4 bump;
		};

//		inline half4 LightingCustom (SurfaceOutputCustom s, half3 viewDir, UnityGI gi)
//		{
//			half4 c = 0;
//			return c;
//		}

		half _BumpStrength;
		half _Transparency;
		sampler2D _RefractionTex;
		sampler2D _DepthTex;

		inline void LightingCustom_GI (
			SurfaceOutputCustom s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
			gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
			#else
			Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, s.Specular);
			gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
			#endif
		}

		uint _HeightMapSize;
		sampler2D _VerticesTex;
		sampler2D _NormalsTex;

		struct Input {
			float4 grabScreenPos;
			float viewZ;
			float4 bumpuv;
		};

		inline half4 LightingCustom_Deferred (SurfaceOutputCustom s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
		{
			half oneMinusReflectivity;
			//s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (1, 0.945, /*out*/ oneMinusReflectivity);
			
			half2 bump = (s.bump.xy + s.bump.zw) * 0.5;
			s.Normal = normalize(half3(bump.x, 1.3, bump.y) + s.Normal);

            //half grazingTerm = saturate(0.89 + (1-oneMinusReflectivity));
            //half fresnel = FresnelTerm (0.02, saturate(dot(s.Normal, viewDir)));
            half fresnel = 0.98 - 0.98 * Pow5 (1 - saturate(dot(s.Normal, viewDir)));
    
			//half4 c = UNITY_BRDF_PBS (1, 0.945, 0.055, 0.89, s.Normal, viewDir, gi.light, gi.indirect);

			UnityStandardData data;
			data.diffuseColor   = 0.11;
			data.diffuseColor   = 0;
			data.occlusion      = 1;
			data.specularColor  = 0.02;
			//data.occlusion      = 0;
			//data.specularColor  = 0;
			
            //half3 h = normalize (gi.light.dir + viewDir);
            //float nh = max (0, dot (s.Normal, h));
            //data.specularColor = pow (nh, 128.0) * 1;
			data.smoothness     = 1;
			data.normalWorld    = s.Normal;

			UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

			//half4 emission = half4(s.Emission + c.rgb, 1);

			//return emission;
			return half4(s.Albedo * fresnel, 1);
			//return half4(-log2(1-fresnel), 0, 0, 1);
			//return half4(fresnel, 0, 0, 1);
			return 0;
			
		}
		
		sampler2D _BumpMap;
		uniform half4 _WaveScale4;
		uniform half4 _WaveOffset;
		void vert(inout appdata_custom v, out Input o)
		{
			half4 uv = half4(v.texcoord.xy, 0, 0);
			float2 slope = tex2Dlod(_NormalsTex, uv).xy * _BumpStrength;
			v.normal = float3(0,1,0);
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.grabScreenPos = ComputeGrabScreenPos(UnityObjectToClipPos(v.vertex));
			o.viewZ = -UnityObjectToViewPos(v.vertex).z;
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
			o.bumpuv = worldPos.xzxz * _WaveScale4 + _WaveOffset;
		}

		void surf (Input IN, inout SurfaceOutputCustom o) {
			// Albedo comes from a texture tinted by color
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.bump.xy = UnpackNormal(tex2D(_BumpMap, IN.bumpuv.xy)).xy;
			o.bump.zw = UnpackNormal(tex2D(_BumpMap, IN.bumpuv.wz)).xy;
			fixed3 refr = tex2Dproj(_RefractionTex, IN.grabScreenPos);
			half sceneZ = LinearEyeDepth (tex2Dproj(_DepthTex, UNITY_PROJ_COORD(IN.grabScreenPos)).r);
			half depth = saturate((sceneZ - IN.viewZ) / _Transparency);
			o.Albedo = refr;
			// Metallic and smoothness come from slider variables
			o.Specular = 0.945;//fixed3(9/255, 12/255, 14/255);
			//o.Occlusion = depth;
			o.Smoothness = 0.99;
			o.Alpha = 1;
		}
		
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float3 worldPos : TEXCOORD0;
  float4 custompack0 : TEXCOORD1; // grabScreenPos
  float custompack1 : TEXCOORD2; // viewZ
  float4 custompack2 : TEXCOORD3; // bumpuv
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_custom v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.grabScreenPos;
  o.custompack1.x = customInputData.viewZ;
  o.custompack2.xyzw = customInputData.bumpuv;
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos = worldPos;
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.grabScreenPos.x = 1.0;
  surfIN.viewZ.x = 1.0;
  surfIN.bumpuv.x = 1.0;
  surfIN.grabScreenPos = IN.custompack0.xyzw;
  surfIN.viewZ = IN.custompack1.x;
  surfIN.bumpuv = IN.custompack2.xyzw;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputCustom o = (SurfaceOutputCustom)0;
  #else
  SurfaceOutputCustom o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
  metaIN.SpecularColor = o.Specular;
  return UnityMetaFragment(metaIN);
}

ENDCG

}

	// ---- end of surface shader generated code

#LINE 151

	}
	//FallBack "Diffuse"
}

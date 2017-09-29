Shader "Hidden/RenderDepth" {
	SubShader {
		Tags { "RenderType"="Opaque" }
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

            #pragma fragmentoption ARB_precision_hint_nicest

			struct v2f {
				float4 pos : SV_POSITION;
				float2 depth : TEXCOORD0;
			};

			v2f vert (appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.depth = o.pos.zw;
				return o;
			}

			float4 frag(v2f i) : COLOR {
				#if defined(SHADER_API_GLES)
				return ((i.depth.x/i.depth.y + 1) * 0.5);
				#else
				return i.depth.x/i.depth.y;
				#endif
			}
			ENDCG
		}
	}
}
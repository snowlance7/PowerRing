Shader "Hidden/FXV/FXVPostprocessBlitAdd"
{
	Properties 
    {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_ColorMultiplier ("ColorMultiplier", Range(0,5)) = 1.0
	}
	SubShader 
    {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 300

        Blend One One
		//Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off
        ZTest Off

		Pass // 0 - Default
        {
            HLSLPROGRAM

            #define FXV_SHIELD_HDRP

#if defined(FXV_SHIELD_URP)
            //https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@13.0/manual/renderer-features/how-to-fullscreen-blit-in-xr-spi.html
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#else
            #include "UnityCG.cginc"
#endif

			#include "../../Shaders/FXVShieldPostprocess.cginc"

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            struct appdata
            {
                FXV_SHIELD_POSTPROCESS_APPDATA
            };

            struct v2f
            {
                FXV_SHIELD_POSTPROCESS_V2F_COORDS
            };

            v2f vert(appdata v)
            {
                v2f o;

                FXV_SHIELD_POSTPROCESS_VERTEX_DEFAULT(v, o);

                return o;
            }

            FXV_DEFINE_SCREEN_TEXTURE(_MainTex);

			float _ColorMultiplier;

            float4 frag (v2f i) : SV_Target
            {
                FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i);

                float4 tex = FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, i.uv);

                return tex * pow(abs(_ColorMultiplier), 1.0/2.2);
            }
            ENDHLSL
        }

        Pass // 1 - SSGI
        {
            HLSLPROGRAM

            #define FXV_SHIELD_HDRP

#if defined(FXV_SHIELD_URP)
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#else
            #include "UnityCG.cginc"
#endif

            #include "../../Shaders/FXVShieldPostprocess.cginc"

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            struct appdata
            {
                FXV_SHIELD_POSTPROCESS_APPDATA
            };

            struct v2f
            {
                FXV_SHIELD_POSTPROCESS_V2F_COORDS
            };

            v2f vert(appdata v)
            {
                v2f o;

                FXV_SHIELD_POSTPROCESS_VERTEX_DEFAULT(v, o);

                return o;
            }

            sampler2D _MainTex;
            sampler2D _SSGIBuffer;

            float _ColorMultiplier;

            float4 frag(v2f i) : SV_Target
            {
                FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i);

                return tex2D(_MainTex, i.uv) * pow(abs(_ColorMultiplier), 1.0 / 2.2) + tex2D(_SSGIBuffer, i.uv);
            }
            ENDHLSL
        }
	}
	FallBack "Diffuse"
}

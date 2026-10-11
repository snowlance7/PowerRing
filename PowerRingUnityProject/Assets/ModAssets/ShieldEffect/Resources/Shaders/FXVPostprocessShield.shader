Shader "Hidden/FXV/FXVPostprocessShield" 
{
	Properties 
    {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 300
	
		Blend Off
		ZWrite Off


        // 0: Gaussian
		Pass
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

            v2f vert (appdata v)
            {
                v2f o;

                FXV_SHIELD_POSTPROCESS_VERTEX_DEFAULT(v, o);

                return o;
            }

            FXV_DEFINE_SCREEN_TEXTURE(_MainTex);
            uniform float4 _MainTex_TexelSize;

            uniform float GAUSSIAN_COEFF_H[16];
            uniform int GAUSSIAN_KERNEL_RADIUS_H;
            uniform float GAUSSIAN_COEFF_V[16];
            uniform int GAUSSIAN_KERNEL_RADIUS_V;

            uniform float GAUSSIAN_TEXEL_SIZE_H;
            uniform float GAUSSIAN_TEXEL_SIZE_V;

            float4 frag (v2f i) : SV_Target
            {
                FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i);

            	float4 finalColor = float4(0.0, 0.0, 0.0, 0.0);

                float texelSize = _MainTex_TexelSize.x * GAUSSIAN_TEXEL_SIZE_H;
				
                int idx = 0;
                float2 offsetUV;
                for (idx = -GAUSSIAN_KERNEL_RADIUS_H; idx <= GAUSSIAN_KERNEL_RADIUS_H; idx++)
                {
                    offsetUV = i.uv + float2(idx, 0.0) * texelSize;
                    finalColor += FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, offsetUV) * GAUSSIAN_COEFF_H[idx + GAUSSIAN_KERNEL_RADIUS_H];
                }

                texelSize = _MainTex_TexelSize.x * GAUSSIAN_TEXEL_SIZE_V;

                for (idx = -GAUSSIAN_KERNEL_RADIUS_V; idx <= GAUSSIAN_KERNEL_RADIUS_V; idx++)
                {
                    offsetUV = i.uv + float2(0.0, idx) * texelSize;
                    finalColor += FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, offsetUV) * GAUSSIAN_COEFF_V[idx + GAUSSIAN_KERNEL_RADIUS_V];
                }

                return finalColor;
            }
            ENDHLSL
        }

        
        // 1: Box
        Pass
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

            uniform float4 _MainTex_TexelSize;

            FXV_DEFINE_SCREEN_TEXTURE(_MainTex);

            half3 Sample (float2 uv) 
            {
			    return FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, uv).rgb;
		    }

            half3 SampleBox (float2 uv) 
            {
			    float4 o = _MainTex_TexelSize.xxxx * float2(-1, 1).xxyy;
			    half3 s =
				    Sample(uv + o.xy) + Sample(uv + o.zy) +
				    Sample(uv + o.xw) + Sample(uv + o.zw);
			    return s * 0.25f;
		    }

            float4 frag (v2f i) : SV_Target
            {
                FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i);

            	half4 finalColor = half4(SampleBox(i.uv), 1);

                return finalColor;
            }
            ENDHLSL
        }

        // 2: Gaussian Horizontal
		Pass
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

            uniform float4 _MainTex_TexelSize;

            FXV_DEFINE_SCREEN_TEXTURE(_MainTex);

            uniform float GAUSSIAN_COEFF_H[16];
            uniform int GAUSSIAN_KERNEL_RADIUS_H;
            uniform float GAUSSIAN_TEXEL_SIZE_H;

            float4 frag (v2f i) : SV_Target
            {
                FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i);

            	float4 finalColor = float4(0.0, 0.0, 0.0, 0.0);

                float texelSize = _MainTex_TexelSize.x * GAUSSIAN_TEXEL_SIZE_H;
				
                int idx = 0;
                float2 offsetUV;
                for (idx = -GAUSSIAN_KERNEL_RADIUS_H; idx <= GAUSSIAN_KERNEL_RADIUS_H; idx++)
                {
                    offsetUV = i.uv + float2(idx, 0.0) * texelSize;
                    finalColor += FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, offsetUV) * GAUSSIAN_COEFF_H[idx + GAUSSIAN_KERNEL_RADIUS_H];
                }

                return finalColor;
            }
            ENDHLSL
        }

        // 3: Gaussian Vertical
		Pass
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

            uniform float4 _MainTex_TexelSize;

            FXV_DEFINE_SCREEN_TEXTURE(_MainTex);

            uniform float GAUSSIAN_COEFF_V[16];
            uniform int GAUSSIAN_KERNEL_RADIUS_V;
            uniform float GAUSSIAN_TEXEL_SIZE_V;

            float4 frag (v2f i) : SV_Target
            {
                FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i);

            	float4 finalColor = float4(0.0, 0.0, 0.0, 0.0);

                float texelSize = _MainTex_TexelSize.x * GAUSSIAN_TEXEL_SIZE_V;
				
                int idx = 0;
                float2 offsetUV;
                for (idx = -GAUSSIAN_KERNEL_RADIUS_V; idx <= GAUSSIAN_KERNEL_RADIUS_V; idx++)
                {
                    offsetUV = i.uv + float2(0.0, idx) * texelSize;
                    finalColor += FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, offsetUV) * GAUSSIAN_COEFF_V[idx + GAUSSIAN_KERNEL_RADIUS_V];
                }

                return finalColor;
            }
            ENDHLSL
        }

        // 4: Dilate
        Pass
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

            uniform float4 _MainTex_TexelSize;

            FXV_DEFINE_SCREEN_TEXTURE(_MainTex);
            FXV_DEFINE_SCREEN_TEXTURE(_LightSource);

            half3 Sample(float2 uv)
            {
                return FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, uv).rgb;
            }

            half3 SampleBox(float2 uv)
            {
                float4 o = _MainTex_TexelSize.xxxx * float2(-1, 1).xxyy;
                half3 s =
                    max(max(Sample(uv + o.xy), Sample(uv + o.zy)),
                    max(Sample(uv + o.xw), Sample(uv + o.zw)));
                return s;
            }

            float4 frag(v2f i) : SV_Target
            {
                FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i);

                //half4 finalColor = half4(SampleBox(i.uv), 1);
                float2 _min = float2(0, 0);
                float2 _max = float2(1, 1);

                float2 _PixelOffset = _MainTex_TexelSize.xy;

                //get the color of 8 neighbour pixel
                float4 U = FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, clamp(i.uv + float2(0, _PixelOffset.y), _min, _max));
                float4 UR = FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, clamp(i.uv + float2(_PixelOffset.x, _PixelOffset.y), _min, _max));
                float4 R = FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, clamp(i.uv + float2(_PixelOffset.x, 0), _min, _max));
                float4 DR = FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, clamp(i.uv + float2(_PixelOffset.x, -_PixelOffset.y), _min, _max));
                float4 D = FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, clamp(i.uv + float2(0, -_PixelOffset.y), _min, _max));
                float4 DL = FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, clamp(i.uv + float2(-_PixelOffset.x, -_PixelOffset.y), _min, _max));
                float4 L = FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, clamp(i.uv + float2(-_PixelOffset.x, 0), _min, _max));
                float4 UL = FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, clamp(i.uv + float2(-_PixelOffset.x, _PixelOffset.y), _min, _max));

                //add all colors up to one final color
                float4 finalColor = U + UR + R + DR + D + DL + L + UL;

                return finalColor;
            }
            ENDHLSL
        }

        // 5: SSGI - sample
        Pass
        {
            HLSLPROGRAM

            #define FXV_SHIELD_HDRP

            #pragma multi_compile_local FXV_NUM_SAMPLES_LOW FXV_NUM_SAMPLES_MEDIUM FXV_NUM_SAMPLES_HIGH

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
                float4 screenPos : TEXCOORD1;
                float3 worldDirection : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;

                FXV_SHIELD_POSTPROCESS_VERTEX_DEFAULT(v, o);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.worldDirection.xyz = _FXV_PostprocessWorldViewRay(o.uv);

                return o;
            }

            uniform float4 _MainTex_TexelSize;
            FXV_DEFINE_SCREEN_TEXTURE(_MainTex);

            float4 frag(v2f i) : SV_Target
            {
                FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i);

                float4 light = _FXV_CalculateLightLoop(FXV_SCREEN_TEXTURE_ARGS(_MainTex), i.screenPos, i.worldDirection, i.uv);

                return light;
            }
            ENDHLSL
        }

        // 6: Denoise
        Pass
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
                float4 screenPos : TEXCOORD1;
                float3 worldDirection : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;

                FXV_SHIELD_POSTPROCESS_VERTEX_DEFAULT(v, o);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.worldDirection.xyz = _FXV_PostprocessWorldViewRay(o.uv);

                return o;
            }

            uniform float4 _MainTex_TexelSize;
            FXV_DEFINE_SCREEN_TEXTURE(_MainTex);

            float4 frag(v2f i) : SV_Target
            {
                FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i);

                float2 offset[8] = { float2(-1.0, 0.0f), float2(1.0, 0.0f), float2(0.0, 1.0f), float2(0.0, -1.0f), float2(-0.70710678118, 0.70710678118), float2(0.70710678118, -0.70710678118), float2(0.70710678118, 0.70710678118), float2(-0.70710678118, -0.70710678118) };
                float kernel[8] = { 0.125, 0.125, 0.125, 0.125, 0.125, 0.125, 0.125, 0.125 };

                float4 finalColor = _FXV_Wavelet(FXV_SCREEN_TEXTURE_ARGS(_MainTex), FXV_SCREEN_TEXTURE_ARGS(UNITY_NORMALS_BUFFER), i.uv, i.screenPos, i.worldDirection, offset, kernel);

                return finalColor;
            }
            ENDHLSL
        }

        // 7: Box + SSGI
        Pass
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
                float4 screenPos : TEXCOORD1;
                float3 worldDirection : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;

                FXV_SHIELD_POSTPROCESS_VERTEX_DEFAULT(v, o);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.worldDirection.xyz = _FXV_PostprocessWorldViewRay(o.uv);

                return o;
            }

            uniform float4 _MainTex_TexelSize;
            FXV_DEFINE_SCREEN_TEXTURE(_MainTex);
            FXV_DEFINE_SCREEN_TEXTURE(_SSGIBuffer);

            half3 Sample (float2 uv) 
            {
			    return FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, uv).rgb;
		    }

            half3 SampleBox (float2 uv) 
            {
			    float4 o = _MainTex_TexelSize.xxxx * float2(-1, 1).xxyy;
			    half3 s =
				    Sample(uv + o.xy) + Sample(uv + o.zy) +
				    Sample(uv + o.xw) + Sample(uv + o.zw);
			    return s * 0.25f;
		    }

            float4 frag (v2f i) : SV_Target
            {
                FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i);

            	half4 finalColor = half4(SampleBox(i.uv), 1);

                return finalColor + FXV_SAMPLE_SCREEN_TEXTURE(_SSGIBuffer, i.uv);
            }
            ENDHLSL
        }

        // 8: Simple
        Pass
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

            uniform float4 _MainTex_TexelSize;
            FXV_DEFINE_SCREEN_TEXTURE(_MainTex);

            half3 Sample(float2 uv)
            {
                return FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, uv).rgb;
            }

            float4 frag(v2f i) : SV_Target
            {
                FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i);

                half4 finalColor = half4(Sample(i.uv), 1);

                return finalColor;
            }
            ENDHLSL
        }

        // 9: Simple + SSGI
        Pass
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
                float4 screenPos : TEXCOORD1;
                float3 worldDirection : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;

                FXV_SHIELD_POSTPROCESS_VERTEX_DEFAULT(v, o);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.worldDirection.xyz = _FXV_PostprocessWorldViewRay(o.uv);

                return o;
            }

            uniform float4 _MainTex_TexelSize;
            FXV_DEFINE_SCREEN_TEXTURE(_MainTex);
            FXV_DEFINE_SCREEN_TEXTURE(_SSGIBuffer);

            half3 Sample(float2 uv)
            {
                return FXV_SAMPLE_SCREEN_TEXTURE(_MainTex, uv).rgb;
            }

            float4 frag(v2f i) : SV_Target
            {
                FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i);

                half4 finalColor = half4(Sample(i.uv), 1);

                return finalColor + FXV_SAMPLE_SCREEN_TEXTURE(_SSGIBuffer, i.uv);
            }
            ENDHLSL
        }

	}
}

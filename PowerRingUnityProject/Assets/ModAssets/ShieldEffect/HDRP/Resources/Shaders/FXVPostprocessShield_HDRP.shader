Shader "Hidden/FXV/FXVPostprocessShield_HDRP"
{
    HLSLINCLUDE

        #pragma target 4.5
        #pragma editor_sync_compilation
        #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
        #pragma multi_compile _ DISABLE_TEXTURE2D_X_ARRAY
        #pragma multi_compile _ BLIT_SINGLE_SLICE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"
        #include "../../Shaders/FXVShieldEffectHDRP.cginc"
        #include "../../Shaders/FXVShieldPostprocessHDRP.cginc" 
    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }

        // 0: Gaussian
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragGaussian

                uniform float GAUSSIAN_COEFF_H[16];
                uniform int GAUSSIAN_KERNEL_RADIUS_H;
                uniform float GAUSSIAN_COEFF_V[16];
                uniform int GAUSSIAN_KERNEL_RADIUS_V;

                uniform float GAUSSIAN_TEXEL_SIZE_H;
                uniform float GAUSSIAN_TEXEL_SIZE_V;

#if UNITY_VERSION < 202320
                float4 _BlitTexture_TexelSize;
#endif

                float4 Sample (float2 uv) 
                {
			        return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
		        }

                float4 FragGaussian(Varyings input) : SV_Target
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                    float4 finalColor = float4(0.0, 0.0, 0.0, 0.0);

                    float texelSize = _BlitTexture_TexelSize.x * GAUSSIAN_TEXEL_SIZE_H;
				
                    int idx = 0;
                    float2 offsetUV;
                    for (idx = -GAUSSIAN_KERNEL_RADIUS_H; idx <= GAUSSIAN_KERNEL_RADIUS_H; idx++)
                    {
                        offsetUV = input.texcoord.xy + float2(idx, 0.0) * texelSize;
                        finalColor += Sample(offsetUV) * GAUSSIAN_COEFF_H[idx + GAUSSIAN_KERNEL_RADIUS_H];
                    }

                    texelSize = _BlitTexture_TexelSize.x * GAUSSIAN_TEXEL_SIZE_V;

                    for (idx = -GAUSSIAN_KERNEL_RADIUS_V; idx <= GAUSSIAN_KERNEL_RADIUS_V; idx++)
                    {
                        offsetUV = input.texcoord.xy + float2(0.0, idx) * texelSize;
                        finalColor += Sample(offsetUV) * GAUSSIAN_COEFF_V[idx + GAUSSIAN_KERNEL_RADIUS_V];
                    }

                    return finalColor;
                }
            ENDHLSL
        }

        // 1: Box
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragBox

#if UNITY_VERSION < 202320
                float4 _BlitTexture_TexelSize;
#endif

                float4 Sample (float2 uv) 
                {
			        return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
		        }

                float4 SampleBox (float2 uv) 
                {
			        float4 o = float2(-1, 1).xxyy * _BlitTexture_TexelSize.xxxx;
			        float4 s =
				        Sample(uv + o.xy) + Sample(uv + o.zy) +
				        Sample(uv + o.xw) + Sample(uv + o.zw);
			        return s * 0.25f;
		        }
               
                float4 FragBox(Varyings input) : SV_Target
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                    return float4(SampleBox(input.texcoord.xy).rgb, 1);
                }
            ENDHLSL
        }

        // 2: Gaussian Horizontal
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragGaussian

                uniform float GAUSSIAN_COEFF_H[16];
                uniform int GAUSSIAN_KERNEL_RADIUS_H;

                uniform float GAUSSIAN_TEXEL_SIZE_H;

#if UNITY_VERSION < 202320
                float4 _BlitTexture_TexelSize;
#endif

                float4 Sample (float2 uv) 
                {
			        return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
		        }

                float4 FragGaussian(Varyings input) : SV_Target
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                    float4 finalColor = float4(0.0, 0.0, 0.0, 0.0);

                    float texelSize = _BlitTexture_TexelSize.x * GAUSSIAN_TEXEL_SIZE_H;
				
                    int idx = 0;
                    float2 offsetUV;
                    for (idx = -GAUSSIAN_KERNEL_RADIUS_H; idx <= GAUSSIAN_KERNEL_RADIUS_H; idx++)
                    {
                        offsetUV = input.texcoord.xy + float2(idx, 0.0) * texelSize;
                        finalColor += Sample(offsetUV) * GAUSSIAN_COEFF_H[idx + GAUSSIAN_KERNEL_RADIUS_H];
                    }

                    return finalColor;
                }
            ENDHLSL
        }

        // 3: Gaussian Vertical
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragGaussian

                uniform float GAUSSIAN_COEFF_V[16];
                uniform int GAUSSIAN_KERNEL_RADIUS_V;

                uniform float GAUSSIAN_TEXEL_SIZE_V;

#if UNITY_VERSION < 202320
                float4 _BlitTexture_TexelSize;
#endif

                float4 Sample (float2 uv) 
                {
			        return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
		        }

                float4 FragGaussian(Varyings input) : SV_Target
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                    float4 finalColor = float4(0.0, 0.0, 0.0, 0.0);

                    float texelSize = _BlitTexture_TexelSize.x * GAUSSIAN_TEXEL_SIZE_V;
				
                    int idx = 0;
                    float2 offsetUV;
                    for (idx = -GAUSSIAN_KERNEL_RADIUS_V; idx <= GAUSSIAN_KERNEL_RADIUS_V; idx++)
                    {
                        offsetUV = input.texcoord.xy + float2(0.0, idx) * texelSize;
                        finalColor += Sample(offsetUV) * GAUSSIAN_COEFF_V[idx + GAUSSIAN_KERNEL_RADIUS_V];
                    }

                    return finalColor;
                }
            ENDHLSL
        }

        // 4: Dilate
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragDilate

                uniform float GAUSSIAN_COEFF[16];
                uniform int GAUSSIAN_KERNEL_RADIUS;
                uniform int GAUSSIAN_TEXEL_SIZE;

#if UNITY_VERSION < 202320
                float4 _BlitTexture_TexelSize;
#endif

                float4 Sample (float2 uv) 
                {
			        return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
		        }

                float4 SampleBox (float2 uv) 
                {
			        float4 o = float2(-1, 1).xxyy * _BlitTexture_TexelSize.xxxx;
			        float4 s =
				        Sample(uv + o.xy) + Sample(uv + o.zy) +
				        Sample(uv + o.xw) + Sample(uv + o.zw);
			        return s * 0.25f;
		        }
               
                float4 FragDilate(Varyings input) : SV_Target
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                    float2 _min = float2(0, 0);
                    float2 _max = float2(1, 1);

                    float2 _PixelOffset = _BlitTexture_TexelSize.xy;

                    float2 uv = input.texcoord.xy;

                    float4 U = Sample(clamp(uv + float2(0, _PixelOffset.y), _min, _max));
                    float4 UR = Sample(clamp(uv + float2(_PixelOffset.x, _PixelOffset.y), _min, _max));
                    float4 R = Sample(clamp(uv + float2(_PixelOffset.x, 0), _min, _max));
                    float4 DR = Sample(clamp(uv + float2(_PixelOffset.x, -_PixelOffset.y), _min, _max));
                    float4 D = Sample(clamp(uv + float2(0, -_PixelOffset.y), _min, _max));
                    float4 DL = Sample(clamp(uv + float2(-_PixelOffset.x, -_PixelOffset.y), _min, _max));
                    float4 L = Sample(clamp(uv + float2(-_PixelOffset.x, 0), _min, _max));
                    float4 UL = Sample(clamp(uv + float2(-_PixelOffset.x, _PixelOffset.y), _min, _max));

                    float4 finalColor = U + UR + R + DR + D + DL + L + UL;

                    return finalColor;
                }
            ENDHLSL
        }

        // 5: SSGI sample
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

            //HLSLINCLUDE
            //    #include "../../Shaders/FXVShieldEffectHDRP.cginc"
             //   #include "../../Shaders/FXVShieldPostprocessHDRP.cginc"
           // ENDHLSL

            HLSLPROGRAM

                #pragma multi_compile_local FXV_NUM_SAMPLES_LOW FXV_NUM_SAMPLES_MEDIUM FXV_NUM_SAMPLES_HIGH

                #pragma vertex fxvVert //vertex function with screenPos and worldDirection
                #pragma fragment FragSSGI

#if UNITY_VERSION < 202320
                float4 _BlitTexture_TexelSize;
#endif

                float4 Sample (float2 uv) 
                {
			        return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
		        }
               
                float4 FragSSGI(v2f i) : SV_Target
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                    float4 light = _FXV_CalculateLightLoop(_BlitTexture, i.screenPos, i.worldDirection.xyz, i.texcoord);

                    return light;
                }
            ENDHLSL
        }

        // 6: Denoise
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

           // HLSLINCLUDE
           //     #include "../../Shaders/FXVShieldEffectHDRP.cginc"
           //     #include "../../Shaders/FXVShieldPostprocessHDRP.cginc" 
           // ENDHLSL

            HLSLPROGRAM
                #pragma vertex fxvVert //vertex function with screenPos and worldDirection
                #pragma fragment FragDenoise

#if UNITY_VERSION < 202320
                float4 _BlitTexture_TexelSize;
#endif

                float4 Sample(float2 uv)
                {
                    return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
                }

                float4 FragDenoise(v2f i) : SV_Target
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                    float2 offset[8] = { float2(-1.0, 0.0f), float2(1.0, 0.0f), float2(0.0, 1.0f), float2(0.0, -1.0f), float2(-0.70710678118, 0.70710678118), float2(0.70710678118, -0.70710678118), float2(0.70710678118, 0.70710678118), float2(-0.70710678118, -0.70710678118) };
                    float kernel[8] = { 0.125, 0.125, 0.125, 0.125, 0.125, 0.125, 0.125, 0.125 };

                    float4 finalColor = _FXV_Wavelet(_BlitTexture, i.texcoord, i.screenPos, i.worldDirection.xyz, offset, kernel);

                    return finalColor;
                }
            ENDHLSL
        }

        // 7: Box + SSGI buffer
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragBox

#if UNITY_VERSION < 202320
                float4 _BlitTexture_TexelSize;
#endif

                TEXTURE2D_X(_SSGIBuffer);

                float4 Sample(float2 uv)
                {
                    return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
                }

                float4 SampleBox(float2 uv)
                {
                    float4 o = float2(-1, 1).xxyy * _BlitTexture_TexelSize.xxxx;
                    float4 s =
                        Sample(uv + o.xy) + Sample(uv + o.zy) +
                        Sample(uv + o.xw) + Sample(uv + o.zw);
                    return s * 0.25f;
                }

                float4 FragBox(Varyings input) : SV_Target
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                    return float4(SampleBox(input.texcoord.xy).rgb, 1) + Sample(_SSGIBuffer, input.texcoord.xy);
                }
            ENDHLSL
        }        
        
        // 8: Simple
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragBox

#if UNITY_VERSION < 202320
                float4 _BlitTexture_TexelSize;
#endif

                float4 Sample(float2 uv)
                {
                    return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
                }

                float4 FragBox(Varyings input) : SV_Target
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                    return float4(Sample(input.texcoord.xy).rgb, 1) ;
                }
            ENDHLSL
        }

        // 9: Simple + SSGI buffer
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment FragBox

#if UNITY_VERSION < 202320
                float4 _BlitTexture_TexelSize;
#endif

                TEXTURE2D_X(_SSGIBuffer);

                float4 Sample(float2 uv)
                {
                    return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);
                }

                float4 FragBox(Varyings input) : SV_Target
                {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                    return float4(Sample(input.texcoord.xy).rgb, 1) + Sample(_SSGIBuffer, input.texcoord.xy);
                }
            ENDHLSL
        }
    }
}

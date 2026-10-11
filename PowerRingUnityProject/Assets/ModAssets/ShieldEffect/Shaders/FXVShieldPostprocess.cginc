#ifdef FXV_SHIELD_URP
    #define FXV_SHIELD_POSTPROCESS_APPDATA	float4 vertex : POSITION; \
                                            float2 uv : TEXCOORD0; \
                                            UNITY_VERTEX_INPUT_INSTANCE_ID 

	#define FXV_SHIELD_POSTPROCESS_V2F_COORDS	float2 uv : TEXCOORD0; \
                                                float4 vertex : SV_POSITION; \
                                                UNITY_VERTEX_INPUT_INSTANCE_ID \
											    UNITY_VERTEX_OUTPUT_STEREO 

    #define FXV_SHIELD_POSTPROCESS_VERTEX_DEFAULT(v, o)	UNITY_SETUP_INSTANCE_ID(v); \
													    UNITY_TRANSFER_INSTANCE_ID(v, o); \
													    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); \
													    o.vertex = FXV_TranformVertexPosition(v.vertex.xyz); \
                                                        o.uv = v.uv; 

    #define FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i)	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    #define FXV_DEFINE_SCREEN_TEXTURE(t)    TEXTURE2D_X(t); \
			                                SAMPLER(sampler##t);

    #define FXV_SAMPLE_SCREEN_TEXTURE(t,u)  SAMPLE_TEXTURE2D_X(t, sampler##t, UnityStereoTransformScreenSpaceTex(u))

    #define FXV_SCREEN_TEXTURE_PARAM(t)     TEXTURE2D_X_PARAM(t, sampler##t)
    #define FXV_SCREEN_TEXTURE_ARGS(t)      TEXTURE2D_X_ARGS(t, sampler##t)
#else
    #define FXV_SHIELD_POSTPROCESS_APPDATA	float4 vertex : POSITION; \
                                            float2 uv : TEXCOORD0;

	#define FXV_SHIELD_POSTPROCESS_V2F_COORDS	float2 uv : TEXCOORD0; \
                                                float4 vertex : SV_POSITION;

    #define FXV_SHIELD_POSTPROCESS_VERTEX_DEFAULT(v, o)	o.vertex = UnityObjectToClipPos(v.vertex); \
                                                        o.uv = v.uv;

    #define FXV_SHIELD_POSTPROCESS_FRAGMENT_DEFAULT(i)	

    #define FXV_DEFINE_SCREEN_TEXTURE(t)    sampler2D t
    #define FXV_SAMPLE_SCREEN_TEXTURE(t,u)  tex2D(t, u)
    #define FXV_SCREEN_TEXTURE_PARAM(t)     sampler2D t
    #define FXV_SCREEN_TEXTURE_ARGS(t)      t
#endif

FXV_DEFINE_SCREEN_TEXTURE(_CameraDepthTexture);

float GLOBAL_ResolutionCompensation;

float4 _CameraDepthTexture_TexelSize;

float4x4 _ViewProjectInverse;

float4 FXV_TranformVertexPosition(float3 inVertex)
{
#if STEREO_INSTANCING_ON
     float4 vertex = float4(inVertex, 1.0);
#if UNITY_UV_STARTS_AT_TOP
     vertex.y *= -1;
#endif
#else
    #ifdef FXV_SHIELD_URP
    VertexPositionInputs vertInputs = GetVertexPositionInputs(inVertex);
    float4 vertex = vertInputs.positionCS;
    #else
    float4 vertex = UnityObjectToClipPos(inVertex);
    #endif
#endif
    return vertex;
}

half _FXV_GetLinearEyeDepth(half4 screenPos) // screenPos as ComputeScreenPos(o.vertex)
{
#if defined(FXV_SHIELD_URP)
	half z = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(screenPos.xy / screenPos.w)).r;
#else
    half z = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(screenPos));
#endif
    
    // Orthographic linear depth
    half near = _ProjectionParams.y;
    half far = _ProjectionParams.z;
#if defined(UNITY_REVERSED_Z)
    float depthOrtho = lerp(far, near, z);
#else
    half depthOrtho = lerp(near, far, z);
#endif

    half isOrtho = unity_OrthoParams.w; // 0 - perspective, 1 - ortho
    
#if defined(FXV_SHIELD_URP)
	half eyeDepth = LinearEyeDepth(z, _ZBufferParams);
#else
    half eyeDepth = LinearEyeDepth(z);
#endif

    return lerp(eyeDepth, depthOrtho, isOrtho);
}

half _FXV_GetLinearEyeDepth(half2 uv) //screen uv
{
#if defined(FXV_SHIELD_URP)
	half z = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(uv)).r;
#else
    half z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
#endif

    // Orthographic linear depth
    half near = _ProjectionParams.y;
    half far = _ProjectionParams.z;
#if defined(UNITY_REVERSED_Z)
    float depthOrtho = lerp(far, near, z);
#else
    half depthOrtho = lerp(near, far, z);
#endif

    half isOrtho = unity_OrthoParams.w; // 0 - perspective, 1 - ortho

#if defined(FXV_SHIELD_URP)
	half eyeDepth = LinearEyeDepth(z, _ZBufferParams);
#else
    half eyeDepth = LinearEyeDepth(z);
#endif
    
    return lerp(eyeDepth, depthOrtho, isOrtho);
}

float4 _FXV_SampleWorldPositionFromDepth(in float2 uv)
{
#if defined(FXV_SHIELD_URP)
	 float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(uv)).r;
#else
    float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
#endif

    float4 H = float4(uv.x * 2.0 - 1.0, (uv.y) * 2.0 - 1.0, depth, 1.0);
    float4 D = mul(_ViewProjectInverse, H);
    return D / D.w;
}

float3 _FXV_SampleWorldPositionFromDepth(in float4 screenPos, in float3 worldDirection)
{
    float depthValue = _FXV_GetLinearEyeDepth(screenPos);

    return _WorldSpaceCameraPos.xyz + worldDirection * depthValue;
}

float3 _FXV_SampleWorldPositionFromDepth(in float2 uv, in float3 worldDirection)
{
    float depthValue = _FXV_GetLinearEyeDepth(uv);

    return _WorldSpaceCameraPos.xyz + worldDirection * depthValue;
}

sampler2D _GI_RandomNoise;
float _GI_RandomSize;
float _GI_SampleRadius;
float _GI_Intensity;
float _GI_LightRange;
float _GI_FadeBehind;
float4 _GI_FrameSize;
uniform float _GI_SampleDirections[128];

float2 _Seed2;
float3 _Seed3;

float hash2(float2 offset)
{
    _Seed2 += offset;
    return frac(sin(dot(_Seed2, float2(12.9898, 78.233))) * 43758.55);
}

float hash3(float3 offset)
{
    _Seed3 += offset;
    return frac(sin(dot(_Seed3, float3(12.9898, 78.233, 45.5432))) * 43758.55);
}

float2 _FXV_RandInsideUnitCircle(float3 offset)
{
    float angle = hash3(offset) * 2.0 * 3.14159;
    float distance = sqrt(hash3(offset)); //sqrt - to make it uniform

    float x = distance * cos(angle);
    float y = distance * sin(angle);

    return float2(x,y);
}

float2 _FXV_RandInsideUnitQuad(float3 offset)
{
    float x = 2.0 * hash3(offset) - 1.0;
    float y = 2.0 * hash3(offset) - 1.0;
    int tries = 0;

    return float2(x, y);
}

float _GI_c_phi = 1.0;
float _GI_n_phi = 1.0;
float _GI_p_phi = 1.0;
float _GI_Stepwidth = 2.0;

float4 _FXV_Wavelet(in FXV_SCREEN_TEXTURE_PARAM(colorMap), in FXV_SCREEN_TEXTURE_PARAM(normalMap), in float2 in_uv, in float4 screenPos, in float3 worldViewDir, in float2 offset[8], in float kernel[8])

{
    float4 sum = float4(0,0,0,0);
    float2 step = _GI_FrameSize.xy;
    float4 cval = FXV_SAMPLE_SCREEN_TEXTURE(colorMap, in_uv);
    float4 nval = FXV_SAMPLE_SCREEN_TEXTURE(normalMap, in_uv);
    float4 pval = float4(_FXV_SampleWorldPositionFromDepth(in_uv, worldViewDir), 0.0);
    float cum_w = 0.0;

    for (int i = 0; i < 8; i++) 
    {
        float2 uv = in_uv + offset[i] * step * _GI_Stepwidth * GLOBAL_ResolutionCompensation;
        float4 ctmp = FXV_SAMPLE_SCREEN_TEXTURE(colorMap, uv);
        float4 t = cval - ctmp;
        float dist2 = dot(t, t);
        float c_w = min(exp(-(dist2) / _GI_c_phi), 1.0);
        float4 ntmp = FXV_SAMPLE_SCREEN_TEXTURE(normalMap, uv);
        t = nval - ntmp;
        dist2 = max(dot(t, t) / (_GI_Stepwidth * _GI_Stepwidth), 0.0);
        float n_w = min(exp(-(dist2) / _GI_n_phi), 1.0);
        float4 ptmp = float4(_FXV_SampleWorldPositionFromDepth(uv, worldViewDir), 0.0);
        t = pval - ptmp;
        dist2 = dot(t, t);
        float p_w = min(exp(-(dist2) / _GI_p_phi), 1.0);
        float weight = c_w * n_w * p_w;
        sum += ctmp * weight * kernel[i];
        cum_w += weight * kernel[i];
    }

    return saturate(sum / cum_w);
}

float2 _GI_RandomSeed;

float2 _FXV_GetRandomNoise(in float2 uv)
{
    //_GI_RandomSeed += _Time.x;

    return tex2D(_GI_RandomNoise, _GI_FrameSize.zw * uv / _GI_RandomSize + _GI_RandomSeed).xy * 2.0 - 1.0;
}

float2 _FXV_GetRandomNoise(in float3 pos)
{
    //_GI_RandomSeed += _Time.x;

    return tex2D(_GI_RandomNoise, float2(pos.x+pos.z, pos.z+pos.y)).xy * 2.0 - 1.0;
}

sampler2D _FXVPositionFrontBuffer;
sampler2D _FXVPositionBackBuffer;

#if defined(FXV_SHIELD_URP)
    #define UNITY_NORMALS_BUFFER _GBuffer2
#else
    #define UNITY_NORMALS_BUFFER _CameraGBufferTexture2
#endif

FXV_DEFINE_SCREEN_TEXTURE(UNITY_NORMALS_BUFFER);

float3 _FXV_PostprocessWorldViewRay(in float2 uv)
{
    //https://forum.unity.com/threads/world-space-position-in-a-post-processing-shader.114392/
    float4 D = mul(_ViewProjectInverse, float4((uv.x) * 2 - 1, (uv.y) * 2 - 1, 0.5, 1));
    D.xyz /= D.w;
    D.xyz -= _WorldSpaceCameraPos;
    float4 D0 = mul(_ViewProjectInverse, float4(0, 0, 0.5, 1));
    D0.xyz /= D0.w;
    D0.xyz -= _WorldSpaceCameraPos;
    return D.xyz / length(D0.xyz);
}

float4 _FXV_ClaculateLight(in FXV_SCREEN_TEXTURE_PARAM(mainTex), in float2 uv, in float2 offsetUV, in float3 worldPixelPos, in float3 worldNormal)
{
    float2 uvOff = uv + offsetUV;

    if (uvOff.x > 1.0 || uvOff.x < 0.0 || uvOff.y > 1.0 || uvOff.y < 0.0)
    {
        return (float4)0;
    }

    float invLightRangeSqr = 1.0 / (_GI_LightRange * _GI_LightRange);

    float4 shieldPositionF = tex2D(_FXVPositionFrontBuffer, uvOff);
    float4 shieldPositionB = tex2D(_FXVPositionBackBuffer, uvOff);

    float4 shieldColor = saturate(FXV_SAMPLE_SCREEN_TEXTURE(mainTex, uvOff));

    float3 diffF = worldPixelPos - shieldPositionF.xyz;
    float distSqrF = dot(diffF, diffF);
    float factorF = distSqrF * invLightRangeSqr;

    float3 diffB = worldPixelPos - shieldPositionB.xyz;
    float distSqrB = dot(diffB, diffB);
    float factorB = distSqrB * invLightRangeSqr;

    float dF = saturate(dot(worldNormal, -normalize(diffF)));
    float dB = saturate(dot(worldNormal, -normalize(diffB)));

    return shieldColor * (pow(saturate(1.0 - factorF), 1) * dF * shieldPositionF.a + pow(saturate(1.0 - factorB), 1) * dB * shieldPositionB.a) * 0.5;
}

float4 _FXV_CalculateLightLoop(in FXV_SCREEN_TEXTURE_PARAM(mainTex), in float4 screenPos, in float3 worldDirection, in float2 uv)
{
    half depthValue = _FXV_GetLinearEyeDepth(uv);

    float3 worldCamRelativePosition = worldDirection * depthValue;
    float3 worldPosition = _WorldSpaceCameraPos.xyz + worldCamRelativePosition;
    float3 worldNormal = normalize(FXV_SAMPLE_SCREEN_TEXTURE(UNITY_NORMALS_BUFFER, uv).xyz * 2.0 - 1.0);

    float rad = _GI_SampleRadius / depthValue;
    float2 rand = _FXV_GetRandomNoise(uv); //texture sample is faster
   //float2 rand = _FXV_GetRandomNoise(worldPosition); //texture sample is faster
   // float2 rand = _FXV_RandInsideUnitCircle(worldPosition);

    float4 light = float4(0, 0, 0, 0);
    float isHit = 0.0;

#if defined(FXV_NUM_SAMPLES_LOW)
    int iterations = 8;
    int itD = 16; 
#elif defined(FXV_NUM_SAMPLES_MEDIUM)
    int iterations = 16;
    int itD = 8;
#elif defined(FXV_NUM_SAMPLES_HIGH)
    int iterations = 32;
    int itD = 4;
#else
    int iterations = 16;
    int itD = 8;
#endif

    for (int j = 0; j < iterations; ++j)
    {
        int idx = j * itD;
        float2 coord1 = reflect(float2(_GI_SampleDirections[idx], _GI_SampleDirections[idx + 1]), rand) * rad;
        float2 coord2 = float2(coord1.x * 0.707 - coord1.y * 0.707, coord1.x * 0.707 + coord1.y * 0.707);

        light += _FXV_ClaculateLight(FXV_SCREEN_TEXTURE_ARGS(mainTex), uv, coord1, worldCamRelativePosition, worldNormal);
        light += _FXV_ClaculateLight(FXV_SCREEN_TEXTURE_ARGS(mainTex), uv, -coord1, worldCamRelativePosition, worldNormal);
        //light += _FXV_ClaculateLight(mainTex, uv, coord2 * 0.25, worldCamRelativePosition, worldNormal);
        //light += _FXV_ClaculateLight(mainTex, uv, coord1 * 0.15, worldCamRelativePosition, worldNormal);
        light += _FXV_ClaculateLight(FXV_SCREEN_TEXTURE_ARGS(mainTex), uv, coord2, worldCamRelativePosition, worldNormal);
        light += _FXV_ClaculateLight(FXV_SCREEN_TEXTURE_ARGS(mainTex), uv, -coord2, worldCamRelativePosition, worldNormal);
    }

    float4 shieldPositionF = tex2D(_FXVPositionFrontBuffer, uv);

    if (_GI_FadeBehind > 0.0)
    {
        float3 diff = worldCamRelativePosition - shieldPositionF.xyz;
        float diffDot = dot(diff, worldDirection) - 0.05; // -0.05 treshold surface which we are rendering
        if (shieldPositionF.a * diffDot > 0.0) //lower intensity inside or behind shield
        {
            float diffT = diffDot;
            diffT = saturate(diffT - _GI_FadeBehind);
            return saturate(light * _GI_Intensity) * lerp(0.0, 1.0, diffT);
        }
    }

    return saturate(light * _GI_Intensity);
}
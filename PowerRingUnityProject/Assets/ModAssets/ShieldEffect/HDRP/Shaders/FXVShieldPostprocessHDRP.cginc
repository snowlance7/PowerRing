#define FXV_DEFINE_TEXTURE_SAMPLER(t) TEXTURE2D(t); \
										  SAMPLER(sampler_##t);
#define FXV_TEXTURE_SAMPLER_PARAMS(t) 0
#define FXV_TEXTURE_SAMPLER_PARAMS_DEF(t) float dummy

float4x4 _ViewProjectInverse;

float GLOBAL_ResolutionCompensation;

#if SHADER_API_GLES
struct appdata
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
#else
struct appdata
{
    uint vertexID : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
#endif

struct v2f
{
    float4 positionCS : SV_POSITION;
    float2 texcoord : TEXCOORD0;
    float4 screenPos : TEXCOORD1;
    float4 worldDirection : TEXCOORD2;
    UNITY_VERTEX_OUTPUT_STEREO
};

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

v2f fxvVert(appdata input)
{
    v2f output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

#if SHADER_API_GLES
    float4 pos = input.positionOS;
    float2 uv = input.uv;
#else
    float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
    float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);
#endif

    output.positionCS = pos;
    output.texcoord = uv * _BlitScaleBias.xy + _BlitScaleBias.zw;
    output.screenPos = ComputeScreenPos(pos);
    output.worldDirection = float4(_FXV_PostprocessWorldViewRay(uv), 0);

    return output;
}

float _FXV_GetLinearEyeDepth(FXV_TEXTURE_SAMPLER_PARAMS_DEF(depthTexture), half4 screenPos) // screenPos as ComputeScreenPos(o.vertex)
{
    float z = SampleCameraDepth(screenPos.xy / screenPos.w);

    // Orthographic linear depth
    float near = _ProjectionParams.y;
    float far = _ProjectionParams.z;
#if defined(UNITY_REVERSED_Z)
    float depthOrtho = lerp(far, near, z);
#else
    float depthOrtho = lerp(near, far, z);
#endif

    float isOrtho = unity_OrthoParams.w; // 0 - perspective, 1 - ortho

    float eyeDepth = LinearEyeDepth(z, _ZBufferParams);

    return lerp(eyeDepth, depthOrtho, isOrtho);
}

float _FXV_GetLinearEyeDepth(FXV_TEXTURE_SAMPLER_PARAMS_DEF(depthTexture), half2 uv) //screen uv
{
    float z = SampleCameraDepth(uv.xy);

    // Orthographic linear depth
    float near = _ProjectionParams.y;
    float far = _ProjectionParams.z;
#if defined(UNITY_REVERSED_Z)
    float depthOrtho = lerp(far, near, z);
#else
    float depthOrtho = lerp(near, far, z);
#endif

    float isOrtho = unity_OrthoParams.w; // 0 - perspective, 1 - ortho

    float eyeDepth = LinearEyeDepth(z, _ZBufferParams);

    return lerp(eyeDepth, depthOrtho, isOrtho);
}

float4 _FXV_SampleWorldPositionFromDepth(in float2 uv)
{
    float depth = SampleCameraDepth(uv.xy);
    float4 H = float4(uv.x * 2.0 - 1.0, (uv.y) * 2.0 - 1.0, depth, 1.0);
    float4 D = mul(_ViewProjectInverse, H);
    return D / D.w;
}

float3 _FXV_SampleWorldPositionFromDepth(in float4 screenPos, in float3 worldDirection)
{
    float depthValue = _FXV_GetLinearEyeDepth(FXV_TEXTURE_SAMPLER_PARAMS(_CameraDepthTexture), screenPos);

    return _WorldSpaceCameraPos.xyz + worldDirection * depthValue;
}

float3 _FXV_SampleWorldPositionFromDepth(in float2 uv, in float3 worldDirection)
{
    float depthValue = _FXV_GetLinearEyeDepth(FXV_TEXTURE_SAMPLER_PARAMS(_CameraDepthTexture), uv);

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

float hash2(float3 offset)
{
    _Seed2 += offset.xy;
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

    return float2(x, y);
}

float2 _FXV_RandInsideUnitQuad(float3 offset)
{
    float x = 2.0 * hash3(offset) - 1.0;
    float y = 2.0 * hash3(offset) - 1.0;
    int tries = 0;

    return float2(x, y);
}

float4 Sample(in TEXTURE2D_X(tex), float2 uv)
{
    return SAMPLE_TEXTURE2D_X_LOD(tex, sampler_LinearClamp, uv, _BlitMipLevel);
}

float3 _FXV_GetNormalWSFromGBuffer(in float2 uv)
{
    NormalData normalData;
    float4 normalBuffer = Sample(_NormalBufferTexture, uv);
    DecodeFromNormalBuffer(normalBuffer, normalData);
    return normalData.normalWS;
}

float _GI_c_phi = 1.0;
float _GI_n_phi = 1.0;
float _GI_p_phi = 1.0;
float _GI_Stepwidth = 2.0;

float4 _FXV_Wavelet(in TEXTURE2D_X(colorMap), in float2 in_uv, in float4 screenPos, in float3 worldViewDir, in float2 offset[8], in float kernel[8])
{
    float4 sum = float4(0, 0, 0, 0);
    float2 step = _GI_FrameSize.xy;
    float4 cval = Sample(colorMap, in_uv);
    float4 nval = float4(_FXV_GetNormalWSFromGBuffer(in_uv), 0.0);
    float4 pval = float4(_FXV_SampleWorldPositionFromDepth(in_uv, worldViewDir), 0.0);
    float cum_w = 0.0;

    int iterations = 8;
    for (int i = 0; i < iterations; ++i)
    {
        float2 uv = in_uv + offset[i] * step * _GI_Stepwidth * GLOBAL_ResolutionCompensation;
        float4 ctmp = Sample(colorMap, uv);
        float4 t = cval - ctmp;
        float dist2 = dot(t, t);
        float c_w = min(exp(-(dist2) / _GI_c_phi), 1.0);
        float4 ntmp = float4(_FXV_GetNormalWSFromGBuffer(uv), 0);
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

TEXTURE2D_X(_FXVPositionFrontBuffer);
TEXTURE2D_X(_FXVPositionBackBuffer);

float4 _FXV_ClaculateLight(in TEXTURE2D_X(mainTex), in float2 uv, in float2 offsetUV, in float3 worldPixelPos, in float3 worldNormal)
{
    float2 uvOff = uv + offsetUV;

    if (uvOff.x > 1.0 || uvOff.x < 0.0 || uvOff.y > 1.0 || uvOff.y < 0.0)
    {
        return (float4)0;
    }

    float invLightRangeSqr = 1.0 / (_GI_LightRange * _GI_LightRange);

    float4 shieldPositionF = Sample(_FXVPositionFrontBuffer, uvOff);
    float4 shieldPositionB = Sample(_FXVPositionBackBuffer, uvOff);

    //shieldPositionF.xyz = GetAbsolutePositionWS(shieldPositionF.xyz); //TODO: maybe we should do this in camera relative space, than worldPixelPos should be calculated differently
    //shieldPositionB.xyz = GetAbsolutePositionWS(shieldPositionB.xyz);

    float4 shieldColor = saturate(Sample(mainTex, uvOff));

    float3 diffF = worldPixelPos - shieldPositionF.xyz;
    float distSqrF = dot(diffF, diffF);
    float factorF = distSqrF * invLightRangeSqr;

    float3 diffB = worldPixelPos - shieldPositionB.xyz;
    float distSqrB = dot(diffB, diffB);
    float factorB = distSqrB * invLightRangeSqr;

    float dF = saturate(dot(worldNormal, -normalize(diffF)));
    float dB = saturate(dot(worldNormal, -normalize(diffB)));

    return shieldColor * (pow(abs(saturate(1.0 - factorF)), 1) * dF * shieldPositionF.a + pow(abs(saturate(1.0 - factorB)), 1) * dB * shieldPositionB.a) * 0.5;
}

float4 _FXV_CalculateLightLoop(in TEXTURE2D_X(mainTex), in float4 screenPos, in float3 worldDirection, in float2 uv)
{
    float depthValue = _FXV_GetLinearEyeDepth(FXV_TEXTURE_SAMPLER_PARAMS(_CameraDepthTexture), uv);

    //float3 worldPosition = _WorldSpaceCameraPos.xyz + worldDirection * depthValue;
    float3 worldCamRelativePosition = worldDirection * depthValue;
    float3 worldNormal = _FXV_GetNormalWSFromGBuffer(uv);

    float rad = _GI_SampleRadius / depthValue;
    float2 rand = _FXV_GetRandomNoise(uv); //texture sample is faster
    //float2 rand = _FXV_GetRandomNoise(worldPosition); //texture sample is faster
    //float2 rand = _FXV_RandInsideUnitCircle(worldCamRelativePositionworldCamRelativePosition);

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

        light += _FXV_ClaculateLight(mainTex, uv, coord1, worldCamRelativePosition, worldNormal);
        light += _FXV_ClaculateLight(mainTex, uv, coord2 * 0.5, worldCamRelativePosition, worldNormal);
        light += _FXV_ClaculateLight(mainTex, uv, coord1 * 0.75, worldCamRelativePosition, worldNormal);
        light += _FXV_ClaculateLight(mainTex, uv, coord2, worldCamRelativePosition, worldNormal);
    }

    float4 shieldPositionF = Sample(_FXVPositionFrontBuffer, uv);
    
    if (_GI_FadeBehind > 0.0)
    {
        if (shieldPositionF.a > 0.0) //lower intensity inside or behind shield
        {
            float diffT = length(worldCamRelativePosition - shieldPositionF.xyz);
            diffT = saturate(diffT - _GI_FadeBehind);
            return saturate(light * _GI_Intensity) * lerp(0.0, 1.0, diffT);
        }
    }

    return saturate(light * _GI_Intensity);
}
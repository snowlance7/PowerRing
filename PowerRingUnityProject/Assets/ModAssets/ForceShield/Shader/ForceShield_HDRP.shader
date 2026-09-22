Shader "IndieChest/ForceShield_HDRP"
{
    Properties
    {
        _Color("Color", Color) = (0,1,0,1)
        _Opacity("Opacity", Range(0,1)) = 0.378

        _ShieldPatternColor("Additional Color", Color) = (0,1,0,1)
        _ShieldPatternPower("Additional Color Power", Range(0,100)) = 3
        _ShieldRimPower("Shield Rim Power", Range(0.1,10)) = 7.76

        [HideInInspector]_HitPosition("Hit Position (World)", Vector) = (0,0,0,0)
        _HitTime("Hit Time", Float) = 0
        _HitColor("Hit Color", Color) = (0,1,0,1)
        _HitSize("Hit Size", Float) = 0.2
        _HitWave("Hit Wave", Range(0,1)) = 0.15

        _PatternScale("Pattern Scale", Range(1,100)) = 24
        _PatternSpeed("Pattern Speed", Range(0,10)) = 1
        _PatternStrength("Pattern Strength", Range(0,2)) = 0.35
        _WaveScale("Wave Scale", Range(1,50)) = 12
        _WaveSpeed("Wave Speed", Range(0,10)) = 1.5
        _WaveStrength("Wave Strength", Range(0,2)) = 0.25

        _HitDuration("Hit Duration", Range(0.01,20)) = 1
        _HitRingWidth("Hit Ring Width", Range(0.001,1)) = 0.08
        _HitGlow("Hit Glow", Range(0,20)) = 5
    }

    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 vulkan

    #pragma multi_compile_instancing

    #define ATTRIBUTES_NEED_NORMAL
    #define ATTRIBUTES_NEED_TANGENT
    #define ATTRIBUTES_NEED_TEXCOORD0

    #define VARYINGS_NEED_POSITION_WS
    #define VARYINGS_NEED_TANGENT_TO_WORLD
    #define VARYINGS_NEED_TEXCOORD0

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassRenderers.hlsl"

    float4 _Color;
    float _Opacity;

    float4 _ShieldPatternColor;
    float _ShieldPatternPower;
    float _ShieldRimPower;

    float4 _HitPosition;
    float _HitTime;
    float4 _HitColor;
    float _HitSize;
    float _HitWave;

    float _PatternScale;
    float _PatternSpeed;
    float _PatternStrength;
    float _WaveScale;
    float _WaveSpeed;
    float _WaveStrength;

    float _HitDuration;
    float _HitRingWidth;
    float _HitGlow;

    float hash21(float2 p)
    {
        p = frac(p * float2(123.34, 456.21));
        p += dot(p, p + 45.32);
        return frac(p.x * p.y);
    }

    float noise21(float2 p)
    {
        float2 i = floor(p);
        float2 f = frac(p);

        f = f * f * (3.0 - 2.0 * f);

        float a = hash21(i);
        float b = hash21(i + float2(1,0));
        float c = hash21(i + float2(0,1));
        float d = hash21(i + float2(1,1));

        return lerp(lerp(a,b,f.x), lerp(c,d,f.x), f.y);
    }

    float spherePattern(float3 worldPos, float time)
    {
        float3 p = worldPos * _PatternScale;

        float xy = noise21(p.xy + time * _PatternSpeed);
        float yz = noise21(p.yz - time * _PatternSpeed * 0.73);
        float xz = noise21(p.xz + time * _PatternSpeed * 0.41);

        return (xy + yz + xz) / 3.0;
    }

    float wavePattern(float3 worldPos, float time)
    {
        float3 p = worldPos * _WaveScale;

        float a = sin((p.x + p.y + p.z) + time * _WaveSpeed);
        float b = sin((p.x - p.y + p.z) - time * _WaveSpeed * 0.71);

        return saturate((a + b) * 0.25 + 0.5);
    }

    void GetSurfaceAndBuiltinData(
        FragInputs fragInputs,
        float3 viewDirection,
        inout PositionInputs posInput,
        out SurfaceData surfaceData,
        out BuiltinData builtinData)
    {
        ZERO_INITIALIZE(SurfaceData, surfaceData);
        ZERO_INITIALIZE(BuiltinData, builtinData);

        float3 worldPos = GetAbsolutePositionWS(fragInputs.positionRWS);

        float3 normalWS = normalize(fragInputs.tangentToWorld[2]);

        // HDRP viewDirection points from the surface toward the camera.
        float fresnel = pow(
            saturate(1.0 - dot(normalWS, normalize(viewDirection))),
            _ShieldRimPower
        );

        float time = _TimeParameters.x;

        // Procedural animated shield pattern. No textures are required.
        float pattern = spherePattern(worldPos, time);
        float waves = wavePattern(worldPos, time);

        float patternMask =
            1.0 +
            (pattern - 0.5) * _PatternStrength +
            (waves - 0.5) * _WaveStrength;

        // Hit ripple.
        float hit = 0.0;

        if (_HitTime > 0.0)
        {
            float3 hitDelta = worldPos - _HitPosition.xyz;
            float distanceFromHit = length(hitDelta);

            // Expanding ring. _HitTime is interpreted as seconds.
            float radius = _HitTime * _HitWave;

            float ring = 1.0 -
                saturate(abs(distanceFromHit - radius) / max(_HitRingWidth, 0.0001));

            // Local flash around the impact point that fades with time.
            float flash =
                1.0 -
                saturate(distanceFromHit / max(_HitSize, 0.0001));

            float fade = 1.0 - saturate(_HitTime / max(_HitDuration, 0.01));

            hit = max(ring, flash * fade);
            hit *= fade;
        }

        float3 baseColor = _Color.rgb;
        float3 patternColor = _ShieldPatternColor.rgb;

        // The original shader's main visible contribution is its Fresnel rim.
        float3 shieldColor =
            baseColor +
            patternColor * (_ShieldPatternPower * patternMask * fresnel);

        shieldColor *= (0.15 + fresnel * 1.5);

        // Impact color overrides the normal shield color near the hit wave.
        shieldColor += _HitColor.rgb * hit * _HitGlow;

        // Keep the interior faint while making the rim bright.
        float alpha = _Opacity * saturate(0.12 + fresnel * 0.88);

        // Make impacts visible through alpha as well as emission.
        alpha = saturate(alpha + hit * _Opacity);

        surfaceData.color = shieldColor;

        builtinData.opacity = alpha;
        builtinData.emissiveColor = shieldColor;
    }

    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "HDRenderPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode" = "ForwardOnly" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassForwardUnlit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            ENDHLSL
        }
    }

    Fallback Off
}

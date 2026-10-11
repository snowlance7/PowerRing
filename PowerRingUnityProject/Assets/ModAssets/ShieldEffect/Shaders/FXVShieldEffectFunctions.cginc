#ifndef FXV_SHIELD_EFFECT_FUNCTIONS_INCLUDED
#define FXV_SHIELD_EFFECT_FUNCTIONS_INCLUDED

inline float3 _FXV_ObjectToWorldPos(in float3 pos)
{
#if defined(FXV_SHIELD_URP)
	return TransformObjectToWorld(pos);
#elif defined(FXV_SHIELD_HDRP)
	return TransformObjectToWorld(pos) + _WorldSpaceCameraPos.xyz; //HDRP returns cam relative
#else
    return mul(unity_ObjectToWorld, float4(pos, 1)).xyz;
#endif
}

inline float3 _FXV_ObjectToWorldPos(float4 pos) // overload for float4; avoids "implicit truncation" warning for existing shaders
{
    return _FXV_ObjectToWorldPos(pos.xyz);
}

inline float3 _FXV_ObjectToWorldCamRelativePos(in float3 pos)
{
#if defined(FXV_SHIELD_URP)
	return TransformObjectToWorld(pos) - _WorldSpaceCameraPos.xyz;
#elif defined(FXV_SHIELD_HDRP)
	return TransformObjectToWorld(pos); //HDRP returns cam relative already
#else
    return mul(unity_ObjectToWorld, float4(pos, 1)).xyz - _WorldSpaceCameraPos.xyz;
#endif
}

inline float3 _FXV_ObjectToWorldCamRelativePos(float4 pos) // overload for float4; avoids "implicit truncation" warning for existing shaders
{
    return _FXV_ObjectToWorldCamRelativePos(pos.xyz);
}

inline float3 _FXV_ObjectToWorldDir(in float3 dir)
{
#if defined(FXV_SHIELD_URP)
	return mul(GetObjectToWorldMatrix(), float4(dir, 0)).xyz;
#elif defined(FXV_SHIELD_HDRP)
	return TransformObjectToWorldDir(dir, false); //HDRP returns cam relative World Matrix so use specific function
#else
    return mul(unity_ObjectToWorld, float4(dir, 0)).xyz;
#endif
}

inline float3 _FXV_ObjectToWorldDir(float4 dir) // overload for float4; avoids "implicit truncation" warning for existing shaders
{
    return _FXV_ObjectToWorldDir(dir.xyz);
}

float3 _FXV_IntensityCorrection(float3 color, float _param)
{
#if UNITY_COLORSPACE_GAMMA
	return color * (0.25 * _param + 0.25);
#else
	return color * _param;
#endif
}

float _FXV_AlphaCorrection(float alpha, float _param)
{
#if UNITY_COLORSPACE_GAMMA
	return pow(abs(alpha), (_param * 0.5 + 0.5));
#else
	return pow(abs(alpha), _param);
#endif
} 

half GetHitInfluence(half influenceScale) // Hit influence defines how much whole shield effect will fade when hit
{
#if defined(HIT_EFFECT_ON) //return 1 for hit effect so multiple hits will not have different power
	return 1.0;
#else
	return (1.0 - influenceScale * (1.0 - FXV_ACCESS_PROP(_HitEffectValue)));
#endif
}

float2 _FXV_GenerateUV(v2f i) //experimental procedural uv generation - we want to achieve something like triplanar mapping but with single texture sample
{
	float3 pos = i.objectSpacePos.xyz / _ShieldBounds.xyz + float3(0.5, 0.5, 0.5);

	//float2 uv = lerp(pos.xy, pos.zy, round(i.normal.x));
	//uv = lerp(uv, pos.xz, round(i.normal.y));

	//float2 uv = pos.xy;
	//return uv + float2(0.0, i.normal.y * pos.z);

	float2 uv = pos.xy + float2(pos.x + pos.z, pos.y); //float2(dot(pos, float3(0, 0, 1)) + dot(pos, float3(0, 0, 0)), dot(pos, float3(0, 1, 0)) + dot(pos, float3(0, 0, 0)));
	return uv;
}

float4 _FXV_SampleTexture(v2f i, sampler2D _texture, float2 scale, float2 offset)
{
#if defined(USE_GENERATED_UV)
    float3 bf = normalize(abs(i.normal));
    bf /= dot(bf, (float3) 1);

    float3 localTrilanarPos = i.objectSpacePos.xyz;

	if (bf.x > bf.y && bf.x > bf.z)
	{
		float2 tx = localTrilanarPos.yz * scale.x;
		return tex2D(_texture, tx - offset);
	}
	
	if (bf.y > bf.x && bf.y > bf.z)
	{
		float2 ty = localTrilanarPos.zx * scale.x;
		return tex2D(_texture, ty + offset);
	}
	
	float2 tz = localTrilanarPos.xy * scale.x;
	return tex2D(_texture, tz + float2(offset.y, offset.x));
#else
    return tex2D(_texture, i.uv * scale.xy + offset.xy);
#endif
}

void _FXV_ShieldGetVDN(v2f i, inout half3 vdn)
{
#if RIM_SOURCE_NORMAL
	vdn = 1.0 - max(dot(i.rimV, i.rimN), 0.0);
#elif RIM_SOURCE_TEXTURE
	float2 uv = i.uv *_RimTexture_ST.xy + _RimTexture_ST.zw;
	vdn = tex2D(_RimTexture, uv).rgb;
#endif
}

void _FXV_ShieldDepthParams(v2f i, inout half depthVisibility, inout half depthRim)
{
    depthVisibility = 1.0;
    //depthRim = 0.0;
	
	half depthValue = _FXV_GetLinearEyeDepth(i.screenPos);
	half depthDiff = (depthValue - i.depth);
	//depthVisibility = max(_Preview, step(-depthDiff - 0.001, 0.0)); // -0.001 for hit depth fighting fix

    depthRim = (1.0 - _Preview) * (1.0 - min(1.0, depthDiff / _OverlapRim)) * _OverlapRimPower * _FEATURE_DepthOverlapRim;
}

void _FXV_ShieldDepthParamsPostprocess(v2f i, inout half depthVisibility, inout half depthRim)
{
	half depthValue = _FXV_GetLinearEyeDepth(i.screenPos);
	half depthDiff = (depthValue - i.depth);
	depthVisibility = max(_Preview, step(-depthDiff - 0.001, 0.0)); // -0.001 for hit depth fighting fix

    depthRim = (1.0 - _Preview) * (1.0 - min(1.0, depthDiff / _OverlapRim)) * _OverlapRimPower * _FEATURE_DepthOverlapRim;
}

void _FXV_ShieldColorRim(half3 vdn, half4 distortCoord, half depthRim, inout half colorRim)
{
	colorRim = smoothstep(_ColorRimMin, _ColorRimMax, vdn.x) * GetHitInfluence(_ColorRimHitInfluence);
	colorRim = max(colorRim, depthRim);
	colorRim *= distortCoord.z * distortCoord.w;
	colorRim *= _FEATURE_ColorRim;
}

void _FXV_DistortCoord(v2f i, inout half4 distortCoord)
{
#if USE_DISTORTION_FOR_MAIN_TEXTURE
	distortCoord = _FXV_SampleTexture(i, _DistortTex, _DistortTex_ST.xy, _DistortTex_ST.zw + float2(_Time.x * _DistortionSpeedX, _Time.x * _DistortionSpeedY));
	//  tex2D(_DistortTex, i.uv*_DistortTex_ST.xy + _DistortTex_ST.zw + float2(_Time.x * _DistortionSpeedX, _Time.x * _DistortionSpeedY));
	
	if (_RimVariationScale > 0.0)
	{
		half4 normDistort = tex2D(_DistortTex, normalize(float2(i.rimN.x, i.rimN.y)) * _RimVariationFrequency + float2(_Time.x * _DistortionSpeedX, _Time.x * _DistortionSpeedY));
		distortCoord.z = lerp(1.0, pow(abs(normDistort.z), _FadePow) * (1.0 - i.rimN.z), _RimVariationScale);
	}
	else
	{
		distortCoord.z = 1.0;
	}

	distortCoord.xy -= float2(0.5, 0.5);
	distortCoord.xy *= 2.0 * _DistortionFactor;
	distortCoord.w = lerp(1.0, pow(abs(distortCoord.w), _FadePow), _FadeScale);
#endif
}

void _FXV_ShieldMainTexture(v2f i, half3 vdn, half depthRim, half4 distortCoord, half2 additionalUV, inout half4 tex, inout half texRim)
{
#if USE_MAIN_TEXTURE
	texRim = smoothstep(_TextureRimMin, _TextureRimMax, vdn.y);
	texRim = max(texRim, depthRim);
	texRim *= distortCoord.z;

	//half2 texUV = i.uv*_MainTex_ST.xy + _MainTex_ST.zw + distortCoord.xy * _TextureDistortionInfluence + half2(_TextureScrollX *_Time.x, _TextureScrollY * _Time.x) + additionalUV;

	half4 mainColor = _FXV_SampleTexture(i, _MainTex, _MainTex_ST.xy, _MainTex_ST.zw + distortCoord.xy * _TextureDistortionInfluence + half2(_TextureScrollX *_Time.x, _TextureScrollY * _Time.x) + additionalUV);
	//tex2D(_MainTex, texUV);

	//#if USE_MAIN_TEXTURE_ANIMATION
		half animatedT = (sin(_Time.x * _TextureAnimationSpeed + mainColor.r * _TextureAnimationFactor) + 1.0) * 0.5;
		half t = 1.0 + (animatedT - 1.0) * _FEATURE_MainTextureAnimation;
		//half t = (sin(_Time.x * _TextureAnimationSpeed + mainColor.r * _TextureAnimationFactor) + 1.0f) * 0.5f;
		tex = clamp((mainColor * t), 0.0, 1.0) * _TexturePower * GetHitInfluence(_TextureHitInfluence) * distortCoord.w;
	//#else
	//	tex = mainColor * _TexturePower * GetHitInfluence(_TextureHitInfluence) * distortCoord.w;
	//#endif
#endif
}

void _FXV_ShieldPatternTexture(v2f i, half3 vdn, half4 distortCoord, half2 additionalUV, inout half4 pattern, inout half patternRim)
{
#if USE_PATTERN_TEXTURE
	patternRim = 1.0 - smoothstep(_PatternRimMin, _PatternRimMax, vdn.z);
	patternRim *= distortCoord.z;

	//half2 patternUV = i.uv * _PatternTex_ST.xy + _PatternTex_ST.zw + distortCoord.xy * _PatternDistortionInfluence + half2(_PatternScrollX *_Time.x, _PatternScrollY * _Time.x) + additionalUV;
	half4 patternColor = _FXV_SampleTexture(i, _PatternTex, _PatternTex_ST.xy, _PatternTex_ST.zw + distortCoord.xy * _PatternDistortionInfluence + half2(_PatternScrollX *_Time.x, _PatternScrollY * _Time.x) + additionalUV);
	//tex2D(_PatternTex, patternUV);

	//#if USE_PATTERN_TEXTURE_ANIMATION
		half animatedT = (sin(_Time.x * _PatternAnimationSpeed + patternColor.r * _PatternAnimationFactor) + 1.0) * 0.5;
		half t = 1.0 + (animatedT - 1.0) * _FEATURE_PatternTextureAnimation;
		//half t = (sin(_Time.x * _PatternAnimationSpeed + patternColor.r * _PatternAnimationFactor) + 1.0f) * 0.5f;
		pattern = clamp((patternColor * t), 0.0, 1.0) * _PatternPower * GetHitInfluence(_PatternHitInfluence) * distortCoord.w;
	//#else
	//	pattern = patternColor * _PatternPower * GetHitInfluence(_PatternHitInfluence) * distortCoord.w;
	//#endif
#endif
}

void _FXV_ShieldDirectionVisibility(v2f i, inout half dirVisibility)
{
	half3 diff = (_ShieldDirection.xyz - i.objectSpacePos.xyz) * _DirectionVisibility;
	dirVisibility = lerp(1.0, clamp(dot(diff, normalize(_ShieldDirection.xyz)), 0.0, 1.0), _FEATURE_DirectionalVisibility);
}

void _FXV_ShieldActivationRim(v2f i, half colorRim, half4 tex, half texRim, half4 pattern, half patternRim, inout half activationRim, inout half activationVisibility)
{
#if ACTIVATION_EFFECT_ON
		half unscaledTime = FXV_ACCESS_PROP(_ActivationTime);
		const half scaledTime = unscaledTime * (1.0 + _ActivationRim  + tex.r * _ActivationInluenceByMainTex + pattern.r * _ActivationInluenceByPatternTex);
		half activationVal = 0.0;
		if (_FEATURE_ActivationType == 0) // FINALCOLOR
		{
			activationVal = clamp(pow(abs(colorRim + tex.r + pattern.r), 0.02),0,1);
		}
		else if (_FEATURE_ActivationType == 1) // FINALCOLOR_UVX
		{
			activationVal = (pow(abs(tex.r + pattern.r), 0.02) + i.uv.x) * 0.5;
		}
		else if (_FEATURE_ActivationType == 2) // FINALCOLOR_UVY
		{
			activationVal = (pow(abs(tex.r + pattern.r), 0.02) + i.uv.y) * 0.5;
		}
		else if (_FEATURE_ActivationType == 3) // CUSTOM_TEX
		{
			float2 baseUv = i.uv;
			activationVal = clamp(tex2D(_ActivationTex, baseUv * _ActivationTex_ST.xy + _ActivationTex_ST.zw).r + tex.r * _ActivationInluenceByMainTex + pattern.r * _ActivationInluenceByPatternTex, 0, 1);
		}
		else if (_FEATURE_ActivationType == 4) // UVX
		{
			activationVal = i.uv.x;
		}
		else if (_FEATURE_ActivationType == 5) // UVY
		{
			activationVal = i.uv.y;
		}
		else if (_FEATURE_ActivationType == 6) // POSY
		{			
			activationVal = (pow(abs(tex.r + pattern.r), 0.02) + (i.objectSpacePos.y/_ShieldBounds.y + 0.5)) * 0.5;
		}
		else if (_FEATURE_ActivationType == 7) // POSX
		{
			activationVal = (pow(abs(tex.r + pattern.r), 0.02) + (i.objectSpacePos.x/_ShieldBounds.x + 0.5)) * 0.5;
		}
		else if (_FEATURE_ActivationType == 8) // POSZ
		{
			activationVal = (pow(abs(tex.r + pattern.r), 0.02) + (i.objectSpacePos.z/_ShieldBounds.z + 0.5)) * 0.5;
		}
		float fadeOut = saturate(FXV_ACCESS_PROP(_ActivationTime01)/_ActivationFadeOut);
	
		activationVisibility = step(activationVal, scaledTime);
		half t = 2.0*abs(clamp(((scaledTime - activationVal) / _ActivationRim), 0.0, 1.0) - 0.5);

		activationRim = lerp(1.0, 0.0, t) * _ActivationRimPower * fadeOut;
#endif
}

void _FXV_ShieldBasicColor(half colorRim, half activationRim, inout half4 basicColor)
{
#if ACTIVATION_EFFECT_ON
	colorRim = max(colorRim, activationRim);
#endif
    basicColor = _Color * colorRim * _FEATURE_ColorRim;
} 

void _FXV_ShieldHitEffect(v2f i, half4 distorCoord, inout half2 rippleUV, inout half rippleFade, inout half4 hitTex)
{
#if HIT_EFFECT_ON
		float3 hitPos = FXV_ACCESS_HIT_PROP(_HitPos).xyz;
		float3 diff = hitPos - _FXV_ObjectToWorldPos(i.objectSpacePos);

		float dist = length(diff);

		float hitR = FXV_ACCESS_HIT_PROP(_HitRadius);

		float3 hitTan1 = FXV_ACCESS_HIT_PROP(_HitTan1).xyz;
		float3 hitTan2 = FXV_ACCESS_HIT_PROP(_HitTan2).xyz;

		float2 dirFX = normalize(float2(dot(diff, hitTan1), dot(diff, hitTan2)));

		float fadeVariance = max(0.0, dot(normalize(_FXV_ObjectToWorldDir(i.normal)), cross(hitTan1, hitTan2)));

#if USE_HIT_VARIATION
		float hitT = FXV_ACCESS_HIT_PROP(_HitT);
		float4 variationTex = tex2D(_HitVariationTex, (dirFX * hitT * 0.5 + float2(0.5, 0.5)));

		hitR *= (0.5 + lerp(0.5, variationTex.r, _HitVariationScale));
		fadeVariance *= lerp(1.0, variationTex.r, _HitVariationColor);
#endif

		if (dist > hitR)
			discard;

#if USE_HIT_RIPPLE
		dirFX *= length(diff)/hitR;

		hitTex = tex2D(_HitRippleTex, dirFX * 0.5 + float2(0.5, 0.5)) * fadeVariance;

		half4 hitColor = FXV_ACCESS_HIT_PROP(_HitColor);
		rippleUV = normalize(dirFX.xy) * hitTex.g * 0.05 * _HitRippleDistortion * hitColor.a;
		rippleUV.y = -rippleUV.y;
#else
		half hitAttenuation = (1.0 - min(dist / FXV_ACCESS_HIT_PROP(_HitRadius), 1.0));
		hitTex = half4(hitAttenuation, hitAttenuation, hitAttenuation, hitAttenuation) * fadeVariance;         
		rippleUV = half2(0,0);
#endif

#else
		hitTex = half4(0.0, 0.0, 0.0, 0.0);
#endif
}

half4 _FXV_GetFinalColor_Hit(v2f i, half4 tex, half4 pattern, half4 hitTex, half hitRippleFade, half depthVisibility, half dirVisibility, half activationVisibility, half postprocess)
{
	half4 retColor;

#if HIT_EFFECT_ON
	half4 hitColor = FXV_ACCESS_HIT_PROP(_HitColor);
	half4 hitTexColor = FXV_ACCESS_HIT_PROP(_HitTexColor);
	retColor.rgb = 2.0 * ((max(tex.rgb * _TextureColor.rgb, pattern.rgb * _PatternColor.rgb) * hitTex.rgb * hitTexColor.rgb * dirVisibility + _HitColorAffect * hitTex.rgb * hitColor.rgb)) * _HitPower * hitColor.a;// * hitRippleFade;
	retColor.a = depthVisibility * activationVisibility * hitTex.r * _HitPower * hitColor.a * postprocess;
	retColor.a = saturate(retColor.a);
#endif

	return retColor;
}

half4 _FXV_GetFinalColor_Default(v2f i, half4 basicColor, half4 tex, half texRim, half4 pattern, half patternRim, half depthVisibility, half depthRim, half activationRim, half activationVisibility, half dirVisibility, float facing, half postprocess)
{
	tex *= texRim;
	pattern *= patternRim;

	float alpha = 1.0;
	float alphaFromEffects = tex.r * _TextureColor.a + pattern.r * _PatternColor.a + basicColor.a;

#if ACTIVATION_EFFECT_ON
	alpha = max(activationRim * lerp(1.0, dirVisibility, _DirVisActivationInfluence), dirVisibility * activationVisibility * alphaFromEffects) * depthVisibility * postprocess;
#else
    alpha = dirVisibility * depthVisibility * alphaFromEffects * postprocess;
#endif

	if (facing == -1.0)
	{
		alpha *= _BackfacesIntensity;
	}

	half4 retColor;
	
	retColor.rgb = _FXV_IntensityCorrection(pattern.rgb * _PatternColor.rgb + tex.rgb * _TextureColor.rgb + basicColor.rgb, _GlobalIntensity);
    retColor.a = saturate(_FXV_AlphaCorrection(alpha, _GlobalAlphaCurve));
	
	return retColor;
}

half4 _FXV_GetFinalColor_Refraction(v2f i, half3 vdn, half4 basicColor, half4 tex, half texRim, half4 pattern, half patternRim, half2 distortCoord, half depthVisibility, half depthRim, half activationRim, half activationVisibility, half dirVisibility)
{
	half4 retColor;

#ifdef USE_REFRACTION
	tex *= texRim;
	pattern *= patternRim;

	float alpha = 1.0;
	float alphaFromEffects = tex.r * _TextureColor.a + pattern.r * _PatternColor.a + basicColor.a;

	#if ACTIVATION_EFFECT_ON
		alpha = max(activationRim * lerp(1.0, dirVisibility, _DirVisActivationInfluence), dirVisibility * activationVisibility * alphaFromEffects) * depthVisibility;
	#else
		alpha = dirVisibility * depthVisibility * alphaFromEffects;
	#endif

	float4 grabCoords = i.grabScreenPos;
	float refractionRim = smoothstep(_RefractionRimMin, _RefractionRimMax, vdn.z);

	grabCoords.xy += (basicColor.rg * _RefractionColorRimInfluence + pattern.rb * _RefractionPatternRimInfluence - float2(tex.r, -pattern.g) * _RefractionTextureRimInfluence + distortCoord) * _RefractionScale * refractionRim * dirVisibility;

	#ifdef FXV_SHIELD_URP
		float4 grabColor = _FXV_SampleScreenTexture(grabCoords); 
	#else
		float4 grabColor = _FXV_SampleScreenTexture(grabCoords);
	#endif

	retColor.rgb = grabColor.rgb * (1.0 - _Preview) * (1.0 + _RefractionBackgroundExposure * dirVisibility) + _FXV_IntensityCorrection((pattern.rgb * _PatternColor.rgb + tex.rgb * _TextureColor.rgb + basicColor.rgb) * _FXV_AlphaCorrection(alpha, _GlobalAlphaCurve), _GlobalIntensity);

	#if ACTIVATION_EFFECT_ON
		retColor.a = activationVisibility * depthVisibility;
	#else
		retColor.a = depthVisibility;
	#endif
#endif
	
    retColor.a = saturate(retColor.a);
	
	return retColor;
}


#endif
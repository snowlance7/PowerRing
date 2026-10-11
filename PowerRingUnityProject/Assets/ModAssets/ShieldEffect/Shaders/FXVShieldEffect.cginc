#ifndef FXV_SHIELD_EFFECT_INCLUDED
#define FXV_SHIELD_EFFECT_INCLUDED

#define FXV_ACCESS_PROP(p) UNITY_ACCESS_INSTANCED_PROP(Props, p)
#define FXV_DEFINE_PROP(t, p) UNITY_DEFINE_INSTANCED_PROP( t, p )

#if USE_TEXTURE_UV1
	#define FXV_TEXCOORD_ID TEXCOORD1
#else
	#define FXV_TEXCOORD_ID TEXCOORD0
#endif

#define FXV_SHIELD_EFFECT_APPDATA	float4 vertex : POSITION; \
									float3 normal : NORMAL; \
									float4 tangent : TANGENT; \
									float2 uv : FXV_TEXCOORD_ID; \
									UNITY_VERTEX_INPUT_INSTANCE_ID 

#if defined(FXV_SHIELD_URP) || defined(FXV_SHIELD_BUILTIN)
	#define FXV_SHIELD_EFFECT_V2F_COORDS	float2 uv : TEXCOORD0; \
											float4 pos : SV_POSITION; \
											float3 rimN : TEXCOORD1; \
											float3 rimV : TEXCOORD2; \
											float depth : TEXCOORD3; \
											float4 screenPos : TEXCOORD4; \
											float4 objectSpacePos : TEXCOORD5; \
											float3 normal : TEXCOORD6; \
											float4 tangent : TEXCOORD7; \
											float4 grabScreenPos : TEXCOORD8; \
											UNITY_VERTEX_INPUT_INSTANCE_ID \
											UNITY_VERTEX_OUTPUT_STEREO 
#else
	#define FXV_SHIELD_EFFECT_V2F_COORDS	float2 uv : TEXCOORD0; \
											float4 pos : SV_POSITION; \
											float3 rimN : TEXCOORD1; \
											float3 rimV : TEXCOORD2; \
											float depth : TEXCOORD3; \
											float4 screenPos : TEXCOORD4; \
											float4 objectSpacePos : TEXCOORD5; \
											float3 normal : TEXCOORD6; \
											float4 tangent : TEXCOORD7; \
											float4 grabScreenPos : TEXCOORD8; \
											UNITY_VERTEX_INPUT_INSTANCE_ID 
#endif

#define FXV_SHIELD_EFFECT_FRAGMENT_DEFAULT(i)   UNITY_SETUP_INSTANCE_ID(i) \
												if (facing < 0.0) \
													i.rimN = -i.rimN;


#if defined(FXV_SHIELD_URP)
	#define FXV_DEFINE_TEXTURE_SAMPLER(t) TEXTURE2D(t); \
										  SAMPLER(sampler_##t);
	#define FXV_TEXTURE_SAMPLER_PARAMS(t) t, sampler_##t
	#define FXV_TEXTURE_SAMPLER_PARAMS_DEF(t) Texture2D t, SamplerState sampler_##t

	#define FXV_SHIELD_EFFECT_VERTEX_DEFAULT(v, o)	UNITY_SETUP_INSTANCE_ID(v); \
													UNITY_TRANSFER_INSTANCE_ID(v, o); \
													UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); \
													half isOrtho = unity_OrthoParams.w; \
													VertexPositionInputs vertInputs = GetVertexPositionInputs (v.vertex.xyz); \
													o.pos = vertInputs.positionCS; \
													o.uv = v.uv; \
													o.screenPos = ComputeScreenPos(o.pos); \
													o.grabScreenPos = o.screenPos; \
													o.objectSpacePos = v.vertex; \
													o.normal = v.normal; \
													o.tangent = v.tangent; \
													o.rimN = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, _FXV_GetNormal(v.normal, v.vertex.xyz))); \
													float3 p = vertInputs.positionVS; \
													o.rimV = lerp(normalize(-p), half3(0.0,0.0,1.0), isOrtho); \
													o.depth = _FXV_ComputeVertexLinearEyeDepth(o.screenPos);
#elif defined(FXV_SHIELD_HDRP)
	#include "../HDRP/Shaders/FXVShieldEffectHDRP.cginc"

	#define FXV_DEFINE_TEXTURE_SAMPLER(t) TEXTURE2D(t); \
										  SAMPLER(sampler_##t);
	#define FXV_TEXTURE_SAMPLER_PARAMS(t) 0
	#define FXV_TEXTURE_SAMPLER_PARAMS_DEF(t) float dummy

	#define FXV_SHIELD_EFFECT_VERTEX_DEFAULT(v, o)	UNITY_SETUP_INSTANCE_ID(v); \
													UNITY_TRANSFER_INSTANCE_ID(v, o); \
													half isOrtho = unity_OrthoParams.w; \
													o.pos = TransformWorldToHClip(TransformObjectToWorld(v.positionOS)); \
													o.uv = v.uv0; \
													o.screenPos = ComputeScreenPos(o.pos); \
													o.grabScreenPos = o.screenPos; \
													o.objectSpacePos = float4(v.positionOS, 0); \
													o.normal = v.normalOS; \
													o.tangent = v.tangentOS; \
													o.rimN = normalize(mul((float3x3)transpose(mul(UNITY_MATRIX_I_M, Inverse(UNITY_MATRIX_V))), _FXV_GetNormal(v.normalOS, v.positionOS.xyz))); \
													float3 p = TransformWorldToView(TransformObjectToWorld(v.positionOS)); \
													o.rimV = lerp(normalize(-p), half3(0.0,0.0,1.0), isOrtho); \
													o.depth = _FXV_ComputeVertexLinearEyeDepth(o.screenPos);
#else
   	#define FXV_DEFINE_TEXTURE_SAMPLER(t) sampler2D t;
	#define FXV_TEXTURE_SAMPLER_PARAMS(t) t
	#define FXV_TEXTURE_SAMPLER_PARAMS_DEF(t) sampler2D t

	#define FXV_SHIELD_EFFECT_VERTEX_DEFAULT(v, o)	UNITY_SETUP_INSTANCE_ID(v); \
													UNITY_TRANSFER_INSTANCE_ID(v, o); \
                                                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); \
													half isOrtho = unity_OrthoParams.w; \
													o.pos = UnityObjectToClipPos(v.vertex); \
													o.uv = v.uv; \
													o.screenPos = ComputeScreenPos(o.pos); \
													o.grabScreenPos = ComputeGrabScreenPos(o.pos); \
													o.objectSpacePos = v.vertex; \
													o.normal = v.normal; \
													o.tangent = v.tangent; \
													o.rimN = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, _FXV_GetNormal(v.normal, v.vertex.xyz))); \
													float3 p = UnityObjectToViewPos(v.vertex); \
													o.rimV = lerp(normalize(-p), half3(0.0,0.0,1.0), isOrtho); \
													o.depth = _FXV_ComputeVertexLinearEyeDepth(o.screenPos);
#endif

sampler2D _RimTexture;
sampler2D _MainTex;
sampler2D _PatternTex;
sampler2D _DistortTex;
sampler2D _ActivationTex;
sampler2D _HitRippleTex;
sampler2D _HitVariationTex;

#if !defined(FXV_SHIELD_HDRP)
#ifdef FXV_SHIELD_URP
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#else
	UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
#endif
#endif

#ifdef FXV_SHIELD_URP
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
#else
	FXV_DEFINE_TEXTURE_SAMPLER(_CameraOpaqueTextureBuiltin);
#endif


//Property definitions block
//--------------------------------------------------------------------------------------------------------------------------

#ifdef FXV_SHIELD_URP
	CBUFFER_START(UnityPerMaterial)
#endif

float _FEATURE_ActivationType;
float _FEATURE_ColorRim;
float _FEATURE_DirectionalVisibility;
float _FEATURE_DepthOverlapRim;
float _FEATURE_MainTextureAnimation;
float _FEATURE_PatternTextureAnimation;

float4 _RimTexture_ST;
float4 _MainTex_ST;
float4 _PatternTex_ST;
float4 _DistortTex_ST;

float _NormalCorrection;
float _GlobalIntensity;
float _GlobalAlphaCurve;
float _PostprocessIntensity;
float _BackfacesIntensity;
half4 _Color;
float _ColorRimMin;
float _ColorRimMax;
float _ColorRimHitInfluence;
half4 _TextureColor;
float _TexturePower;
float _TextureRimMin;
float _TextureRimMax;
float _TextureScrollX;
float _TextureScrollY;
float _TextureDistortionInfluence;
float _TextureHitInfluence;
float _TextureAnimationSpeed;
float _TextureAnimationFactor;
float _DistortionFactor;
float _DistortionSpeedX;
float _DistortionSpeedY;
float _RimVariationScale;
float _RimVariationFrequency;
float _FadeScale;
float _FadePow;
half4 _PatternColor;
float _PatternPower;
float _PatternRimMin;
float _PatternRimMax;
float _PatternScrollX;
float _PatternScrollY;
float _PatternDistortionInfluence;
float _PatternHitInfluence;
float _PatternAnimationSpeed;
float _PatternAnimationFactor;
float _OverlapRim;
float _OverlapRimPower;
float _DirectionVisibility;
float _DirVisActivationInfluence;
float4 _ShieldDirection;
float4 _ShieldBounds;
float4 _ActivationTex_ST;
float _ActivationRim;
float _ActivationRimPower;
float _ActivationFadeOut;
float _ActivationInluenceByMainTex;
float _ActivationInluenceByPatternTex;
float _RefractionScale;
float _RefractionRimMin;
float _RefractionRimMax;
float _RefractionBackgroundExposure;
float _RefractionColorRimInfluence;
float _RefractionTextureRimInfluence;
float _RefractionPatternRimInfluence;

#if HIT_EFFECT_ON
float _HitPower;
float _HitColorAffect;
float _HitRippleDistortion;
float _HitRefractionScale;
float _HitVariationScale;
float _HitVariationColor;
#endif

float _Preview;

//Instanced properties definitions
UNITY_INSTANCING_BUFFER_START(Props)

#if HIT_EFFECT_ON
#if !defined(HIT_EFFECT_SKINNED_MESH)
FXV_DEFINE_PROP(half4, _HitColor)
FXV_DEFINE_PROP(half4, _HitTexColor)
FXV_DEFINE_PROP(float4, _HitPos)
FXV_DEFINE_PROP(float4, _HitTan1)
FXV_DEFINE_PROP(float4, _HitTan2)
FXV_DEFINE_PROP(float, _HitT)
FXV_DEFINE_PROP(float, _HitRadius)
#endif
#endif

FXV_DEFINE_PROP(float, _VisuallyActive)
FXV_DEFINE_PROP(float, _ActivationTime)
FXV_DEFINE_PROP(float, _ActivationTime01)
FXV_DEFINE_PROP(float, _HitEffectValue)

UNITY_INSTANCING_BUFFER_END(Props)

#ifdef FXV_SHIELD_URP
	CBUFFER_END
#endif


#if HIT_EFFECT_SKINNED_MESH
	half4 _HitColor;
	half4 _HitTexColor;
	float4 _HitPos;
	float4 _HitTan1;
	float4 _HitTan2;
	float _HitT;
	float _HitRadius;

	#define FXV_ACCESS_HIT_PROP(p) p
#else
	#define FXV_ACCESS_HIT_PROP(p) FXV_ACCESS_PROP(p)
#endif

void _FXV_CheckVisuallyActive()
{
#if !HIT_EFFECT_ON
    if (FXV_ACCESS_PROP(_VisuallyActive) == 0.0)
    {
        discard;
    }
#endif
}

float3 _FXV_GetNormal(float3 inNormal, float3 inVertex)
{
    return lerp(normalize(inNormal), normalize(inVertex), _NormalCorrection);
}

//--------------------------------------------------------------------------------------------------------------------------

// Z buffer to linear depth. (not perspecitve corrected)
// Works in all cases.
// Typically, this is the cheapest variant, provided you've already computed 'positionWS'.
// Assumes that the 'positionWS' is in front of the camera.
float _FXV_ComputeVertexDepth(float3 positionWS)
{
#if defined(FXV_SHIELD_URP)
	return LinearEyeDepth(positionWS, GetWorldToViewMatrix());
#elif defined(FXV_SHIELD_HDRP)
	return LinearEyeDepth(positionWS, GetWorldToViewMatrix());
#else
	// calculated as in Library\PackageCache\com.unity.render-pipelines.core@14.0.11\ShaderLibrary\Common.hlsl
    float viewSpaceZ = mul(UNITY_MATRIX_V, float4(positionWS, 1.0)).z;
    // If the matrix is right-handed, we have to flip the Z axis to get a positive value.
    return abs(viewSpaceZ);
#endif
}

// Z buffer to linear depth. (perspecitve corrected compatible with _FXV_GetLinearEyeDepth below)
float _FXV_ComputeVertexLinearEyeDepth(half4 screenPos)
{
    if (unity_OrthoParams.w == 0) // perspective
    {
#if defined(FXV_SHIELD_URP)
		return LinearEyeDepth(screenPos.z / screenPos.w, _ZBufferParams);
#elif defined(FXV_SHIELD_HDRP)
		return LinearEyeDepth(screenPos.z / screenPos.w, _ZBufferParams);
#else
        return LinearEyeDepth(screenPos.z / screenPos.w);
#endif
    }
	else
    {
		// Orthographic linear depth
		// near = _ProjectionParams.y;
		// far = _ProjectionParams.z;
		// calculated as in Library\PackageCache\com.unity.render-pipelines.universal@14.0.11\ShaderLibrary\ShaderVariablesFunctions.hlsl
#if UNITY_REVERSED_Z
		return _ProjectionParams.z - (_ProjectionParams.z - _ProjectionParams.y) * (screenPos.z / screenPos.w);
#else
        return _ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) * (screenPos.z / screenPos.w);
#endif
    }
}

float _FXV_GetLinearEyeDepth(half4 screenPos)
{
#if defined(FXV_SHIELD_URP)
	float z = SampleSceneDepth( screenPos.xy / screenPos.w );
#elif defined(FXV_SHIELD_HDRP)
	float z = SampleCameraDepth( screenPos.xy / screenPos.w );
#else
    float z = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(screenPos));
#endif

    if (unity_OrthoParams.w == 0) // perspective
    {
		// Perspective linear depth
#if defined(FXV_SHIELD_URP)
		return LinearEyeDepth(z, _ZBufferParams);
#elif defined(FXV_SHIELD_HDRP)
		return LinearEyeDepth(z, _ZBufferParams);
#else
        return LinearEyeDepth(z);
#endif
    }
	else
    {
		// Orthographic linear depth
		// near = _ProjectionParams.y;
		// far = _ProjectionParams.z;
		// calculated as in Library\PackageCache\com.unity.render-pipelines.universal@14.0.11\ShaderLibrary\ShaderVariablesFunctions.hlsl
#if UNITY_REVERSED_Z
		return _ProjectionParams.z - (_ProjectionParams.z - _ProjectionParams.y) * z;
#else
        return _ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) * z;
#endif
    }
}

float4 _FXV_SampleScreenTexture(half4 grabScreenPos)
{
#if defined(FXV_SHIELD_URP)
	return float4(SampleSceneColor(grabScreenPos.xy / grabScreenPos.w), 1.0);
#elif defined(FXV_SHIELD_HDRP)
	return float4(SampleCameraColor( grabScreenPos.xy / grabScreenPos.w ), 1.0);
#else
    return tex2Dproj(_CameraOpaqueTextureBuiltin, UNITY_PROJ_COORD(grabScreenPos));
#endif
}

#endif
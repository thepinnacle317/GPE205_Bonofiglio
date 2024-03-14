#ifndef FXV_SHIELD_EFFECT_INCLUDED
#define FXV_SHIELD_EFFECT_INCLUDED

#if USE_MATERIAL_PROPERTY_BLOCKS
	#define FXV_ACCESS_PROP(p) UNITY_ACCESS_INSTANCED_PROP(Props, p)
	#define FXV_DEFINE_PROP(t, p) UNITY_DEFINE_INSTANCED_PROP( t, p )
#else
	#define FXV_ACCESS_PROP(p) p
	#define FXV_DEFINE_PROP(t, p) t p;
#endif

#define FXV_SHIELD_EFFECT_APPDATA	float4 vertex : POSITION; \
									float3 normal : NORMAL; \
									float4 tangent : TANGENT; \
									float2 uv : TEXCOORD0; \
									UNITY_VERTEX_INPUT_INSTANCE_ID

#define FXV_SHIELD_EFFECT_V2F_COORDS	float2 uv : TEXCOORD0; \
										float4 pos : SV_POSITION; \
										float3 rimN : TEXCOORD1; \
										float3 rimV : TEXCOORD2; \
										float depth : TEXCOORD3; \
										float4 screenPos : TEXCOORD4; \
										float4 objectSpacePos : TEXCOORD5; \
										float3 normal : TEXCOORD6; \
										float4 tangent : TEXCOORD7; \
										UNITY_VERTEX_INPUT_INSTANCE_ID

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
													half isOrtho = unity_OrthoParams.w; \
													VertexPositionInputs vertInputs = GetVertexPositionInputs (v.vertex.xyz); \
													o.pos = vertInputs.positionCS; \
													o.uv = v.uv; \
													o.screenPos = ComputeScreenPos(o.pos); \
													o.objectSpacePos = v.vertex; \
													o.normal = v.normal; \
													o.tangent = v.tangent; \
													o.rimN = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal)); \
													float3 p = vertInputs.positionVS; \
													o.rimV = lerp(normalize(-p), half3(0.0,0.0,1.0), isOrtho); \
													o.depth = _FXV_ComputeVertexDepth(p, o.screenPos);
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
													o.objectSpacePos = float4(v.positionOS, 0); \
													o.normal = v.normalOS; \
													o.tangent = v.tangentOS; \
													o.rimN = normalize(mul((float3x3)transpose(mul(UNITY_MATRIX_I_M, Inverse(UNITY_MATRIX_V))), v.normalOS)); \
													float3 p = TransformWorldToView(TransformObjectToWorld(v.positionOS)); \
													o.rimV = lerp(normalize(-p), half3(0.0,0.0,1.0), isOrtho); \
													o.depth = _FXV_ComputeVertexDepth(p, o.screenPos);
#else
   	#define FXV_DEFINE_TEXTURE_SAMPLER(t) sampler2D t;
	#define FXV_TEXTURE_SAMPLER_PARAMS(t) t
	#define FXV_TEXTURE_SAMPLER_PARAMS_DEF(t) sampler2D t

	#define FXV_SHIELD_EFFECT_VERTEX_DEFAULT(v, o)	UNITY_SETUP_INSTANCE_ID(v); \
													UNITY_TRANSFER_INSTANCE_ID(v, o); \
													half isOrtho = unity_OrthoParams.w; \
													o.pos = UnityObjectToClipPos(v.vertex); \
													o.uv = v.uv; \
													o.screenPos = ComputeScreenPos(o.pos); \
													o.objectSpacePos = v.vertex; \
													o.normal = v.normal; \
													o.tangent = v.tangent; \
													o.rimN = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal)); \
													float3 p = UnityObjectToViewPos(v.vertex); \
													o.rimV = lerp(normalize(-p), half3(0.0,0.0,1.0), isOrtho); \
													o.depth = _FXV_ComputeVertexDepth(p, o.screenPos);
#endif

sampler2D _RimTexture;
sampler2D _MainTex;
sampler2D _PatternTex;
sampler2D _DistortTex;
sampler2D _ActivationTex;
sampler2D _HitRippleTex;

#if !defined(FXV_SHIELD_HDRP)
FXV_DEFINE_TEXTURE_SAMPLER(_CameraDepthTexture);
#endif

#if USE_REFRACTION
	#ifdef FXV_SHIELD_URP
		FXV_DEFINE_TEXTURE_SAMPLER(_CameraOpaqueTexture);
	#else
		FXV_DEFINE_TEXTURE_SAMPLER(_CameraOpaqueTextureBuiltin);
	#endif
#endif

//Property definitions block
//--------------------------------------------------------------------------------------------------------------------------

#ifdef FXV_SHIELD_URP
	CBUFFER_START(UnityPerMaterial)
#endif

float4 _MainTex_ST;
float4 _PatternTex_ST;
float4 _DistortTex_ST;

float _GlobalIntensity;
float _GlobalAlphaCurve;
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
float4 _ShieldDirection;
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
float3 _WorldScale;
float _HitPower;
float _HitColorAffect;
float _HitRippleDistortion;
float _HitRefractionScale;
#endif

float _Preview;

//Instanced properties definitions
#if USE_MATERIAL_PROPERTY_BLOCKS
	UNITY_INSTANCING_BUFFER_START(Props)
#endif

#if HIT_EFFECT_ON
FXV_DEFINE_PROP(half4, _HitColor)
FXV_DEFINE_PROP(float3, _HitPos)
FXV_DEFINE_PROP(float, _HitT)
FXV_DEFINE_PROP(float, _HitRadius)
#endif

FXV_DEFINE_PROP(float, _ActivationTime)
FXV_DEFINE_PROP(float, _ActivationTime01)
FXV_DEFINE_PROP(float, _HitEffectValue)

#if USE_MATERIAL_PROPERTY_BLOCKS
	UNITY_INSTANCING_BUFFER_END(Props)
#endif

#ifdef FXV_SHIELD_URP
	CBUFFER_END
#endif

//--------------------------------------------------------------------------------------------------------------------------

half _FXV_ComputeVertexDepth(float3 viewPos, float4 screenPos)
{
	half isOrtho = unity_OrthoParams.w; // 0 - perspective, 1 - ortho);

	half near = _ProjectionParams.y;
	half far = _ProjectionParams.z;

#if defined(UNITY_REVERSED_Z)
	half depthOrtho = lerp(far, near, screenPos.z);
#else
	half depthOrtho = lerp(near, far, screenPos.z);
#endif

	return lerp(-viewPos.z, depthOrtho, isOrtho);
}

half _FXV_GetLinearEyeDepth(FXV_TEXTURE_SAMPLER_PARAMS_DEF(depthTexture), half4 screenPos)
{
	// Perspective linear depth
#if defined(FXV_SHIELD_URP)
	half z = SAMPLE_DEPTH_TEXTURE( depthTexture, sampler_depthTexture, screenPos.xy / screenPos.w );
#elif defined(FXV_SHIELD_HDRP)
	float z = SampleCameraDepth( screenPos.xy / screenPos.w );
#else
   	half z = SAMPLE_DEPTH_TEXTURE_PROJ( FXV_TEXTURE_SAMPLER_PARAMS(depthTexture), UNITY_PROJ_COORD(screenPos) );
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
#elif defined(FXV_SHIELD_HDRP)
	half eyeDepth = LinearEyeDepth(z, _ZBufferParams);
#else
   	half eyeDepth = LinearEyeDepth(z);
#endif

	return lerp(eyeDepth, depthOrtho, isOrtho);
}

float4 _FXV_SampleScreenTexture(FXV_TEXTURE_SAMPLER_PARAMS_DEF(screenTexture), half4 screenPos)
{
#if defined(FXV_SHIELD_URP)
	return screenTexture.Sample(sampler_screenTexture, screenPos.xy / screenPos.w);
#elif defined(FXV_SHIELD_HDRP)
	return float4(SampleCameraColor( screenPos.xy / screenPos.w ), 1.0);
#else
   	return tex2Dproj(screenTexture, UNITY_PROJ_COORD(screenPos));
#endif
}

#endif
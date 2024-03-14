#ifndef FXV_SHIELD_EFFECT_FUNCTIONS_INCLUDED
#define FXV_SHIELD_EFFECT_FUNCTIONS_INCLUDED

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
	return pow(alpha, (_param * 0.5 + 0.5));
#else
	return pow(abs(alpha), _param);
#endif
} 

half GetHitInfluence(half influenceScale)
{
	return (1.0 - influenceScale * (1.0 - FXV_ACCESS_PROP(_HitEffectValue)));
}

void _FXV_ShieldGetVDN(v2f i, inout half3 vdn)
{
#if RIM_SOURCE_NORMAL
	vdn = 1.0 - max(dot(i.rimV, i.rimN), 0.0);
#elif RIM_SOURCE_TEXTURE
	vdn = tex2D(_RimTexture, i.uv).rgb;
#endif
}

void _FXV_ShieldDepthParams(v2f i, inout half depthVisibility, inout half depthRim)
{
	half depthValue = _FXV_GetLinearEyeDepth(FXV_TEXTURE_SAMPLER_PARAMS(_CameraDepthTexture), i.screenPos);
	half depthDiff = (depthValue - i.depth);
	depthVisibility = max(_Preview, step(-depthDiff, 0.0));
#if USE_DEPTH_OVERLAP_RIM
	depthRim = (1.0 - _Preview) * (1.0 - min(1.0, depthDiff / _OverlapRim)) * _OverlapRimPower;
#endif
}

void _FXV_ShieldColorRim(half3 vdn, half4 distortCoord, half depthRim, inout half colorRim)
{
#if USE_COLOR_RIM
	colorRim = smoothstep(_ColorRimMin, _ColorRimMax, vdn.x) * GetHitInfluence(_ColorRimHitInfluence);
	#if USE_DEPTH_OVERLAP_RIM
		colorRim = max(colorRim, depthRim);
	#endif
	colorRim *= distortCoord.z;
#endif
}

void _FXV_DistortCoord(v2f i, inout half4 distortCoord)
{
#if USE_DISTORTION_FOR_MAIN_TEXTURE
	distortCoord = tex2D(_DistortTex, i.uv*_DistortTex_ST.xy + _DistortTex_ST.zw + float2(_Time.x * _DistortionSpeedX, _Time.x * _DistortionSpeedY));

	distortCoord.xy -= float2(0.5, 0.5);
	distortCoord.xy *= 2.0 * _DistortionFactor;
	distortCoord.z = lerp(1.0, distortCoord.z, _RimVariationScale);
	distortCoord.w = lerp(1.0, pow(distortCoord.w, _FadePow), _FadeScale);
#endif
}

void _FXV_ShieldMainTexture(v2f i, half3 vdn, half depthRim, half4 distortCoord, half2 additionalUV, inout half4 tex, inout half texRim)
{
#if USE_MAIN_TEXTURE
	texRim = smoothstep(_TextureRimMin, _TextureRimMax, vdn.y);
	#if USE_DEPTH_OVERLAP_RIM
		texRim = max(texRim, depthRim);
	#endif
	texRim *= distortCoord.z;

	half2 texUV = i.uv*_MainTex_ST.xy + _MainTex_ST.zw + distortCoord.xy * _TextureDistortionInfluence + half2(_TextureScrollX *_Time.x, _TextureScrollY * _Time.x) + additionalUV;

	half4 mainColor = tex2D(_MainTex, texUV);

	#if USE_MAIN_TEXTURE_ANIMATION
		half t = (sin(_Time.x * _TextureAnimationSpeed + mainColor.r * _TextureAnimationFactor) + 1.0f) * 0.5f;
		tex = clamp((mainColor * t), 0.0, 1.0) * _TexturePower * GetHitInfluence(_TextureHitInfluence) * distortCoord.w;
	#else
		tex = mainColor * _TexturePower * GetHitInfluence(_TextureHitInfluence) * distortCoord.w;
	#endif
#endif
}

void _FXV_ShieldPatternTexture(v2f i, half3 vdn, half4 distortCoord, half2 additionalUV, inout half4 pattern, inout half patternRim)
{
#if USE_PATTERN_TEXTURE
	patternRim = 1.0 - smoothstep(_PatternRimMin, _PatternRimMax, vdn.z);
	patternRim *= distortCoord.z;

	half2 patternUV = i.uv * _PatternTex_ST.xy + _PatternTex_ST.zw + distortCoord.xy * _PatternDistortionInfluence + half2(_PatternScrollX *_Time.x, _PatternScrollY * _Time.x) + additionalUV;

	half4 patternColor = tex2D(_PatternTex, patternUV);

	#if USE_PATTERN_TEXTURE_ANIMATION
		half t = (sin(_Time.x * _PatternAnimationSpeed + patternColor.r * _PatternAnimationFactor) + 1.0f) * 0.5f;
		pattern = clamp((patternColor * t), 0.0, 1.0) * _PatternPower * distortCoord.w;
	#else
		pattern = patternColor * _PatternPower * GetHitInfluence(_PatternHitInfluence) * distortCoord.w;
	#endif
#endif
}

void _FXV_ShieldDirectionVisibility(v2f i, inout half dirVisibility)
{
#if USE_DIRECTION_VISIBILITY
	half3 diff = (_ShieldDirection.xyz - i.objectSpacePos.xyz) * _DirectionVisibility;
	dirVisibility = clamp(dot(diff, normalize(_ShieldDirection.xyz)), 0.0, 1.0);
#endif
}

void _FXV_ShieldActivationRim(v2f i, half colorRim, half4 tex, half texRim, half4 pattern, half patternRim, inout half activationRim, inout half activationVisibility)
{
#if ACTIVATION_EFFECT_ON
		half unscaledTime = FXV_ACCESS_PROP(_ActivationTime);
		const half scaledTime = unscaledTime * (1.0 + _ActivationRim  + tex.r * _ActivationInluenceByMainTex + pattern.r * _ActivationInluenceByPatternTex);
		half activationVal = 0.0;
	#if ACTIVATION_TYPE_FINALCOLOR
		activationVal = clamp(pow(colorRim + tex.r + pattern.r, 0.02),0,1);
	#elif ACTIVATION_TYPE_FINALCOLOR_UVX
		activationVal = (pow(tex.r + pattern.r, 0.02) + i.uv.x) * 0.5;
	#elif ACTIVATION_TYPE_FINALCOLOR_UVY
		activationVal = (pow(tex.r + pattern.r, 0.02) + i.uv.y) * 0.5;
	#elif ACTIVATION_TYPE_UVX
		activationVal = i.uv.x;
	#elif ACTIVATION_TYPE_UVY
		activationVal = i.uv.y;
	#elif ACTIVATION_TYPE_CUSTOM_TEX
		activationVal = clamp(tex2D(_ActivationTex, i.uv * _ActivationTex_ST.xy + _ActivationTex_ST.zw).r + tex.r * _ActivationInluenceByMainTex + pattern.r * _ActivationInluenceByPatternTex, 0, 1);
	#endif
		activationVisibility = step(activationVal, scaledTime); 
		half t = 2.0*abs(clamp(((scaledTime - activationVal) / _ActivationRim), 0.0, 1.0) - 0.5);

		activationRim = lerp(1.0, 0.0, t) * _ActivationRimPower * clamp(FXV_ACCESS_PROP(_ActivationTime01)/_ActivationFadeOut, 0, 1);
#endif
}

void _FXV_ShieldBasicColor(half colorRim, half activationRim, inout half4 basicColor)
{
#if USE_COLOR_RIM
	#if ACTIVATION_EFFECT_ON
		colorRim = max(colorRim, activationRim);
	#endif
		basicColor = _Color * colorRim;
#endif
}

void _FXV_ShieldHitEffect(v2f i, inout half2 rippleUV, inout half rippleFade, inout half4 hitTex)
{
#if HIT_EFFECT_ON
		float3 hitPos = FXV_ACCESS_PROP(_HitPos);
		float3 diff = hitPos - i.objectSpacePos;
				
		diff.x *= _WorldScale.x;
		diff.y *= _WorldScale.y;
		diff.z *= _WorldScale.z;

		float dist = length(diff);

		float hitR = FXV_ACCESS_PROP(_HitRadius);

#if USE_HIT_RIPPLE
		float3 dirHit = diff / dist;
		float3 dir = i.normal;
		float3 up = float3(0,1,0);
		float3 hitTan1 = i.tangent.xyz;
		float3 hitTan2 = cross(dir, hitTan1);

		float2 dirFX = clamp(float2(dot(diff, hitTan1), dot(diff, hitTan2)) / max(0.05, hitR), float2(-0.99,-0.99), float2(0.99,0.99));
		hitTex = tex2D(_HitRippleTex, dirFX * 0.5 + float2(0.5, 0.5));

		half4 hitColor = FXV_ACCESS_PROP(_HitColor);
		rippleUV = normalize(dirFX.xy) * hitTex.g * 0.05 * _HitRippleDistortion * hitColor.a;
		rippleUV.y = -rippleUV.y;
		rippleFade = clamp((hitR-dist)*10.0, 0, 1);
#else
		half hitAttenuation = (1.0 - min(dist / FXV_ACCESS_PROP(_HitRadius), 1.0));
		hitTex = half4(hitAttenuation, hitAttenuation, hitAttenuation, 1.0);         
		rippleUV = half2(0,0);
		rippleFade = 1.0;
#endif

#else
		hitTex = half4(0.0, 0.0, 0.0, 0.0);
#endif
}

half4 _FXV_GetFinalColor_Hit(v2f i, half4 tex, half4 pattern, half4 hitTex, half hitRippleFade)
{
	half4 retColor;

#if HIT_EFFECT_ON
	half4 hitColor = FXV_ACCESS_PROP(_HitColor);
	retColor.rgb = 2.0 * ((max(tex.rgb * _TextureColor.rgb, pattern.rgb * _PatternColor.rgb) * hitTex.rgb + _HitColorAffect * hitTex.rgb * hitColor)) * _HitPower * hitColor.a * hitRippleFade;
	retColor.a = 1.0;
#endif

	return retColor;
}

half4 _FXV_GetFinalColor_Default(v2f i, half4 basicColor, half4 tex, half texRim, half4 pattern, half patternRim, half depthVisibility, half depthRim, half activationRim, half activationVisibility, half dirVisibility)
{
	tex *= texRim;
	pattern *= patternRim;

	float alpha = 1.0;
	float alphaFromEffects = tex.r * _TextureColor.a + pattern.r * _PatternColor.a + basicColor.a;

#if ACTIVATION_EFFECT_ON
	alpha = max(dirVisibility * activationRim, dirVisibility * activationVisibility * alphaFromEffects) * depthVisibility;
#else
	alpha = dirVisibility * depthVisibility * alphaFromEffects;
#endif

	half4 retColor;
	retColor.rgb = _FXV_IntensityCorrection(pattern.rgb * _PatternColor.rgb + tex.rgb * _TextureColor.rgb + basicColor.rgb, _GlobalIntensity);
	retColor.a =  _FXV_AlphaCorrection(alpha, _GlobalAlphaCurve); 
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
		alpha = max(dirVisibility * activationRim, dirVisibility * activationVisibility * alphaFromEffects) * depthVisibility;
	#else
		alpha = dirVisibility * depthVisibility * alphaFromEffects;
	#endif

	float4 grabCoords = i.screenPos;
	float refractionRim = smoothstep(_RefractionRimMin, _RefractionRimMax, vdn.z);

	grabCoords.xy += (basicColor.rg * _RefractionColorRimInfluence + pattern.rb * _RefractionPatternRimInfluence - float2(tex.r, -pattern.g) * _RefractionTextureRimInfluence + distortCoord) * _RefractionScale * refractionRim * dirVisibility;

	#ifdef FXV_SHIELD_URP
		float4 grabColor = _FXV_SampleScreenTexture(FXV_TEXTURE_SAMPLER_PARAMS(_CameraOpaqueTexture), grabCoords); 
	#else
		float4 grabColor = _FXV_SampleScreenTexture(FXV_TEXTURE_SAMPLER_PARAMS(_CameraOpaqueTextureBuiltin), grabCoords);
	#endif

	retColor.rgb = grabColor.rgb * (1.0 - _Preview) * (1.0 + _RefractionBackgroundExposure * dirVisibility) + _FXV_IntensityCorrection((pattern.rgb * _PatternColor.rgb + tex.rgb * _TextureColor.rgb + basicColor.rgb) * _FXV_AlphaCorrection(alpha, _GlobalAlphaCurve), _GlobalIntensity);

	#if ACTIVATION_EFFECT_ON
		retColor.a = activationVisibility * depthVisibility;
	#else
		retColor.a = depthVisibility;
	#endif
#endif

	return retColor;
}


#endif
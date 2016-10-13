#ifndef VACUUM_WIREFRAME_CORE_CGINC
#define VACUUM_WIREFRAME_CORE_CGINC

//Curved World Compatibility
//#include "Assets/VacuumShaders/Curved World/Shaders/cginc/CurvedWorld_Base.cginc"
//#define V_CURVEDWORLD_COMPATIBILITY_ON


#if defined(V_WIRE_DYNAMI_MASK_PLANE) || defined(V_WIRE_DYNAMIC_MASK_SPHERE)
	#define V_WIRE_DYNAMIC_MASK_ON
#endif 
  

half4 _V_WIRE_Color;
half _V_WIRE_FixedSize;
half _V_WIRE_Size;

#ifdef V_WIRE_DYNAMIC_MASK_ON
	half3 _V_WIRE_DynamicMaskWorldPos;
	half3 _V_WIRE_DynamicMaskWorldNormal;	
	half _V_WIRE_DynamicMaskRadius;	
	
	half _V_WIRE_DynamicMaskInvert;	
	half _V_WIRE_DynamicMaskEffectsBaseTexInvert;
#endif

sampler2D _V_WIRE_WireTex;
half4 _V_WIRE_WireTex_ST;
half _V_WIRE_WireTex_UVSet;
half2 _V_WIRE_WireTex_Scroll;
float _V_WIRE_WireVertexColor;
#ifdef V_WIRE_TRANSPARENCY_ON	
	half _V_WIRE_TransparentTex_Invert;
	half _V_WIRE_TransparentTex_Alpha_Offset;
#endif

#ifdef V_WIRE_FRESNEL_ON
	half _V_WIRE_FresnelBias;
	half _V_WIRE_FresnelInvert;

	#if defined(V_WIRE_LEGACY) || defined(V_WIRE_PBR)
		half _V_WIRE_FresnelPow;
	#endif
#endif

float _V_WIRE_DistanceFade;
float _V_WIRE_DistanceFadeStart;
float _V_WIRE_DistanceFadeEnd;



inline fixed3 ExtructWireframeFromVertexUV(half4 uv)
{
	return fixed3(floor(uv.z), frac(uv.z) * 10, uv.w);
}

inline half ExtructWireframeFromMass(half3 mass, float fixedSize)
{
	float size = lerp(_V_WIRE_Size * 0.1, (_V_WIRE_Size / fixedSize) * 0.01, _V_WIRE_FixedSize);

	#ifndef V_WIRE_ANTIALIASING_DISABLE
		half3 width = abs(ddx(mass)) + abs(ddy(mass));
		half3 eF = smoothstep(half3(0, 0, 0), width * size * 20, mass);
	
		float w = size > 0 ? min(min(eF.x, eF.y), eF.z) : 1;

		return w * w * w;
	#else		
		return step(size, min(min(mass.x, mass.y), mass.z));
	#endif
}
 
inline half WireOpaque(fixed4 wireTexColor, inout fixed4 srcColor, fixed3 mass, float wireFade, float fixedSize, half maskValue)
{
	half value = ExtructWireframeFromMass(mass, fixedSize);
	
			
	#ifdef V_WIRE_DYNAMIC_MASK_ON
		value = lerp(value, 1, maskValue);

		#ifdef V_WIRE_DYNAMIC_MASK_BASE_TEX_ON
			#if defined(V_WIRE_ADDATIVE)
				srcColor.rgb = lerp(srcColor.rgb * (1 - maskValue), srcColor.rgb * maskValue, _V_WIRE_DynamicMaskEffectsBaseTexInvert);
			#elif defined(V_WIRE_MULTIPLY)
				srcColor.rgb = lerp(lerp(srcColor.rgb, 1, maskValue), lerp(1, srcColor.rgb, maskValue), _V_WIRE_DynamicMaskEffectsBaseTexInvert);
			#endif
		#endif 
	#endif 

	float wireColorAlpha = _V_WIRE_Color.a * lerp(1, wireFade, _V_WIRE_DistanceFade);

	srcColor = lerp(lerp(srcColor, _V_WIRE_Color * wireTexColor, wireColorAlpha), srcColor, value);

	return value;
}

#ifdef V_WIRE_TRANSPARENT
inline half WireTransparent(fixed4 wireTexColor, inout fixed4 srcColor, fixed3 mass, float wireFade, float fixedSize, half maskValue)
{
	half value = ExtructWireframeFromMass(mass, fixedSize);

	float4 wColor = _V_WIRE_Color * wireTexColor;


	#ifdef V_WIRE_SAME_COLOR
		srcColor.rgb = wColor.rgb;
	#endif		
	
		 
	#ifdef V_WIRE_DYNAMIC_MASK_ON 
		value = lerp(value, 1, maskValue);

		#ifdef V_WIRE_DYNAMIC_MASK_BASE_TEX_ON
			srcColor.a = lerp(srcColor.a, maskValue * srcColor.a, _V_WIRE_DynamicMaskEffectsBaseTexInvert);
		#endif
	#endif 	   

	float wireColorAlpha = _V_WIRE_Color.a * lerp(1, wireFade, _V_WIRE_DistanceFade);
	
	half3 wireColor = lerp(lerp(wColor.rgb, srcColor.rgb, srcColor.a), wColor.rgb, wireColorAlpha);
	wireColor = lerp(srcColor.rgb, wireColor, wireColorAlpha);

	srcColor.rgb = lerp(wireColor.rgb, srcColor.rgb, value);
	srcColor.a = saturate(srcColor.a + (1 - value) * wireColorAlpha);

	return value;
}
#endif

#ifdef V_WIRE_CUTOUT
inline half WirCutout(fixed4 wireTexColor, inout fixed4 srcColor, fixed3 mass, float wireFade, float fixedSize, half maskValue, half customAlpha, half cutoff)
{
	half value = ExtructWireframeFromMass(mass, fixedSize);

	float4 wColor = _V_WIRE_Color * wireTexColor;

	#ifdef V_WIRE_SAME_COLOR
		srcColor.rgb = wColor.rgb;
	#endif 
	  

	#ifdef V_WIRE_DYNAMIC_MASK_ON
		value = lerp(value, 1, maskValue); 
	#endif 

	 
	half clipValue = 1;
	#ifdef V_WIRE_TRANSPARENCY_ON

		wColor.rgb = lerp(srcColor.rgb, wColor.rgb, customAlpha);
		srcColor.rgb = cutoff > 0.01 ? lerp(wColor.rgb, (srcColor.a - cutoff) > 0 ? srcColor.rgb : wColor.rgb, value) : lerp(wColor.rgb, srcColor.rgb, value);

		clipValue = srcColor.a - cutoff;
		clipValue = (1 - value) * customAlpha > 0.5 ? 1 : clipValue;
		 
	#else
			
		srcColor.rgb = lerp(wColor.rgb, (srcColor.a - cutoff + 0.01) > 0 ? srcColor.rgb : wColor.rgb, value);
		 
		clipValue = srcColor.a - cutoff;
		clipValue = (1 - value) > 0.5 ? 1 : clipValue;
	#endif


	clipValue *= lerp(1, wireFade, _V_WIRE_DistanceFade);

	return clipValue;
}
#endif

 
#ifdef V_WIRE_DYNAMIC_MASK_ON 
inline half V_WIRE_DynamicMask(half3 maskPos)
{
	#if defined(V_WIRE_DYNAMI_MASK_PLANE)

		half gValue = saturate(dot(_V_WIRE_DynamicMaskWorldNormal, maskPos - _V_WIRE_DynamicMaskWorldPos) * 512 + 0.5);

		return lerp(1 - gValue, gValue, _V_WIRE_DynamicMaskInvert);

	#elif defined(V_WIRE_DYNAMIC_MASK_SPHERE)
		
		half aCoef = distance(maskPos, _V_WIRE_DynamicMaskWorldPos) / _V_WIRE_DynamicMaskRadius;
		aCoef = aCoef >= 1 ? 1 : 0;

		return lerp(aCoef, 1 - aCoef, _V_WIRE_DynamicMaskInvert);

	#else 

		return 0;

	#endif
}
#endif


#if defined(V_WIRE_CUTOUT)
#define DoWire(wireTexColor,srcColor,mass,wireFade,fixedsize,maskValue,customAlpha,cutoff) WirCutout(wireTexColor,srcColor,mass,wireFade,fixedsize,maskValue,customAlpha,cutoff)
#elif defined(V_WIRE_TRANSPARENT)
#define DoWire(wireTexColor,srcColor,mass,wireFade,fixedsize,maskValue) WireTransparent(wireTexColor,srcColor,mass,wireFade,fixedsize,maskValue)
#else
#define DoWire(wireTexColor,srcColor,mass,wireFade,fixedsize,maskValue) WireOpaque(wireTexColor,srcColor,mass,wireFade,fixedsize,maskValue)
#endif

#endif

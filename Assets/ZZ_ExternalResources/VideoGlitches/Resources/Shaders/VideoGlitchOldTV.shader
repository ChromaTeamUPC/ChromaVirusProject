///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Video Glitches.
//
// Copyright (c) Ibuprogames <hello@ibuprogames.com>. All rights reserved.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// http://unity3d.com/support/documentation/Components/SL-Shader.html
Shader "Hidden/Video Glitches/VideoGlitchOldTV"
{
	// http://unity3d.com/support/documentation/Components/SL-Properties.html
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}

		// Amount of the effect (0 none, 1 full).
		_Amount("Amount", Range(0.0, 1.0)) = 1.0
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "VideoGlitchCG.cginc"

	/////////////////////////////////////////////////////////////
	// BEGIN CONFIGURATION REGION
	/////////////////////////////////////////////////////////////

	// Define this to change the strength of the effect.
	#define USE_AMOUNT

	// Define this to change brightness / contrast / gamma.
	#define USE_BRIGHTNESSCONTRASTGAMMA

  // Define this to change saturation.
  #define USE_SATURATION

	// Enable VHS bars.
	#define USE_VHS_BARS

	// Enable VHS stripes.
	#define USE_VHS_STRIPES

	// Enable moire noise.
	#define USE_MOIRE

	// Enable grain noise.
	#define USE_GRAIN

	// Enable vignette.
	#define USE_VIGNETTE

	// Enable TV vignette.
	#define USE_TV_TUBE_VIGNETTE

	// Enable TV lines.
	#define USE_TV_TUBE_LINES

	// Enable tube moire noise.
	#define USE_TUBE_MOIRE

	// Enable distortion.
	#define USE_VCR_DISTORTION

	// Enable slow scan.
	#define USE_SLOW_SCAN

	// Enable scanlines.
	#define USE_SCANLINE_SCAN

	// Enable tube distortion.
	#define USE_TUBE_DISTORTION

	// Enable scanline distortion.
	#define USE_SCANLINE_DISTORT

	/////////////////////////////////////////////////////////////
	// END CONFIGURATION REGION
	/////////////////////////////////////////////////////////////

	sampler2D _MainTex;

	float _Amount;
	float _Brightness;
	float _Contrast;
	float _Gamma;
  float _Saturation;

	float _Scanline;
	float _Slowscan;
	float _VignetteSoftness;
	float _VignetteScale;
	float _GrainOpacity;
	float _SaturationTV;
	float _ScanDistort;
	float _Timer;
	float _Speed;
	float _Distort;
	float _Scale;
	float _StripesCount;
	float _Opacity;
	float _BarsCount;
	float _OpacityMoire;
	float _MoireScale;
	float _TVLines;
	float _TVLinesOpacity;
	float _TVTubeVignetteScale;
	float _TVDots;
	float _TVDotsBlend;

	inline float3 Noise(float2 uv)
	{
		float2 c = (_ScreenParams.x) * float2(1.0, (_ScreenParams.y / _ScreenParams.x));

		float r = Rand(float2((2.0 + _Time.y) * floor(uv.x * c.x) / c.x, (2.0 + _Time.y) * floor(uv.y * c.y) / c.y));
		float g = Rand(float2((5.0 + _Time.y) * floor(uv.x * c.x) / c.x, (5.0 + _Time.y) * floor(uv.y * c.y) / c.y));
		float b = Rand(float2((9.0 + _Time.y) * floor(uv.x * c.x) / c.x, (9.0 + _Time.y) * floor(uv.y * c.y) / c.y));

		return float3(r, g, b);
	}

	inline float Overlay(float s, float d)
	{
		return (d < 0.5) ? 2.0 * s * d : 1.0 - 2.0 * (1.0 - s) * (1.0 - d);
	}

	inline float3 Overlay(float3 s, float3 d)
	{
		float3 pixel;
		pixel.x = Overlay(s.x, d.x);
		pixel.y = Overlay(s.y, d.y);
		pixel.z = Overlay(s.z, d.z);
		
		return pixel;
	}

	inline float Ramp(float y, float start, float end)
	{
		float inside = step(start, y) - step(end, y);
		float fact = (y - start) / (end - start) * inside;

		return (1.0 - fact) * inside;
	}

	inline float Scanline(float2 uv)
	{
		return sin(_ScreenParams.y * uv.y * _Scanline - _Time.y * 10.0);
	}

	inline float SlowScan(float2 uv)
	{
		return sin(_ScreenParams.y * uv.y * _Slowscan + _Time.y * 6.0);
	}

	inline float2 CRT(float2 coord, float bend)
	{
		coord = (coord - 0.5) * 2.0 / _Scale;
		coord *= 0.5;
		coord.x *= 1.0 + pow((abs(coord.y) / bend * _Distort), 2.0);
		coord.y *= 1.0 + pow((abs(coord.x) / bend * _Distort), 2.0);
		coord = (coord / 1.0) + 0.5;

		return coord;
	}

	inline float2 ScanDistort(float2 uv)
	{
		float scan1 = clamp(cos(uv.y * _Speed + _Time.y * _Timer), 0.0, 1.0);
		float scan2 = clamp(cos(uv.y * _Speed + _Time.y * _Timer + 4.0) * 10.0, 0.0, 1.0);
		
		float amount = scan1 * scan2 * uv.x;

		uv.x -= _ScanDistort * lerp(tex2D(_MainTex, float2(uv.x, amount)).r * amount, amount, 0.9);

		return uv;
	}

	inline float2 ScanDistortLinear(float2 uv)
	{
		float scan1 = clamp(cos(uv.y * _Speed + _Time.y * _Timer), 0.0, 1.0);
		float scan2 = clamp(cos(uv.y * _Speed + _Time.y * _Timer + 4.0) * 10.0, 0.0, 1.0);

		float amount = scan1 * scan2 * uv.x;

		uv.x -= _ScanDistort * lerp(sRGB(tex2D(_MainTex, float2(uv.x, amount))).r * amount, amount, 0.9);

		return uv;
	}

	inline float OnOff(float a, float b, float c)
	{
		return step(c, sin(_Time.y + a * cos(_Time.y * b)));
	}

	inline float3 VideoDistortion(float2 uv)
	{
		float2 look = uv;
		float window = 1.0 / (1.0 + 20.0 * (look.y - fmod(_Time.y / 4.0, 1.0)) * (look.y - fmod(_Time.y / 4.0, 1.0)));
		look.x = look.x + sin(look.y * 10.0 + _Time.y) / 50.0 * OnOff(4.0, 4.0, 0.3) * (1.0 + cos(_Time.y * 80.0)) * window;

		float vShift = 0.4 * OnOff(2.0, 3.0, 0.9) * (sin(_Time.y) * sin(_Time.y * 20.0) + (0.5 + 0.1 * sin(_Time.y * 200.0) * cos(_Time.y)));
		look.y = fmod(look.y + vShift, 1.0);

		return tex2D(_MainTex, look).rgb;
	}

	inline float3 VideoDistortionLinear(float2 uv)
	{
		float2 look = uv;
		float window = 1.0 / (1.0 + 20.0 * (look.y - fmod(_Time.y / 4.0, 1.0)) * (look.y - fmod(_Time.y / 4.0, 1.0)));
		look.x = look.x + sin(look.y * 10.0 + _Time.y) / 50.0 * OnOff(4.0, 4.0, 0.3) * (1.0 + cos(_Time.y * 80.0)) * window;

		float vShift = 0.4 * OnOff(2.0, 3.0, 0.9) * (sin(_Time.y) * sin(_Time.y * 20.0) + (0.5 + 0.1 * sin(_Time.y * 200.0) * cos(_Time.y)));
		look.y = fmod(look.y + vShift, 1.0);

		return sRGB(tex2D(_MainTex, look).rgb);
	}

	inline float Vignette(float2 uv)
	{
		uv = (uv - 0.5) * 0.98;
		
		return clamp(pow(cos(uv.x * _PI), _VignetteScale) * pow(cos(uv.y * _PI), _VignetteScale) * _VignetteSoftness, 0.0, 1.0);
	}

	inline float Stripes(float2 uv)
	{
		float stripes = Rand(uv * float2(0.5, 1.0) + float2(1.0, 3.0)) * _Opacity;

		return Ramp(fmod(uv.y * _StripesCount + _Time.y / 2.0 + sin(_Time.y + sin(_Time.y * 2.0)), 1.0), 0.5, 0.6) * stripes;
	}

	float4 frag_gamma(v2f_img i) : COLOR
	{
		float2 uv = i.uv;
		float2 uv2 = i.uv * 2.0 - 1.0;

		float3 grain = Noise(uv);

		float2 scanDistortUV = uv;
#ifdef USE_SCANLINE_DISTORT
		scanDistortUV = ScanDistort(uv);
#endif
		float2 crtUV = scanDistortUV;

#ifdef USE_TUBE_DISTORTION
		crtUV = CRT(scanDistortUV, 2.0);
#endif
		float3 pixel = tex2D(_MainTex, i.uv).rgb;

		float3 color = tex2D(_MainTex, crtUV).rgb;
		float3 scanlineColor = color;
		float3 slowscanColor = color;

#ifdef USE_VCR_DISTORTION
		color = VideoDistortion(crtUV);
#endif

#ifdef USE_SCANLINE_SCAN
		scanlineColor = Scanline(crtUV);
#endif

#ifdef USE_SLOW_SCAN
		slowscanColor = SlowScan(crtUV);
#endif

#ifdef USE_TUBE_MOIRE
		color *= 1.0 + _TVDotsBlend * 0.2 * sin(crtUV.x * _ScreenParams.x * 5.0 * _TVDots);
		color *= 1.0 + _TVDotsBlend * 0.2 * cos(crtUV.y * _ScreenParams.y) * sin(0.5 + crtUV.x * _ScreenParams.x);
#endif

#ifdef USE_VHS_STRIPES
		color *= (1.0 + Stripes(crtUV));
#endif

#ifdef USE_VHS_BARS
		color *= (12.0 + fmod(crtUV.y * _BarsCount + _Time.y, 1.0)) / 13.0;
#endif

#ifdef USE_MOIRE
		color *= (0.45 + (Rand(crtUV * 0.01 * _MoireScale)) * _OpacityMoire);
#endif

#ifdef USE_GRAIN
		grain = lerp(0.5, grain, _GrainOpacity * 0.1);
		float3 bw_grain = grain.r;
		grain = lerp(bw_grain, grain, _SaturationTV);
		color = Overlay(grain, color);
#endif

#ifdef USE_VIGNETTE
		color *= Vignette(uv);
#endif

#ifdef USE_TV_TUBE_VIGNETTE
		color *= 1.0 - pow(length(uv2 * uv2 * uv2 * uv2) * 1.0, 6.0 * 1.0 / _TVTubeVignetteScale);
#endif

#ifdef USE_TV_TUBE_LINES
		crtUV.y *= _ScreenParams.y / _ScreenParams.y * _TVLines;
		color.r *= (0.55 + abs(0.5 - fmod(crtUV.y, 0.021) / 0.021) * _TVLinesOpacity) * 1.2;
		color.g *= (0.55 + abs(0.5 - fmod(crtUV.y + 0.007, 0.021) / 0.021) * _TVLinesOpacity) * 1.2;
		color.b *= (0.55 + abs(0.5 - fmod(crtUV.y + 0.014, 0.021) / 0.021) * _TVLinesOpacity) * 1.2;
#endif

		float3 final = lerp(color, lerp(scanlineColor, slowscanColor, 0.5), 0.05);

#ifdef USE_BRIGHTNESSCONTRASTGAMMA
		final = PixelBrightnessContrastGamma(final, _Brightness, _Contrast, _Gamma);
#endif

#ifdef USE_SATURATION
    final = PixelSaturation(final, _Saturation);
#endif

#ifdef USE_AMOUNT
		final = PixelAmount(pixel, final, _Amount);
#endif

#ifdef ENABLE_ALL_DEMO
		final = PixelDemo(pixel, final, i.uv, 2);
#endif

		return float4(final, 1.0);
	}

	float4 frag_linear(v2f_img i) : COLOR
	{
		float2 uv = i.uv;
		float2 uv2 = i.uv * 2.0 - 1.0;

		float3 grain = Noise(uv);

		float2 scanDistortUV = uv;
#ifdef USE_SCANLINE_DISTORT
		scanDistortUV = ScanDistortLinear(uv);
#endif
		float2 crtUV = scanDistortUV;

#ifdef USE_TUBE_DISTORTION
		crtUV = CRT(scanDistortUV, 2.0);
#endif
		float3 pixel = sRGB(tex2D(_MainTex, i.uv).rgb);

		float3 color = sRGB(tex2D(_MainTex, crtUV).rgb);
		float3 scanlineColor = color;
		float3 slowscanColor = color;

#ifdef USE_VCR_DISTORTION
		color = VideoDistortionLinear(crtUV);
#endif

#ifdef USE_SCANLINE_SCAN
		scanlineColor = Scanline(crtUV);
#endif

#ifdef USE_SLOW_SCAN
		slowscanColor = SlowScan(crtUV);
#endif

#ifdef USE_TUBE_MOIRE
		color *= 1.0 + _TVDotsBlend * 0.2 * sin(crtUV.x * _ScreenParams.x * 5.0 * _TVDots);
		color *= 1.0 + _TVDotsBlend * 0.2 * cos(crtUV.y * _ScreenParams.y) * sin(0.5 + crtUV.x * _ScreenParams.x);
#endif

#ifdef USE_VHS_STRIPES
		color *= (1.0 + Stripes(crtUV));
#endif

#ifdef USE_VHS_BARS
		color *= (12.0 + fmod(crtUV.y * _BarsCount + _Time.y, 1.0)) / 13.0;
#endif

#ifdef USE_MOIRE
		color *= (0.45 + (Rand(crtUV * 0.01 * _MoireScale)) * _OpacityMoire);
#endif

#ifdef USE_GRAIN
		grain = lerp(0.5, grain, _GrainOpacity * 0.1);
		float3 bw_grain = grain.r;
		grain = lerp(bw_grain, grain, _SaturationTV);
		color = Overlay(grain, color);
#endif

#ifdef USE_VIGNETTE
		color *= Vignette(uv);
#endif

#ifdef USE_TV_TUBE_VIGNETTE
		color *= 1.0 - pow(length(uv2 * uv2 * uv2 * uv2) * 1.0, 6.0 * 1.0 / _TVTubeVignetteScale);
#endif

#ifdef USE_TV_TUBE_LINES
		crtUV.y *= _ScreenParams.y / _ScreenParams.y * _TVLines;
		color.r *= (0.55 + abs(0.5 - fmod(crtUV.y, 0.021) / 0.021) * _TVLinesOpacity) * 1.2;
		color.g *= (0.55 + abs(0.5 - fmod(crtUV.y + 0.007, 0.021) / 0.021) * _TVLinesOpacity) * 1.2;
		color.b *= (0.55 + abs(0.5 - fmod(crtUV.y + 0.014, 0.021) / 0.021) * _TVLinesOpacity) * 1.2;
#endif

		float3 final = lerp(color, lerp(scanlineColor, slowscanColor, 0.5), 0.05);

#ifdef USE_BRIGHTNESSCONTRASTGAMMA
		final = PixelBrightnessContrastGamma(final, _Brightness, _Contrast, _Gamma);
#endif

#ifdef USE_SATURATION
    final = PixelSaturation(final, _Saturation);
#endif

#ifdef USE_AMOUNT
		final = PixelAmount(pixel, final, _Amount);
#endif

#ifdef ENABLE_ALL_DEMO
		final = PixelDemo(pixel, final, i.uv, 2);
#endif

		return float4(Linear(final), 1.0f);
	}
	ENDCG

	// Techniques (http://unity3d.com/support/documentation/Components/SL-SubShader.html).
	SubShader
	{
		// Tags (http://docs.unity3d.com/Manual/SL-CullAndDepth.html).
		ZTest Always
		Cull Off
		ZWrite Off
		Fog { Mode off }

		// Pass 0: Color Space Gamma.
		Pass
		{
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0
			#pragma vertex vert_img
			#pragma fragment frag_gamma
			ENDCG
		}

		// Pass 1: Color Space Linear.
		Pass
		{
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0
			#pragma vertex vert_img
			#pragma fragment frag_linear
			ENDCG
		}
	}

	Fallback off
}
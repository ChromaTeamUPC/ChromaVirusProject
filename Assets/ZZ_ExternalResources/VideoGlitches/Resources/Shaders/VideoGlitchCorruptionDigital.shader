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
Shader "Hidden/Video Glitches/VideoGlitchCorruptionDigital"
{
  // http://unity3d.com/support/documentation/Components/SL-Properties.html
  Properties
  {
    _MainTex("Base (RGB)", 2D) = "white" {}

    // Default 'Resources/Textures/Noise256.png'.
    _NoiseTex("Noise (RGB)", 2D) = "" {}

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

  /////////////////////////////////////////////////////////////
  // END CONFIGURATION REGION
  /////////////////////////////////////////////////////////////

  sampler2D _MainTex;
  sampler2D _NoiseTex;

  float _Amount;
  float _Brightness;
  float _Contrast;
  float _Gamma;
  float _Saturation;

  float _Strength;
  float _Speed;
  float _TileSize;
 
  inline float3 Posterize(float3 color, float steps)
  {
    return floor(color * steps) / steps;
  }

  inline float Quantize(float n, float steps)
  {
    return floor(n * steps) / steps;
  }

  float4 Downsample(sampler2D tex, float2 uv, float pixelSize)
  {
    return tex2D(tex, uv - fmod(uv, pixelSize / _ScreenParams.xy));
  }

  inline float Noise(float p)
  {
    float fl = floor(p);
    float fc = frac(p);
  
    return lerp(Rand(fl), Rand(fl + 1.0), fc);
  }

  inline float Noise(float2 p)
  {
    float2 ip = floor(p);
    float2 u = frac(p);
    u = u * u * (3.0 - 2.0 * u);

    float res = lerp(lerp(Rand(ip), Rand(ip + float2(1.0, 0.0)), u.x),
                     lerp(Rand(ip + float2(0.0, 1.0)), Rand(ip + float2(1.0, 1.0)), u.x), u.y);
  
    return res * res;
  }

  inline float3 EdgeGamma(sampler2D tex, float2 uv, float sampleSize)
  {
    float dx = sampleSize / _ScreenParams.x;
    float dy = sampleSize / _ScreenParams.y;

    return (lerp(Downsample(tex, uv - float2(dx, 0.0), sampleSize), Downsample(tex, uv + float2(dx, 0.0), sampleSize), fmod(uv.x, dx) / dx) +
            lerp(Downsample(tex, uv - float2(0.0, dy), sampleSize), Downsample(tex, uv + float2(0.0, dy), sampleSize), fmod(uv.y, dy) / dy)).rgb / 2.0 - tex2D(tex, uv).rgb;
  }

  inline float3 EdgeLinear(sampler2D tex, float2 uv, float sampleSize)
  {
    float dx = sampleSize / _ScreenParams.x;
    float dy = sampleSize / _ScreenParams.y;

    return (lerp(sRGB(Downsample(tex, uv - float2(dx, 0.0), sampleSize).rgb), sRGB(Downsample(tex, uv + float2(dx, 0.0), sampleSize).rgb), fmod(uv.x, dx) / dx) +
            lerp(sRGB(Downsample(tex, uv - float2(0.0, dy), sampleSize).rgb), sRGB(Downsample(tex, uv + float2(0.0, dy), sampleSize).rgb), fmod(uv.y, dy) / dy) / 2.0 - sRGB(tex2D(tex, uv).rgb));
  }

  inline float3 DistortGamma(sampler2D tex, float2 uv, float edgeSize)
  {
    float2 pixel = 1.0 / _ScreenParams.xy;
    float3 field = RGB2HSV(EdgeGamma(tex, uv, edgeSize));
    float2 distort = pixel * sin((field.rb) * _PI * 2.0);
    
    float speed = _Time.x * _Speed * 2.0;
    float shiftx = Noise(float2(Quantize(uv.y + 31.5, _ScreenParams.y / _TileSize) * speed, frac(speed) * 300.0));
    float shifty = Noise(float2(Quantize(uv.x + 11.5, _ScreenParams.x / _TileSize) * speed, frac(speed) * 100.0));
  
    float3 rgb = tex2D(tex, uv + (distort + (pixel - pixel / 2.0) * float2(shiftx, shifty) * (50.0 + 100.0 * _Strength)) * _Strength).rgb;
    
    float3 hsv = RGB2HSV(rgb);
    hsv.y = fmod(hsv.y + shifty * pow(_Strength, 5.0) * 0.25, 1.0);

    return Posterize(HSV2RGB(hsv), floor(lerp(256.0, pow(1.0 - hsv.z - 0.5, 2.0) * 64.0 * shiftx + 4.0, 1.0 - pow(1.0 - _Strength, 5.0))));
  }

  inline float3 DistortLinear(sampler2D tex, float2 uv, float edgeSize)
  {
    float2 pixel = 1.0 / _ScreenParams.xy;
    float3 field = RGB2HSV(EdgeLinear(tex, uv, edgeSize));
    float2 distort = pixel * sin((field.rb) * _PI * 2.0);

    float speed = _Time.x * _Speed * 2.0;
    float shiftx = Noise(float2(Quantize(uv.y + 31.5, _ScreenParams.y / _TileSize) * speed, frac(speed) * 300.0));
    float shifty = Noise(float2(Quantize(uv.x + 11.5, _ScreenParams.x / _TileSize) * speed, frac(speed) * 100.0));

    float3 rgb = sRGB(tex2D(tex, uv + (distort + (pixel - pixel / 2.0) * float2(shiftx, shifty) * (50.0 + 100.0 * _Strength)) * _Strength).rgb);

    float3 hsv = RGB2HSV(rgb);
    hsv.y = fmod(hsv.y + shifty * pow(_Strength, 5.0) * 0.25, 1.0);

    return Posterize(HSV2RGB(hsv), floor(lerp(256.0, pow(1.0 - hsv.z - 0.5, 2.0) * 64.0 * shiftx + 4.0, 1.0 - pow(1.0 - _Strength, 5.0))));
  }

  float4 frag_gamma(v2f_img i) : COLOR
  {
    float3 pixel = tex2D(_MainTex, i.uv).rgb;

    float3 final = DistortGamma(_MainTex, i.uv, 8.0);

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
    float3 pixel = sRGB(tex2D(_MainTex, i.uv).rgb);

    float3 final = DistortLinear(_MainTex, i.uv, 8.0);

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
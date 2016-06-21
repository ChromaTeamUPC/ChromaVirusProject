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
Shader "Hidden/Video Glitches/VideoGlitchBrokenScreen"
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

  /////////////////////////////////////////////////////////////
  // END CONFIGURATION REGION
  /////////////////////////////////////////////////////////////

  sampler2D _MainTex;

  float _Amount;
  float _Brightness;
  float _Contrast;
  float _Gamma;
  float _Saturation;

  float2 _Center;
  int _Splits;
  float _SplitThreshold;
  float4 _SplitColor;
  float _Distortion;
  
  inline float Rand01(float2 p)
  {
    float3 p3 = frac(p.xyx * 0.1031);
    p3 += dot(p3, p3.yzx + 19.19);

    return frac((p3.x + p3.y) * p3.z) * 2.0 - 1.0;
  }

  float4 frag_gamma(v2f_img i) : COLOR
  {
    float3 pixel = tex2D(_MainTex, i.uv).rgb;

    float2 uv = (i.uv * _ScreenParams.xy * 2.0 - _ScreenParams.xy) / _ScreenParams.x;

    float2 v = 1000.0;
    float2 v2 = 10000.0;

    for (int c = 0; c < _Splits; c++)
    {
      float fc = float(c);
      float angle = floor(Rand01(float2(fc, 387.44)) * 16.0) * 3.1415 * 0.4 - 0.5;
      float dist = pow(Rand01(float2(fc, 78.21)), 2.0) * 0.5;

      float2 vc = float2(_Center.x + cos(angle) * dist + Rand01(float2(fc, 349.3)) * 7E-3, _Center.y + sin(angle) * dist + Rand01(float2(fc, 912.7)) * 7E-3);
      if (length(vc - uv) < length(v - uv))
      {
        v2 = v;
        v = vc;
      }
      else if (length(vc - uv) < length(v2 - uv))
        v2 = vc;
    }

    float col = abs(length(dot(uv - v, normalize(v - v2))) - length(dot(uv - v2, normalize(v - v2)))) + 0.002 * length(uv - _Center);
    col /= 0.0025;

    if (length(v - v2) < 0.0004)
      col = 0.0;

    float3 final = tex2D(_MainTex, i.uv + Rand01(v) * _Distortion).rgb;

    if (col < _SplitThreshold)
      final *= _SplitColor;

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

    float2 uv = (i.uv * _ScreenParams.xy * 2.0 - _ScreenParams.xy) / _ScreenParams.x;

    float2 v = 1000.0;
    float2 v2 = 10000.0;

    for (int c = 0; c < _Splits; c++)
    {
      float fc = float(c);
      float angle = floor(Rand01(float2(fc, 387.44)) * 16.0) * 3.1415 * 0.4 - 0.5;
      float dist = pow(Rand01(float2(fc, 78.21)), 2.0) * 0.5;

      float2 vc = float2(_Center.x + cos(angle) * dist + Rand01(float2(fc, 349.3)) * 7E-3, _Center.y + sin(angle) * dist + Rand01(float2(fc, 912.7)) * 7E-3);
      if (length(vc - uv) < length(v - uv))
      {
        v2 = v;
        v = vc;
      }
      else if (length(vc - uv) < length(v2 - uv))
        v2 = vc;
    }

    float col = abs(length(dot(uv - v, normalize(v - v2))) - length(dot(uv - v2, normalize(v - v2)))) + 0.002 * length(uv - _Center);
    col /= 0.0025;

    if (length(v - v2) < 0.0004)
      col = 0.0;

    float3 final = sRGB(tex2D(_MainTex, i.uv + Rand01(v) * _Distortion).rgb);

    if (col < _SplitThreshold)
      final *= _SplitColor;

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
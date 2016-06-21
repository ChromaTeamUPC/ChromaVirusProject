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
Shader "Hidden/Video Glitches/VideoGlitchOldVHS"
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

  // Define this to use bloom.
  #define USE_BLOOM

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

  float _Waving;
  float _Noise;
  float _StripeCount;
  float _StripeVelocity;
  float _StripeStrength;
  float _StripeNoise;
  float _SwitchingNoise;
  float _ACBeatWidth;
  float _ACBeatVelocity;
  float _BloomPasses;

  inline float Hash(float2 _v, float2 _r)
  {
    float h00 = Rand(float2(floor(_v * _r + float2(0.0, 0.0)) / _r));
    float h10 = Rand(float2(floor(_v * _r + float2(1.0, 0.0)) / _r));
    float h01 = Rand(float2(floor(_v * _r + float2(0.0, 1.0)) / _r));
    float h11 = Rand(float2(floor(_v * _r + float2(1.0, 1.0)) / _r));

    float2 ip = float2(smoothstep(0.0, 1.0, fmod(_v * _r, 1.0)));

    return (h00 * (1.0 - ip.x) + h10 * ip.x) * (1.0 - ip.y) + (h01 * (1.0 - ip.x) + h11 * ip.x) * ip.y;
  }
  
  inline float Noise(float2 _v)
  {
    float sum = 0.0;

    for (int i = 1; i < 9; i++)
    {
	    float fi = float(i);
	    float ft = pow(2.0, fi);
	    sum += Hash(_v + fi, 2.0 * ft) / ft;
    }

    return sum;
  }

  inline float3 HueShift(float3 _i, float _p)
  {
    float3 p = 0.0;
    p.x = clamp(cos(_p), 0.0, 1.0);
    p.y = clamp(cos(_p - _PI / 3.0 * 2.0), 0.0, 1.0);
    p.z = clamp(cos(_p - _PI / 3.0 * 4.0), 0.0, 1.0);

    return float3(dot(_i, p.xyz), dot(_i, p.zxy), dot(_i, p.yzx));
  }

  float4 frag_gamma(v2f_img i) : COLOR
  {
    float3 pixel = tex2D(_MainTex, i.uv).rgb;

    float3 final = pixel;

    float2 uv = i.uv;
    float huen = 0.0;
    float3 tex = pixel;

    // Waving.
    uv.x += (Noise(float2(uv.y, _Time.y)) - 0.5) * 0.04 * _Waving;

    // Noise.
    uv.x += (Noise(float2(uv.y * 100.0, _Time.y * 10.0)) - 0.5) * 0.1 * _Noise;

    // Stripe.
    float tcPhase = clamp((sin(uv.y * _StripeCount - _Time.y * _PI * _StripeVelocity) - 0.92) * Noise(_Time.y), 0.0, 0.01) * _StripeStrength;
    float tcNoise = max(Noise(float2(uv.y * _StripeNoise, _Time.y * 10.0)) - 0.5, 0.0);
    uv.x = uv.x - tcNoise * tcPhase;
    huen += (tcNoise + 5.0) * tcPhase;

    // Switching noise.
    float snPhase = clamp(pow(1.0 - uv.y, 500.0) * 1000.0, 0.0, 1.0) * _SwitchingNoise;
    uv.x *= (1.0 - snPhase * 0.5) + snPhase * (Noise(float2(uv.y * 100.0, _Time.y * 10.0)) * 0.2 + 0.1);
    huen += snPhase;
    final = HueShift(tex2D(_MainTex, uv).rgb, huen);

    // Bloom
#ifdef USE_BLOOM
    if (_BloomPasses > 0.0)
    {
#ifdef SHADER_API_D3D9
      final += tex2D(_MainTex, uv).rgb * 0.06;
      final += tex2D(_MainTex, uv + float2(1.0, 0.0) * 7E-3).rgb * 0.06;
      final += tex2D(_MainTex, uv + float2(2.0, 0.0) * 7E-3).rgb * 0.06;
      final += tex2D(_MainTex, uv + float2(3.0, 0.0) * 7E-3).rgb * 0.06;
      final += tex2D(_MainTex, uv + float2(4.0, 0.0) * 7E-3).rgb * 0.06;
#else
      float bloomFactor = 0.3 / _BloomPasses;
      for (float x = 0.0; x < _BloomPasses; x += 1.0)
        final += tex2D(_MainTex, uv + float2(x, 0.0) * 7E-3).rgb * bloomFactor;
#endif
    }
#endif

    // AC beat
    final *= 1.0 + clamp(Noise(float2(0.0, uv.y + _Time.y * _ACBeatVelocity)) * _ACBeatWidth - 0.25, 0.0, 0.1);

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

    float3 final = pixel;

    float2 uv = i.uv;
    float huen = 0.0;
    float3 tex = pixel;

    // Waving.
    uv.x += (Noise(float2(uv.y, _Time.y)) - 0.5) * 0.04 * _Waving;

    // Noise.
    uv.x += (Noise(float2(uv.y * 100.0, _Time.y * 10.0)) - 0.5) * 0.1 * _Noise;

    // Stripe.
    float tcPhase = clamp((sin(uv.y * _StripeCount - _Time.y * _PI * _StripeVelocity) - 0.92) * Noise(_Time.y), 0.0, 0.01) * _StripeStrength;
    float tcNoise = max(Noise(float2(uv.y * _StripeNoise, _Time.y * 10.0)) - 0.5, 0.0);
    uv.x = uv.x - tcNoise * tcPhase;
    huen += (tcNoise + 5.0) * tcPhase;

    // Switching noise.
    float snPhase = clamp(pow(1.0 - uv.y, 500.0) * 1000.0, 0.0, 1.0) * _SwitchingNoise;
    uv.x *= (1.0 - snPhase * 0.5) + snPhase * (Noise(float2(uv.y * 100.0, _Time.y * 10.0)) * 0.2 + 0.1);
    huen += snPhase;
    final = HueShift(sRGB(tex2D(_MainTex, uv).rgb), huen);

    // Bloom
#ifdef USE_BLOOM
    if (_BloomPasses > 0.0)
    {
#ifdef SHADER_API_D3D9
      final += tex2D(_MainTex, uv).rgb * 0.12;
      final += tex2D(_MainTex, uv + float2(1.0, 0.0) * 7E-3).rgb * 0.12;
      final += tex2D(_MainTex, uv + float2(2.0, 0.0) * 7E-3).rgb * 0.12;
      final += tex2D(_MainTex, uv + float2(3.0, 0.0) * 7E-3).rgb * 0.12;
      final += tex2D(_MainTex, uv + float2(4.0, 0.0) * 7E-3).rgb * 0.12;
#else
      float bloomFactor = 0.6 / _BloomPasses;
      for (float x = 0.0; x < _BloomPasses; x += 1.0)
        final += tex2D(_MainTex, uv + float2(x, 0.0) * 7E-3).rgb * bloomFactor;
#endif
    }
#endif

    // AC beat
    final *= 1.0 + clamp(Noise(float2(0.0, uv.y + _Time.y * _ACBeatVelocity)) * _ACBeatWidth - 0.25, 0.0, 0.1);

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
﻿///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
using UnityEngine;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Shift.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Video Glitches/Video Glitch Shift")]
  public sealed class VideoGlitchShift : ImageEffectBase
  {
    /// <summary>
    /// Shift amplitude [0..1].
    /// </summary>
    public float amplitude = 0.1f;

    /// <summary>
    /// Shift speed [0..0.2].
    /// </summary>
    public float speed = 0.02f;

    /// <summary>
    /// Default 'Textures/Noise256.png'.
    /// </summary>
    private Texture noiseTex;

    private const string variableNoise = @"_NoiseTex";
    private const string variableAmplitude = @"_Amplitude";
    private const string variableSpeed = @"_Speed";

    /// <summary>
    /// Shader path.
    /// </summary>
    protected override string ShaderPath { get { return @"Shaders/VideoGlitchShift"; } }

    /// <summary>
    /// Creates the material and textures.
    /// </summary>
    protected override void CreateMaterial()
    {
      noiseTex = VideoGlitchesHelper.LoadTextureFromResources(@"Textures/Noise256");

      base.CreateMaterial();
    }

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public override void ResetDefaultValues()
    {
      amplitude = 0.1f;
      speed = 0.02f;

      base.ResetDefaultValues();
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected override void SendValuesToShader()
    {
      this.Material.SetFloat(variableAmplitude, amplitude);
      this.Material.SetFloat(variableSpeed, speed);
      this.Material.SetTexture(variableNoise, noiseTex);

      base.SendValuesToShader();
    }
  }
}
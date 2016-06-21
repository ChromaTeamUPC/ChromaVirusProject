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
using UnityEngine;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Old VHS recorder.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Video Glitches/Video Glitch Old VHS")]
  public sealed class VideoGlitchOldVHS : ImageEffectBase
  {
    /// <summary>
    /// Wave distortion [0.0 - 1.0].
    /// </summary>
    public float waving = 0.5f;

    /// <summary>
    /// Noise distortion [0.0 - 1.0].
    /// </summary>
    public float noise = 0.25f;

    /// <summary>
    /// Stripes count [0 - 32].
    /// </summary>
    public float stripeCount = 8.0f;

    /// <summary>
    /// Stripes velocity [-10.0 - 10.0].
    /// </summary>
    public float stripeVelocity = 1.2f;

    /// <summary>
    /// Stripes strength [0.0 - 1.0].
    /// </summary>
    public float stripeStrength = 1.0f;

    /// <summary>
    /// Stripes noise [0.0 - 1.0].
    /// </summary>
    public float stripeNoise = 0.5f;

    /// <summary>
    /// Head switching noise [0.0 - 1.0].
    /// </summary>
    public float switchingNoise = 0.5f;

    /// <summary>
    /// Ground loop interference width [0.0 - 1.0].
    /// </summary>
    public float acBeatWidth = 0.6f;

    /// <summary>
    /// Ground loop interference velocity [-10.0 - 10.0].
    /// </summary>
    public float acBeatVelocity = 0.2f;

    /// <summary>
    /// Bloom passes [0 - 10].
    /// </summary>
    public float bloomPasses = 5.0f;

    /// <summary>
    /// Shader path.
    /// </summary>
    protected override string ShaderPath { get { return @"Shaders/VideoGlitchOldVHS"; } }

    private const string variableWaving = @"_Waving";
    private const string variableNoise = @"_Noise";
    private const string variableStripeCount = @"_StripeCount";
    private const string variableStripeVelocity = @"_StripeVelocity";
    private const string variableStripeStrength = @"_StripeStrength";
    private const string variableStripeNoise = @"_StripeNoise";
    private const string variableSwitchingNoise = @"_SwitchingNoise";
    private const string variableACBeatWidth = @"_ACBeatWidth";
    private const string variableACBeatVelocity = @"_ACBeatVelocity";
    private const string variableBloomPasses = @"_BloomPasses";

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public override void ResetDefaultValues()
    {
      waving = 0.5f;
      noise = 0.25f;
      stripeCount = 2.0f;
      stripeVelocity = 1.2f;
      stripeStrength = 1.0f;
      stripeNoise = 0.5f;
      switchingNoise = 0.5f;
      acBeatWidth = 0.6f;
      acBeatVelocity = 0.2f;
      bloomPasses = 5.0f;

      base.ResetDefaultValues();
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected override void SendValuesToShader()
    {
      this.Material.SetFloat(variableWaving, waving);
      this.Material.SetFloat(variableNoise, noise);
      this.Material.SetFloat(variableStripeCount, Mathf.Floor(stripeCount) * 6.0f);
      this.Material.SetFloat(variableStripeVelocity, stripeVelocity);
      this.Material.SetFloat(variableStripeStrength, stripeStrength * 10.0f);
      this.Material.SetFloat(variableStripeNoise, stripeNoise * 500.0f);
      this.Material.SetFloat(variableSwitchingNoise, switchingNoise);
      this.Material.SetFloat(variableACBeatWidth, acBeatWidth);
      this.Material.SetFloat(variableACBeatVelocity, acBeatVelocity);
      this.Material.SetFloat(variableBloomPasses, bloomPasses);

      base.SendValuesToShader();
    }
  }
}
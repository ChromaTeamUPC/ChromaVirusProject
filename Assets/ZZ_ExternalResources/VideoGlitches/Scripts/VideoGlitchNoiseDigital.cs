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
  /// Video Glitch Digital.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Video Glitches/Video Glitch Noise Digital")]
  public sealed class VideoGlitchNoiseDigital : ImageEffectBase
  {
    /// <summary>
    /// Threshold [0..1].
    /// </summary>
    public float threshold = 0.1f;

    /// <summary>
    /// Max offset [0..1].
    /// </summary>
    public float maxOffset = 0.1f;

    /// <summary>
    /// Threshold YUV [0..1]
    /// </summary>
    public float thresholdYUV = 0.5f;

    /// <summary>
    /// Shader path.
    /// </summary>
    protected override string ShaderPath { get { return @"Shaders/VideoGlitchNoiseDigital"; } }

    private const string variableThreshold = @"_Threshold";
    private const string variableMaxOffset = @"_MaxOffset";
    private const string variableThresholdYUV = @"_ThresholdYUV";

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public override void ResetDefaultValues()
    {
      threshold = 0.1f;
      maxOffset = 0.1f;
      thresholdYUV = 0.5f;

      base.ResetDefaultValues();
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected override void SendValuesToShader()
    {
      this.Material.SetFloat(variableThreshold, threshold);
      this.Material.SetFloat(variableMaxOffset, maxOffset);
      this.Material.SetFloat(variableThresholdYUV, thresholdYUV);

      base.SendValuesToShader();
    }
  }
}
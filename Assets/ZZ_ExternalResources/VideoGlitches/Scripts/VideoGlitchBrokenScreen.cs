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
  /// Video Glitch Groen Screen.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Video Glitches/Video Glitch Broken Screen")]
  public sealed class VideoGlitchBrokenScreen : ImageEffectBase
  {
    /// <summary>
    /// Impact point [(-1.0, -1.0) - (1.0, 1.0)].
    /// </summary>
    public Vector2 impact = Vector2.zero;

    /// <summary>
    /// Number of splits [2 - 100].
    /// </summary>
    public int splits = 25;

    /// <summary>
    /// Split threshold [0.0 - 1.0].
    /// </summary>
    public float splitThreshold = 0.5f;

    /// <summary>
    /// Split color.
    /// </summary>
    public Color splitColor = Color.gray;

    /// <summary>
    /// Image distortion [0.0 - 1.0].
    /// </summary>
    public float distortion = 0.2f;

    private const string variableCenter = @"_Center";
    private const string variableSplits = @"_Splits";
    private const string variableSplitThreshold = @"_SplitThreshold";
    private const string variableSplitColor = @"_SplitColor";
    private const string variableDistortion = @"_Distortion";

    /// <summary>
    /// Shader path.
    /// </summary>
    protected override string ShaderPath { get { return @"Shaders/VideoGlitchBrokenScreen"; } }

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public override void ResetDefaultValues()
    {
      impact = Vector2.zero;
      splits = 25;
      splitThreshold = 0.5f;
      splitColor = Color.gray;
      distortion = 0.2f;

      base.ResetDefaultValues();
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected override void SendValuesToShader()
    {
      this.Material.SetVector(variableCenter, impact);
      this.Material.SetInt(variableSplits, splits);
      this.Material.SetFloat(variableSplitThreshold, splitThreshold);
      this.Material.SetColor(variableSplitColor, splitColor);
      this.Material.SetFloat(variableDistortion, distortion * 0.1f);

      base.SendValuesToShader();
    }
  }
}
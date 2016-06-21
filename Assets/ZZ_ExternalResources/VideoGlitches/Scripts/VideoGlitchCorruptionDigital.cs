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
  /// Video Glitch Corruption Digital.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Video Glitches/Video Glitch Corruption Digital")]
  public sealed class VideoGlitchCorruptionDigital : ImageEffectBase
  {
    /// <summary>
    /// Effect strength [0.0 - 1.0].
    /// </summary>
    public float strength = 0.5f;

    /// <summary>
    /// Distortion speed [0.0 - 1.0].
    /// </summary>
    public float speed = 1.0f;

    /// <summary>
    /// Tile size [1 - 128].
    /// </summary>
    public int tileSize = 64;

    private const string variableStrength = @"_Strength";
    private const string variableSpeed = @"_Speed";
    private const string variableTileSize = @"_TileSize";

    /// <summary>
    /// Shader path.
    /// </summary>
    protected override string ShaderPath { get { return @"Shaders/VideoGlitchCorruptionDigital"; } }

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public override void ResetDefaultValues()
    {
      strength = 0.5f;
      speed = 1.0f;
      tileSize = 64;

      base.ResetDefaultValues();
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected override void SendValuesToShader()
    {
      this.Material.SetFloat(variableStrength, strength);
      this.Material.SetFloat(variableSpeed, speed);
      this.Material.SetFloat(variableTileSize, tileSize);

      base.SendValuesToShader();
    }
  }
}
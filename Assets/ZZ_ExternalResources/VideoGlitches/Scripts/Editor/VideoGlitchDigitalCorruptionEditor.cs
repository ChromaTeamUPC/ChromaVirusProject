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
using UnityEditor;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Corruption Digital Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchCorruptionDigital))]
  public class VideoGlitchCorruptionDigitalEditor : ImageEffectBaseEditor
  {
    private VideoGlitchCorruptionDigital thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchCorruptionDigital)target;

      this.Help = @"Digital signal corruption.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.strength = VideoGlitchEditorHelper.SliderWithReset(@"Strength", @"Corruption strength.", Mathf.CeilToInt(thisTarget.strength * 100.0f), 0, 100, 50) * 0.01f;

      thisTarget.speed = VideoGlitchEditorHelper.SliderWithReset(@"Speed", @"Distortion speed.", Mathf.CeilToInt(thisTarget.speed * 100.0f), 0, 100, 100) * 0.01f;

      thisTarget.tileSize = VideoGlitchEditorHelper.IntSliderWithReset(@"Tile size", @"Block size.", Mathf.FloorToInt(thisTarget.tileSize), 1, 128, 64);
    }
  }
}
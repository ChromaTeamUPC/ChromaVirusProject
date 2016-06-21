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
  /// Video Glitch Noise Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchNoiseAnalog))]
  public class VideoGlitchNoiseEditor : ImageEffectBaseEditor
  {
    private VideoGlitchNoiseAnalog thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchNoiseAnalog)target;

      this.Help = @"Analogue noise.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.threshold = VideoGlitchEditorHelper.IntSliderWithReset("Threshold", @"Strength of the effect.", Mathf.CeilToInt(thisTarget.threshold * 100.0f), 0, 100, 100) * 0.01f;
    }
  }
}
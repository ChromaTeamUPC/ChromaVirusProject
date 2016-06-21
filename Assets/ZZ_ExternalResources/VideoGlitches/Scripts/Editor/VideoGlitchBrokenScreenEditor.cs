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
  /// Video Glitch Broken Screen Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchBrokenScreen))]
  public class VideoGlitchBrokenScreenEditor : ImageEffectBaseEditor
  {
    private VideoGlitchBrokenScreen thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchBrokenScreen)target;

      this.Help = @"Broken sceen.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.impact = VideoGlitchEditorHelper.Vector2WithReset(@"Impact", @"Point of impact.", thisTarget.impact, Vector2.zero);

      thisTarget.distortion = VideoGlitchEditorHelper.SliderWithReset("Distortion", @"Image distortion [0.0 - 1.0].", thisTarget.distortion, 0.0f, 1.0f, 0.2f);

      EditorGUILayout.LabelField(@"Splits");

      EditorGUI.indentLevel++;

      thisTarget.splits = VideoGlitchEditorHelper.IntSliderWithReset("Count", @"Number of splits [1 - 100].", thisTarget.splits, 2, 100, 25);

      thisTarget.splitThreshold = VideoGlitchEditorHelper.SliderWithReset("Threshold", @"Split threshold [0.0 - 1.0].", thisTarget.splitThreshold, 0.0f, 1.0f, 0.5f);

      thisTarget.splitColor = VideoGlitchEditorHelper.ColorWithReset("Color", @"Split color.", thisTarget.splitColor, Color.gray);

      EditorGUI.indentLevel--;
    }
  }
}
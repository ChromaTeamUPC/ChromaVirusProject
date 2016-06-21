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
  /// Video Glitch Old VHS Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchOldVHS))]
  public class VideoGlitchOldVHSEditor : ImageEffectBaseEditor
  {
    private VideoGlitchOldVHS thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchOldVHS)target;

      this.Help = @"Old VHS recorder.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      EditorGUILayout.LabelField(@"Distortions");

      EditorGUI.indentLevel++;

      thisTarget.noise = VideoGlitchEditorHelper.SliderWithReset(@"Noise", @"Noise [0.0 - 1.0].", thisTarget.noise, 0.0f, 1.0f, 0.25f);

      thisTarget.waving = VideoGlitchEditorHelper.SliderWithReset(@"Waving", @"Wave strength [0.0 - 1.0].", thisTarget.waving, 0.0f, 1.0f, 0.5f);

      thisTarget.switchingNoise = VideoGlitchEditorHelper.SliderWithReset(@"Switching noise", @"Head switching noise [0.0 - 1.0].", thisTarget.switchingNoise, 0.0f, 1.0f, 0.5f);

      EditorGUI.indentLevel--;

      EditorGUILayout.LabelField(@"Stripes");

      EditorGUI.indentLevel++;

      thisTarget.stripeStrength = VideoGlitchEditorHelper.SliderWithReset(@"Strength", @"Stripes strength [0.0 - 1.0].", thisTarget.stripeStrength, 0.0f, 1.0f, 1.0f);
      thisTarget.stripeCount = Mathf.Floor(VideoGlitchEditorHelper.SliderWithReset(@"Count", @"Stripes count [0 - 32].", thisTarget.stripeCount, 0.0f, 32.0f, 2.0f));
      thisTarget.stripeVelocity = VideoGlitchEditorHelper.SliderWithReset(@"Velocity", @"Stripes velocity [-10.0 - 10.0].", thisTarget.stripeVelocity, -10.0f, 10.0f, 1.2f);
      thisTarget.stripeNoise = VideoGlitchEditorHelper.SliderWithReset(@"Noise", @"Stripes noise [0.0 - 1.0].", thisTarget.stripeNoise, 0.0f, 1.0f, 0.5f);

      EditorGUI.indentLevel--;

      EditorGUILayout.LabelField(@"AC Beat");

      EditorGUI.indentLevel++;

      thisTarget.acBeatWidth = VideoGlitchEditorHelper.SliderWithReset(@"Width", @"Electrical ground loop interference width [0.0 - 1.0].", thisTarget.acBeatWidth, 0.0f, 1.0f, 0.6f);
      thisTarget.acBeatVelocity = VideoGlitchEditorHelper.SliderWithReset(@"Velocity", @"Electrical ground loop interference velocity [-10.0 - 10.0].", thisTarget.acBeatVelocity, -10.0f, 10.0f, 0.2f);

      EditorGUI.indentLevel--;

      thisTarget.bloomPasses = Mathf.Floor(VideoGlitchEditorHelper.SliderWithReset(@"Bloom passes", @"Bloom passes [0 - 10].", thisTarget.bloomPasses, 0.0f, 10.0f, 5.0f));
    }
  }
}
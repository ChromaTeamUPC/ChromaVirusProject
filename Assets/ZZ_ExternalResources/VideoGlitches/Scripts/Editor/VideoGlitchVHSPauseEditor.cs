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
using UnityEditor;

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch VHS Pause Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchVHSPause))]
  public class VideoGlitchVHSPauseEditor : ImageEffectBaseEditor
  {
    private VideoGlitchVHSPause thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchVHSPause)target;

      this.Help = @"VHS pause.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.strength = VideoGlitchEditorHelper.SliderWithReset(@"Strength", @"Effect strength [0 - 1].", thisTarget.strength, 0.0f, 1.0f, 0.5f);

      thisTarget.colorNoise = VideoGlitchEditorHelper.SliderWithReset(@"Color noise", @"Color noise [0 - 1].", thisTarget.colorNoise, 0.0f, 1.0f, 0.1f);

      thisTarget.noiseColor = EditorGUILayout.ColorField(@"Noise color", thisTarget.noiseColor);
    }
  }
}
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
  /// Video Glitch Black and White Distortion Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchBlackWhiteDistortion))]
  public class VideoGlitchBlackWhiteDistortionEditor : ImageEffectBaseEditor
  {
    private VideoGlitchBlackWhiteDistortion thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchBlackWhiteDistortion)target;

      this.Help = @"Black and White distortion.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.distortionSteps = VideoGlitchEditorHelper.IntSliderWithReset("Distortion steps", @"Distortion steps.", (int)thisTarget.distortionSteps, 1, 10, 2);

      VideoGlitchEditorHelper.MinMaxSliderWithReset("Distortion range", @"Distortion range.", ref thisTarget.distortionAmountMinLimit, ref thisTarget.distortionAmountMaxLimit, 0.0f, 360.0f, 340.0f, 360.0f);

      thisTarget.distortionSpeed = VideoGlitchEditorHelper.SliderWithReset("Distortion speed", @"Distortion speed.", thisTarget.distortionSpeed, 0.0f, 10.0f, 1.0f);
    }
  }
}
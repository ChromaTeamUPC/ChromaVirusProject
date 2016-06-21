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
  /// Video Glitch Spectrum Offset Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchSpectrumOffset))]
  public class VideoGlitchSpectrumOffsetEditor : ImageEffectBaseEditor
  {
    private VideoGlitchSpectrumOffset thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchSpectrumOffset)target;

      this.Help = @"Spectrum color offset.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.strength = VideoGlitchEditorHelper.SliderWithReset("Strength", @"Effect strength.", thisTarget.strength, 0.0f, 1.0f, 0.1f);

      thisTarget.steps = VideoGlitchEditorHelper.IntSliderWithReset("Steps", @"Effect steps.", (int)thisTarget.steps, 3, 10, 5);
    }
  }
}
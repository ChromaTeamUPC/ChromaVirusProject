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
  /// Video Glitch Old tape Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchOldTape))]
  public class VideoGlitchOldTapeEditor : ImageEffectBaseEditor
  {
    private VideoGlitchOldTape thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchOldTape)target;

      this.Help = @"Old VCR tape.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.noiseSpeed = VideoGlitchEditorHelper.SliderWithReset("Speed", @"Noise speed.", thisTarget.noiseSpeed, 0.0f, 100.0f, 100.0f);

      thisTarget.noiseAmplitude = VideoGlitchEditorHelper.SliderWithReset("Amplitude", @"Noise amplitude.", thisTarget.noiseAmplitude, 1.0f, 100.0f, 1.0f);
    }
  }
}
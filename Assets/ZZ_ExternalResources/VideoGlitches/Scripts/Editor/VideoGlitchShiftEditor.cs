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
  /// Video Glitch Shift Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchShift))]
  public class VideoGlitchShiftEditor : ImageEffectBaseEditor
  {
    private VideoGlitchShift thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchShift)target;

      this.Help = @"Displacement of the color channels.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.amplitude = VideoGlitchEditorHelper.IntSliderWithReset("Amplitude", @"Offset amount.", Mathf.CeilToInt(thisTarget.amplitude * 100.0f), 0, 100, 100) * 0.01f;

      thisTarget.speed = (float)VideoGlitchEditorHelper.IntSliderWithReset(@"Speed", @"Speed of change.", Mathf.FloorToInt(thisTarget.speed * 500.0f), 0, 100, 10) * 0.002f;
    }
  }
}
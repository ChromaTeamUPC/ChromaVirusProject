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
  /// Video Glitch RGB Display Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchRGBDisplay))]
  public class VideoGlitchRGBDisplayEditor : ImageEffectBaseEditor
  {
    private VideoGlitchRGBDisplay thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchRGBDisplay)target;

      this.Help = @"RGB display.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      thisTarget.cellSize = VideoGlitchEditorHelper.IntSliderWithReset("Cell size", @"Cell size.", thisTarget.cellSize, 1, 10, 2);
    }
  }
}
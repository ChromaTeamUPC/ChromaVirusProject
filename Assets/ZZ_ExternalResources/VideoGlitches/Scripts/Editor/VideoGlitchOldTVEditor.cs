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
  /// Video Glitch Old TV Editor.
  /// </summary>
  [CustomEditor(typeof(VideoGlitchOldTV))]
  public class VideoGlitchOldTVEditor : ImageEffectBaseEditor
  {
    private VideoGlitchOldTV thisTarget;

    private void OnEnable()
    {
      thisTarget = (VideoGlitchOldTV)target;

      this.Help = @"Old TV.";
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected override void Inspector()
    {
      EditorGUILayout.LabelField(@"Slow scan");

      EditorGUI.indentLevel++;

      thisTarget.slowScan = VideoGlitchEditorHelper.SliderWithReset(@"Slow scan", "", thisTarget.slowScan, 0.0f, 1.0f, 0.01f);
      thisTarget.scanLine = VideoGlitchEditorHelper.SliderWithReset(@"Scanlines", "", thisTarget.scanLine, 0.0f, 2.0f, 0.5f);

      EditorGUI.indentLevel--;

      EditorGUILayout.LabelField(@"Vignette");

      EditorGUI.indentLevel++;

      thisTarget.vignetteSoftness = VideoGlitchEditorHelper.SliderWithReset(@"Softness", "", thisTarget.vignetteSoftness, 0.0f, 1.0f, 0.9f);
      thisTarget.vignetteScale = VideoGlitchEditorHelper.SliderWithReset(@"Scale", "", thisTarget.vignetteScale, 0.0f, 1.0f, 0.14f);
      thisTarget.vignetteTubeScale = VideoGlitchEditorHelper.SliderWithReset(@"Tube scale", "", thisTarget.vignetteTubeScale, 0.01f, 10.0f, 0.7f);

      EditorGUI.indentLevel--;

      EditorGUILayout.LabelField(@"Grain");

      EditorGUI.indentLevel++;

      thisTarget.grainSaturation = VideoGlitchEditorHelper.SliderWithReset(@"Saturation", "", thisTarget.grainSaturation, 0.0f, 1.0f, 0.0f);
      thisTarget.grainOpacity = VideoGlitchEditorHelper.SliderWithReset(@"Strength", "", thisTarget.grainOpacity, 0.0f, 100.0f, 5.0f);

      EditorGUI.indentLevel--;

      EditorGUILayout.LabelField(@"Distortion");

      EditorGUI.indentLevel++;

      thisTarget.scanDistort = VideoGlitchEditorHelper.SliderWithReset(@"Scanline", "", thisTarget.scanDistort, 0.0f, 5.0f, 0.03f);
      thisTarget.timer = VideoGlitchEditorHelper.SliderWithReset(@"Frequency", "", thisTarget.timer, 0.0f, 5.0f, 0.85f);
      thisTarget.speed = VideoGlitchEditorHelper.SliderWithReset(@"Speed", "", thisTarget.speed, 0.0f, 5.0f, 0.85f);

      EditorGUI.indentLevel--;

      EditorGUILayout.LabelField(@"VHS Stripes");

      EditorGUI.indentLevel++;

      thisTarget.stripesCount = VideoGlitchEditorHelper.SliderWithReset(@"Stripes", "", thisTarget.stripesCount, 0.0f, 1000.0f, 0.5f);
      thisTarget.stripesOpacity = VideoGlitchEditorHelper.SliderWithReset(@"Opacity", "", thisTarget.stripesOpacity, 0.0f, 10.0f, 1.0f);
      thisTarget.barsCount = VideoGlitchEditorHelper.SliderWithReset(@"Bars", "", thisTarget.barsCount, 0.0f, 1000.0f, 5.0f);

      EditorGUI.indentLevel--;

      EditorGUILayout.LabelField(@"TV Tube Dots");

      EditorGUI.indentLevel++;

      thisTarget.tvDots = VideoGlitchEditorHelper.IntSliderWithReset(@"Style", "", (int)thisTarget.tvDots, 0, 4, 1);
      thisTarget.tvDotsBlend = VideoGlitchEditorHelper.SliderWithReset(@"Stripes", "", thisTarget.tvDotsBlend, 0.0f, 1000.0f, 1.0f);

      EditorGUI.indentLevel--;

      EditorGUILayout.LabelField(@"Moire");

      EditorGUI.indentLevel++;

      thisTarget.moireOpacity = VideoGlitchEditorHelper.SliderWithReset(@"Gain", "", thisTarget.moireOpacity, 0.0f, 100.0f, 1.0f);
      thisTarget.moireScale = VideoGlitchEditorHelper.SliderWithReset(@"Scale", "", thisTarget.moireScale, 0.01f, 100.0f, 0.15f);

      EditorGUI.indentLevel--;

      EditorGUILayout.LabelField(@"CRT Distortion");

      EditorGUI.indentLevel++;

      thisTarget.crtDistort = VideoGlitchEditorHelper.SliderWithReset(@"Tube distortion", "", thisTarget.crtDistort, 0.0f, 10.0f, 1.0f);
      thisTarget.crtScale = VideoGlitchEditorHelper.SliderWithReset(@"Scale", "", thisTarget.crtScale, 1.0f, 10.0f, 1.06f);

      EditorGUI.indentLevel--;

      EditorGUILayout.LabelField(@"TV Lines");

      EditorGUI.indentLevel++;

      thisTarget.tvLines = VideoGlitchEditorHelper.SliderWithReset(@"Lines", "", thisTarget.tvLines, 0.01f, 10.0f, 2.5f);
      thisTarget.tvLinesOpacity = VideoGlitchEditorHelper.SliderWithReset(@"Opacity", "", thisTarget.tvLinesOpacity, 0.0f, 10.0f, 1.0f);

      EditorGUI.indentLevel--;
    }
  }
}
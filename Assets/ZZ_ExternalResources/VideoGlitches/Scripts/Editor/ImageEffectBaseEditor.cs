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
  /// ImageEffect Editor Base.
  /// </summary>
  [CustomEditor(typeof(ImageEffectBase))]
  public abstract class ImageEffectBaseEditor : Editor
  {
    /// <summary>
    /// Help text.
    /// </summary>
    public string Help { get; set; }

    /// <summary>
    /// Warnings.
    /// </summary>
    public string Warnings { get; set; }

    /// <summary>
    /// Errors.
    /// </summary>
    public string Errors { get; set; }

    private ImageEffectBase baseTarget;

    private bool foldoutColorControls = false;

    /// <summary>
    /// OnInspectorGUI.
    /// </summary>
    public override void OnInspectorGUI()
    {
      if (baseTarget == null)
        baseTarget = this.target as ImageEffectBase;

      EditorGUIUtility.fieldWidth = 0.0f;
      
      EditorGUI.indentLevel = 0;

      EditorGUIUtility.labelWidth = 125.0f;

      EditorGUILayout.BeginVertical();
      {
        EditorGUILayout.Separator();

#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6)
        if (EditorGUIUtility.isProSkin == true)
#endif
        {
          /////////////////////////////////////////////////
          // Common.
          /////////////////////////////////////////////////
          baseTarget.amount = VideoGlitchEditorHelper.IntSliderWithReset(@"Amount", "The strength of the effect.\nFrom 0 (no effect) to 100 (full effect).", Mathf.RoundToInt(baseTarget.amount * 100.0f), 0, 100, 100) * 0.01f;

          /////////////////////////////////////////////////
          // Custom.
          /////////////////////////////////////////////////
          Inspector();

          EditorGUILayout.Separator();

          /////////////////////////////////////////////////
          // Color.
          /////////////////////////////////////////////////
          foldoutColorControls = EditorGUILayout.Foldout(foldoutColorControls, "Color controls");
          if (foldoutColorControls == true)
          {
            EditorGUI.indentLevel++;

            baseTarget.brightness = VideoGlitchEditorHelper.IntSliderWithReset(@"Brightness", "The Screen appears to be more o less radiating light.\nFrom -100 (dark) to 100 (full light).", Mathf.RoundToInt(baseTarget.brightness * 100.0f), -100, 100, 0) * 0.01f;

            baseTarget.contrast = VideoGlitchEditorHelper.IntSliderWithReset(@"Contrast", "The difference in color and brightness.\nFrom -100 (no constrast) to 100 (full constrast).", Mathf.RoundToInt(baseTarget.contrast * 100.0f), -100, 100, 0) * 0.01f;

            baseTarget.gamma = VideoGlitchEditorHelper.SliderWithReset(@"Gamma", "Optimizes the contrast and brightness in the midtones.\nFrom 0.01 to 10.", baseTarget.gamma, 0.01f, 10.0f, 1.0f);

            if (baseTarget.GetType() != typeof(VideoGlitchBlackWhiteDistortion))
            {
              EditorGUILayout.Separator();

              baseTarget.saturation = VideoGlitchEditorHelper.SliderWithReset(@"Saturation", "Colorfulness. From 0.0 to 1.0", baseTarget.saturation, 0.0f, 1.0f, 1.0f);
            }

            EditorGUI.indentLevel--;
          }

          EditorGUILayout.Separator();

          /////////////////////////////////////////////////
          // Misc.
          /////////////////////////////////////////////////

          EditorGUILayout.BeginHorizontal();
          {
            if (GUILayout.Button(new GUIContent("[doc]", "Online documentation"), GUI.skin.label) == true)
              Application.OpenURL(VideoGlitchEditorHelper.DocumentationURL);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset ALL") == true)
              baseTarget.ResetDefaultValues();
          }
          EditorGUILayout.EndHorizontal();

          EditorGUILayout.Separator();
        }
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6)
        else
        {
          this.Help = string.Empty;
          this.Errors = "'Video Glitches' require Unity Pro version!";
        }
#endif
        if (string.IsNullOrEmpty(Warnings) == false)
        {
          EditorGUILayout.HelpBox(Warnings, MessageType.Warning);

          EditorGUILayout.Separator();
        }

        if (string.IsNullOrEmpty(Errors) == false)
        {
          EditorGUILayout.HelpBox(Errors, MessageType.Error);

          EditorGUILayout.Separator();
        }

        if (string.IsNullOrEmpty(Help) == false)
          EditorGUILayout.HelpBox(Help, MessageType.Info);
      }
      EditorGUILayout.EndVertical();

      Warnings = Errors = string.Empty;

      if (GUI.changed == true)
        EditorUtility.SetDirty(target);

      EditorGUIUtility.fieldWidth = EditorGUIUtility.labelWidth = 0.0f;

      EditorGUI.indentLevel = 0;
    }

    /// <summary>
    /// Inspector.
    /// </summary>
    protected virtual void Inspector()
    {
      DrawDefaultInspector();
    }
  }
}
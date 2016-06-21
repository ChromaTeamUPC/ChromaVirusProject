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

namespace VideoGlitches
{
  /// <summary>
  /// Video Glitch Old TV.
  /// </summary>
  [ExecuteInEditMode]
  [RequireComponent(typeof(Camera))]
  [AddComponentMenu("Image Effects/Video Glitches/Video Glitch Old TV")]
  public sealed class VideoGlitchOldTV : ImageEffectBase
  {
    /// <summary>
    /// Slow moving scanlines [0.0 - 1.0].
    /// </summary>
    public float slowScan = 0.01f;

    /// <summary>
    /// Tiny scanlines [0.0 - 2.0].
    /// </summary>
    public float scanLine = 0.6f;

    /// <summary>
    /// Softness of the vignette [0.0 - 1.0].
    /// </summary>
    public float vignetteSoftness = 0.9f;

    /// <summary>
    /// Scale of the vignette [0.0 - 1.0].
    /// </summary>
    public float vignetteScale = 0.14f;

    /// <summary>
    /// Vignette tube scale [0.01 - 10.0].
    /// </summary>
    public float vignetteTubeScale = 0.7f;

    /// <summary>
    /// Grain opacity [0.0 - 100.0].
    /// </summary>
    public float grainOpacity = 5.0f;

    /// <summary>
    /// Saturation of the grain[0.0 - 1.0].
    /// </summary>
    public float grainSaturation = 0.0f;

    /// <summary>
    /// Scanline distortion [0.0 - 10.0].
    /// </summary>
    public float scanDistort = 0.03f;

    /// <summary>
    /// Distortion frecuency [0.0 - 5.0].
    /// </summary>
    public float timer = 0.85f;

    /// <summary>
    /// Distortion speed [1.0 - 5.0].
    /// </summary>
    public float speed = 2.0f;

    /// <summary>
    /// Tube distortion [0.0 - 5.0].
    /// </summary>
    public float crtDistort = 0.03f;

    /// <summary>
    /// Tube distortion scale [1.0 - 10.0].
    /// </summary>
    public float crtScale = 1.06f;

    /// <summary>
    /// VHS stripes count [0.0 - 1000.0].
    /// </summary>
    public float stripesCount = 0.5f;

    /// <summary>
    /// VHS stripes opacity [0.0 - 10.0].
    /// </summary>
    public float stripesOpacity = 1.0f;

    /// <summary>
    /// VHS bars count [0.0 - 1000.0].
    /// </summary>
    public float barsCount = 5.0f;

    /// <summary>
    /// Moire opacity [0.0 - 100.0].
    /// </summary>
    public float moireOpacity = 1.0f;

    /// <summary>
    /// Moire scale [0.01 - 100.0].
    /// </summary>
    public float moireScale = 0.15f;

    /// <summary>
    /// TV lines [0.01 - 10.0].
    /// </summary>
    public float tvLines = 2.5f;

    /// <summary>
    /// TV lines opacity [0.0 - 10.0].
    /// </summary>
    public float tvLinesOpacity = 1.0f;

    /// <summary>
    /// TV dots style [0 - 4].
    /// </summary>
    public float tvDots = 1.0f;

    /// <summary>
    /// TV dots blend [0.0 - 1000.0].
    /// </summary>
    public float tvDotsBlend = 1.0f;

    private const string variableScanline = @"_Scanline";
    private const string variableSlowscan = @"_Slowscan";
    private const string variableVignetteSoftness = @"_VignetteSoftness";
    private const string variableVignetteScale = @"_VignetteScale";
    private const string variableGrainOpacity = @"_GrainOpacity";
    private const string variableSaturation = @"_SaturationTV";
    private const string variableScanDistort = @"_ScanDistort";
    private const string variableTimer = @"_Timer";
    private const string variableSpeed = @"_Speed";
    private const string variableDistort = @"_Distort";
    private const string variableScale = @"_Scale";
    private const string variableStripesCount = @"_StripesCount";
    private const string variableOpacity = @"_Opacity";
    private const string variableBarsCount = @"_BarsCount";
    private const string variableOpacityMoire = @"_OpacityMoire";
    private const string variableMoireScale = @"_MoireScale";
    private const string variableTVLines = @"_TVLines";
    private const string variableTVLinesOpacity = @"_TVLinesOpacity";
    private const string variableTVTubeVignetteScale = @"_TVTubeVignetteScale";
    private const string variableTVDots = @"_TVDots";
    private const string variableTVDotsBlend = @"_TVDotsBlend";

    /// <summary>
    /// Shader path.
    /// </summary>
    protected override string ShaderPath { get { return @"Shaders/VideoGlitchOldTV"; } }

    /// <summary>
    /// Set the default values of the shader.
    /// </summary>
    public override void ResetDefaultValues()
    {
      slowScan = 0.01f;
      scanLine = 0.6f;
      vignetteSoftness = 0.9f;
      vignetteScale = 0.14f;
      grainOpacity = 5.0f;
      grainSaturation = 0.0f;
      scanDistort = 0.03f;
      timer = 0.85f;
      speed = 2.0f;
      crtDistort = 1.0f;
      crtScale = 1.06f;
      stripesCount = 0.5f;
      stripesOpacity = 1.0f;
      barsCount = 5.0f;
      moireOpacity = 1.0f;
      moireScale = 0.15f;
      tvLines = 2.5f;
      tvLinesOpacity = 1.0f;
      vignetteTubeScale = 0.7f;
      tvDots = 1.0f;
      tvDotsBlend = 1.0f;

      base.ResetDefaultValues();
    }

    /// <summary>
    /// Set the values to shader.
    /// </summary>
    protected override void SendValuesToShader()
    {
      this.Material.SetFloat(variableScanline, scanLine);
      this.Material.SetFloat(variableSlowscan, slowScan);
      this.Material.SetFloat(variableVignetteSoftness, vignetteSoftness);
      this.Material.SetFloat(variableVignetteScale, vignetteScale);
      this.Material.SetFloat(variableGrainOpacity, grainOpacity);
      this.Material.SetFloat(variableSaturation, grainSaturation);
      this.Material.SetFloat(variableScanDistort, scanDistort);
      this.Material.SetFloat(variableTimer, timer);
      this.Material.SetFloat(variableSpeed, speed);
      this.Material.SetFloat(variableDistort, crtDistort);
      this.Material.SetFloat(variableScale, crtScale);
      this.Material.SetFloat(variableStripesCount, stripesCount);
      this.Material.SetFloat(variableOpacity, stripesOpacity);
      this.Material.SetFloat(variableBarsCount, barsCount);
      this.Material.SetFloat(variableOpacityMoire, moireOpacity);
      this.Material.SetFloat(variableMoireScale, moireScale);
      this.Material.SetFloat(variableTVLines, tvLines);
      this.Material.SetFloat(variableTVLinesOpacity, tvLinesOpacity);
      this.Material.SetFloat(variableTVTubeVignetteScale, vignetteTubeScale);
      this.Material.SetFloat(variableTVDots, tvDots);
      this.Material.SetFloat(variableTVDotsBlend, tvDotsBlend);

      base.SendValuesToShader();
    }
  }
}
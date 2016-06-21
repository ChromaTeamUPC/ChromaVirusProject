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
  /// Utilities.
  /// </summary>
  public static class VideoGlitchesHelper
  {
    /// <summary>
    /// Load a 2D texture from "Resources/Textures".
    /// </summary>
    public static Texture2D LoadTextureFromResources(string texturePathFromResources)
    {
      Texture2D texture = Resources.Load<Texture2D>(texturePathFromResources);
      if (texture == null)
        Debug.LogWarning(string.Format("Texture '{0}' not found in 'Resources/Textures' folder.", texturePathFromResources));

      return texture;
    }
  }
}
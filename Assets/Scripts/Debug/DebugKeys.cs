using UnityEngine;
using System.Collections;

public class DebugKeys : MonoBehaviour {
    public KeyCode toggleGodMode = KeyCode.F1;
    public KeyCode mainCameraFollowPlayersKey = KeyCode.F2;
    public KeyCode mainCameraActivationKey = KeyCode.Alpha1;
    public KeyCode godCameraActivationKey = KeyCode.Alpha2;
    public KeyCode staticCamera1ActivationKey = KeyCode.Alpha3;
    public KeyCode staticCamera2ActivationKey = KeyCode.Alpha4;
    public KeyCode staticCamera3ActivationKey = KeyCode.Alpha5;
    //... morrrre camerrasssss

    public KeyCode toggleStatsKey = KeyCode.F7;
    public KeyCode toggleWireframeKey = KeyCode.F8;
    public KeyCode toggleGizmos = KeyCode.F9;

    public KeyCode toggleGrayScaleKey;
    public KeyCode toggleNoiseKey;
    public KeyCode toggleMusic;
    public KeyCode playNoiseFx;

    public KeyCode doubleSpeed = KeyCode.KeypadPlus;
    public KeyCode halfSpeed = KeyCode.KeypadMinus;

    //God camera control
    public KeyCode godCameraForward = KeyCode.UpArrow;
    public KeyCode godCameraBackward = KeyCode.DownArrow;
    public KeyCode godCameraLeft = KeyCode.LeftArrow;
    public KeyCode godCameraRight = KeyCode.RightArrow;

    public KeyCode takeScreenshot = KeyCode.Return;
}

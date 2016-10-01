using UnityEngine;
using System.Collections;

public class DebugKeys : MonoBehaviour {
    public KeyCode toggleGodMode = KeyCode.F1;
    public KeyCode toggleAlwaysKillOk = KeyCode.F2;
    public KeyCode toggleShowPlayerLimits = KeyCode.F3;
    public KeyCode mainCameraFollowPlayersKey = KeyCode.F4;
    public KeyCode mainCameraActivationKey = KeyCode.Alpha1;
    public KeyCode godCameraActivationKey = KeyCode.Alpha2;
    //... morrrre camerrasssss

    public KeyCode toggleStatsKey = KeyCode.F7;
    public KeyCode toggleWireframeKey = KeyCode.F8;

    public KeyCode doubleSpeed = KeyCode.KeypadPlus;
    public KeyCode halfSpeed = KeyCode.KeypadMinus;

    //God camera control
    public KeyCode godCameraForward = KeyCode.UpArrow;
    public KeyCode godCameraBackward = KeyCode.DownArrow;
    public KeyCode godCameraLeft = KeyCode.LeftArrow;
    public KeyCode godCameraRight = KeyCode.RightArrow;

    public KeyCode takeScreenshot = KeyCode.Return;
}

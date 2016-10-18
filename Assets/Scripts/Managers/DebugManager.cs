using UnityEngine;
using System.Collections;

public class DebugManager : MonoBehaviour {

    [Header("Debug Keys")]
    public KeyCode toggleGodMode = KeyCode.F1;
    public KeyCode toggleAlwaysKillOk = KeyCode.F2;
    public KeyCode toggleShowPlayerLimits = KeyCode.F3;
    public KeyCode mainCameraFollowPlayersKey = KeyCode.F4;
    public KeyCode toggleUI = KeyCode.F5;
    public KeyCode toggleGodCameraMovement = KeyCode.F6;
    public KeyCode toggleStatsKey = KeyCode.F7;
    public KeyCode toggleWireframeKey = KeyCode.F8;

    public KeyCode mainCameraActivationKey = KeyCode.Alpha1;
    public KeyCode godCameraActivationKey = KeyCode.Alpha2;

    public KeyCode doubleSpeed = KeyCode.KeypadPlus;
    public KeyCode halfSpeed = KeyCode.KeypadMinus;

    //God camera control
    public KeyCode godCameraForward = KeyCode.UpArrow;
    public KeyCode godCameraBackward = KeyCode.DownArrow;
    public KeyCode godCameraLeft = KeyCode.LeftArrow;
    public KeyCode godCameraRight = KeyCode.RightArrow;

    public KeyCode takeScreenshot = KeyCode.Return;

    [Header("Control Variables")]
    public bool debugModeEnabled = false;

    public float refreshFPSTime = 0.5f;

    private int linePosition;
    private int triangleCount;

    private string textFPS;
    private string textTriangles;

    public bool canMoveGodCamera = true;
    public bool UIVisible = true;
    private bool statsVisible = false;
    private bool calculateTriangles = false;
    public bool viewWireFrame = false;

    public bool godMode = false;
    public bool alwaysKillOk = false;
    public bool showPlayerLimits = false;

    private float elapsedTime;
    private int frameCount;
    private int fps;

    void Start()
    {
        frameCount = 0;
        elapsedTime = 0f;
        fps = 0;
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (debugModeEnabled)
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
            {
                debugModeEnabled = false;
                rsc.audioMng.backFx.Play();

                canMoveGodCamera = true;
                UIVisible = true;
                statsVisible = false;
                viewWireFrame = false;
                godMode = false;
                alwaysKillOk = false;
                showPlayerLimits = false;
                rsc.camerasMng.SetCameraFollowPlayers();
                rsc.camerasMng.ChangeCamera(0);
                if(!rsc.gameMng.IsPaused())
                    Time.timeScale = 1;
            }

            if (Input.GetKeyDown(toggleStatsKey))
            {
                statsVisible = !statsVisible;
                if (statsVisible) calculateTriangles = true;
            }

            if (Input.GetKeyDown(toggleWireframeKey))
            {
                viewWireFrame = !viewWireFrame;
            }

            if (Input.GetKeyDown(toggleGodMode))
            {
                godMode = !godMode;
            }

            if (Input.GetKeyDown(toggleAlwaysKillOk))
            {
                alwaysKillOk = !alwaysKillOk;
            }

            if (Input.GetKeyDown(toggleShowPlayerLimits))
            {
                showPlayerLimits = !showPlayerLimits;
            }

            if (Input.GetKeyDown(toggleUI))
            {
                UIVisible = !UIVisible;
            }

            if (Input.GetKeyDown(toggleGodCameraMovement))
            {
                canMoveGodCamera = !canMoveGodCamera;
            }

            if (Input.GetKeyDown(mainCameraFollowPlayersKey))
            {
                rsc.camerasMng.ToggleCameraFollowPlayers();
            }

            if (Input.GetKeyDown(mainCameraActivationKey))
            {
                rsc.camerasMng.ChangeCamera(0);
            }

            if (Input.GetKeyDown(godCameraActivationKey))
            {
                rsc.camerasMng.ChangeCamera(2);
            }

            if (Input.GetKeyDown(doubleSpeed))
            {
                Debug.Log("Was: " + Time.timeScale);
                Time.timeScale *= 2;
                Debug.Log("Now: " + Time.timeScale);
            }

            if (Input.GetKeyDown(halfSpeed))
            {
                Debug.Log("Was: " + Time.timeScale);
                Time.timeScale /= 2;
                Debug.Log("Now: " + Time.timeScale);
            }

            elapsedTime += Time.deltaTime;
            ++frameCount;
            if (elapsedTime >= refreshFPSTime)
            {
                fps = (int)(frameCount / elapsedTime);
                frameCount = 0;
                elapsedTime -= refreshFPSTime;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
            {
                debugModeEnabled = true;
                rsc.audioMng.acceptFx.Play();
            }
        }
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(takeScreenshot))
        {
            //string datetime = System.String.Format("yyyyMMddHHmmss", System.DateTime.Now);
            string datetime = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            Debug.Log(datetime);
            Application.CaptureScreenshot("ChromavirusScreenshot_" + datetime + ".png", 4);
            //Application.CaptureScreenshot("ChromavirusScreenshot_" + screenshotnum++ + ".png");
        }
    }

    void OnGUI()
    {
        if (calculateTriangles)
        {
            triangleCount = GetCurrentTriangleCount();
            calculateTriangles = false;
        }

        if (statsVisible)
        {
            linePosition = 0;

            //textFPS = "Fps: " + GetCurrentFPS();
            textFPS = "Fps: " + fps;
            textTriangles = "Triangles in scene: " + triangleCount;

            ShowStat(ref textFPS);
            ShowStat(ref textTriangles);
        }
    }

    void ShowStat(ref string text)
    {
        int w = Screen.width;
        int h = Screen.height;
        GUIStyle textStyle = new GUIStyle();

        Rect rect = new Rect(-10, linePosition * 20 + 10, w, h * 2 / 100);

        textStyle.alignment = TextAnchor.UpperRight;
        textStyle.fontSize = h * 2 / 100;
        textStyle.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        GUI.Label(rect, text, textStyle);
        linePosition++;

    }

    int GetCurrentFPS()
    {
        return (int)(1.0f / Time.smoothDeltaTime);
    }

    int GetCurrentTriangleCount()
    {
        int triangleCount = 0;

        object[] allGameObjects = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject singleObject in allGameObjects)
        {
            Component[] filters = singleObject.GetComponents(typeof(MeshFilter));
            foreach (MeshFilter f in filters)
            {
                triangleCount += f.sharedMesh.triangles.Length / 3;
            }
        }

        return triangleCount;
    }
}

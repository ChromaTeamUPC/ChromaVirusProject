﻿using UnityEngine;
using System.Collections;

public class DebugManager : MonoBehaviour {

    public DebugKeys keys;

    public float refreshFPSTime = 0.5f;

    private int linePosition;
    private int triangleCount;

    private string textFPS;
    private string textTriangles;

    private bool statsVisible = false;
    private bool calculateTriangles = false;

    public bool godMode = false;
    public bool alwaysKillOk = false;

    private float elapsedTime;
    private int frameCount;
    private int fps;

    void Start()
    {
        frameCount = 0;
        elapsedTime = 0f;
        fps = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keys.toggleStatsKey))
        {
            statsVisible = !statsVisible;
            if (statsVisible) calculateTriangles = true;
        }
        
        if(Input.GetKeyDown(keys.toggleGodMode))
        {
            godMode = !godMode;
        }

        if (Input.GetKeyDown(keys.toggleAlwaysKillOk))
        {
            alwaysKillOk = !alwaysKillOk;
        }

        if (Input.GetKeyDown(keys.doubleSpeed))
        {
            Time.timeScale *= 2;
        }

        if(Input.GetKeyDown(keys.halfSpeed))
        {
            Time.timeScale /= 2;
        }

        elapsedTime += Time.deltaTime;
        ++frameCount;
        if(elapsedTime >= refreshFPSTime)
        {
            fps = (int)(frameCount / elapsedTime);
            frameCount = 0;
            elapsedTime -= refreshFPSTime;
        }
    }

    void LateUpdate()
    {

        if (Input.GetKeyDown(keys.takeScreenshot))
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

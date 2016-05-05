using UnityEngine;
using System.Collections;

public class DebugManager : MonoBehaviour {

    public DebugKeys keys;

    private int linePosition;
    private int triangleCount;

    private string textFPS;
    private string textTriangles;

    private bool statsVisible = false;
    private bool calculateTriangles = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keys.toggleStatsKey))
        {
            statsVisible = !statsVisible;
            if (statsVisible) calculateTriangles = true;
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

            textFPS = "Fps: " + GetCurrentFPS();
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

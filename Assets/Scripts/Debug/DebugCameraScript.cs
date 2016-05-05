using UnityEngine;
using System.Collections;

public class DebugCameraScript : MonoBehaviour {

    private DebugKeys keys;

    private bool viewWireFrame = false;

    void Start()
    {
        keys = rsc.debugMng.keys;
    }

    void Update()
    {
        if (Input.GetKeyDown(keys.toggleWireframeKey))
            viewWireFrame = !viewWireFrame;
    }

    void OnPreRender()
    {
        if (viewWireFrame)
            GL.wireframe = true;
    }

    void OnPostRender()
    {
        GL.wireframe = false;
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RotateObjects))]
public class RotateObjectInEditor : Editor
{
    private void CallbackFunction()
    {
        RotateObjects mTarget = target as RotateObjects;
        mTarget.Update();
    }

    void OnEnable()
    {
        EditorApplication.update += CallbackFunction;
    }

    void OnDisable()
    {
        //EditorApplication.update -= CallbackFunction;
    }
}

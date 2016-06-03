using UnityEngine;
using System.Collections;

public class CoroutineHelper : MonoBehaviour {

    void OnDestroy()
    {
        StopAllCoroutines();
    }

	public void StartCoroutineHelp(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public void StopCoroutineHelp(IEnumerator coroutine)
    {
        StopCoroutine(coroutine);
    }
}

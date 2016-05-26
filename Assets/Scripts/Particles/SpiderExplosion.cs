using UnityEngine;
using System.Collections;

public class SpiderExplosion : MonoBehaviour {

	void OnEnable()
    {
        StartCoroutine(DisableSelf());
    }

    private IEnumerator DisableSelf()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
}

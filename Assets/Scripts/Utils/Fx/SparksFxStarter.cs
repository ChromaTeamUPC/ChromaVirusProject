using UnityEngine;
using System.Collections;

public class SparksFxStarter : MonoBehaviour 
{
    public float minStartTime = 0f;
    public float maxStartTime = 2f;

    private ParticleSystem ps;
	// Use this for initialization
	void Awake () 
	{
        ps = GetComponent<ParticleSystem>();
	}
	
	void OnEnable()
    {
        StartCoroutine(StartPS());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        ps.Stop();
    }

    private IEnumerator StartPS()
    {
        yield return new WaitForSeconds(Random.Range(minStartTime, maxStartTime));

        ps.Play();
    }
}

using UnityEngine;
using System.Collections;

public class lightScript : MonoBehaviour 
{
	bool Impact = false;
	public float Sqr;

    private Light flashLight;

    void Awake()
    {
        flashLight = GetComponent<Light>();
    }

	// Use this for initialization
	void OnEnable () 
	{
		Impact = true;
        flashLight.intensity = 7;
		Sqr = flashLight.intensity * flashLight.intensity * ((flashLight.intensity < 0.0f ) ? -1.0f : 1.0f);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Impact)
		{
            flashLight.intensity -= (1.0f / Time.deltaTime) * Sqr * .0001f;
			if (flashLight.intensity <= 0)
			{
                flashLight.intensity = 0;
                Impact = false;
			}
		}
	}
}
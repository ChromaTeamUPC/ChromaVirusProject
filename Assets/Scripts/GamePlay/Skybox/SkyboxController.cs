using UnityEngine;
using System.Collections;

public class SkyboxController : MonoBehaviour 
{
    public float changeDuration = 2f;
    private float elapsedTime;

    [Header("Skybox")]
    public Material skyBoxMat;
    public Color originalSkyTint;
    public Color originalGroundTint;

    public Color clearSkyTint;
    public Color cloarGroundTint;

    [Header("Sphere")]
    public Material sphereMat;
    public Color originalColor;
    public Color clearColor;

	// Use this for initialization
	void Start () 
	{
        sphereMat.color = originalColor;
        skyBoxMat.SetColor("_SkyTint", originalSkyTint);
        skyBoxMat.SetColor("_GroundColor", originalGroundTint);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_CLEARED, LevelCleared);
	}

    void OnDestroy()
    {
        if(rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_CLEARED, LevelCleared);
        }
    }
	
	private void LevelCleared(EventInfo eventInfo)
    {
        StartCoroutine(ChangeColor());
    }

    private IEnumerator ChangeColor()
    {
        elapsedTime = 0f;

        while(elapsedTime < changeDuration)
        {
            float factor = elapsedTime / changeDuration;

            sphereMat.color = Color.Lerp(originalColor, clearColor, factor);
            skyBoxMat.SetColor("_SkyTint", Color.Lerp(originalSkyTint, clearSkyTint, factor));
            skyBoxMat.SetColor("_GroundColor", Color.Lerp(originalGroundTint, cloarGroundTint, factor));
            yield return null;

            elapsedTime += Time.deltaTime;
        }

        sphereMat.color = clearColor;
        skyBoxMat.SetColor("_SkyTint", clearSkyTint);
        skyBoxMat.SetColor("_GroundColor", cloarGroundTint);
    }
}

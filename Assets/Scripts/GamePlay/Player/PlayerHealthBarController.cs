using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealthBarController : MonoBehaviour 
{
    public PlayerController playerController;

    public Color healthEmptyColor;
    public float healthThreshold1 = 0.2f;
    public Color healthThreshold1Color;
    public float healthThreshold2 = 0.3f;
    public Color healthThreshold2Color;
    public float healthThreshold3 = 0.4f;
    public Color healthThreshold3Color;
    public float startGradientThreshold = 0.5f;

    public float brightnessCicleDuration = 0.5f;
    public float startBrightnessThreshold = 0.3f;
    private Color currentHealthColor;
    private float currentBrightness;
    private float brightnessSpeed;

    public Slider playerHealth;
    public Image playerHealthFill;
    private float referenceHealthFactor;

    // Use this for initialization
    void Start () 
	{
        referenceHealthFactor = playerHealth.maxValue / playerController.maxHealth;

        currentBrightness = 1f;
        currentHealthColor = Color.white;

        if (brightnessCicleDuration > 0)
            brightnessSpeed = 1 / brightnessCicleDuration;
        else
            brightnessSpeed = 1;
    }
	
	// Update is called once per frame
	void Update () 
	{
        currentBrightness = (Mathf.Sin(Time.time * Mathf.PI * brightnessSpeed) / 2) + 1; //Values between 0.5 and 1.5

        float pHealthValue = playerController.Health * referenceHealthFactor;
        playerHealth.value = pHealthValue;

        if (pHealthValue > startGradientThreshold)
            currentHealthColor = Color.white;
        else if (pHealthValue >= healthThreshold3)
            currentHealthColor = Color.Lerp(healthThreshold3Color, Color.white, (pHealthValue - healthThreshold3) / (startGradientThreshold - healthThreshold3));
        else if (pHealthValue >= healthThreshold2)
            currentHealthColor = Color.Lerp(healthThreshold2Color, healthThreshold3Color, (pHealthValue - healthThreshold2) / (healthThreshold3 - healthThreshold2));
        else if (pHealthValue >= healthThreshold1)
            currentHealthColor = Color.Lerp(healthThreshold1Color, healthThreshold2Color, (pHealthValue - healthThreshold1) / (healthThreshold2 - healthThreshold1));
        else
            currentHealthColor = Color.Lerp(healthEmptyColor, healthThreshold1Color, pHealthValue / healthThreshold1);

        if (pHealthValue < startBrightnessThreshold)
        {
            currentHealthColor *= currentBrightness;
            currentHealthColor.a = 1f;
        }

        playerHealthFill.color = currentHealthColor;

        //Always oriented the same
        Vector3 lookAt = playerController.blackboard.GetScreenRelativeDirection(Vector3.up);
        lookAt.y = gameObject.transform.position.y;
        gameObject.transform.LookAt(lookAt, Vector3.up);
    }
}

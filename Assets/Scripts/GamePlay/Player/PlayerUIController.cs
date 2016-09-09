using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour 
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

    public Slider playerEnergy;
    public Image playerEnergyPowerTxt;
    private float referenceEnergyFactor;
    public float energyBarInitialValue = 7.5f;
    public float energyBarFinalValue = 93.2f;
    public float energyBarTextBlinkInterval = 0.5f;
    private float energyTextElapsedTime;
    private float energyMaxValue;

    // Use this for initialization
    void Start () 
	{
        energyMaxValue = energyBarFinalValue - energyBarInitialValue; //We must transform player energy (normally 0 to 100) to this top value (0 to energyMaxValue)

        referenceHealthFactor = playerHealth.maxValue / playerController.maxHealth;
        referenceEnergyFactor = energyMaxValue / playerController.maxEnergy;

        currentHealthColor = Color.white;

        currentBrightness = 1f;
        if (brightnessCicleDuration > 0)
            brightnessSpeed = 1 / brightnessCicleDuration;
        else
            brightnessSpeed = 1;
    }
	
	// Update is called once per frame
	void Update () 
	{
        if (playerController.bb.active && playerController.bb.alive)
        {
            currentBrightness = (Mathf.Sin(Time.time * Mathf.PI * brightnessSpeed) / 2) + 1; //Values between 0.5 and 1.5

            //Health
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

            //Energy
            float pEnergyValue = energyBarInitialValue + (playerController.Energy * referenceEnergyFactor);
            playerEnergy.value = pEnergyValue;

            if (playerController.Energy == playerController.maxEnergy)
            {
                energyTextElapsedTime += Time.deltaTime;
                if (energyTextElapsedTime >= energyBarTextBlinkInterval)
                {
                    energyTextElapsedTime -= energyBarTextBlinkInterval;
                    playerEnergyPowerTxt.enabled = !playerEnergyPowerTxt.enabled;
                }
            }
            else
            {
                playerEnergyPowerTxt.enabled = false;
                energyTextElapsedTime = 0f;
            }

            //Always oriented the same
            Vector3 lookAt = playerController.bb.GetScreenRelativeDirection(Vector3.up);
            lookAt.y = 0;
            lookAt = gameObject.transform.position + lookAt;
            gameObject.transform.LookAt(lookAt, Vector3.up);
        }
    }
}

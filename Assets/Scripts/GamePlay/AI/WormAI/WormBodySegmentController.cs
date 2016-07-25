using UnityEngine;
using System.Collections;

public class WormBodySegmentController : MonoBehaviour 
{
    private enum State
    {
        SETTING,
        NORMAL,
        DEACTIVATED,
        DESTROYED
    }

    public float settingMinTime = 1f;
    public float settingMaxTime = 3f;
    public float settingChangeTime = 0.1f;

    public ChromaColor color;
    public float maxHealth = 50f;
    public float wrongColorDamageModifier = 0.25f;

    public WormAIBehaviour worm;

    private State state;
    private float currentHealth;
    private float currentHealthWrongColor;

    private BlinkController blinkController;
    protected Renderer rend;

    void Awake()
    {
        blinkController = GetComponent<BlinkController>();
        rend = GetComponentInChildren<Renderer>();
    }

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

    public void Init(ChromaColor color)
    {
        this.color = color;
        currentHealth = maxHealth;
        currentHealthWrongColor = maxHealth;
        SetMaterial(new[] { rsc.coloredObjectsMng.GetWormBodyMaterial(color) });
        state = State.NORMAL;
    }

    public void Reset(ChromaColor color)
    {
        if (state == State.DESTROYED) return;

        this.color = color;
        currentHealth = maxHealth;
        currentHealthWrongColor = maxHealth;
        state = State.SETTING;

        StartCoroutine(SetRandomColors());
    }

    private IEnumerator SetRandomColors()
    {
        float duration = Random.Range(settingMinTime, settingMaxTime);

        float elapsedTime = 0;

        while(elapsedTime < duration)
        {
            SetMaterial(new[] { rsc.coloredObjectsMng.GetWormBodyMaterial(ChromaColorInfo.Random) });

            yield return new WaitForSeconds(settingChangeTime);
            elapsedTime += settingChangeTime;
        }

        SetMaterial(new[] { rsc.coloredObjectsMng.GetWormBodyMaterial(color) });
        state = State.NORMAL;
    }

    private void SetMaterial(Material[] materials)
    {
        Material[] mats = rend.sharedMaterials;

        if (mats[1] != materials[0])
        {
            mats[1] = materials[0];
            rend.sharedMaterials = mats;

            blinkController.InvalidateMaterials();
        }
    }

    public void ImpactedByShot(ChromaColor shotColor, float damage, PlayerController player)
    {
        if (state != State.NORMAL) return;

        blinkController.BlinkWhiteOnce();

        if (shotColor != color)
        {
            currentHealthWrongColor -= damage * wrongColorDamageModifier;

            if(currentHealthWrongColor <= 0)
            {
                worm.DischargeHead();
            }
        }
        else
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                //Set material grey
                SetMaterial(new[] { rsc.coloredObjectsMng.GetWormGreyMaterial() });
                state = State.DEACTIVATED;

                //Explosion FX?

                worm.ChargeHead();
            }
        }
    }
}

using UnityEngine;
using System.Collections;

public class SpawnPointController : MonoBehaviour {

    public const float portalDuration = 2f;
    public GameObject redPortal;
    private ParticleSystem redPS;
    private float redPortalDuration;
    public GameObject greenPortal;
    private ParticleSystem greenPS;
    private float greenPortalDuration;
    public GameObject bluePortal;
    private ParticleSystem bluePS;
    private float bluePortalDuration;
    public GameObject yellowPortal;
    private ParticleSystem yellowPS;
    private float yellowPortalDuration;

    void Awake()
    {
        redPS = redPortal.GetComponent<ParticleSystem>();
        greenPS = greenPortal.GetComponent<ParticleSystem>();
        bluePS = bluePortal.GetComponent<ParticleSystem>();
        yellowPS = yellowPortal.GetComponent<ParticleSystem>();
        redPS.Stop();
        greenPS.Stop();
        bluePS.Stop();
        yellowPS.Stop();
        redPortalDuration = 0f;
        greenPortalDuration = 0f;
        bluePortalDuration = 0f;
        yellowPortalDuration = 0f;
    }

    void Update()
    {
        if (redPortalDuration > 0)
            UpdatePortal(redPS, ref redPortalDuration);

        if (greenPortalDuration > 0)
            UpdatePortal(greenPS, ref greenPortalDuration);

        if (bluePortalDuration > 0)
            UpdatePortal(bluePS, ref bluePortalDuration);

        if (yellowPortalDuration > 0)
            UpdatePortal(yellowPS, ref yellowPortalDuration);
    }

    private void UpdatePortal(ParticleSystem particleSystem, ref float duration)
    {
        duration -= Time.deltaTime;

        if (duration <= 0)
        {
            particleSystem.Stop();
            duration = 0;
        }
    }

    public void CreatePortal(ChromaColor color, float duration = portalDuration)
    {
        switch (color)
        {
            case ChromaColor.RED:
                InitPortal(redPS, ref redPortalDuration, duration);
                //StartCoroutine(ManagePortal(redPortal, redPS));
                break;
            case ChromaColor.GREEN:
                InitPortal(greenPS, ref greenPortalDuration, duration);
                //StartCoroutine(ManagePortal(greenPortal, greenPS));
                break;
            case ChromaColor.BLUE:
                InitPortal(bluePS, ref bluePortalDuration, duration);
                //StartCoroutine(ManagePortal(bluePortal, bluePS));
                break;
            case ChromaColor.YELLOW:
                InitPortal(yellowPS, ref yellowPortalDuration, duration);
                //StartCoroutine(ManagePortal(yellowPortal, yellowPS));
                break;
            default:
                break;
        }
    }

    private void InitPortal(ParticleSystem particleSystem, ref float currentDuration, float newDuration)
    {
        if (currentDuration == 0)
            particleSystem.Play();

        if (newDuration > currentDuration)
            currentDuration = newDuration;
    }

    private IEnumerator ManagePortal(GameObject portal, ParticleSystem ps)
    {
        portal.SetActive(true);
        ps.Play();
        yield return new WaitForSeconds(portalDuration);
        portal.SetActive(false);
    }
}

using UnityEngine;
using System.Collections;

public class SpawnPointController : MonoBehaviour {

    public const float portalDuration = 1.25f;

    public GameObject redPortal;
    private float redPortalDuration;
    private bool isRedStopping;

    public GameObject greenPortal;
    private float greenPortalDuration;
    private bool isGreenStopping;

    public GameObject bluePortal;
    private float bluePortalDuration;
    private bool isBlueStopping;

    public GameObject yellowPortal;
    private float yellowPortalDuration;
    private bool isYellowStopping;

    private ParticleSystem[] redPSs;
    private ParticleSystem[] greenPSs;
    private ParticleSystem[] bluePSs;
    private ParticleSystem[] yellowPSs;

    void Awake()
    {
        redPortalDuration = 0f;
        greenPortalDuration = 0f;
        bluePortalDuration = 0f;
        yellowPortalDuration = 0f;
        isRedStopping = false;
        isGreenStopping = false;
        isBlueStopping = false;
        isYellowStopping = false;

        redPSs = redPortal.GetComponentsInChildren<ParticleSystem>();
        greenPSs = greenPortal.GetComponentsInChildren<ParticleSystem>();
        bluePSs = bluePortal.GetComponentsInChildren<ParticleSystem>();
        yellowPSs = yellowPortal.GetComponentsInChildren<ParticleSystem>();
    }

    void Update()
    {
        CheckStop(redPSs, ref isRedStopping);
        CheckStop(greenPSs, ref isGreenStopping);
        CheckStop(bluePSs, ref isBlueStopping);
        CheckStop(yellowPSs, ref isYellowStopping);

        UpdatePortal(redPSs, ref redPortalDuration, ref isRedStopping);
        UpdatePortal(greenPSs, ref greenPortalDuration, ref isGreenStopping);
        UpdatePortal(bluePSs, ref bluePortalDuration, ref isBlueStopping);
        UpdatePortal(yellowPSs, ref yellowPortalDuration, ref isYellowStopping);
    }

    private void UpdatePortal(ParticleSystem[] particleSystems, ref float duration, ref bool checkStop)
    {
        if (duration > 0)
        {
            duration -= Time.deltaTime;

            if (duration <= 0)
            {
                for (int i = 0; i < particleSystems.Length; ++i)
                {
                    particleSystems[i].Stop(false);
                }

                checkStop = true;
                duration = 0;
            }
        }
    }


    private void CheckStop(ParticleSystem[] particleSystems, ref bool isStopping)
    {
        if (isStopping)
        {
            isStopping = false;
            for (int i = 0; i < particleSystems.Length; ++i)
            {
                if (particleSystems[i].isStopped)
                {
                    particleSystems[i].Clear(true);
                    particleSystems[i].gameObject.SetActive(false);
                }
                else
                {
                    particleSystems[i].Stop(false);
                    isStopping = true;
                }
            }
        }
    }

    public void CreatePortal(ChromaColor color, float duration = portalDuration)
    {
        switch (color)
        {
            case ChromaColor.RED:
                InitPortal(redPSs, ref isRedStopping, ref redPortalDuration, duration);
                isRedStopping = false;
                break;
            case ChromaColor.GREEN:
                InitPortal(greenPSs, ref isGreenStopping, ref greenPortalDuration, duration);
                isGreenStopping = false;
                break;
            case ChromaColor.BLUE:
                InitPortal(bluePSs, ref isBlueStopping, ref bluePortalDuration, duration);
                isBlueStopping = false;
                break;
            case ChromaColor.YELLOW:
                InitPortal(yellowPSs, ref isYellowStopping, ref yellowPortalDuration, duration);
                isYellowStopping = false;
                break;
            default:
                break;
        }
    }

    private void InitPortal(ParticleSystem[] particleSystems, ref bool isStopping, ref float currentDuration, float newDuration)
    {
        for(int i= 0; i<particleSystems.Length; ++i)
        {
            if (!particleSystems[i].gameObject.activeSelf)
            {
                particleSystems[i].gameObject.SetActive(true);
            }

            if(!particleSystems[i].isPlaying || isStopping)
                particleSystems[i].Play(false);
        }

        if (newDuration > currentDuration)
            currentDuration = newDuration;

        isStopping = false;
    }
}

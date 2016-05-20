using UnityEngine;
using System.Collections;

public class SpawnPointController : MonoBehaviour {

    public const float portalDuration = 2f;

    public GameObject redPortal;
    //private ParticleSystem redPS;
    private float redPortalDuration;
    private bool checkRedStop;

    public GameObject greenPortal;
    //private ParticleSystem greenPS;
    private float greenPortalDuration;
    private bool checkGreenStop;

    public GameObject bluePortal;
    //private ParticleSystem bluePS;
    private float bluePortalDuration;
    private bool checkBlueStop;

    public GameObject yellowPortal;
    //private ParticleSystem yellowPS;
    private float yellowPortalDuration;
    private bool checkYellowStop;

    private ParticleSystem[] redPSs;
    private ParticleSystem[] greenPSs;
    private ParticleSystem[] bluePSs;
    private ParticleSystem[] yellowPSs;

    void Awake()
    {
        /*redPS = redPortal.GetComponent<ParticleSystem>();
        greenPS = greenPortal.GetComponent<ParticleSystem>();
        bluePS = bluePortal.GetComponent<ParticleSystem>();
        yellowPS = yellowPortal.GetComponent<ParticleSystem>();
        redPS.Stop();
        redPS.Clear();
        greenPS.Stop();
        greenPS.Clear();
        bluePS.Stop();
        bluePS.Clear();
        yellowPS.Stop();
        yellowPS.Clear();*/
        redPortalDuration = 0f;
        greenPortalDuration = 0f;
        bluePortalDuration = 0f;
        yellowPortalDuration = 0f;
        checkRedStop = false;
        checkGreenStop = false;
        checkBlueStop = false;
        checkYellowStop = false;

        redPSs = redPortal.GetComponentsInChildren<ParticleSystem>();
        greenPSs = greenPortal.GetComponentsInChildren<ParticleSystem>();
        bluePSs = bluePortal.GetComponentsInChildren<ParticleSystem>();
        yellowPSs = yellowPortal.GetComponentsInChildren<ParticleSystem>();
    }

    void Update()
    {
        CheckStop(redPSs, ref checkRedStop);
        CheckStop(greenPSs, ref checkGreenStop);
        CheckStop(bluePSs, ref checkBlueStop);
        CheckStop(yellowPSs, ref checkYellowStop);

        UpdatePortal(redPSs, ref redPortalDuration, ref checkRedStop);
        UpdatePortal(greenPSs, ref greenPortalDuration, ref checkGreenStop);
        UpdatePortal(bluePSs, ref bluePortalDuration, ref checkBlueStop);
        UpdatePortal(yellowPSs, ref yellowPortalDuration, ref checkYellowStop);

        //TODO DEbug, remove when done
        /*if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            //if (!redPS.isPlaying)
                redPS.Play();
            //Debug.Log("Manual Play +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            //if (!redPS.isStopped)
                redPS.Stop();
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            //if (!greenPS.isPlaying)
                greenPS.Play();
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            //if (!greenPS.isStopped)
                greenPS.Stop();
        }
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            //if (!bluePS.isPlaying)
                bluePS.Play();
        }
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            //if (!bluePS.isStopped)
                bluePS.Stop();
        }
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            //if (!yellowPS.isPlaying)
                yellowPS.Play();
        }
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            //if (!yellowPS.isStopped)
                yellowPS.Stop();
        }*/

        //Debug.Log("Is playing: " + redPS.isPlaying);
        //Debug.Log("Is stopped: " + redPS.isStopped);
    }

    /*private void UpdatePortal(ParticleSystem particleSystem, ref float duration, ref bool checkStop)
    {
        if (duration > 0)
        {
            duration -= Time.deltaTime;

            if (duration <= 0)
            {
                particleSystem.Stop();
                checkStop = true;
                duration = 0;
            }
        }
    }*/

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

    /*private void CheckStop(ParticleSystem particleSystem, ref bool checkStop)
    {
        if(checkStop)
        {
            if(particleSystem.isStopped)
            {
                particleSystem.Clear();
                particleSystem.gameObject.SetActive(false);
                checkStop = false;
            }
        }
    }*/

    private void CheckStop(ParticleSystem[] particleSystems, ref bool checkStop)
    {
        if (checkStop)
        {
            checkStop = false;
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
                    checkStop = true;
                }
            }
        }
    }

    public void CreatePortal(ChromaColor color, float duration = portalDuration)
    {
        switch (color)
        {
            case ChromaColor.RED:
                InitPortal(redPSs, ref redPortalDuration, duration);
                checkRedStop = false;
                break;
            case ChromaColor.GREEN:
                InitPortal(greenPSs, ref greenPortalDuration, duration);
                checkGreenStop = false;
                break;
            case ChromaColor.BLUE:
                InitPortal(bluePSs, ref bluePortalDuration, duration);
                checkBlueStop = false;
                break;
            case ChromaColor.YELLOW:
                InitPortal(yellowPSs, ref yellowPortalDuration, duration);
                checkYellowStop = false;
                break;
            default:
                break;
        }
    }

    /*private void InitPortal(ParticleSystem particleSystem, ref float currentDuration, float newDuration)
    {
        if(!particleSystem.gameObject.activeSelf)
            particleSystem.gameObject.SetActive(true);

        particleSystem.Play();

        if (newDuration > currentDuration)
            currentDuration = newDuration;
    }*/

    private void InitPortal(ParticleSystem[] particleSystems, ref float currentDuration, float newDuration)
    {
        for(int i= 0; i<particleSystems.Length; ++i)
        {
            if (!particleSystems[i].gameObject.activeSelf)
                particleSystems[i].gameObject.SetActive(true);

            particleSystems[i].Play(false);
        }

        if (newDuration > currentDuration)
            currentDuration = newDuration;
    }
}

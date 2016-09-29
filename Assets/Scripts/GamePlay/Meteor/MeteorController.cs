using UnityEngine;
using System.Collections;

public class MeteorController : MonoBehaviour 
{
    public enum SubState
    {
        GOING_UP,
        FALLING,
        EXPLODING
    }

    public float upSpeed = 60f;
    public float upDuration = 3f;

    public float destructionDuration = 1.5f;

    public float minXRotationDPS = 90f;
    public float maxXRotationDPS = 180f;
    public float minYRotationDPS = 90f;
    public float maxYRotationDPS = 180f;
    public float minZRotationDPS = 90f;
    public float maxZRotationDPS = 180f;

    private SubState subState;
    private float elapsedTime;

    [SerializeField]
    private GameObject mainModel;
    [SerializeField]
    private GameObject destructionModel;

    private RotateParticles rotateModel;
    private Animator animator;

    public ParticleSystem fireTrail;
    public GameObject explosionFX;
    public GameObject meteorThrowFx;

    public AudioSource fallingSoundFx;

    void Awake()
    {
        rotateModel = mainModel.GetComponent<RotateParticles>();
        animator = destructionModel.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update () 
	{
        switch (subState)
        {
            case SubState.GOING_UP:
                if(elapsedTime >= upDuration)
                {
                    meteorThrowFx.SetActive(false);
                    rsc.poolMng.meteorPool.AddObject(this);
                }
                else
                {
                    transform.Translate(0f, upSpeed * Time.deltaTime, 0f, Space.World);
                    elapsedTime += Time.deltaTime;
                }
                break;

            case SubState.FALLING:
                //Do nothing. Position controlled by hexagon
                break;

            case SubState.EXPLODING:
                if (elapsedTime >= destructionDuration)
                {
                    explosionFX.SetActive(false);
                    rsc.poolMng.meteorPool.AddObject(this);
                }
                else
                {
                    elapsedTime += Time.deltaTime;
                }
                break;
            default:
                break;
        }
    }

    void OnEnable()
    {
        mainModel.SetActive(true);
        destructionModel.SetActive(false);
        explosionFX.SetActive(false);
        meteorThrowFx.SetActive(false);
        fireTrail.Play();
        fallingSoundFx.Play();

        mainModel.transform.rotation = Random.rotation;
        //Default state is falling
        subState = SubState.FALLING;

        rotateModel.dpsSpeedX = Random.Range(minXRotationDPS, maxXRotationDPS);
        rotateModel.dpsSpeedY = Random.Range(minYRotationDPS, maxYRotationDPS);
        rotateModel.dpsSpeedZ = Random.Range(minZRotationDPS, maxZRotationDPS);
        rotateModel.active = true;
    }


    public void GoUp(float speed)
    {
        upSpeed = speed;
        elapsedTime = 0f;
        subState = SubState.GOING_UP;
        rotateModel.active = false;
        fireTrail.Stop();
        fallingSoundFx.Stop();
        meteorThrowFx.SetActive(true);
    }

    public void Explode()
    {
        fireTrail.Stop();
        fallingSoundFx.Stop();

        mainModel.SetActive(false);
        destructionModel.SetActive(true);
        destructionModel.transform.Rotate(0f, Random.Range(0f, 360f), 0f, Space.World);

        rsc.rumbleMng.Rumble(0, 0.3f, 0.4f, 0.4f);
        rsc.camerasMng.PlayEffect(0, 0.5f, 0.2f);

        animator.Rebind();
        animator.SetFloat("SpeedFactor", Random.Range(1.5f, 2.5f));

        explosionFX.SetActive(true);

        elapsedTime = 0;

        subState = SubState.EXPLODING;
    }
}

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

    public float minXRotationDPS = 90f;
    public float maxXRotationDPS = 180f;
    public float minYRotationDPS = 90f;
    public float maxYRotationDPS = 180f;
    public float minZRotationDPS = 90f;
    public float maxZRotationDPS = 180f;

    private SubState subState;
    private float elapsedTime;

    private GameObject model;
    private RotateParticles rotateModel;

    public ParticleSystem fireTrail;

    void Awake()
    {
        model = transform.Find("Model").gameObject;
        rotateModel = model.GetComponent<RotateParticles>();
    }

    // Update is called once per frame
    void Update () 
	{
        switch (subState)
        {
            case SubState.GOING_UP:
                if(elapsedTime >= upDuration)
                {
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
                //TODO: FX, animations
                rsc.poolMng.meteorPool.AddObject(this);
                break;
            default:
                break;
        }
    }

    void OnEnable()
    {
        model.transform.rotation = Random.rotation;
        //Default state is falling
        subState = SubState.FALLING;

        rotateModel.dpsSpeedX = Random.Range(minXRotationDPS, maxXRotationDPS);
        rotateModel.dpsSpeedY = Random.Range(minYRotationDPS, maxYRotationDPS);
        rotateModel.dpsSpeedZ = Random.Range(minZRotationDPS, maxZRotationDPS);
        rotateModel.active = true;
        fireTrail.Play();
    }


    public void GoUp(float speed)
    {
        upSpeed = speed;
        elapsedTime = 0f;
        subState = SubState.GOING_UP;
        rotateModel.active = false;
        fireTrail.Stop();
    }

    public void Explode()
    {
        fireTrail.Stop();
        subState = SubState.EXPLODING;
    }
}

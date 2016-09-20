using UnityEngine;
using System.Collections;

public class EnergyVoxelController : MonoBehaviour {
    private enum State
    {
        ENTRY,
        IDLE,
        BLINKING,
        ATTRACTED,
        WAIT_PICK_UP_FX
    }
    public bool big = false;
    public float duration = 10;
    public float startBlinkingRatio = 0.6f;

    public float attractionSpeed = 10f;
    public float targetDistanceThreshold = 0.5f;

    public float energyCharge = 0.5f;

    public ParticleSystem lifeTimeParticles;
    public ParticleSystem pickUpParticles;

    private  State state;

    private float elapsedTime;
    private float blinkingTime;
    private PlayerController target;
    private BlinkController blinkController;

    private Rigidbody rigid;

	// Use this for initialization
	void Awake () {
        blinkController = GetComponent<BlinkController>();
        rigid = GetComponent<Rigidbody>();
	}

    void OnEnable()
    {
        target = null;
        elapsedTime = 0f;
        state = State.ENTRY;
        blinkingTime = duration * startBlinkingRatio;
        rigid.isKinematic = false;

        StartCoroutine(StartLifeTimeParticles());
    }

    void OnDisable()
    {
        blinkController.StopPreviousBlinkings();
        lifeTimeParticles.Stop();
        pickUpParticles.Stop();
    }
	
	// Update is called once per frame
	void Update ()
    {
        elapsedTime += Time.deltaTime;

        switch (state)
        {
            case State.ENTRY:
                //First second can not be attracted
                if (elapsedTime >= 1f)
                    state = State.IDLE;
                break;
            case State.IDLE:
                if (!CheckTarget())
                {
                    if (elapsedTime >= blinkingTime)
                    {
                        //blinkController.BlinkTransparentIncremental(duration - blinkingTime);
                        blinkController.BlinkTransparentMultipleTimes(duration - blinkingTime);
                        state = State.BLINKING;
                    }
                    else if (elapsedTime >= duration)
                    {
                        rsc.poolMng.energyVoxelPool.AddObject(this);
                    }
                }
                break;

            case State.BLINKING:
                if(CheckTarget())
                {
                    blinkController.StopPreviousBlinkings();
                }
                else if (elapsedTime >= duration)
                {
                    if(big)
                        rsc.poolMng.bigEnergyVoxelPool.AddObject(this);
                    else
                        rsc.poolMng.energyVoxelPool.AddObject(this);
                }
                break;

            case State.ATTRACTED:
                if (target.ActiveAndAlive)
                {
                    /*check target distance
                     if < threshold add energy to player and go to pool
                     else continue moving to target*/
                    float distance = Vector3.Distance(target.transform.position, transform.position);

                    if (distance > targetDistanceThreshold)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * attractionSpeed);
                    }
                    else
                    {
                        target.RechargeEnergy(energyCharge);
                        elapsedTime = 0f;
                        lifeTimeParticles.Stop();
                        pickUpParticles.Play();
                        state = State.WAIT_PICK_UP_FX;
                    }
                }
                else
                {
                    target = null;
                    rigid.isKinematic = false;
                    state = State.IDLE;
                }
                break;

            case State.WAIT_PICK_UP_FX:
                if(elapsedTime >= pickUpParticles.duration)
                {
                    if (big)
                        rsc.poolMng.bigEnergyVoxelPool.AddObject(this);
                    else
                        rsc.poolMng.energyVoxelPool.AddObject(this);
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

    private IEnumerator StartLifeTimeParticles()
    {
        yield return new WaitForSeconds(Random.value);

        lifeTimeParticles.Play();
    }


    private bool CheckTarget()
    {
        if(target != null)
        {
            rigid.isKinematic = true;
            state = State.ATTRACTED;
            return true;
        }

        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (state == State.ATTRACTED) return;

        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();

            if(player.ActiveAndAlive)
            {               
                target = player;
            }
        }
    }
}

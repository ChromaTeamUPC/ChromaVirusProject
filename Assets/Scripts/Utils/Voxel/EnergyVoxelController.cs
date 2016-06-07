using UnityEngine;
using System.Collections;

public class EnergyVoxelController : MonoBehaviour {
    private enum State
    {
        IDLE,
        BLINKING,
        ATTRACTED
    }
    public float duration = 10;
    public float startBlinkingRatio = 0.6f;

    public float attractionSpeed = 10f;
    public float targetDistanceThreshold = 0.5f;

    public float energyCharge = 0.5f;

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
        state = State.IDLE;
        blinkingTime = duration * startBlinkingRatio;
        rigid.isKinematic = false;
    }

    void OnDisable()
    {
        blinkController.StopPreviousBlinkings();
    }
	
	// Update is called once per frame
	void Update ()
    {
        elapsedTime += Time.deltaTime;

        switch (state)
        {
            case State.IDLE:
                if (elapsedTime >= blinkingTime)
                {
                    blinkController.BlinkTransparentIncremental(duration - blinkingTime);
                    state = State.BLINKING;
                }
                else if (elapsedTime >= duration)
                {
                    rsc.poolMng.energyVoxelPool.AddObject(this);
                }
                break;

            case State.BLINKING:
                if (elapsedTime >= duration)
                {
                    rsc.poolMng.energyVoxelPool.AddObject(this);
                }
                break;

            case State.ATTRACTED:
                if (target.Active && target.Alive)
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
                        rsc.poolMng.energyVoxelPool.AddObject(this);
                    }
                }
                else
                {
                    target = null;
                    rigid.isKinematic = false;
                    state = State.IDLE;
                }
                break;
            default:
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (state == State.ATTRACTED) return;

        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();

            if(player.Active && player.Alive)
            {
                switch (state)
                {
                    case State.IDLE:
                        target = player;
                        rigid.isKinematic = true;
                        state = State.ATTRACTED;
                        break;
                    case State.BLINKING:
                        blinkController.StopPreviousBlinkings();
                        rigid.isKinematic = true;
                        target = player;
                        state = State.ATTRACTED;
                        break;
                }
                target = player;
            }
        }
    }
}

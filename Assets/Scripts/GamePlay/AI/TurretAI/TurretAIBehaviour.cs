using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TurretAIBehaviour : MonoBehaviour 
{
    private enum State
    {
        DISABLED,
        ENABLED,
        KNOCKED_OUT
    }

    [Serializable]
    public class AttackSettings
    {
        public bool active = true;
        public float maxHealth = 500;
        public float damagePerShot = 10;
        public float knockOutTime = 10f;
        public float detectionRadius = 25;
        public float minWaitTime = 2f;
        public float maxWaitTime = 6f;
    }

    public Transform rotationObject;
    public float angularSpeed = 720f;
    public float maxShootAngle = 5f;
    public Transform shotSpawnPoint1;
    public Transform shotSpawnPoint2;
    public Transform muzzleSpawnPoint1;
    public Transform muzzleSpawnPoint2;
    public float knockedOutMaxXoffset = 1.32f;
    public float knockedOutMaxZoffset = 1.32f;
    private float knockedOutYOffset;
    public float knockOutFXChangeTime = 0.2f;
    private float knockOutElapsedTime;
    public ParticleSystem knockedOutFx;
    private BlinkController blinkController;
    public Animator anim;


    public AudioSource stunnedSoundFx;

    [HideInInspector]
    public PlayerController player1;
    [HideInInspector]
    public PlayerController player2;

    [Header("Attack Settings")]
    public SphereCollider sphereCollider;
    public bool isPhaseControlled = true;
    private int currentPhase = 0;
    [SerializeField]
    public AttackSettings[] attackSettings = new AttackSettings[4];
    public AttackSettings AttackSettingsPhase { get { return attackSettings[currentPhase]; } }

    private State state;
    private float attackWaitTime;
    private float elapsedTime;
    private ChromaColor currentColor;
    private float currentHealth;

    public bool Active { get { return state == State.ENABLED; } }

    // Use this for initialization
    void Awake () 
	{
        player1 = null;
        player2 = null;
        blinkController = GetComponent<BlinkController>();
        knockedOutYOffset = knockedOutFx.transform.localPosition.y;
        currentColor = ChromaColor.RED;
	}

    void Start()
    {
        currentPhase = 0;
        currentHealth = AttackSettingsPhase.maxHealth;
        state = State.ENABLED;
        sphereCollider.radius = AttackSettingsPhase.detectionRadius;
        attackWaitTime = UnityEngine.Random.Range(AttackSettingsPhase.minWaitTime, AttackSettingsPhase.maxWaitTime);
        elapsedTime = 0f;

        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        rsc.eventMng.StartListening(EventManager.EventType.KILL_ENEMIES, SelfDisable);
        if(isPhaseControlled)    
            rsc.eventMng.StartListening(EventManager.EventType.WORM_PHASE_ACTIVATED, SetCurrentPhase);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.KILL_ENEMIES, SelfDisable);
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
            if (isPhaseControlled)
                rsc.eventMng.StopListening(EventManager.EventType.WORM_PHASE_ACTIVATED, SetCurrentPhase);
        }
    }
	
	// Update is called once per frame
	void Update () 
	{
        switch (state)
        {
            case State.ENABLED:
                if (elapsedTime < attackWaitTime)
                    elapsedTime += Time.deltaTime;

                PlayerController target = GetNearestPlayer();
                if (target != null)
                {
                    Vector3 lookingVector;

                    lookingVector = target.transform.position - transform.position;
                    lookingVector.y = 0f;

                    Quaternion newRotation = Quaternion.LookRotation(lookingVector);
                    newRotation = Quaternion.RotateTowards(rotationObject.rotation, newRotation, angularSpeed * Time.deltaTime);
                    rotationObject.rotation = newRotation;

                    float angle = Vector3.Angle(lookingVector, rotationObject.forward);

                    if (angle <= maxShootAngle && elapsedTime >= attackWaitTime)
                    {
                        //Shoot
                        MosquitoMainAttackControllerBase attack1;
                        MosquitoMainAttackControllerBase attack2;

                        switch (currentColor)
                        {
                            case ChromaColor.RED:
                                attack1 = rsc.poolMng.mosquitoHomingProjectilePool.GetObject();
                                attack2 = rsc.poolMng.mosquitoHomingProjectilePool.GetObject();
                                break;

                            case ChromaColor.GREEN:
                                attack1 = rsc.poolMng.mosquitoFanProjectilePool.GetObject();
                                attack2 = rsc.poolMng.mosquitoFanProjectilePool.GetObject();
                                break;

                            case ChromaColor.BLUE:
                                attack1 = rsc.poolMng.mosquitoMultipleProjectilePool.GetObject();
                                attack2 = rsc.poolMng.mosquitoMultipleProjectilePool.GetObject();
                                break;

                            case ChromaColor.YELLOW:
                                attack1 = rsc.poolMng.mosquitoSingleProjectilePool.GetObject();
                                attack2 = rsc.poolMng.mosquitoSingleProjectilePool.GetObject();
                                break;

                            default:
                                attack1 = null;
                                attack2 = null;
                                break;
                        }

                        if (attack1 != null)
                        {
                            attack1.Shoot(shotSpawnPoint1, target);

                            MuzzleController muzzle1 = rsc.coloredObjectsMng.GetTurretMuzzle();
                            if (muzzle1 != null)
                            {
                                muzzle1.transform.position = muzzleSpawnPoint1.position;
                                muzzle1.transform.rotation = muzzleSpawnPoint1.rotation;
                                muzzle1.transform.SetParent(muzzleSpawnPoint1);
                                muzzle1.Play();
                            }
                        }

                        if (attack2 != null)
                        {
                            attack2.Shoot(shotSpawnPoint2, target);

                            MuzzleController muzzle2 = rsc.coloredObjectsMng.GetTurretMuzzle();
                            if (muzzle2 != null)
                            {
                                muzzle2.transform.position = muzzleSpawnPoint2.position;
                                muzzle2.transform.rotation = muzzleSpawnPoint2.rotation;
                                muzzle2.transform.SetParent(muzzleSpawnPoint2);
                                muzzle2.Play();
                            }
                        }

                        anim.SetTrigger("Fire");

                        attackWaitTime = UnityEngine.Random.Range(AttackSettingsPhase.minWaitTime, AttackSettingsPhase.maxWaitTime);
                        elapsedTime = 0f;
                    }
                }
                break;

            case State.KNOCKED_OUT:
                if (elapsedTime < AttackSettingsPhase.knockOutTime)
                {
                    elapsedTime += Time.deltaTime;

                    if (knockOutElapsedTime >= knockOutFXChangeTime)
                    {
                        knockOutElapsedTime -= knockOutFXChangeTime;
                        float newX = UnityEngine.Random.Range(knockedOutMaxXoffset * -1, knockedOutMaxXoffset);
                        float newZ = UnityEngine.Random.Range(knockedOutMaxZoffset * -1, knockedOutMaxZoffset);
                        Vector3 newPos = new Vector3(newX, knockedOutYOffset, newZ);
                        knockedOutFx.transform.localPosition = newPos;
                    }
                    else
                    {
                        knockOutElapsedTime += Time.deltaTime;
                    }
                }
                else
                {
                    elapsedTime = 0;
                    currentHealth = AttackSettingsPhase.maxHealth;
                    attackWaitTime = UnityEngine.Random.Range(AttackSettingsPhase.minWaitTime, AttackSettingsPhase.maxWaitTime);
                    state = State.ENABLED;
                    anim.SetBool("Broken", false);
                    knockedOutFx.Stop();
                }
                break;
        }       
	}

    private void SelfDisable(EventInfo eventInfo)
    {
        state = State.DISABLED;
    }

    public bool CanTakeDamage()
    {
        return state == State.ENABLED;
    }

    public void TakeDamage()
    {
        if (state == State.KNOCKED_OUT) return;

        currentHealth -= AttackSettingsPhase.damagePerShot;
        blinkController.BlinkWhiteOnce();

        if(currentHealth <= 0)
        {
            anim.SetInteger("BrokenAnim", UnityEngine.Random.Range(0, 2));
            anim.SetBool("Broken", true);
            knockedOutFx.Play();
            stunnedSoundFx.Play();
            elapsedTime = 0f;
            rsc.rumbleMng.Rumble(0, 0.35f, 0f, 0.75f);
            state = State.KNOCKED_OUT;
        }
    }

    public void ImpactedBySpecial()
    {
        if (state == State.KNOCKED_OUT) return;

        currentHealth = 0;
        blinkController.BlinkWhiteOnce();

        if (currentHealth <= 0)
        {
            anim.SetInteger("BrokenAnim", UnityEngine.Random.Range(0, 2));
            anim.SetBool("Broken", true);
            knockedOutFx.Play();
            stunnedSoundFx.Play();
            elapsedTime = 0f;
            rsc.rumbleMng.Rumble(0, 0.35f, 0f, 0.75f);
            state = State.KNOCKED_OUT;
        }
    }

    private void ColorChanged(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;

        currentColor = info.newColor;
    }

    private void SetCurrentPhase(EventInfo eventInfo)
    {
        WormEventInfo info = (WormEventInfo)eventInfo;
        currentPhase = info.wormBb.wormCurrentPhase;

        sphereCollider.radius = AttackSettingsPhase.detectionRadius;
        attackWaitTime = UnityEngine.Random.Range(AttackSettingsPhase.minWaitTime, AttackSettingsPhase.maxWaitTime);
    }

    private PlayerController GetNearestPlayer()
    {
        PlayerController result = null;
        float distance = 0;

        if(player1 != null && player1.ActiveAndAlive)
        {
            result = player1;
            distance = (transform.position - player1.transform.position).sqrMagnitude;
        }

        if (player2 != null && player2.ActiveAndAlive)
        {
            if(result == null)
            {
                result = player2;
            }
            else
            {
                float distance2 = (transform.position - player2.transform.position).sqrMagnitude;
                if (distance2 < distance)
                    result = player2;
            }
        }

        return result;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TurretAIBehaviour : MonoBehaviour 
{
    [Serializable]
    public class AttackSettings
    {
        public bool active = true;
        public float detectionRadius = 25;
        public float minWaitTime = 2f;
        public float maxWaitTime = 6f;
    }

    public LevelBossController ctrl;
    public Transform rotationObject;
    public float angularSpeed = 720f;
    public float maxShootAngle = 5f;
    public Transform shotSpawnPoint1;
    public Transform shotSpawnPoint2;
    private SphereCollider sphereCollider;

    private PlayerController player1;
    private PlayerController player2;

    [Header("Attack Settings")]
    public bool isPhaseControlled = true;
    private int currentPhase = 0;
    [SerializeField]
    public AttackSettings[] attackSettings = new AttackSettings[4];
    public AttackSettings AttackSettingsPhase { get { return attackSettings[currentPhase]; } }

    private float attackWaitTime;
    private float elapsedTime;
    private ChromaColor currentColor;

    // Use this for initialization
    void Awake () 
	{
        player1 = null;
        player2 = null;
        sphereCollider = GetComponent<SphereCollider>();
        currentColor = ChromaColor.RED;
	}

    void Start()
    {
        currentPhase = 0;
        sphereCollider.radius = AttackSettingsPhase.detectionRadius;
        attackWaitTime = UnityEngine.Random.Range(AttackSettingsPhase.minWaitTime, AttackSettingsPhase.maxWaitTime);
        elapsedTime = 0f;

        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        if(isPhaseControlled)    
            rsc.eventMng.StartListening(EventManager.EventType.WORM_HEAD_ACTIVATED, SetCurrentPhase);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_HEAD_ACTIVATED, SetCurrentPhase);
        }
    }
	
	// Update is called once per frame
	void Update () 
	{
        if (elapsedTime < attackWaitTime)
            elapsedTime += Time.deltaTime;

        PlayerController target = GetNearestPlayer();
        if(target != null)
        {
            Vector3 lookingVector;

            lookingVector = target.transform.position - transform.position;
            lookingVector.y = 0f;

            Quaternion newRotation = Quaternion.LookRotation(lookingVector);
            newRotation = Quaternion.RotateTowards(rotationObject.rotation, newRotation, angularSpeed * Time.deltaTime);
            rotationObject.rotation = newRotation;

            float angle = Vector3.Angle(lookingVector, rotationObject.forward);

            if(angle <= maxShootAngle && elapsedTime >= attackWaitTime)
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
                }

                if (attack2 != null)
                {
                    attack2.Shoot(shotSpawnPoint2, target);
                }

                attackWaitTime = UnityEngine.Random.Range(AttackSettingsPhase.minWaitTime, AttackSettingsPhase.maxWaitTime);
                elapsedTime = 0f;
            }
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

        if(player1 != null && player1.Active && player1.Alive)
        {
            result = player1;
            distance = (transform.position - player1.transform.position).sqrMagnitude;
        }

        if (player2 != null && player2.Active && player2.Alive)
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

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1")
        {
            player1 = other.GetComponent<PlayerController>();
            //Debug.Log("Player 1 entered turret range");
        }

        if (other.tag == "Player2")
        {
            player2 = other.GetComponent<PlayerController>();
            //Debug.Log("Player 2 entered turret range");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player1")
        {
            player1 = null;
            //Debug.Log("Player 1 exit turret range");
        }

        if (other.tag == "Player2")
        {
            player2 = null;
            //Debug.Log("Player 2 exit turret range");
        }
    }
}

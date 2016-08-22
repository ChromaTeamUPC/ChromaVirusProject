using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WormAIBehaviour : MonoBehaviour
{
    private enum HeadSubState
    {
        DEACTIVATED,
        ACTIVATED,
        KNOCKED_OUT
    }

    private WormBlackboard bb;

    [Header("Route Settings")]
    public WormRoute[] routes;
    private WormWayPoint headWayPoint;

    [Header("Fx")]
    public ParticleSystem phaseExplosion;

    [Header("Misc Settings")]
    public GameObject headModel;
    public GameObject testSphere;

    private HeadSubState headState;

    public WormAISpawningState spawningState;
    public WormAIWanderingState wanderingState;
    public WormAIBelowAttackState belowAttackState;
    public WormAIAboveAttackState aboveAttackState;
    public WormAIHeadDestroyedState headDestroyedState;
    public WormAIDyingState dyingState;

    public WormAITestState testState;

    private WormAIBaseState currentState;

    [HideInInspector]
    public NavMeshAgent agent;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public AudioSource audioSource;

    private BlinkController blinkController;
    private Renderer rend;
    private VoxelizationClient voxelization;

    private Color[] gizmosColors = { Color.blue, Color.cyan, Color.green, Color.grey, Color.magenta, Color.red, Color.yellow };
    //Debug
    void OnDrawGizmos()
    {

        for (int i = 0; i < routes.Length; ++i)
        {
            WormRoute route = routes[i];         

            Gizmos.color = gizmosColors[i % gizmosColors.Length];

            for (int j = 1; j < route.wayPoints.Length; ++j)
            {
                Gizmos.DrawLine(route.wayPoints[j - 1].transform.position, route.wayPoints[j].transform.position);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(route.wayPoints[0].transform.position, 1f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(route.wayPoints[route.wayPoints.Length-1].transform.position, 1f);
        }
    }

    // Use this for initialization
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        blinkController = GetComponent<BlinkController>();
        rend = GetComponentInChildren<Renderer>();
        voxelization = GetComponentInChildren<VoxelizationClient>();       
        headState = HeadSubState.DEACTIVATED;
    }

    void Start()
    {
        SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headCurrentChargeLevel));
    }

    public void SetBlackboard(WormBlackboard bb)
    {
        this.bb = bb;

        spawningState = new WormAISpawningState(bb);
        wanderingState = new WormAIWanderingState(bb);
        belowAttackState = new WormAIBelowAttackState(bb);
        aboveAttackState = new WormAIAboveAttackState(bb);
        headDestroyedState = new WormAIHeadDestroyedState(bb);
        dyingState = new WormAIDyingState(bb);

        testState = new WormAITestState(bb);
    }

    public void Init()
    {
        StartNewPhase();
        bb.ConsolidateBodyParts();
        SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headCurrentChargeLevel));
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_ACTIVATED, EventInfo.emptyInfo);
        ChangeState(spawningState);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState != null)
        {
            if(bb.aboveAttackCurrentCooldownTime < bb.AboveAttackSettingsPhase.aboveAttackCooldownTime)
            {
                bb.aboveAttackCurrentCooldownTime += Time.deltaTime;
            }
            currentState.UpdateBodyMovement();

            WormAIBaseState newState = currentState.Update();

            if (newState != null)
            {
                ChangeState(newState);
            }
        }
    }

    protected void ChangeState(WormAIBaseState newState)
    {
        if (currentState != null)
        {
            //Debug.Log(this.GetType().Name + " Exiting: " + currentState.GetType().Name);
            currentState.OnStateExit();
        }
        currentState = newState;
        if (currentState != null)
        {
            //Debug.Log(this.GetType().Name + " Entering: " + currentState.GetType().Name);
            currentState.OnStateEnter();
        }
    }

    public void ChargeHead()
    {
        bb.headCurrentChargeLevel++;
        SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headCurrentChargeLevel));

        if (bb.headCurrentChargeLevel >= bb.headMaxChargeLevel)
        {
            bb.DisableBodyParts();
            headState = HeadSubState.ACTIVATED;
        }

        //rsc.colorMng.PrintColors();
    }

    public void DischargeHead()
    {
        bb.headCurrentChargeLevel = 0;
        SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headCurrentChargeLevel));
        bb.ShuffleBodyParts();
    }

    public void SetMaterial(Material materials)
    {
        Material[] mats = rend.sharedMaterials;

        if (mats[1] != materials)
        {
            mats[1] = materials;
            rend.sharedMaterials = mats;

            blinkController.InvalidateMaterials();
        }
    }

    public void ImpactedByShot(ChromaColor shotColor, float damage, PlayerController player)
    {
        if (headState != HeadSubState.ACTIVATED) return;
   
        if (currentState != null)
        {
            WormAIBaseState newState = currentState.ImpactedByShot(shotColor, damage, player);

            if (newState != null)
            {
                ChangeState(newState);
            }
        }
    }

    public WormAIBaseState ProcessShotImpact(ChromaColor shotColor, float damage, PlayerController player)
    {
        blinkController.BlinkWhiteOnce();

        bb.headCurrentDamage += damage;

        if (bb.headCurrentDamage >= bb.HealthSettingsPhase.headMaxHealth)
        {
            bb.killerPlayer = player;
            headState = HeadSubState.DEACTIVATED;

            return headDestroyedState;

            /*phaseExplosion.Play();

            //If we are not reached last phase, keep going
            if (bb.wormCurrentPhase < bb.wormMaxPhases -1)
            {
                StartNewPhase();
                SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headCurrentChargeLevel));
                rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_ACTIVATED, EventInfo.emptyInfo);
            }
            //Else worm destroyed
            else
            {
                 return dyingState;
            }*/
        }

        return null;
    }

    public void StartNewPhase()
    {
        bb.wormCurrentPhase++;
        //Debug.Log("Worm phase: " + wormPhase);
        bb.headCurrentDamage = 0;
        bb.headCurrentChargeLevel = 0;
    }

    public void Explode()
    {
        StartCoroutine(RandomizeAndExplode());
    }

    private IEnumerator RandomizeAndExplode()
    {
        float elapsedTime = 0;
        int chargeLevel = 0;

        while (elapsedTime < bb.bodyColorsCarrouselMinTime)
        {
            SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(chargeLevel++ % 4));

            yield return new WaitForSeconds(bb.bodyColorsCarrouselChangeInterval);
            elapsedTime += bb.bodyColorsCarrouselChangeInterval;
        }

        voxelization.SpawnFakeVoxels();
        headModel.SetActive(false);

        yield return new WaitForSeconds(2f);

        LevelEventInfo.eventInfo.levelId = -1;
        rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_CLEARED, LevelEventInfo.eventInfo);

        //Destroy(gameObject);
        gameObject.SetActive(false);
    }

    public void SetVisible(bool visible)
    {
        rend.enabled = visible;
    }

    public bool IsVisible()
    {
        return rend.enabled;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            PlayerTouched(player, transform.position);
        }
    }

    public void PlayerTouched(PlayerController player, Vector3 origin)
    {
        if (currentState != null)
            currentState.PlayerTouched(player, origin);
    }

    public bool CheckPlayerInSight(bool filterMinDistance, bool savePlayer)
    {
        bb.playerInSight = null;

        Vector3 forward = transform.forward;
        forward.y = 0;
        bool result = false;

        PlayerController player1 = null;
        float distanceP1 = 0; ;
        float angleP1 = 0;
        PlayerController player2 = null;
        float distanceP2 = 0; ;
        float angleP2 = 0; ;

        //Check player 1
        GameObject player = rsc.enemyMng.GetPlayerIfActive(1);

        if(player != null)
        {
            Vector3 wormPlayer = player.transform.position - transform.position;
            wormPlayer.y = 0;

            //Check distance
            distanceP1 = wormPlayer.magnitude;
            if ((!filterMinDistance || distanceP1 >= bb.AboveAttackSettingsPhase.aboveAttackExposureMinHexagons * HexagonController.DISTANCE_BETWEEN_HEXAGONS)
                && distanceP1 <= bb.AboveAttackSettingsPhase.aboveAttackExposureMaxHexagons * HexagonController.DISTANCE_BETWEEN_HEXAGONS)
            {
                //Check angle
                angleP1 = Vector3.Angle(forward, wormPlayer);
                if (angleP1 <= bb.AboveAttackSettingsPhase.aboveAttackExposureMaxAngle)
                {
                    if(!savePlayer)
                        return true;
                    else
                    {
                        result = true;
                        player1 = player.GetComponent<PlayerController>();
                    }
                }
            }
        }

        //Check player 2
        player = rsc.enemyMng.GetPlayerIfActive(2);

        if (player != null)
        {
            Vector3 wormPlayer = player.transform.position - transform.position;
            wormPlayer.y = 0;

            //Check distance
            distanceP2 = wormPlayer.magnitude;
            if ((!filterMinDistance || distanceP2 >= bb.AboveAttackSettingsPhase.aboveAttackExposureMinHexagons * HexagonController.DISTANCE_BETWEEN_HEXAGONS)
                && distanceP2 <= bb.AboveAttackSettingsPhase.aboveAttackExposureMaxHexagons * HexagonController.DISTANCE_BETWEEN_HEXAGONS)
            {
                //Check angle
                angleP2 = Vector3.Angle(forward, wormPlayer);
                if (angleP2 <= bb.AboveAttackSettingsPhase.aboveAttackExposureMaxAngle)
                {
                    if (!savePlayer)
                        return true;
                    else
                    {
                        result = true;
                        player2 = player.GetComponent<PlayerController>();
                    }
                }
            }
        }

        if(result && savePlayer)
        {
            if (player1 != null && player2 != null)
            {
                bb.playerInSight = (angleP1 <= angleP2 ? player1 : player2);
            }
            else if (player1 != null)
                bb.playerInSight = player1;
            else
                bb.playerInSight = player2;
        }

        return result;
    }   
}

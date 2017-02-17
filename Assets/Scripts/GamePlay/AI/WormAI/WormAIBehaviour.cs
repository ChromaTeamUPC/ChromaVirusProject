using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WormAIBehaviour : MonoBehaviour
{
    public enum HeadSubState
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
    public ParticleSystem angryEyes;
    public ParticleSystem fireSpray;
    public GameObject knockOutFx;

    [Header("Sound Fx")]
    public AudioSource inOutSoundFx;
    public AudioSource attackWarningSoundFx;
    public AudioSource watchingSoundFx;
    public AudioSource jumpAttackSoundFx;
    public AudioSource explosion1SoundFx;
    public AudioSource explosion2SoundFx;
    public AudioSource recoverSoundFx;
    public AudioClip finalExplosionSoundFx;

    [Header("Misc Settings")]
    public GameObject headModel;
    public Transform meteorSpawnPoint;
    public Transform[] energyVoxelsSpawnPoints;

    private HeadSubState headState;
    public HeadSubState HeadState { get { return headState; } }

    public WormAISpawningState spawningState;
    public WormAIWanderingState wanderingState;
    public WormAIBelowAttackState belowAttackState;
    public WormAIAboveAttackState aboveAttackState;
    public WormAIMeteorAttackState meteorAttackState;
    public WormAIKnockOutState knockOutState;
    public WormAIHeadDestroyedState headDestroyedState;
    public WormAIDyingState dyingState;

    private WormAIBaseState currentState;

    [HideInInspector]
    public UnityEngine.AI.NavMeshAgent agent;
    public Animator animator;
    [HideInInspector]
    public AudioSource audioSource;

    private BlinkController blinkController;
    private Renderer rend;
    private VoxelizationClient voxelization;

    private bool overground;

    private bool invulnerable;

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
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        blinkController = GetComponent<BlinkController>();
        rend = GetComponentInChildren<Renderer>();
        voxelization = GetComponentInChildren<VoxelizationClient>();       
        headState = HeadSubState.DEACTIVATED;

        overground = false;
        invulnerable = false;
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
        meteorAttackState = new WormAIMeteorAttackState(bb);
        knockOutState = new WormAIKnockOutState(bb);
        headDestroyedState = new WormAIHeadDestroyedState(bb);
        dyingState = new WormAIDyingState(bb);
    }

    public void Init()
    {
        StartNewPhase();
        bb.ConsolidateBodyParts();
        SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headCurrentChargeLevel));
        //rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_ACTIVATED, EventInfo.emptyInfo);
        ChangeState(spawningState);
    }

    // Update is called once per frame
    void Update()
    {
        if ((transform.position.y > -1) != overground)
        {
            inOutSoundFx.Play();
            overground = transform.position.y > -1;
        }

        if (currentState != null)
        {
            if(bb.aboveAttackCurrentCooldownTime < bb.AboveAttackSettingsPhase.cooldownTime)
            {
                bb.aboveAttackCurrentCooldownTime += Time.deltaTime;
            }

            if (bb.spawningMinionsCurrentCooldownTime < bb.SpawningMinionsSettingsPhase.cooldownTime)
            {
                bb.spawningMinionsCurrentCooldownTime += Time.deltaTime;
            }

            currentState.UpdateBodyMovement();

            WormAIBaseState newState = currentState.Update();

            if (newState != null)
            {
                ChangeState(newState);
            }
        }
    }

    public void SetInvulnerable()
    {
        invulnerable = true;
    }

    public void SetVulnerable()
    {
        invulnerable = false;
    }

    public bool CanSpawnMinion()
    {
        if (currentState != null)
        {
            return currentState.CanSpawnMinion();
        }

        return false;
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

        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_CHARGED, EventInfo.emptyInfo);

        if (bb.headCurrentChargeLevel >= bb.headMaxChargeLevel)
        {
            bb.DisableBodyParts();
            ActivateHead();
        }

        if (bb.attacksEnabled && bb.MeteorAttackSettingsPhase.active && 
            bb.MeteorAttackSettingsPhase.triggerAfterNumberOfBodySegmentsDestroyed > 0 &&
            bb.headCurrentChargeLevel % bb.MeteorAttackSettingsPhase.triggerAfterNumberOfBodySegmentsDestroyed == 0)
        {
            if (bb.meteorInmediate)
                ChangeState(meteorAttackState);
            else
                bb.shouldMeteorBeTriggedAfterWandering = true;
        }
        //rsc.colorMng.PrintColors();
    }

    public void DischargeHead()
    {
        bb.headCurrentChargeLevel = 0;
        SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headCurrentChargeLevel));

        bb.ShuffleBodyParts();

        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_DISCHARGED, EventInfo.emptyInfo);
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

    public void DeactivateHead()
    {
        if (headState == HeadSubState.DEACTIVATED) return;

        knockOutFx.SetActive(false);
        StopAllCoroutines();
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_DEACTIVATED, EventInfo.emptyInfo);
        headState = HeadSubState.DEACTIVATED;
    }

    public void ActivateHead()
    {
        if (headState == HeadSubState.ACTIVATED) return;

        StopAllCoroutines();
        headState = HeadSubState.ACTIVATED;
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_ACTIVATED, EventInfo.emptyInfo);
        StartCoroutine(BlinkHeadActivated());
    }

    public IEnumerator BlinkHeadActivated()
    {
        while (true)
        {
            SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headMaxChargeLevel));
            yield return new WaitForSeconds(bb.headVulnerableBlinkInterval);
            SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(0));
            yield return new WaitForSeconds(bb.headVulnerableBlinkInterval);
        }
    }

    public void HeadKnockOut()
    {
        if (headState == HeadSubState.KNOCKED_OUT) return;

        knockOutFx.SetActive(true);
        StopAllCoroutines();
        headState = HeadSubState.KNOCKED_OUT;
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_STUNNED, EventInfo.emptyInfo);
        StartCoroutine(BlinkHeadKnockOut());
    }

    public IEnumerator BlinkHeadKnockOut()
    {
        while (true)
        {
            SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headMaxChargeLevel));
            yield return new WaitForSeconds(bb.knockOutBlinkInterval);
            SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(0));
            yield return new WaitForSeconds(bb.knockOutBlinkInterval);
        }
    }

    public void ImpactedByShot(ChromaColor shotColor, float damage, PlayerController player)
    {
        if (headState != HeadSubState.ACTIVATED || invulnerable) return;
   
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
            return knockOutState;
        }

        return null;
    }

    public void SpecialAttackInRange()
    {
        if (currentState != null)
            currentState.SpecialAttackInRange();
    }

    public void ImpactedBySpecial(float damage, PlayerController player)
    {
        if (headState != HeadSubState.KNOCKED_OUT || invulnerable) return;

        if (currentState != null)
        {
            WormAIBaseState newState = currentState.ImpactedBySpecial(damage, player);

            if (newState != null)
            {
                ChangeState(newState);
            }
        }
    }

    public void StartNewPhase()
    {
        bb.wormCurrentPhase++;
        //Debug.Log("Worm phase: " + wormPhase);
        bb.headCurrentDamage = 0;
        bb.headCurrentChargeLevel = 0;
        WormEventInfo.eventInfo.wormBb = bb;
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_PHASE_ACTIVATED, WormEventInfo.eventInfo);
    }

    public void ResetPhase()
    {
        bb.headCurrentDamage = 0;
        DeactivateHead();
        DischargeHead();
    }


    public void SpawnEnergyVoxels()
    {
        EnergyVoxelPool pool = rsc.poolMng.bigEnergyVoxelPool;
        for (int i = 0; i < bb.knockOutEnergyVoxelsSpawned; ++i)
        {
            EnergyVoxelController voxel = pool.GetObject();
            if (voxel != null)
            {
                //voxel.transform.position = pos;
                voxel.transform.position = energyVoxelsSpawnPoints[i % energyVoxelsSpawnPoints.Length].position;
                voxel.transform.rotation = Random.rotation;
            }
        }
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

        EnemyExplosionController explosion = rsc.poolMng.enemyExplosionPool.GetObject();

        if (explosion != null)
        {
            explosion.transform.position = transform.position;
            explosion.PlayAll(finalExplosionSoundFx);
        }

        rsc.rumbleMng.Rumble(0, 1.5f, 1f, 1f);
        rsc.camerasMng.PlayEffect(0, 1.5f, 0.75f);
        voxelization.SpawnFakeVoxels();
        headModel.SetActive(false);

        yield return new WaitForSeconds(2f);

        //rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_CLEARED, LevelEventInfo.eventInfo);
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_DIED, EnemyDiedEventInfo.eventInfo);

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
        else if (other.tag == "EnemyHexagonBodyProbe")
        {
            EnemyBaseAIBehaviour enemy = other.GetComponent<EnemyBaseAIBehaviour>();

            if (enemy == null)
                enemy = other.GetComponentInParent<EnemyBaseAIBehaviour>();

            if (enemy != null)
                enemy.InstantKill();
        }
    }

    public void PlayerTouched(PlayerController player, Vector3 origin)
    {
        if (currentState != null)
            currentState.PlayerTouched(player, origin);
    }

    public bool CheckPlayerInSight(float minDistance, float maxDistance, bool savePlayer)
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
            if (distanceP1 >= minDistance * HexagonController.DISTANCE_BETWEEN_HEXAGONS
                && distanceP1 <= maxDistance * HexagonController.DISTANCE_BETWEEN_HEXAGONS)
            {
                //Check angle
                angleP1 = Vector3.Angle(forward, wormPlayer);
                if (angleP1 <= bb.AboveAttackSettingsPhase.exposureMaxAngle)
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
            if (distanceP2 >= minDistance
                && distanceP2 <= maxDistance * HexagonController.DISTANCE_BETWEEN_HEXAGONS)
            {
                //Check angle
                angleP2 = Vector3.Angle(forward, wormPlayer);
                if (angleP2 <= bb.AboveAttackSettingsPhase.exposureMaxAngle)
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
    
    public void WatchingPlayer()
    {
        angryEyes.Play();
        watchingSoundFx.Play();
    } 

    public void NotWatchingPlayer()
    {
        if (angryEyes.isPlaying)
            angryEyes.Stop();

        if (watchingSoundFx.isPlaying)
            watchingSoundFx.Stop();
    }
}

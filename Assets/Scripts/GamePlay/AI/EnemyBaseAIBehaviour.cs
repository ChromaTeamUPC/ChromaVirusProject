using UnityEngine;
using System.Collections;

public class EnemyBaseAIBehaviour : MonoBehaviour {

    protected EnemyBaseBlackboard blackboard; //To be instantiated in inherited classes 

    [Header("Enemy Common Settings")]
    public float maxHealth = 30;

    public float shotForceModifier = 1f;

    public float wrongColorDamageModifier = 0.25f;
    public int energyVoxelsSpawnedOnDie = 0;

    [HideInInspector]
    public ChromaColor color;

    protected VoxelizationClient voxelization;
    protected Renderer rend;
    [HideInInspector]
    public Collider mainCollider;
    [HideInInspector]
    public GameObject dyingCollider;

    public GameObject[] shields = new GameObject[4];

    protected AIBaseState currentState;

    protected BlinkController blinkController;


    protected virtual void Awake()
    {
        voxelization = GetComponentInChildren<VoxelizationClient>();
        rend = GetComponentInChildren<Renderer>();
        mainCollider = GetComponent<Collider>();
        dyingCollider = transform.FindDeepChild("DyingCollider").gameObject;
        blinkController = GetComponent<BlinkController>();
    }

    protected virtual void Start()
    {
        rsc.eventMng.StartListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
        rsc.eventMng.StartListening(EventManager.EventType.KILL_ENEMIES, KillMySelf);
    }

    protected virtual void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.COLOR_CHANGED, ColorChanged);
            rsc.eventMng.StopListening(EventManager.EventType.KILL_ENEMIES, KillMySelf);
        }
    }

    protected virtual void ColorChanged(EventInfo eventInfo)
    {
        ColorEventInfo info = (ColorEventInfo)eventInfo;

        if (currentState != null)
            currentState.ColorChanged(info.newColor);
    }


    protected virtual void KillMySelf(EventInfo eventInfo)
    {
        InstantKill();
    }

    public virtual void ProcessColorChanged(ChromaColor newColor)
    {
        if (color == newColor)
            shields[(int)color].SetActive(false);
        else
            shields[(int)color].SetActive(true);
    }

    public virtual void Spawn(Transform spawnPoint)
    {
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        DisableShields();      
    }

    public void DisableShields()
    {
        foreach (GameObject shield in shields)
            shield.SetActive(false);
    }

    void OnDisable()
    {
        blackboard.ResetValues();
        blinkController.StopPreviousBlinkings();
        blackboard.animator.Rebind();
        currentState = null;
    }

    public virtual void SetMaterials(Material[] materials) { }

    public void SpawnAnimationEnded()
    {
        blackboard.spawnAnimationEnded = true;
    }

    public void AttackAnimationTrigger()
    {
        blackboard.attackAnimationTrigger = true;
    }

    public void AttackAnimationEnded()
    {
        blackboard.attackAnimationEnded = true;
    }

    public void DieAnimationEnded()
    {
        blackboard.dieAnimationEnded = true;
    }

    protected virtual void Update()
    {
        if (currentState != null)
        {
            ChangeStateIfNotNull(currentState.Update());
        }
    }

    public void SpawnVoxels()
    {
        if (blackboard.lastShotSameColor)
        {
            voxelization.SetMaterial(color);
            voxelization.SpawnFakeVoxels();
        }
        else
        {
            voxelization.SetGreyMaterial();
            //Wrong color does not explode so, grid has to be more precise
            voxelization.CalculateVoxelsGrid();
            voxelization.SpawnColliderThisTime = false;
            voxelization.SpawnVoxels();
        }

    }

    private void ChangeStateIfNotNull(AIBaseState newState)
    {
        if (newState != null)
            ChangeState(newState);
    }

    protected void ChangeState(AIBaseState newState)
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

    public void ImpactedByShot(ChromaColor shotColor, float damage, Vector3 direction, PlayerController player)
    {
        if (currentState != null)
        {
            ChangeStateIfNotNull(currentState.ImpactedByShot(shotColor, damage, direction, player));           
        }
    }

    public virtual AIBaseState ProcessShotImpact(ChromaColor shotColor, float damage, Vector3 direction, PlayerController player)
    {
        return null;
    }

    public virtual void ImpactedBySpecial(float damage, Vector3 direction, PlayerController player)
    {
        if (currentState != null)
        {
            ChangeStateIfNotNull(currentState.ImpactedBySpecial(damage, direction, player));
        }
    }

    public virtual AIBaseState ProcessSpecialImpact(float damage, Vector3 direction, PlayerController player)
    {
        return null;
    }

    public void ImpactedByBarrel(ChromaColor barrelColor, float damage, Vector3 direction, PlayerController player)
    {
        if (currentState != null)
        {
            ChangeStateIfNotNull(currentState.ImpactedByBarrel(barrelColor, damage, direction, player));
        }
    }

    public virtual AIBaseState ProcessBarrelImpact(ChromaColor barrelColor, float damage, Vector3 direction, PlayerController player)
    {
        return null;
    }

    public void InstantKill()
    {
        if(currentState != null)
        {
            ChangeStateIfNotNull(currentState.InstantKill());
        }
    }

    public virtual AIBaseState ProcessInstantKill()
    {
        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "BarrelAttractor")
        {
            CapacitorController capacitorController = other.GetComponent<CapacitorAttractor>().controller;
            if(blackboard.capacitorController == null && capacitorController.currentColor == blackboard.entity.color)
                blackboard.capacitorController = other.GetComponent<CapacitorAttractor>().controller;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "BarrelAttractor")
        {
            CapacitorController capacitorController = other.GetComponent<CapacitorAttractor>().controller;
            if(capacitorController == blackboard.capacitorController)
                blackboard.capacitorController = null;
        }
    }

    public virtual void CollitionOnDie() { }
}

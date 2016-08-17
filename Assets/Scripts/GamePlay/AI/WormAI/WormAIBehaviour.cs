using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WormAIBehaviour : MonoBehaviour
{
    private enum HeadSubState
    {
        DEACTIVATED,
        ACTIVATED
    }

    private WormBlackboard bb;

    [Header("Route Settings")]
    public WormRoute[] routes;
    private WormWayPoint headWayPoint;

    //Shortcuts
    private Transform head;
    private Transform[] bodySegments;
    private Transform[] junctions;
    private Transform tail;

    private HeadSubState headState;
    private WormAIBaseState currentState;

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
        bb = GetComponent<WormBlackboard>();
        blinkController = GetComponent<BlinkController>();
        rend = GetComponentInChildren<Renderer>();
        voxelization = GetComponentInChildren<VoxelizationClient>();
    }

    void Start()
    {
        head = bb.headTrf;
        bodySegments = bb.bodySegmentsTrf;
        junctions = bb.junctionsTrf;
        tail = bb.tailTrf;

        SetInitialBodyWayPoints();

        SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headChargeLevel));

        headState = HeadSubState.DEACTIVATED;
    }

    public void Init(GameObject sceneCenter)
    {
        bb.sceneCenter = sceneCenter;
        bb.sceneCenterHexagon = sceneCenter.GetComponent<HexagonController>();

        SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headChargeLevel));
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_ACTIVATED, EventInfo.emptyInfo);
        bb.InitBodyParts();
        ChangeState(bb.spawningState);
    }

    // Update is called once per frame
    void Update()
    {
        if(bb.aboveAttackCurrentCooldownTime > 0)
        {
            bb.aboveAttackCurrentCooldownTime -= Time.deltaTime;
        }

        UpdateBodyMovement();

        if (currentState != null)
        {
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
        bb.headChargeLevel++;
        SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headChargeLevel));

        if (bb.headChargeLevel >= bb.headChargeMaxLevel)
        {
            bb.DisableBodyParts();
            headState = HeadSubState.ACTIVATED;
        }

        //rsc.colorMng.PrintColors();
    }

    public void DischargeHead()
    {
        bb.headChargeLevel = 0;
        SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headChargeLevel));
        bb.ShuffleBodyParts();
    }

    private void SetMaterial(Material materials)
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

        bb.headCurrentHealth -= damage;

        if (bb.headCurrentHealth <= 0)
        {
            headState = HeadSubState.DEACTIVATED;

            EnemyDiedEventInfo.eventInfo.color = shotColor;
            EnemyDiedEventInfo.eventInfo.infectionValue = 100 / bb.wormMaxPhases;
            EnemyDiedEventInfo.eventInfo.killerPlayer = player;
            EnemyDiedEventInfo.eventInfo.killedSameColor = true;
            rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_DESTROYED, EnemyDiedEventInfo.eventInfo);

            //If we are not reached last phase, keep going
            if (bb.wormPhase < bb.wormMaxPhases)
            {
                bb.StartNewPhase();
                SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(bb.headChargeLevel));
                rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_HEAD_ACTIVATED, EventInfo.emptyInfo);
            }
            //Else worm destroyed
            else
            {
                 return bb.dyingState;
            }
        }

        return null;
    }

    public void Explode()
    {
        StartCoroutine(RandomizeAndExplode());
    }

    private IEnumerator RandomizeAndExplode()
    {
        float elapsedTime = 0;
        int chargeLevel = 0;

        while (elapsedTime < bb.bodySettingMinTime)
        {
            SetMaterial(rsc.coloredObjectsMng.GetWormHeadMaterial(chargeLevel++ % 4));

            yield return new WaitForSeconds(bb.bodySettingChangeTime);
            elapsedTime += bb.bodySettingChangeTime;
        }

        voxelization.SpawnFakeVoxels();
        bb.headModel.SetActive(false);

        yield return new WaitForSeconds(2f);

        LevelEventInfo.eventInfo.levelId = -1;
        rsc.eventMng.TriggerEvent(EventManager.EventType.LEVEL_CLEARED, LevelEventInfo.eventInfo);

        Destroy(gameObject);
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

    public bool CheckPlayerInSight()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;

        //Check player 1
        GameObject player = rsc.enemyMng.GetPlayerIfActive(1);

        if(player != null)
        {
            Vector3 wormPlayer = player.transform.position - transform.position;
            wormPlayer.y = 0;

            //Check distance
            float distance = wormPlayer.magnitude;
            if (distance >= bb.aboveAttackExposureMinHexagons * HexagonController.DISTANCE_BETWEEN_HEXAGONS
                && distance <= bb.aboveAttackExposureMaxHexagons * HexagonController.DISTANCE_BETWEEN_HEXAGONS)
            {
                //Check angle
                float angle = Vector3.Angle(forward, wormPlayer);
                if (angle <= bb.aboveAttackExposureMaxAngle)
                    return true;
            }
        }

        //Check player 2
        player = rsc.enemyMng.GetPlayerIfActive(2);

        if (player != null)
        {
            Vector3 wormPlayer = player.transform.position - transform.position;
            wormPlayer.y = 0;

            //Check distance
            float distance = wormPlayer.magnitude;
            if (distance >= bb.aboveAttackExposureMinHexagons * HexagonController.DISTANCE_BETWEEN_HEXAGONS
                && distance <= bb.aboveAttackExposureMaxHexagons * HexagonController.DISTANCE_BETWEEN_HEXAGONS)
            {
                //Check angle
                float angle = Vector3.Angle(forward, wormPlayer);
                if (angle <= bb.aboveAttackExposureMaxAngle)
                    return true;
            }
        }

        return false;
    }

    //Body movement functions
    #region BodyMovement
    private void SetInitialBodyWayPoints()
    {
        headWayPoint = null;

        if (tail != null)
        {
            headWayPoint = new WormWayPoint(tail.position, tail.rotation, false);
        }

        for (int i = bodySegments.Length - 1; i >= 0; --i)
        {
            WormWayPoint segmentWayPoint = new WormWayPoint(bodySegments[i].position, bodySegments[i].rotation, false, (headWayPoint != null ? headWayPoint : null));
            headWayPoint = segmentWayPoint;
        }

        if (head != null)
        {
            headWayPoint = new WormWayPoint(head.position, head.rotation, false, (headWayPoint != null ? headWayPoint : null));
        }
    }


    private void UpdateBodyMovement()
    {
        //If head has moved, create a new waypoint and recalculate all segments' position
        if ((head.position != headWayPoint.position))
        {
            headWayPoint = new WormWayPoint(head.position, head.rotation, IsVisible(), headWayPoint);

            WormWayPoint current = headWayPoint;
            WormWayPoint next = current.next;
            //if we are in the last waypoint, there is nothing more we can do, so we quit
            if (next == null) return;

            float totalDistance = bb.headToJunctionDistance;                            //Total distance we have to position the element from the head
            float consolidatedDistance = 0f;                                        //Sum of the distances of evaluated waypoints
            float distanceBetween = (current.position - next.position).magnitude;   //Distance between current current and next waypoints

            //move each body segment through the virtual line
            for (int i = 0; i < bodySegments.Length; ++i)
            {
                //---- Junction ----
                //advance through waypoints until we find the proper distance
                while (consolidatedDistance + distanceBetween < totalDistance)
                {
                    consolidatedDistance += distanceBetween;

                    current = next;
                    next = current.next;
                    //if we are in the last waypoint, there is nothing more we can do, so we quit
                    if (next == null) return;

                    distanceBetween = (current.position - next.position).magnitude;
                }

                //We reached the line segment where this body part must be, so we calculate the point in current segment
                float remainingDistance = totalDistance - consolidatedDistance;
                Vector3 direction = (next.position - current.position).normalized * remainingDistance;

                junctions[i].position = current.position + direction;
                junctions[i].rotation = Quaternion.Slerp(current.rotation, next.rotation, remainingDistance / distanceBetween);
                junctions[i].gameObject.SetActive(current.visible || next.visible);

                //---- Body ----
                totalDistance += bb.segmentToJunctionDistance;

                //advance through waypoints until we find the proper distance
                while (consolidatedDistance + distanceBetween < totalDistance)
                {
                    consolidatedDistance += distanceBetween;

                    current = next;
                    next = current.next;
                    //if we are in the last waypoint, there is nothing more we can do, so we quit
                    if (next == null) return;

                    distanceBetween = (current.position - next.position).magnitude;
                }

                //We reached the line segment where this body part must be, so we calculate the point in current segment
                remainingDistance = totalDistance - consolidatedDistance;
                direction = (next.position - current.position).normalized * remainingDistance;

                bodySegments[i].position = current.position + direction;
                bodySegments[i].rotation = Quaternion.Slerp(current.rotation, next.rotation, remainingDistance / distanceBetween);
                bb.bodySegmentControllers[i].SetVisible(current.visible || next.visible);

                //if it was the final body part and there is no tail, release the oldest waypoints
                if (i == bodySegments.Length - 1)
                {
                    if (tail == null)
                        next.next = null; //Remove reference, let garbage collector do its job
                }
                //else add total distance for the next iteration
                else
                {
                    totalDistance += bb.segmentToJunctionDistance;
                }
            }

            //finally do the same for the tail
            if (tail != null)
            {
                //---- Junction ----
                totalDistance += bb.segmentToJunctionDistance;

                //advance through waypoints until we find the proper distance
                while (consolidatedDistance + distanceBetween < totalDistance)
                {
                    consolidatedDistance += distanceBetween;

                    current = next;
                    next = current.next;
                    //if we are in the last waypoint, there is nothing more we can do, so we quit
                    if (next == null) return;

                    distanceBetween = (current.position - next.position).magnitude;
                }

                //We reached the line segment where this body part must be, so we calculate the point in current segment
                float remainingDistance = totalDistance - consolidatedDistance;
                Vector3 direction = (next.position - current.position).normalized * remainingDistance;

                junctions[junctions.Length - 1].position = current.position + direction;
                junctions[junctions.Length - 1].rotation = Quaternion.Slerp(current.rotation, next.rotation, remainingDistance / distanceBetween);
                junctions[junctions.Length - 1].gameObject.SetActive(current.visible || next.visible);

                //---- Tail ----
                totalDistance += bb.tailToJunctionDistance;

                //advance through waypoints until we find the proper distance
                while (consolidatedDistance + distanceBetween < totalDistance)
                {
                    consolidatedDistance += distanceBetween;

                    current = next;
                    next = current.next;
                    //if we are in the last waypoint, there is nothing more we can do, so we quit
                    if (next == null) return;

                    distanceBetween = (current.position - next.position).magnitude;
                }

                //We reached the line segment where this body part must be, so we calculate the point in current segment
                remainingDistance = totalDistance - consolidatedDistance;
                direction = (next.position - current.position).normalized * remainingDistance;

                tail.position = current.position + direction;
                tail.rotation = Quaternion.Slerp(current.rotation, next.rotation, remainingDistance / distanceBetween);
                bb.tail.SetVisible(current.visible || next.visible);

                //release the oldest waypoints
                next.next = null; //Remove reference, let garbage collector do its job
            }
        }
    }

    #endregion
}

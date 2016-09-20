using UnityEngine;
using System.Collections;

public class WormAIWanderingState : WormAIBaseState 
{
    private enum SubState
    {
        WAITING,
        ENTERING,
        FOLLOWING_PATH,
        EXITING,
        WAITING_FOR_TAIL
    }

    private SubState subState;

    private WormRoute route;
    private int WPIndex;
    private Vector3 currentWP;
    private Vector3 nextWP;
    private Vector3 currentWPUG;
    private Vector3 nextWPUG;
    private Vector3 lastPosition;

    private float t;
    private float duration = 4;
    private float shouldMove;
    private float actuallyMoved;

    private float elapsedTime;

    private bool angryEyes;
    private bool exitRumble;

    public WormAIWanderingState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        SetInitialState();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        head.angryEyes.Stop();
    }

    private void SetInitialState()
    {
        angryEyes = false;
        exitRumble = false;
        head.angryEyes.Stop();

        elapsedTime = 0f;
        WPIndex = 0;
        //Set initial substate, nav mesh layer and select a random route
        subState = SubState.WAITING;

        WormRoute newRoute;
        do
        {
            //int routeNum = Random.Range(0, head.routes.Length);
            int routeNum = Random.Range(bb.WanderingSettingsPhase.routeMinId, bb.WanderingSettingsPhase.routeMaxId + 1);
            newRoute = head.routes[routeNum];
        }
        while (route == newRoute);

        route = newRoute;
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
            case SubState.WAITING:
                if (elapsedTime >= bb.WanderingSettingsPhase.initialWaitTime)
                {
                    head.agent.enabled = false;

                    currentWP = route.wayPoints[WPIndex].transform.position;
                    nextWP = route.wayPoints[WPIndex + 1].transform.position;

                    currentWPUG = currentWP - bb.navMeshLayersDistance;
                    nextWPUG = nextWP - bb.navMeshLayersDistance;

                    headTrf.position = currentWPUG;
                    headTrf.LookAt(nextWPUG, Vector3.up);
                    bb.CalculateWorldEnterBezierPoints(headTrf);
                    head.SetVisible(true);

                    //Rotate head
                    Vector3 headUp = currentWPUG - nextWPUG;
                    headTrf.LookAt(currentWP, headUp);

                    t = 0;

                    HexagonController hexagon = route.wayPoints[WPIndex].GetComponent<HexagonController>();
                    hexagon.WormEnterExit();

                    rsc.rumbleMng.Rumble(0, 1f, 0.5f, 0.5f);
                    head.animator.SetBool("MouthOpen", true);

                    bb.isHeadOverground = true;
                    bb.applySinMovement = true;               

                    subState = SubState.ENTERING;
                }
                else
                    elapsedTime += Time.deltaTime;

                break;

            case SubState.ENTERING:
                if (t <= 2)
                {
                    shouldMove = Time.deltaTime * bb.WanderingSettingsPhase.wanderingSpeed;
                    actuallyMoved = 0;
                    lastPosition = headTrf.position;
                    Vector3 newPos = Vector3.zero;

                    while (actuallyMoved < shouldMove && t <= 2)
                    {
                        newPos = bb.GetEnterCurvePosition(currentWPUG, nextWP, t);
                        
                        actuallyMoved = (newPos - lastPosition).magnitude;

                        t += Time.deltaTime / duration;
                    }
                    headTrf.position = newPos;
                    headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition));
                }
                else
                {
                    ++WPIndex;
                    currentWP = route.wayPoints[WPIndex].transform.position;

                    head.agent.areaMask = WormBlackboard.NAVMESH_FLOOR_LAYER;
                    head.agent.enabled = true;
                    head.agent.speed = bb.WanderingSettingsPhase.wanderingSpeed;
                    head.agent.SetDestination(currentWP);

                    head.animator.SetBool("MouthOpen", false);

                    rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_VULNERABLE, EventInfo.emptyInfo);

                    subState = SubState.FOLLOWING_PATH;
                }

                break;

            case SubState.FOLLOWING_PATH:

                if(head.CheckPlayerInSight(bb.AboveAttackSettingsPhase.exposureMinHexagons, bb.AboveAttackSettingsPhase.exposureMaxHexagons, false)) 
                {
                    bb.aboveAttackCurrentExposureTime += Time.deltaTime;

                    if (!angryEyes)
                    {
                        angryEyes = true;
                        head.angryEyes.Play();
                    }
                }
                else
                {
                    if (angryEyes)
                    {
                        angryEyes = false;
                        head.angryEyes.Stop();
                    }
                }

                //TODO: remove when tested
                if (Input.GetKeyDown(KeyCode.M))
                {
                    rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_INVULNERABLE, EventInfo.emptyInfo);
                    return head.meteorAttackState;
                }

                if (bb.attacksEnabled && bb.AboveAttackSettingsPhase.active && bb.aboveAttackCurrentExposureTime >= bb.AboveAttackSettingsPhase.exposureTimeNeeded &&
                    bb.aboveAttackCurrentCooldownTime >= bb.AboveAttackSettingsPhase.cooldownTime &&
                    head.CheckPlayerInSight(bb.AboveAttackSettingsPhase.attackMinHexagons, bb.AboveAttackSettingsPhase.attackMaxHexagons, true))
                {
                    if (bb.playerInSight != null)
                    {
                        HexagonController destiny = bb.playerInSight.GetNearestHexagon();

                        if (destiny != null && destiny.isWormSelectable)
                        {
                            if (angryEyes)
                            {
                                angryEyes = false;
                                head.angryEyes.Stop();
                            }
                            rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_INVULNERABLE, EventInfo.emptyInfo);
                            return head.aboveAttackState;
                        }
                    }                   
                }
          

                if (!head.agent.hasPath || (head.agent.hasPath && head.agent.remainingDistance <= 0.25))
                {
                    if (WPIndex == route.wayPoints.Length - 2)
                    {
                        head.agent.enabled = false;

                        currentWP = route.wayPoints[WPIndex].transform.position;
                        nextWP = route.wayPoints[WPIndex + 1].transform.position;

                        currentWPUG = currentWP - bb.navMeshLayersDistance;
                        nextWPUG = nextWP - bb.navMeshLayersDistance;

                        headTrf.LookAt(nextWP, Vector3.up);

                        bb.CalculateWorldExitBezierPoints(headTrf);

                        t = 0;

                        HexagonController hexagon = route.wayPoints[WPIndex + 1].GetComponent<HexagonController>();
                        hexagon.WormEnterExit();

                        if (angryEyes)
                        {
                            angryEyes = false;
                            head.angryEyes.Stop();
                        }
                      
                        head.animator.SetBool("MouthOpen", true);                     

                        subState = SubState.EXITING;
                    }
                    else
                    {
                        ++WPIndex;
                        currentWP = route.wayPoints[WPIndex].transform.position;
                        head.agent.SetDestination(currentWP);
                    }
                }
                break;

            case SubState.EXITING:
                if (t <= 2)
                {
                    shouldMove = Time.deltaTime * bb.WanderingSettingsPhase.wanderingSpeed;
                    actuallyMoved = 0;
                    lastPosition = headTrf.position;
                    Vector3 newPos = Vector3.zero;

                    if(t >= 1.6f && !exitRumble)
                    {
                        exitRumble = true;
                        rsc.rumbleMng.Rumble(0, 1f, 0.5f, 0.5f);
                    }

                    while (actuallyMoved < shouldMove && t <= 2)
                    {
                        newPos = bb.GetExitCurvePosition(currentWP, nextWPUG, t);                       
                        actuallyMoved = (newPos - lastPosition).magnitude;

                        t += Time.deltaTime / duration;
                    }
                    headTrf.position = newPos;
                    headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition));
                }
                else
                {
                    SetUndergroundDirection();
                    SetHeadUnderground();
                    head.animator.SetBool("MouthOpen", false);

                    subState = SubState.WAITING_FOR_TAIL;
                }

                break;

            case SubState.WAITING_FOR_TAIL:
                //move head until tail is undeground
                if(!bb.tailReachedMilestone)
                {
                    MoveUndergroundDirection();
                }
                else
                {
                    bb.applySinMovement = false;
                    rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_INVULNERABLE, EventInfo.emptyInfo);

                    //If some random condition attack, else new wandering state
                    if (bb.attacksEnabled && bb.BelowAttackSettingsPhase.active &&
                        Random.Range(0f, 1f) <= bb.BelowAttackSettingsPhase.chancesOfBelowAttackAfterWandering / 100)
                        return head.belowAttackState;
                    else
                        SetInitialState();
                }

                break;

            default:
                break;
        }

        return null;
    }

    public override WormAIBaseState ImpactedByShot(ChromaColor shotColor, float damage, PlayerController player)
    {
        if (subState != SubState.FOLLOWING_PATH) return null;

        return head.ProcessShotImpact(shotColor, damage, player);
    }           
}

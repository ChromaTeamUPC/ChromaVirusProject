using UnityEngine;
using System.Collections;

public class WormAISpawningState : WormAIBaseState
{
    private enum SubState
    {
        GOING_TO_ENTRY,
        JUMPING,
        EXITING,
        WAITING_FOR_TAIL
    }

    private SubState subState;

    private float currentX;
    private Vector3 lastPosition;
    private float rotation;
    private bool highestPointReached;
    private float destinyInRangeDistance = 1f;
    private bool destinyInRange;

    private HexagonController origin;
    private HexagonController destiny;

    public WormAISpawningState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        origin = bb.spawnEntry.GetComponent<HexagonController>();
        destiny = bb.spawnExit.GetComponent<HexagonController>();

        bb.CalculateParabola(bb.spawnEntry.transform.position, bb.spawnExit.transform.position);

        bb.head.agent.enabled = false;

        rotation = 0f;
        highestPointReached = false;

        subState = SubState.GOING_TO_ENTRY;

        WormEventInfo.eventInfo.wormBb = bb;
        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_SPAWNED, WormEventInfo.eventInfo);
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
            case SubState.GOING_TO_ENTRY:
                //Position head below entry point
                currentX = bb.GetJumpXGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false);
                Vector3 startPosition = bb.GetJumpPositionGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false);
                headTrf.position = startPosition;
                lastPosition = startPosition;
                bb.head.SetVisible(true);
             
                origin.WormEnterExit();

                subState = SubState.JUMPING;
                break;

            case SubState.JUMPING:
                //While not again below underground navmesh layer advance
                currentX += Time.deltaTime * bb.wanderingSpeed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition), headTrf.up);

                if( lastPosition.y > headTrf.position.y && rotation < 90f)
                {
                    if(!highestPointReached)
                    {
                        bb.head.StartNewPhase();
                        highestPointReached = true;
                    }

                    float angle = 30 * Time.deltaTime;
                    headTrf.Rotate(new Vector3(0, 0, angle));
                    rotation += angle;
                }

                if (!destinyInRange)
                {
                    float distanceToDestiny = (headTrf.position - destiny.transform.position).magnitude;
                    if (distanceToDestiny <= destinyInRangeDistance)
                    {
                        destinyInRange = true;
                        destiny.WormAboveAttackStart();
                        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_ATTACK, WormEventInfo.eventInfo);
                    }
                }

                if (headTrf.position.y < -WormBlackboard.NAVMESH_LAYER_HEIGHT)
                {
                    bb.head.SetVisible(false);

                    subState = SubState.EXITING;
                }
                break;

            case SubState.EXITING:
                currentX += Time.deltaTime * bb.wanderingSpeed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition));

                if (bb.isTailUnderground)
                {
                    Vector3 pos = headTrf.position;
                    pos.y = -WormBlackboard.NAVMESH_LAYER_HEIGHT;
                    headTrf.position = pos;

                    /*bb.agent.areaMask = WormBlackboard.NAVMESH_UNDERGROUND_LAYER;
                    bb.agent.enabled = true;
                    bb.agent.speed = bb.undergroundSpeed;
                    bb.agent.SetDestination(bb.GetJumpPositionGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false)); //Back to entry in the underground
                    */
                    return bb.head.wanderingState;
                }
                break;

            default:
                break;
        }

        return null;
    }
}

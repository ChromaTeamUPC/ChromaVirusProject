using UnityEngine;
using System.Collections;

public class WormAIBelowAttackState : WormAIBaseState
{
    private const int destinyHexagonsDistance = 5;

    private enum SubState
    {
        WAITING,
        WARNING_PLAYER,
        JUMPING,
        EXITING
    }

    private SubState subState;

    private HexagonController origin;
    private HexagonController destiny;

    private float currentX;
    private Vector3 lastPosition;
    private float rotation;
    private float destinyInRangeDistance = 1f;
    private bool destinyInRange;

    private float elapsedTime;

    public WormAIBelowAttackState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        origin = null;
        elapsedTime = 0f;
        subState = SubState.WAITING;     
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        head.animator.SetBool("Bite", false);
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
            case SubState.WAITING:
                if (elapsedTime >= bb.BelowAttackSettingsPhase.initialWaitTime)
                {
                    GameObject playerGO = rsc.enemyMng.SelectPlayerRandom();
                    if (playerGO != null)
                    {
                        PlayerController player = playerGO.GetComponent<PlayerController>();
                        origin = player.GetNearestHexagon();

                        if (origin != null)
                        {
                            if (!origin.isWormSelectable)
                                return head.wanderingState;

                            head.animator.SetBool("Bite", true);

                            destiny = GetHexagonFacingCenter(origin, destinyHexagonsDistance);

                            bb.CalculateParabola(origin.transform.position, destiny.transform.position);

                            head.agent.enabled = false;

                            rotation = 0f;
                            destinyInRange = false;
                        }
                        else
                            return head.wanderingState;
                    }
                    else
                        return head.wanderingState;

                    origin.WormBelowAttackWarning(bb.BelowAttackSettingsPhase.adjacentDamagingCells);

                    elapsedTime = 0f;
                    subState = SubState.WARNING_PLAYER;
                }
                else
                    elapsedTime += Time.deltaTime;

                break;

            case SubState.WARNING_PLAYER:

                if (elapsedTime >= bb.BelowAttackSettingsPhase.warningTime)
                {
                    //Position head below entry point
                    currentX = bb.GetJumpXGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false);
                    Vector3 startPosition = bb.GetJumpPositionGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false);
                    headTrf.position = startPosition;
                    lastPosition = startPosition;
                    head.SetVisible(true);

                    origin.WormBelowAttackStart();
                    WormEventInfo.eventInfo.wormBb = bb;
                    rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_ATTACK, WormEventInfo.eventInfo);

                    bb.isHeadOverground = true;
                    subState = SubState.JUMPING;
                }
                else
                    elapsedTime += Time.deltaTime;

                break;

            case SubState.JUMPING:
                //While not again below underground navmesh layer advance
                currentX += Time.deltaTime * bb.BelowAttackSettingsPhase.jumpSpeed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition), headTrf.up);

                float angle = bb.BelowAttackSettingsPhase.rotationSpeed * Time.deltaTime;
                headTrf.Rotate(new Vector3(0, 0, angle));
                rotation += angle;

                if (!destinyInRange)
                {
                    float distanceToDestiny = (headTrf.position - destiny.transform.position).magnitude;
                    if (distanceToDestiny <= destinyInRangeDistance ||
                        (headTrf.position.y < destiny.transform.position.y &&
                        currentX >= 0)) //Safety check. When jump is too fast distance can never be less than range distance
                    {
                        destinyInRange = true;
                        destiny.WormEnterExit();
                    }
                }

                if (headTrf.position.y < -WormBlackboard.NAVMESH_LAYER_HEIGHT)
                {
                    bb.isHeadOverground = false;
                    bb.tailReachedMilestone = false;
                    bb.FlagCurrentWaypointAsMilestone();
                    head.SetVisible(false);

                    subState = SubState.EXITING;
                }
                break;

            case SubState.EXITING:
                currentX += Time.deltaTime * bb.BelowAttackSettingsPhase.jumpSpeed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition));

                if (bb.tailReachedMilestone)
                {
                    //Debug.Break();
                    Vector3 pos = headTrf.position;
                    pos.y = -WormBlackboard.NAVMESH_LAYER_HEIGHT;
                    headTrf.position = pos;

                    return head.wanderingState;
                }
                break;

            default:
                break;
        }

        return null;
    }
}

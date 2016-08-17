using UnityEngine;
using System.Collections;

public class WormAIAboveAttackState : WormAIBaseState
{
    private enum SubState
    {
        WARNING_PLAYER,
        JUMPING,
        EXITING
    }

    private SubState subState;

    private HexagonController destiny;

    private float currentX;
    private Vector3 lastPosition;
    private float rotation;
    private float destinyInRangeDistance = 1f;
    private bool destinyInRange;
    private float speed;

    private Quaternion initialRotation;
    private float elapsedTime;

    public WormAIAboveAttackState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        destiny = null;

        //Set origin and destiny
        GameObject playerGO = rsc.enemyMng.SelectPlayerRandom();
        if (playerGO != null)
        {
            PlayerController player = playerGO.GetComponent<PlayerController>();
            destiny = player.GetNearestHexagon();

            if (destiny != null)
            {
                bb.jumpOrigin = bb.transform.position;
                bb.jumpDestiny = destiny.transform.position;
                speed = (bb.jumpOrigin - bb.jumpDestiny).magnitude / bb.aboveAttackJumpDuration;

                bb.CalculateParabola();

                bb.agent.enabled = false;

                rotation = 0f;
                destinyInRange = false;

                //Calculate start point and prior point
                currentX = bb.GetJumpXGivenY(0, false);
                Vector3 startPosition = bb.GetJumpPositionGivenY(0, false);
                head.position = startPosition;

                float fakeLastX = currentX - Time.deltaTime * speed;
                lastPosition = bb.GetJumpPositionGivenX(currentX);

                float fakeNextX = currentX + Time.deltaTime * 2;
                Vector3 nextPosition = bb.GetJumpPositionGivenX(fakeNextX);
                initialRotation = Quaternion.LookRotation(nextPosition - startPosition, head.up);

                bb.isHeadOverground = true;

                destiny.WormAboveAttackWarning();
                subState = SubState.WARNING_PLAYER;
            }
        }
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        bb.aboveAttackCurrentCooldownTime = bb.aboveAttackCooldownTime;
        bb.aboveAttackCurrentExposureTime = 0f;
    }

    public override WormAIBaseState Update()
    {
        if (destiny == null) return bb.wanderingState;

        switch (subState)
        {
            case SubState.WARNING_PLAYER:
                head.rotation = Quaternion.RotateTowards(head.rotation, initialRotation, 90 / bb.aboveAttackWarningTime * Time.deltaTime);

                if (elapsedTime >= bb.aboveAttackWarningTime)
                {
                    subState = SubState.JUMPING;
                }
                else
                    elapsedTime += Time.deltaTime;

                break;

            case SubState.JUMPING:
                //While not again below underground navmesh layer advance
                currentX += Time.deltaTime * speed;
                lastPosition = head.position;
                head.position = bb.GetJumpPositionGivenX(currentX);

                head.LookAt(head.position + (head.position - lastPosition), head.up);

                if (rotation < bb.aboveAttackSelfRotation)
                {
                    float angle = bb.aboveAttackSelfRotation / bb.aboveAttackJumpDuration * Time.deltaTime;
                    head.Rotate(new Vector3(0, 0, angle));
                    rotation += angle;
                }

                if (!destinyInRange)
                {
                    float distanceToDestiny = (head.position - destiny.transform.position).magnitude;
                    if (distanceToDestiny <= destinyInRangeDistance)
                    {
                        destinyInRange = true;
                        WormEventInfo.eventInfo.wormBb = bb;
                        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_ATTACK, WormEventInfo.eventInfo);
                        destiny.WormAboveAttackStart();
                    }
                }

                if (head.position.y < -WormBlackboard.NAVMESH_LAYER_HEIGHT)
                {
                    bb.isHeadOverground = false;
                    bb.worm.SetVisible(false);

                    subState = SubState.EXITING;
                }
                break;

            case SubState.EXITING:
                currentX += Time.deltaTime * speed;
                lastPosition = head.position;
                head.position = bb.GetJumpPositionGivenX(currentX);

                head.LookAt(head.position + (head.position - lastPosition));

                if (bb.tailIsUnderground)
                {
                    Vector3 pos = head.position;
                    pos.y = -WormBlackboard.NAVMESH_LAYER_HEIGHT;
                    head.position = pos;

                    /*bb.agent.areaMask = WormBlackboard.NAVMESH_UNDERGROUND_LAYER;
                    bb.agent.enabled = true;
                    bb.agent.speed = bb.undergroundSpeed;
                    bb.agent.SetDestination(bb.GetJumpPositionGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false)); //Back to entry in the underground
                    */
                    return bb.wanderingState;
                }
                break;
            default:
                break;
        }

        return null;
    }
}

using UnityEngine;
using System.Collections;

public class WormAIMeteorAttackState : WormAIBaseState
{
    private enum SubState
    {
        WAITING,
        ENTERING,
        OPENING_MOUTH,
        SHOOTING,
        CLOSING_MOUTH,
        JUMPING,
        EXITING,
        WAITING_METEOR_RAIN
    }

    private SubState subState;

    private float elapsedTime;
    private HexagonController origin;
    private HexagonController destiny;

    private float currentX;
    private Vector3 lastPosition;
    private Quaternion initialRotation;
    private float destinyInRangeDistance = 1f;
    private bool destinyInRange;
    private float speed;

    private float timeBetweenShots;
    private int totalShots;

    public WormAIMeteorAttackState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        head.agent.enabled = false;
        destinyInRange = false;
        elapsedTime = 0f;

        timeBetweenShots = bb.MeteorAttackSettingsPhase.timeShooting / (bb.MeteorAttackSettingsPhase.numberOfProjectiles -1);
        totalShots = 0;

        subState = SubState.WAITING;
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
            case SubState.WAITING:
                if (elapsedTime >= bb.MeteorAttackSettingsPhase.initialWaitTime)
                {
                    //Choose exit hexagon

                    origin.WormEnterExit();

                    bb.isHeadOverground = true;
                    subState = SubState.ENTERING;
                }
                else
                    elapsedTime += Time.deltaTime;
                break;

            case SubState.ENTERING:
                if(true) //head.y < some value
                {
                    lastPosition = headTrf.position;
                    float deltaY = bb.MeteorAttackSettingsPhase.enteringSpeed * Time.deltaTime;
                    Vector3 currenPos = headTrf.position;
                    currenPos.y += deltaY;
                    headTrf.position = currenPos;
                }
                else
                {
                    //Open mouth animation
                    elapsedTime = 0f;
                    subState = SubState.OPENING_MOUTH;
                }
                break;

            case SubState.OPENING_MOUTH:
                if (elapsedTime >= 1)
                {
                    elapsedTime = 0;
                    subState = SubState.SHOOTING;
                }
                else
                    elapsedTime += Time.deltaTime;
                break;

            case SubState.SHOOTING:
                if(totalShots < bb.MeteorAttackSettingsPhase.numberOfProjectiles)
                {
                    if(elapsedTime <= 0)
                    {
                        //shoot
                        ++totalShots;
                        elapsedTime = timeBetweenShots;
                    }
                    else
                    {
                        elapsedTime -= Time.deltaTime;
                    }
                }
                else
                {
                    elapsedTime = 0;
                    subState = SubState.CLOSING_MOUTH;
                }
                break;

            case SubState.CLOSING_MOUTH:
                if (elapsedTime >= 1)
                {
                    elapsedTime = 0;
                    subState = SubState.JUMPING;
                }
                else
                    elapsedTime += Time.deltaTime;
                break;

            case SubState.JUMPING:
                //While not again below underground navmesh layer advance
                currentX += Time.deltaTime * speed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition), headTrf.up);

                headTrf.Rotate(new Vector3(0, 0, bb.MeteorAttackSettingsPhase.rotationSpeed * Time.deltaTime));

                if (!destinyInRange)
                {
                    float distanceToDestiny = (headTrf.position - destiny.transform.position).magnitude;
                    if (distanceToDestiny <= destinyInRangeDistance)
                    {
                        destinyInRange = true;
                        WormEventInfo.eventInfo.wormBb = bb;
                        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_ATTACK, WormEventInfo.eventInfo);
                        destiny.WormEnterExit();
                    }
                }

                if (headTrf.position.y < -WormBlackboard.NAVMESH_LAYER_HEIGHT)
                {
                    bb.isHeadOverground = false;
                    head.SetVisible(false);

                    subState = SubState.EXITING;
                }
                break;

            case SubState.EXITING:
                currentX += Time.deltaTime * speed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition));

                if (bb.isTailUnderground)
                {
                    Vector3 pos = headTrf.position;
                    pos.y = -WormBlackboard.NAVMESH_LAYER_HEIGHT;
                    headTrf.position = pos;

                    subState = SubState.WAITING_METEOR_RAIN;
                }
                break;

            case SubState.WAITING_METEOR_RAIN:
                break;

            default:
                break;
        }

        return null;
    }
}

using UnityEngine;
using System.Collections;

public class WormAIMeteorAttackState : WormAIBaseState
{
    private enum SubState
    {
        OPENING_MOUTH,
        SHOOTING,
        CLOSING_MOUTH,
        JUMPING,
        EXITING,
        WAITING_METEOR_RAIN,
        METEOR_RAIN_ENDED
    }

    private SubState subState;

    private float elapsedTime;
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

        head.animator.SetBool("MouthOpen", true);

        timeBetweenShots = bb.MeteorAttackSettingsPhase.timeShooting / (bb.MeteorAttackSettingsPhase.numberOfThrownMeteors -1);
        totalShots = 0;

        rsc.eventMng.StartListening(EventManager.EventType.METEOR_RAIN_ENDED, MeteorRainEnded);

        subState = SubState.OPENING_MOUTH;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        rsc.eventMng.StopListening(EventManager.EventType.METEOR_RAIN_ENDED, MeteorRainEnded);
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
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
                if(totalShots < bb.MeteorAttackSettingsPhase.numberOfThrownMeteors)
                {
                    if(elapsedTime <= 0)
                    {
                        MeteorController meteor = rsc.poolMng.meteorPool.GetObject();
                        meteor.transform.position = headTrf.position;                        
                        meteor.GoUp(bb.MeteorAttackSettingsPhase.speedOfThrownMeteors);

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
                    destiny = GetExitHexagon();
                    bb.CalculateParabola(headTrf.position, destiny.transform.position);
                    speed = (headTrf.position - destiny.transform.position).magnitude / bb.MeteorAttackSettingsPhase.jumpDuration;

                    //Calculate start point and prior point
                    currentX = bb.GetJumpXGivenY(0, false);
                    Vector3 startPosition = bb.GetJumpPositionGivenY(0, false);
                    headTrf.position = startPosition;

                    lastPosition = bb.GetJumpPositionGivenX(currentX);

                    float fakeNextX = currentX + Time.deltaTime * 2;
                    Vector3 nextPosition = bb.GetJumpPositionGivenX(fakeNextX);
                    initialRotation = Quaternion.LookRotation(nextPosition - startPosition, headTrf.up);

                    elapsedTime = 0;
                    head.animator.SetBool("MouthOpen", false);
                    subState = SubState.CLOSING_MOUTH;
                }
                break;

            case SubState.CLOSING_MOUTH:
                headTrf.rotation = Quaternion.RotateTowards(headTrf.rotation, initialRotation, bb.headDestroyedLookRotationSpeed * Time.deltaTime);
                if (elapsedTime >= 1)
                {
                    elapsedTime = 0;
                    subState = SubState.JUMPING;
                }
                else
                {
                    elapsedTime += Time.deltaTime;
                }
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
                    if (distanceToDestiny <= destinyInRangeDistance ||
                        headTrf.position.y < destiny.transform.position.y) //Safety check. When jump is too fast distance can never be less than range distance
                    {
                        destinyInRange = true;
                        WormEventInfo.eventInfo.wormBb = bb;
                        rsc.eventMng.TriggerEvent(EventManager.EventType.WORM_ATTACK, WormEventInfo.eventInfo);
                        destiny.WormEnterExit();
                    }
                }

                if (headTrf.position.y < -WormBlackboard.NAVMESH_LAYER_HEIGHT)
                {
                    SetHeadUnderground();

                    subState = SubState.EXITING;
                }
                break;

            case SubState.EXITING:
                currentX += Time.deltaTime * speed;
                lastPosition = headTrf.position;
                headTrf.position = bb.GetJumpPositionGivenX(currentX);

                headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition));

                if (bb.tailReachedMilestone)
                {
                    Vector3 pos = headTrf.position;
                    pos.y = -WormBlackboard.NAVMESH_LAYER_HEIGHT;
                    headTrf.position = pos;

                    //Trigger meteor rain
                    MeteorAttackEventInfo info = MeteorAttackEventInfo.eventInfo;
                    WormBlackboard.MeteorAttackSettings settings = bb.MeteorAttackSettingsPhase;

                    info.meteorInitialBurst = settings.meteorInitialBurst;
                    info.meteorRainDuration = settings.meteorRainDuration;
                    info.meteorInterval = settings.meteorInterval;
                    info.meteorsPerInterval = settings.meteorsPerInterval;
                    info.meteorWaitTime = settings.meteorWaitTime;
                    info.meteorWarningTime = settings.meteorWarningTime;

                    rsc.eventMng.TriggerEvent(EventManager.EventType.METEOR_RAIN_START, info);
                    subState = SubState.WAITING_METEOR_RAIN;
                }
                break;

            case SubState.WAITING_METEOR_RAIN:
                //Wait
                break;

            case SubState.METEOR_RAIN_ENDED:
                return head.wanderingState;

            default:
                break;
        }

        return null;
    }

    private void MeteorRainEnded(EventInfo eventInfo)
    {
        subState = SubState.METEOR_RAIN_ENDED;
    }
}

﻿using UnityEngine;
using System.Collections;

public class WormAIMeteorAttackState : WormAIBaseState
{
    private enum SubState
    {
        UNDERGROUND_START,
        WARNING_PLAYER,
        HEAD_EXIT,

        OVERGROUND_START,
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

    private bool underground;

    public WormAIMeteorAttackState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        head.agent.enabled = false;
        destinyInRange = false;
        elapsedTime = 0f;

        bb.SetWormInvulnerable();    

        timeBetweenShots = bb.MeteorAttackSettingsPhase.timeShooting / (bb.MeteorAttackSettingsPhase.numberOfThrownMeteors -1);
        totalShots = 0;

        rsc.eventMng.StartListening(EventManager.EventType.METEOR_RAIN_ENDED, MeteorRainEnded);

        if (bb.meteorInmediate)
        {
            underground = false;
            head.animator.SetBool("MouthOpenTrans", true);
            subState = SubState.OVERGROUND_START;
        }
        else
        {
            underground = true;
            head.animator.SetBool("MouthOpen", true);
            subState = SubState.UNDERGROUND_START;
        }

        bb.meteorInmediate = false;
        bb.shouldMeteorBeTriggedAfterWandering = false;
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
            //Underground path
            case SubState.UNDERGROUND_START:
                GameObject playerGO = rsc.enemyMng.SelectPlayerRandom();
                if (playerGO != null)
                {
                    PlayerController player = playerGO.GetComponent<PlayerController>();
                    origin = player.GetNearestHexagon();

                    if (origin != null)
                    {
                        if (!origin.isWormSelectable)
                            return head.wanderingState;

                        destiny = GetHexagonFacingCenter(origin, 2);
                        bb.CalculateParabola(origin.transform.position, destiny.transform.position);

                        head.agent.enabled = false;
                    }
                    else
                        return head.wanderingState;
                }
                else
                    return head.wanderingState;

                origin.WormEnterExit();

                elapsedTime = 0f;
                head.attackWarningSoundFx.Play();

                subState = SubState.WARNING_PLAYER;

                break;

            case SubState.WARNING_PLAYER:

                if (elapsedTime >= bb.MeteorAttackSettingsPhase.warningTime)
                {
                    //Position head below entry point
                    currentX = bb.GetJumpXGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false);
                    Vector3 startPosition = bb.GetJumpPositionGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false);
                    headTrf.position = startPosition;
                    lastPosition = startPosition;
                    head.SetVisible(true);

                    elapsedTime = 0f;
                    bb.isHeadOverground = true;
                    subState = SubState.HEAD_EXIT;
                }
                else
                    elapsedTime += Time.deltaTime;

                break;

            case SubState.HEAD_EXIT:
                if(headTrf.position.y < 2)
                {
                    currentX += Time.deltaTime * bb.MeteorAttackSettingsPhase.enterHeadSpeed;
                    lastPosition = headTrf.position;
                    headTrf.position = bb.GetJumpPositionGivenX(currentX);

                    headTrf.LookAt(headTrf.position + (headTrf.position - lastPosition), headTrf.up);

                    elapsedTime += Time.deltaTime;
                }
                else
                {
                    elapsedTime += bb.MeteorAttackSettingsPhase.warningTime;
                    subState = SubState.OPENING_MOUTH;
                }

                break;

            //Overground path
            case SubState.OVERGROUND_START:
                destiny = GetExitHexagon();
                bb.CalculateParabola(headTrf.position, destiny.transform.position);
                subState = SubState.OPENING_MOUTH;
                break;

            //Common path
            case SubState.OPENING_MOUTH:
                if (elapsedTime >= 1.5f)
                {                  
                    elapsedTime = 0;
                    //head.fireSpray.transform.position = head.headModel.transform.position + (Vector3.up * 0.5f);
                    //head.fireSpray.transform.LookAt(head.fireSpray.transform.position + Vector3.up, head.transform.up);
                    //head.fireSpray.Play();
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
                        meteor.transform.position = head.meteorSpawnPoint.position;               
                        meteor.GoUp(bb.MeteorAttackSettingsPhase.speedOfThrownMeteors);

                        rsc.rumbleMng.Rumble(0, 0.15f, 0, 0.6f);
                        rsc.camerasMng.PlayEffect(0, 0.1f, 0.3f);

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
                    speed = (headTrf.position - destiny.transform.position).magnitude / bb.MeteorAttackSettingsPhase.jumpDuration;

                    //Calculate start point and prior point
                    currentX = bb.GetJumpXGivenY(headTrf.position.y, false);
                    Vector3 startPosition = bb.GetJumpPositionGivenY(headTrf.position.y, false);
                    headTrf.position = startPosition;

                    lastPosition = bb.GetJumpPositionGivenX(currentX);

                    float fakeNextX = currentX + Time.deltaTime * 2;
                    Vector3 nextPosition = bb.GetJumpPositionGivenX(fakeNextX);
                    initialRotation = Quaternion.LookRotation(nextPosition - startPosition, headTrf.up);

                    head.fireSpray.Stop();

                    elapsedTime = 0;
                    if (underground)
                        head.animator.SetBool("MouthOpen", false);
                    else
                    {
                        head.animator.SetBool("MouthOpenTrans", false);
                        head.animator.SetBool("Jump", true);
                    }
                    subState = SubState.CLOSING_MOUTH;
                }
                break;

            case SubState.CLOSING_MOUTH:
                headTrf.rotation = Quaternion.RotateTowards(headTrf.rotation, initialRotation, bb.headDestroyedLookRotationSpeed * Time.deltaTime);
                if (elapsedTime >= 1)
                {
                    elapsedTime = 0;
                    subState = SubState.JUMPING;
                    head.animator.SetBool("Jump", false);
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

                        JumpExitActions();

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

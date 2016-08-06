﻿using UnityEngine;
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

    private float currentX;
    private Vector3 lastPosition;
    private float rotation;
    private bool highestPointReached;

    private float elapsedTime;

    public WormAIBelowAttackState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        origin = null;

        GameObject playerGO = rsc.enemyMng.SelectPlayerRandom();
        if(playerGO != null)
        {
            PlayerController player = playerGO.GetComponent<PlayerController>();
            origin = player.GetNearestHexagon();

            if(origin != null)
            {
                HexagonController destiny = GetHexagonFacingCenter();

                bb.jumpOrigin = origin.transform.position;
                bb.jumpDestiny = destiny.transform.position;
                bb.CalculateParabola();

                bb.agent.enabled = false;

                rotation = 0f;
                highestPointReached = false;
            }

            elapsedTime = 0f;
            subState = SubState.WAITING;
        }
    }

    private HexagonController GetHexagonFacingCenter()
    {
        Vector3 offset;

        //Special case if origin hexagon is the center one
        if (origin.transform.position == bb.sceneCenter.transform.position)
        {
            float angle = Random.Range(0f, 365f);
            offset = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            offset = offset * HexagonController.DISTANCE_BETWEEN_HEXAGONS * destinyHexagonsDistance;
        }
        else
        {
            offset = (bb.sceneCenter.transform.position - origin.transform.position);
            offset.y = 0;
            offset = offset.normalized * HexagonController.DISTANCE_BETWEEN_HEXAGONS * destinyHexagonsDistance;
        }

        Vector3 position = origin.transform.position + offset;
        position.y = 0;

        Collider[] colliders = Physics.OverlapSphere(position, 1f, HexagonController.hexagonLayer);

        if (colliders.Length == 0) return null;

        HexagonController result = colliders[0].GetComponent<HexagonController>();
        float distance = (position - colliders[0].transform.position).sqrMagnitude;

        for (int i = 1; i < colliders.Length; ++i)
        {
            float newDistance = (colliders[i].transform.position - position).sqrMagnitude;

            if (newDistance < distance)
            {
                distance = newDistance;
                result = colliders[i].GetComponent<HexagonController>(); ;
            }
        }

        return result;      
    }

    public override WormAIBaseState Update()
    {
        if (origin == null) return bb.wanderingState;

        switch (subState)
        {
            case SubState.WAITING:
                if (elapsedTime >= bb.belowAttackWaitTime)
                {
                    elapsedTime = 0f;

                    //TODO: Activate hexagon

                    subState = SubState.WARNING_PLAYER;
                }
                else
                    elapsedTime += Time.deltaTime;

                break;

            case SubState.WARNING_PLAYER:

                if (elapsedTime >= bb.belowAttackWarningTime)
                {
                    //Position head below entry point
                    currentX = bb.GetJumpXGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false);
                    Vector3 startPosition = bb.GetJumpPositionGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false);
                    head.position = startPosition;
                    lastPosition = startPosition;
                    bb.worm.SetVisible(true);

                    subState = SubState.JUMPING;
                }
                else
                    elapsedTime += Time.deltaTime;

                break;

            case SubState.JUMPING:
                //While not again below underground navmesh layer advance
                currentX += Time.deltaTime * bb.floorSpeed;
                lastPosition = head.position;
                head.position = bb.GetJumpPositionGivenX(currentX);

                head.LookAt(head.position + (head.position - lastPosition), head.up);

                if (lastPosition.y > head.position.y && rotation < 90f)
                {
                    if (!highestPointReached)
                    {
                        bb.StartNewPhase();
                        highestPointReached = true;
                    }

                    float angle = 30 * Time.deltaTime;
                    head.Rotate(new Vector3(0, 0, angle));
                    rotation += angle;
                }

                if (head.position.y < -WormBlackboard.NAVMESH_LAYER_HEIGHT)
                {
                    bb.worm.SetVisible(false);

                    subState = SubState.EXITING;
                }
                break;

            case SubState.EXITING:
                currentX += Time.deltaTime * bb.floorSpeed;
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

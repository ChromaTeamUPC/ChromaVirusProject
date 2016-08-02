using UnityEngine;
using System.Collections;

public class WormAISpawningState : WormAIBaseState
{
    private enum SubState
    {
        GOING_TO_ENTRY,
        JUMPING,
        EXITING
    }

    private SubState subState;

    private Transform head;

    private float currentX;
    private Vector3 lastPosition;
    private float rotation;

    public WormAISpawningState(WormBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        head = bb.headTrf.transform;

        bb.jumpOrigin = bb.spawnEntry.transform.position;
        bb.jumpDestiny = bb.spawnExit.transform.position;
        bb.CalculateParabola();

        bb.agent.enabled = false;

        rotation = 0f;

        subState = SubState.GOING_TO_ENTRY;
    }

    public override WormAIBaseState Update()
    {
        switch (subState)
        {
            case SubState.GOING_TO_ENTRY:
                //Position head below entry point
                currentX = bb.GetJumpXGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false);
                Vector3 startPosition = bb.GetJumpPositionGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false);
                head.position = startPosition;
                lastPosition = startPosition;
                bb.worm.SetVisible(true);

                subState = SubState.JUMPING;
                break;

            case SubState.JUMPING:
                //While not again below underground navmesh layer advance
                currentX += Time.deltaTime * bb.floorSpeed;
                lastPosition = head.position;
                head.position = bb.GetJumpPositionGivenX(currentX);

                head.LookAt(head.position + (head.position - lastPosition), head.up);

                if( lastPosition.y > head.position.y && rotation < 90f)
                {
                    float angle = 30 * Time.deltaTime;
                    head.Rotate(new Vector3(0, 0, angle));
                    rotation += angle;
                }

                if(head.position.y < -WormBlackboard.NAVMESH_LAYER_HEIGHT)
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

                    bb.agent.areaMask = WormBlackboard.NAVMESH_UNDERGROUND_LAYER;
                    bb.agent.enabled = true;
                    bb.agent.speed = bb.undergroundSpeed;
                    bb.agent.SetDestination(bb.GetJumpPositionGivenY(-WormBlackboard.NAVMESH_LAYER_HEIGHT, false)); //Back to entry in the underground

                    return bb.wanderingState;
                }
                break;

            default:
                break;
        }

        return null;
    }
}

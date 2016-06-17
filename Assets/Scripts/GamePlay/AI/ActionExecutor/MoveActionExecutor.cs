using UnityEngine;
using System.Collections;

public class MoveActionExecutor: BaseExecutor
{
    private MoveAIAction moveAction;
    private Vector3 direction;
    private float elapsedTime;

    public override void SetAction(AIAction act)
    {
        base.SetAction(act);
        moveAction = (MoveAIAction)act;

        string tId = moveAction.targetId;

        if (tId == "leader")
        {
            if(blackBoard.groupInfo != null && blackBoard.groupInfo.leader != null)
                blackBoard.target = blackBoard.groupInfo.leader.gameObject;
        }
        else if (tId != "player")
        {
            blackBoard.target = GameObject.Find(tId);
        }
        else if ((blackBoard.target == null) || (!blackBoard.target.activeSelf))
        {
            blackBoard.target = rsc.enemyMng.SelectPlayer(blackBoard.entityGO);
        }

        if (blackBoard.target != null)
        {
            switch (moveAction.offsetType)
            {
                case MoveAIAction.OffsetType.POSITION_ZERO:
                    direction = new Vector3(0, 0, 0);
                    break;
                case MoveAIAction.OffsetType.AROUND_WORLD_RELATIVE:
                    direction = new Vector3(0, 0, 1);
                    direction = Quaternion.Euler(0, moveAction.angle, 0) * direction;
                    direction *= moveAction.distance;
                    break;
                case MoveAIAction.OffsetType.AROUND_AGENT_RELATIVE:
                    direction = (blackBoard.agent.transform.position - blackBoard.target.transform.position).normalized;
                    direction = Quaternion.Euler(0, moveAction.angle, 0) * direction;
                    direction *= moveAction.distance;
                    break;
                case MoveAIAction.OffsetType.AROUND_FORWARD_RELATIVE:
                    direction = blackBoard.target.transform.forward;
                    direction = Quaternion.Euler(0, moveAction.angle, 0) * direction;
                    direction *= moveAction.distance;
                    break;
            }

            blackBoard.agent.destination = blackBoard.target.transform.position + direction;
        }

        if (moveAction.inertia)
            blackBoard.agent.acceleration = 50;
        else
            blackBoard.agent.acceleration = 1000;

        blackBoard.agent.speed = moveAction.speed * rsc.gameInfo.globalEnemySpeedFactor;
        blackBoard.agent.Resume();

        blackBoard.animator.SetFloat("walkSpeed", blackBoard.agent.speed / 4);
        blackBoard.animator.SetBool("walking", true);
        
        elapsedTime = 0f;
    }

    public override int Execute()
    {
        elapsedTime += Time.deltaTime;

        if (moveAction.focusType == MoveAIAction.FocusType.CONTINUOUS)
        {
            if(moveAction.targetId == "leader")
            {
                if (blackBoard.groupInfo != null && blackBoard.groupInfo.leader != null)
                    blackBoard.target = blackBoard.groupInfo.leader.gameObject;
                else
                    blackBoard.target = null;
            }
            if(moveAction.targetId == "player" && ((blackBoard.target == null) || (!blackBoard.target.activeSelf)))
            {
                blackBoard.target = rsc.enemyMng.SelectPlayer(blackBoard.entityGO);
            }

            if (blackBoard.target != null)
            {
                if (moveAction.offsetType == MoveAIAction.OffsetType.AROUND_AGENT_RELATIVE)
                {
                    direction = (blackBoard.agent.transform.position - blackBoard.target.transform.position).normalized;
                    direction = Quaternion.Euler(0, moveAction.angle, 0) * direction;
                    direction *= moveAction.distance;
                }

                if (moveAction.offsetType == MoveAIAction.OffsetType.AROUND_FORWARD_RELATIVE)
                {
                    direction = blackBoard.target.transform.forward;
                    direction = Quaternion.Euler(0, moveAction.angle, 0) * direction;
                    direction *= moveAction.distance;

                    float angleMeWaypoint = Vector3.Angle(blackBoard.agent.transform.forward, blackBoard.target.transform.position + direction - blackBoard.agent.transform.position);
                    float angleMeTarget = Vector3.Angle(blackBoard.agent.transform.forward, blackBoard.target.transform.position - blackBoard.agent.transform.position);

                    blackBoard.agent.speed = moveAction.speed * rsc.gameInfo.globalEnemySpeedFactor;

                    if (angleMeWaypoint > 30 && angleMeTarget < angleMeWaypoint)
                    {
                        direction = new Vector3(0, 0, 0);
                        blackBoard.agent.speed *= 0.5f; // 50% de la velocidad establecida
                    }
                }

                blackBoard.agent.destination = blackBoard.target.transform.position + direction;
            }
        }


        if ((blackBoard.agent.hasPath && blackBoard.agent.remainingDistance <= 1.5f)
            || (moveAction.maxTime > 0 && elapsedTime > moveAction.maxTime))
        {
            if (!moveAction.inertia)
                blackBoard.agent.velocity = Vector3.zero;

            blackBoard.animator.SetFloat("walkSpeed", 1);

            return moveAction.nextAction;
        }
        else
        {
            return AIAction.ACTION_NOT_FINISHED;
        }
    }
}

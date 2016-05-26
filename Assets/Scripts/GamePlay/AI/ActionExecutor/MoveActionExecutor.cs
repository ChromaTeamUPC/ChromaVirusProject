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

        if (tId != "player")
        {
            blackBoard.target = GameObject.Find(tId);
        }
        else if ((blackBoard.target == null) || (!blackBoard.target.activeSelf))
        {
            blackBoard.target = rsc.enemyMng.SelectTarget(blackBoard.entityGO);
        }

        switch(moveAction.offsetType)
        {
            case AIAction.OffsetType.POSITION_ZERO:
                direction = new Vector3(0, 0, 0);
                break;
            case AIAction.OffsetType.AROUND_WORLD_RELATIVE:
                direction = new Vector3(0, 0, 1);
                direction = Quaternion.Euler(0, moveAction.angle, 0) * direction;
                direction *= moveAction.distance;
                break;
            case AIAction.OffsetType.AROUND_ENEMY_RELATIVE:
                direction = (blackBoard.agent.transform.position - blackBoard.target.transform.position).normalized;
                direction = Quaternion.Euler(0, moveAction.angle, 0) * direction;
                direction *= moveAction.distance;
                break;
        }

        if (moveAction.inertia)
            blackBoard.agent.acceleration = 50;
        else
            blackBoard.agent.acceleration = 1000;

        blackBoard.agent.speed = moveAction.speed * rsc.gameInfo.globalEnemySpeedFactor;
        blackBoard.agent.destination = blackBoard.target.transform.position + direction;
        blackBoard.agent.Resume();

        blackBoard.animator.SetFloat("walkSpeed", blackBoard.agent.speed / 4);
        blackBoard.animator.SetBool("walking", true);
        
        elapsedTime = 0f;
    }

    public override int Execute()
    {
        elapsedTime += Time.deltaTime;

        if (moveAction.focusType == AIAction.FocusType.CONTINUOUS)
        {
            if(moveAction.targetId == "player" && ((blackBoard.target == null) || (!blackBoard.target.activeSelf)))
            {
                blackBoard.target = rsc.enemyMng.SelectTarget(blackBoard.entityGO);
            }

            if(moveAction.offsetType == AIAction.OffsetType.AROUND_ENEMY_RELATIVE)
            {
                direction = (blackBoard.agent.transform.position - blackBoard.target.transform.position).normalized;
                direction = Quaternion.Euler(0, moveAction.angle, 0) * direction;
                direction *= moveAction.distance;
            }

            blackBoard.agent.destination = blackBoard.target.transform.position + direction;
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

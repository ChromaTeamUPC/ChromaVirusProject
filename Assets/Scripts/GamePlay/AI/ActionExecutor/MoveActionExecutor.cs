using UnityEngine;
using System.Collections;

public class MoveActionExecutor: BaseExecutor
{
    private MoveAIAction moveAction;
    private Vector3 direction;
    private float elapsedTime;

    private SpiderAIBehaviour spider; // TOREFACTOR - this action must be used for any enemy


    public override void SetAction(AIAction act)
    {
        base.SetAction(act);
        moveAction = (MoveAIAction)act;

        string tId = moveAction.targetId;

        spider = (SpiderAIBehaviour)state.parent; // TOREFACTOR - this action must be used for any enemy

        if (tId != "player")
        {
            state.target = GameObject.Find(tId);
        }
        else if(!state.target.activeSelf)
        {
            state.target = rsc.enemyMng.SelectTarget();
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
                direction = (state.agent.transform.position - state.target.transform.position).normalized;
                direction = Quaternion.Euler(0, moveAction.angle, 0) * direction;
                direction *= moveAction.distance;
                break;
        }

        if (moveAction.inertia)
            state.agent.acceleration = 50;
        else
            state.agent.acceleration = 1000;

        state.agent.speed = moveAction.speed;
        spider.animator.speed = moveAction.speed / 4;
        state.agent.destination = state.target.transform.position + direction;
        state.agent.Resume();
        elapsedTime = 0f;
    }

    public override int Execute()
    {
        elapsedTime += Time.deltaTime;

        if (moveAction.focusType == AIAction.FocusType.CONTINUOUS)
        {
            if(moveAction.targetId == "player" && !state.target.activeSelf)
            {
                state.target = rsc.enemyMng.SelectTarget();
            }

            if(moveAction.offsetType == AIAction.OffsetType.AROUND_ENEMY_RELATIVE)
            {
                direction = (state.agent.transform.position - state.target.transform.position).normalized;
                direction = Quaternion.Euler(0, moveAction.angle, 0) * direction;
                direction *= moveAction.distance;
            }

            state.agent.destination = state.target.transform.position + direction;
        }


        if ((state.agent.hasPath && state.agent.remainingDistance <= 1f)
            || (moveAction.maxTime > 0 && elapsedTime > moveAction.maxTime))
        {
            Debug.Log(state.agent.remainingDistance);
            if (!moveAction.inertia)
                state.agent.velocity = Vector3.zero;

            return moveAction.nextAction;
        }
        else
        {
            return AIAction.ACTION_NOT_FINISHED;
        }
    }
}

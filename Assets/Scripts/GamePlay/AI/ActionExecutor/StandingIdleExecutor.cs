using UnityEngine;
using System.Collections;

public class StandingIdleExecutor : BaseExecutor
{
    private StandingIdleAIAction standingIdleAction;
    private float elapsedTime;

    public override void SetAction(AIAction act)
    {
        base.SetAction(act);
        standingIdleAction = (StandingIdleAIAction)act;

        blackBoard.agent.Stop();
        blackBoard.animator.SetBool("walking", false);
        elapsedTime = 0f;
    }

    public override int Execute()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= standingIdleAction.seconds)
            return standingIdleAction.nextAction;
        else
            return AIAction.ACTION_NOT_FINISHED;
    }
}

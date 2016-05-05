using UnityEngine;
using System.Collections;

public class LookAtExecutor : BaseExecutor
{
    private const float angularSpeed = 1080f;
    private LookAtAIAction lookAtAction;
    private Vector3 direction;

    public override void SetAction(AIAction act)
    {
        //Debug.Log("Entering LookAt set action");
        base.SetAction(act);
        lookAtAction = (LookAtAIAction)act;

        string tId = lookAtAction.targetId;

        if (tId != "player")
        {
            state.target = GameObject.Find(tId);
        }
        else if (!state.target.activeSelf)
        {
            state.target = rsc.enemyMng.SelectTarget();
        }

        direction = state.target.transform.position - state.agent.transform.position;
        state.agent.Stop();
        //Debug.Log("Exiting LookAt set action");
    }

    public override int Execute()
    {
        //Debug.Log("Entering LookAt Executing");
        Quaternion newRotation = Quaternion.LookRotation(direction);
        newRotation = Quaternion.RotateTowards(state.agent.transform.rotation, newRotation, angularSpeed * Time.deltaTime);
        state.parent.transform.rotation = newRotation;
        if(Vector3.Angle(direction, state.agent.transform.forward) < 5)
        {

            state.agent.Resume();
            return lookAtAction.nextAction;
        }
        else
        {
            return AIAction.ACTION_NOT_FINISHED;
        }
    }
}

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
            blackBoard.target = GameObject.Find(tId);
        }
        else if ((blackBoard.target == null) || (!blackBoard.target.activeSelf))
        {
            blackBoard.target = rsc.enemyMng.SelectTarget(blackBoard.entityGO);
        }

        direction = blackBoard.target.transform.position - blackBoard.agent.transform.position;
        blackBoard.agent.Stop();
        //Debug.Log("Exiting LookAt set action");
    }

    public override int Execute()
    {
        //Debug.Log("Entering LookAt Executing");
        Quaternion newRotation = Quaternion.LookRotation(direction);
        newRotation = Quaternion.RotateTowards(blackBoard.agent.transform.rotation, newRotation, angularSpeed * Time.deltaTime);
        blackBoard.entityGO.transform.rotation = newRotation;
        if(Vector3.Angle(direction, blackBoard.agent.transform.forward) < 5)
        {

            blackBoard.agent.Resume();
            return lookAtAction.nextAction;
        }
        else
        {
            return AIAction.ACTION_NOT_FINISHED;
        }
    }
}

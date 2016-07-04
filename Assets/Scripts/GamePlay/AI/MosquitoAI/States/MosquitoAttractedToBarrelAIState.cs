using UnityEngine;
using System.Collections;

public class MosquitoAttractedToBarrelAIState : MosquitoAIBaseState 
{
    private const float angularSpeed = 1080f;

    private enum AttractedToBarrelSubState
    {
        GOING,
        LOOKING,
        WAITING,
    }

    private AttractedToBarrelSubState subState;
    private Vector3 direction;

    public MosquitoAttractedToBarrelAIState(MosquitoBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        mosquitoBlackboard.lookAtPlayer = false;
        //Go to around barrel at random angle and distance
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(0.7f, 2.5f);

        //direction = new Vector3(0, 0, 1);
        //direction = Quaternion.Euler(0, angle, 0) * direction;
        direction = (mosquitoBlackboard.agent.transform.position - mosquitoBlackboard.barrelController.transform.position).normalized;
        direction *= distance;

        mosquitoBlackboard.agent.destination = mosquitoBlackboard.barrelController.transform.position + direction;
        mosquitoBlackboard.agent.Resume();
        mosquitoBlackboard.animator.SetBool("moving", true);
        subState = AttractedToBarrelSubState.GOING;
        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        mosquitoBlackboard.lookAtPlayer = true;
    }

    public override AIBaseState Update()
    {
        /*When mosquito is in this state could happen this:
        1-If a barrel dissapeared (it exploded and mosquito is alive):
            Go to AttackingPlayer State
        2-If mosquito has not arrived to destination:
            Keep going
        3-If mosquito has arrived to destination:
            Look at barrel
        4-If mosquito is looking at barrel:
            Wait idle
        */

        if (mosquitoBlackboard.barrelController == null)
            return mosquitoBlackboard.attackingPlayerState;

        switch (subState)
        {
            case AttractedToBarrelSubState.GOING:
                //When arrived...
                if (mosquitoBlackboard.agent.hasPath && mosquitoBlackboard.agent.remainingDistance <= 0.25f)
                {
                    mosquitoBlackboard.agent.Stop();
                    subState = AttractedToBarrelSubState.LOOKING;
                }
                break;

            case AttractedToBarrelSubState.LOOKING:
                //...Make sure we look at the barrel
                if (Vector3.Angle(direction, mosquitoBlackboard.agent.transform.forward) < 5)
                {
                    mosquitoBlackboard.animator.SetBool("moving", false);
                    subState = AttractedToBarrelSubState.WAITING;
                }
                else
                {
                    direction = mosquitoBlackboard.barrelController.transform.position - mosquitoBlackboard.agent.transform.position;
                    Quaternion newRotation = Quaternion.LookRotation(direction);
                    newRotation = Quaternion.RotateTowards(mosquitoBlackboard.agent.transform.rotation, newRotation, angularSpeed * Time.deltaTime);
                    mosquitoBlackboard.entityGO.transform.rotation = newRotation;
                }
                break;
            case AttractedToBarrelSubState.WAITING:
                break;
        }

        return null;
    }
}

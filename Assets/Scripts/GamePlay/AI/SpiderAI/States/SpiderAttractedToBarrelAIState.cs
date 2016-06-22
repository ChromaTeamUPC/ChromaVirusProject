using UnityEngine;
using System.Collections;

public class SpiderAttractedToBarrelAIState : SpiderAIBaseState
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

    public SpiderAttractedToBarrelAIState(SpiderBlackboard bb) : base(bb)
    { }

    public override void OnStateEnter()
    {
        //Go to around barrel at random angle and distance
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(0.7f, 2.5f);

        direction = new Vector3(0, 0, 1);
        direction = Quaternion.Euler(0, angle, 0) * direction;
        direction *= distance;

        spiderBlackboard.agent.destination = spiderBlackboard.barrelController.transform.position + direction;
        spiderBlackboard.agent.Resume();
        spiderBlackboard.animator.SetBool("moving", true);
        subState = AttractedToBarrelSubState.GOING;
        base.OnStateEnter();
    }

    public override AIBaseState Update()
    {
        /*When spider is in this state could happen this:
        1-If a barrel dissapeared (it exploded and spider is alive):
            Go to AttackingPlayer State
        2-If spider has not arrived to destination:
            Keep going
        3-If spider has arrived to destination:
            Look at barrel
        4-If spider is looking at barrel:
            Wait idle
        */

        if (spiderBlackboard.barrelController == null)
            return spiderBlackboard.attackingPlayerState;

        switch (subState)
        {
            case AttractedToBarrelSubState.GOING:
                //When arrived...
                if (spiderBlackboard.agent.hasPath && spiderBlackboard.agent.remainingDistance <= 0.25f)
                {
                    spiderBlackboard.agent.Stop();
                    subState = AttractedToBarrelSubState.LOOKING;                
                }
                break;

            case AttractedToBarrelSubState.LOOKING:
                //...Make sure we look at the barrel
                if (Vector3.Angle(direction, spiderBlackboard.agent.transform.forward) < 5)
                {
                    spiderBlackboard.animator.SetBool("moving", false);
                    subState = AttractedToBarrelSubState.WAITING;
                }
                else
                { 
                    direction = spiderBlackboard.barrelController.transform.position - spiderBlackboard.agent.transform.position;
                    Quaternion newRotation = Quaternion.LookRotation(direction);
                    newRotation = Quaternion.RotateTowards(spiderBlackboard.agent.transform.rotation, newRotation, angularSpeed * Time.deltaTime);
                    spiderBlackboard.entityGO.transform.rotation = newRotation;
                }
                break;
            case AttractedToBarrelSubState.WAITING:
                break;
        }

        return null;
    }
}

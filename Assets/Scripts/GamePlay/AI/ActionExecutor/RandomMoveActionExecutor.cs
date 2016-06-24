using UnityEngine;
using System.Collections;

public class RandomMoveActionExecutor : BaseExecutor
{
    private RandomMoveAIAction randomMoveAction;

    private float speed;
    private int totalDisplacements;
    private bool waiting;
    private float waitTime;
    private float elapsedTime;
    private float distance;
    private float angle;
    private int leftRight;
    private Vector3 direction;

    public override void SetAction(AIAction act)
    {
        base.SetAction(act);
        randomMoveAction = (RandomMoveAIAction)act;       

        //Decide the number of displacements
        if (randomMoveAction.totalDisplacementsMin == randomMoveAction.totalDisplacementsMax)
            totalDisplacements = randomMoveAction.totalDisplacementsMax;
        else
            totalDisplacements = Random.Range(randomMoveAction.totalDisplacementsMin, randomMoveAction.totalDisplacementsMax + 1);

        if (Random.Range(0f, 1f) < 0.5f)
            leftRight = -1;
        else
            leftRight = 1;

        if (totalDisplacements > 0)
            SetNewParameters();

    }

    private void SetNewParameters()
    {
        waiting = false;
        elapsedTime = 0f;

        //Calculate speed
        if (randomMoveAction.speedMin == randomMoveAction.speedMax)
            speed = randomMoveAction.speedMax;
        else
            speed = Random.Range(randomMoveAction.speedMin, randomMoveAction.speedMax);

        //Calculate pause
        if (randomMoveAction.pauseBetweenDisplacementsMin == randomMoveAction.pauseBetweenDisplacementsMax)
            waitTime = randomMoveAction.pauseBetweenDisplacementsMax;
        else
            waitTime = Random.Range(randomMoveAction.pauseBetweenDisplacementsMin, randomMoveAction.pauseBetweenDisplacementsMax);

        //Calculate distance
        if (randomMoveAction.distanceMin == randomMoveAction.distanceMax)
            distance = randomMoveAction.distanceMax;
        else
            distance = Random.Range(randomMoveAction.distanceMin, randomMoveAction.distanceMax);

        //Calculate angle
        if (randomMoveAction.angleMin == randomMoveAction.angleMax)
            angle = randomMoveAction.angleMax;
        else
            angle = Random.Range(randomMoveAction.angleMin, randomMoveAction.angleMax);

        angle *= leftRight;

        leftRight *= -1;

        direction = Vector3.zero;
        if(randomMoveAction.reference == RandomMoveAIAction.Reference.PLAYER)
        {
            if (blackBoard.player != null && blackBoard.player.activeSelf)
                direction = blackBoard.player.transform.position - blackBoard.entityGO.transform.position;
        }
        else
        {
            if (blackBoard.target != null && blackBoard.target.activeSelf)
                direction = blackBoard.target.transform.position - blackBoard.entityGO.transform.position;
        }

        if(direction != Vector3.zero)
        {
            direction.y = 0f;
            direction.Normalize();
            direction = Quaternion.Euler(0, angle, 0) * direction;
            direction *= distance;

            blackBoard.agent.speed = speed * rsc.gameInfo.globalEnemySpeedFactor;
            blackBoard.agent.destination = blackBoard.entityGO.transform.position + direction;

            //This movement has no inertia
            blackBoard.agent.acceleration = 1000;

            blackBoard.agent.Resume();

            blackBoard.animator.SetFloat("moveSpeed", blackBoard.agent.speed / 4);
            blackBoard.animator.SetBool("moving", true);
        }
    }


    public override int Execute()
    {
        elapsedTime += Time.deltaTime;

        if (waiting)
        {
            if (elapsedTime > waitTime)
            {
                waiting = false;
                elapsedTime = 0f;
                SetNewParameters();
            }

            return AIAction.ACTION_NOT_FINISHED;
        }
        else
        {
            if (totalDisplacements > 0)
            {
                /*If arrived
                 *  decrement displacements
                 *  if displacement > 0
                 *      wait time
                 *      set new parameters
                 *  else
                 *      return action finished
                 */


                if ((blackBoard.agent.hasPath && blackBoard.agent.remainingDistance <= 1f)
                    || (randomMoveAction.maxTime > 0 && elapsedTime > randomMoveAction.maxTime))
                {
                    blackBoard.agent.velocity = Vector3.zero;
                    blackBoard.agent.Stop();
                    
                    --totalDisplacements;
                    if (totalDisplacements > 0)
                    {
                        
                        waiting = true;
                        elapsedTime = 0f;
                        return AIAction.ACTION_NOT_FINISHED;
                    }
                    else
                    {
                        blackBoard.animator.SetFloat("moveSpeed", 1);
                        return randomMoveAction.nextAction;
                    }
                }
                else
                {
                    return AIAction.ACTION_NOT_FINISHED;
                }
            }
            else
            {
                return randomMoveAction.nextAction;
            }
        }
    }

}

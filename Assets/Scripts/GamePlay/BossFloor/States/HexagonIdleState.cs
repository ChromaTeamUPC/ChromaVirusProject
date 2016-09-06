using UnityEngine;
using System.Collections;

public class HexagonIdleState : HexagonBaseState 
{
    private float moveWaitTime;
    private float elapsedTime;

    public HexagonIdleState(HexagonController hex) : base(hex)
    {
        ResetCounter();
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        ResetCounter();
    }

    private void ResetCounter()
    {
        moveWaitTime = Random.Range(hex.movementMinWaitTime, hex.movementMaxWaitTime);
        elapsedTime = 0f;
    }

    private bool CanMove()
    {
        return !hex.AnyProbeInRange
            && rsc.enemyMng.MinDistanceToPlayer(hex.gameObject) > hex.minDistanceToPlayer;
    }

    public override HexagonBaseState Update()
    {
        if (ReturnToPlace())
        {
            if (elapsedTime >= moveWaitTime)
            {
                if (CanMove())
                {
                    return hex.movingState;
                }
                else
                {
                    ResetCounter();
                }
            }
            else
            {
                elapsedTime += Time.deltaTime;
            }
        }

        return null;
    }

    public override HexagonBaseState WormHeadEntered()
    {
        hex.SetAuxTimer(hex.infectionTimeAfterContactEnds);
        return hex.infectedState;
    }

    public override HexagonBaseState ProbeTouched()
    {
        if (hex.isBorder)
            return hex.borderWallState;

        return null;
    }
}

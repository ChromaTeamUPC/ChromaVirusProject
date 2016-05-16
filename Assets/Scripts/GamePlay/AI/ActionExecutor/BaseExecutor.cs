using UnityEngine;
using System.Collections;

public class BaseExecutor
{
    protected EnemyBaseBlackboard blackBoard;
    protected AIAction action;

    public BaseExecutor() {}

    public virtual void Init(EnemyBaseBlackboard bb)
    {
        blackBoard = bb;
    }

    public virtual void SetAction(AIAction act)
    {
        action = act;
    }

    public virtual int Execute() { return AIAction.ACTION_NOT_FINISHED; }
}

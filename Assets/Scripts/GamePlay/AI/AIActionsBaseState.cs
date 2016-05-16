using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIActionsBaseState : AIBaseState
{
    protected List<AIAction> actionsList;
    protected int currentActionIndex;

    protected BaseExecutor currentExecutor;
    protected SelectPlayerExecutor selectExecutor;
    protected StandingIdleExecutor standIdleExecutor;
    protected LookAtExecutor lookAtExecutor;
    protected MoveActionExecutor moveExecutor;
    

    public AIActionsBaseState(EnemyBaseBlackboard bb): base(bb)
    {
        selectExecutor = new SelectPlayerExecutor();
        selectExecutor.Init(blackboard);
        standIdleExecutor = new StandingIdleExecutor();
        standIdleExecutor.Init(blackboard);
        lookAtExecutor = new LookAtExecutor();
        lookAtExecutor.Init(blackboard);
        moveExecutor = new MoveActionExecutor();
        moveExecutor.Init(blackboard);    
    }

    virtual public void Init(List<AIAction> actions)
    {
        actionsList = actions;
        if (actionsList == null)
            actionsList = new List<AIAction>();
    }

    override public void OnStateEnter()
    {
        currentActionIndex = 0;
        UpdateExecutor();
    }

    public int UpdateExecution()
    {
        if (currentExecutor != null)
            return currentExecutor.Execute();
        else
            return AIAction.LIST_FINISHED;
    }

    public AIBaseState ProcessUpdateExecutionResult(int updateExecutionResult)
    {
        switch (updateExecutionResult)
        {
            case AIAction.LIST_FINISHED: //This case should be managed in inherited classes
                break;

            case AIAction.ACTION_NOT_FINISHED: //Do nothing here       
                break;

            case AIAction.NEXT_ACTION:
                ++currentActionIndex;
                if (currentActionIndex == actionsList.Count)
                    currentActionIndex = 0;

                UpdateExecutor();
                break;

            default:
                currentActionIndex = updateExecutionResult;
                UpdateExecutor();
                break;
        }

        return null;
    }

    override public AIBaseState Update()
    {
        //Default behaviour. Should be override in inherited classes using above two methods
        return ProcessUpdateExecutionResult(UpdateExecution());      
    }

    virtual protected void UpdateExecutor()
    {
        if (actionsList.Count > 0)
        {
            AIAction action = actionsList[currentActionIndex];

            //Enemy specific executors should be managed in inherited classes
            switch (action.actionType)
            {
                case AIAction.Type.SELECT_PLAYER:
                    currentExecutor = selectExecutor;
                    break;

                case AIAction.Type.STANDING_IDLE:
                    currentExecutor = standIdleExecutor;
                    break;

                case AIAction.Type.LOOK_AT:
                    currentExecutor = lookAtExecutor;
                    break;

                case AIAction.Type.MOVE:
                    currentExecutor = moveExecutor;
                    break;
            }

            currentExecutor.SetAction(action);
        }
    }
}

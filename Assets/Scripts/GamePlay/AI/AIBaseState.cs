using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIBaseState
{
    public MonoBehaviour parent;
    public NavMeshAgent agent;

    protected List<AIAction> actionsList;
    protected int currentActionIndex;
    protected AIBaseState nextState;

    public GameObject target;

    protected BaseExecutor currentExecutor;
    protected SelectTargetExecutor selectExecutor;
    protected LookAtExecutor lookAtExecutor;
    protected MoveActionExecutor moveExecutor;
    protected SpiderBiteExecutor spiderBiteExecutor;

    public AIBaseState(MonoBehaviour p)
    {
        parent = p;
        agent = parent.GetComponent<NavMeshAgent>();

        selectExecutor = new SelectTargetExecutor();
        selectExecutor.Init(this);
        lookAtExecutor = new LookAtExecutor();
        lookAtExecutor.Init(this);
        moveExecutor = new MoveActionExecutor();
        moveExecutor.Init(this);
        spiderBiteExecutor = new SpiderBiteExecutor();
        spiderBiteExecutor.Init(this);

    }

    virtual public void Init(List<AIAction> actions, AIBaseState nextSt)
    {
        actionsList = actions;
        nextState = nextSt;
    }

    virtual public void OnStateEnter()
    {
        currentActionIndex = 0;
        UpdateExecutor();
    }
    virtual public void OnStateExit() { }

    virtual public AIBaseState Update()
    {
        int nextAction = currentExecutor.Execute();

        switch(nextAction)
        {
            case AIAction.LIST_FINISHED:
                return nextState;

            case AIAction.ACTION_NOT_FINISHED:
                //Do nothing here
                break;

            case AIAction.NEXT_ACTION:
                ++currentActionIndex;
                if (currentActionIndex == actionsList.Count)
                    currentActionIndex = 0;

                UpdateExecutor();
                break;

            default:
                currentActionIndex = nextAction;
                UpdateExecutor();
                break;
        }

        return null;
    }

    protected void UpdateExecutor()
    {
        AIAction action = actionsList[currentActionIndex];

        switch(action.actionType)
        {
            case AIAction.Type.SELECT_TARGET:
                currentExecutor = selectExecutor;          
                break;

            case AIAction.Type.LOOK_AT:
                currentExecutor = lookAtExecutor;
                break;

            case AIAction.Type.MOVE:
                currentExecutor = moveExecutor;
                break;

            case AIAction.Type.SPIDER_BITE:
                currentExecutor = spiderBiteExecutor;
                break;
        }

        currentExecutor.SetAction(action);
    }

}


using UnityEngine;
using System.Collections;

public class SpiderAIDefaultState : AIBaseState {

    public SpiderAIDefaultState(MonoBehaviour p): base (p)
    {
        currentActionIndex = 0;
    }

    //Spider default state does not reset action list index on enter
    //It resumes whatever it was doing when interrupted
    public override void OnStateEnter()
    {
        UpdateExecutor();
    }
}

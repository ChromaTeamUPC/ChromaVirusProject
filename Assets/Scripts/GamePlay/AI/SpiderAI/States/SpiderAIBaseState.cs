using UnityEngine;
using System.Collections;

public class SpiderAIBaseState : AIBaseState
{
    public SpiderBlackboard spiderBlackboard;

    public SpiderAIBaseState(SpiderBlackboard bb) : base(bb)
    {
        spiderBlackboard = bb;
    }
}

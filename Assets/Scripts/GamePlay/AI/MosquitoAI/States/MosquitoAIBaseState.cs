using UnityEngine;
using System.Collections;

public class MosquitoAIBaseState : AIBaseState
{
    public MosquitoBlackboard mosquitoBlackboard;

    public MosquitoAIBaseState(MosquitoBlackboard bb) : base(bb)
    {
        mosquitoBlackboard = bb;
    }
}

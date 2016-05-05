using UnityEngine;
using System.Collections;

public class AIAction
{
    public const int NEXT_ACTION = -1;
    public const int ACTION_NOT_FINISHED = -2;
    public const int LIST_FINISHED = -3;



    public enum OffsetType
    {
        POSITION_ZERO,      // same coordinates as the waypoint
        AROUND_WORLD_RELATIVE,              // around the target, at some degrees and distance relative to world forward vector
        AROUND_ENEMY_RELATIVE               // around the target, at some degrees and distance relative to the player-enemy vector
    }

    public enum FocusType
    {
        FIXED,              // the waypoint is calculated only the first time
        CONTINUOUS          // the waypoint calculation is refreshed every frame
    }

    public enum Type
    {
        NONE,
        SELECT_TARGET,
        MOVE,
        LOOK_AT,
        SPIDER_BITE
    }

    public Type actionType;
    public int nextAction;

    public AIAction(Type type, int next)
    {
        actionType = type;
        nextAction = next;
    }
}

public class SelectTargetAIAction : AIAction
{
    public string targetId; //TO REFACTOR: Not sure if needed

    public SelectTargetAIAction(string target, int next = AIAction.NEXT_ACTION) : base(Type.SELECT_TARGET, next)
    {
        targetId = target;
    }
}

public class MoveAIAction : AIAction
{
    public string targetId;
    public float speed;
    public FocusType focusType;
    public OffsetType offsetType;
    public int angle;
    public float distance;
    public bool inertia;
    public float maxTime;

    public MoveAIAction(string target, float sp = 4f, bool ine = true, float maxT = 0, int next = AIAction.NEXT_ACTION) : this(target, sp, FocusType.FIXED, OffsetType.POSITION_ZERO, 0, 0, ine, maxT, next)
    {
    }

    public MoveAIAction(string target, float sp, FocusType focusT, OffsetType offsetT, int ang = 0, float dist = 0f, bool ine = true, float maxT = 0, int next = AIAction.NEXT_ACTION) : base(Type.MOVE, next)
    {
        targetId = target;
        speed = sp;
        focusType = focusT;
        offsetType = offsetT;
        angle = ang;
        distance = dist;
        inertia = ine;
        maxTime = maxT;
    }
}

public class LookAtAIAction: AIAction
{
    public string targetId;

    public LookAtAIAction(string target, int next = AIAction.NEXT_ACTION): base (Type.LOOK_AT, next)
    {
        targetId = target;
    }
}

public class SpiderBiteAIAction: AIAction
{
    public float minimumTimeSinceLastAttack;
    
    public SpiderBiteAIAction(float minTime, int next = AIAction.NEXT_ACTION) : base (Type.SPIDER_BITE, next)
    {
        minimumTimeSinceLastAttack = minTime;
    }
}

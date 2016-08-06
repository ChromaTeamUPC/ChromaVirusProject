using UnityEngine;
using System.Collections;

public class WormAIBaseState
{
    public WormBlackboard bb;

    protected Transform head; //Shortcut
    private Vector3 undergroundDirection;

    public WormAIBaseState(WormBlackboard bb)
    {
        this.bb = bb;
        head = bb.headTrf.transform;
    }

    virtual public void Init() { }

    virtual public void OnStateEnter() { }

    virtual public void OnStateExit() { }

    virtual public WormAIBaseState Update()
    {
        return null;
    }

    public virtual WormAIBaseState ImpactedByShot(ChromaColor shotColor, float damage, PlayerController player)
    {
        return bb.worm.ProcessShotImpact(shotColor, damage, player);
    }

    protected void SetUndergroundDirection()
    {
        bb.worm.SetVisible(false);

        //Set direction to scene center
        undergroundDirection = bb.sceneCenter.transform.position - head.position;
        undergroundDirection.y = 0;
        undergroundDirection.Normalize();

        head.LookAt(head.position + undergroundDirection, Vector3.up);
    }

    protected void MoveUndergroundDirection()
    {
        head.position = head.position + (undergroundDirection * bb.undergroundSpeed * Time.deltaTime);
    }
}

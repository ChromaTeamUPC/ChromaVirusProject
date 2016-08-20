using UnityEngine;
using System.Collections;

public class WormAIBaseState
{
    public WormBlackboard bb;

    protected WormAIBehaviour head; //Shortcut
    protected Transform headTrf; //Shortcut
    private Vector3 undergroundDirection;

    public WormAIBaseState(WormBlackboard bb)
    {
        this.bb = bb;
        head = bb.head;
        headTrf = bb.headTrf;
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
        return head.ProcessShotImpact(shotColor, damage, player);
    }

    protected void SetUndergroundDirection()
    {
        head.SetVisible(false);

        //Set direction to scene center
        undergroundDirection = bb.sceneCenter.transform.position - headTrf.position;
        undergroundDirection.y = 0;
        undergroundDirection.Normalize();

        headTrf.LookAt(headTrf.position + undergroundDirection, Vector3.up);
    }

    protected void MoveUndergroundDirection()
    {
        headTrf.position = headTrf.position + (undergroundDirection * bb.undergroundSpeed * Time.deltaTime);
    }

    public virtual void PlayerTouched(PlayerController player, Vector3 origin)
    {
        player.ReceiveInfection(bb.contactDamage, origin, bb.infectionForces);
    }

    public virtual void UpdateBodyMovement()
    {
        bb.UpdateBodyMovement();
    }
}

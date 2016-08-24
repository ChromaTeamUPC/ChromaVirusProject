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
        return null;
    }

    public virtual WormAIBaseState ImpactedBySpecial(float damage, PlayerController player)
    {
        return null;
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
        headTrf.position = headTrf.position + (undergroundDirection * bb.WanderingSettingsPhase.undergroundSpeed * Time.deltaTime);
    }

    public virtual void PlayerTouched(PlayerController player, Vector3 origin)
    {
        player.ReceiveInfection(bb.contactDamage, origin, bb.infectionForces);
    }

    public virtual void UpdateBodyMovement()
    {
        bb.UpdateBodyMovement();
    }

    protected HexagonController GetExitHexagon(int hexagonsDistance = 2)
    {
        Vector3 offset = headTrf.forward;
        offset.y = 0;
        offset.Normalize();

        offset *= (HexagonController.DISTANCE_BETWEEN_HEXAGONS * hexagonsDistance);
        Vector3 position = headTrf.position + offset;
        position.y = 0;

        Collider[] colliders = Physics.OverlapSphere(position, 1f, HexagonController.hexagonLayer);

        if (colliders.Length == 0) return null;

        HexagonController result = colliders[0].GetComponent<HexagonController>();
        float distance = (position - colliders[0].transform.position).sqrMagnitude;

        for (int i = 1; i < colliders.Length; ++i)
        {
            float newDistance = (colliders[i].transform.position - position).sqrMagnitude;

            if (newDistance > distance)
            {
                distance = newDistance;
                result = colliders[i].GetComponent<HexagonController>();
            }
        }

        return result;
    }
}

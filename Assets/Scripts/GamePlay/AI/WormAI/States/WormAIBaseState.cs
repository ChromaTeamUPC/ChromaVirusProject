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

    public virtual bool CanSpawnMinion()
    {
        int wormPhases = bb.wormMaxPhases - bb.wormCurrentPhase;

        //if not reached cooldown or too many enemies in screen can not spawn
        if (rsc.enemyMng.bb.activeEnemies >= bb.SpawningMinionsSettingsPhase.maxMinionsOnScreen + wormPhases ||
            bb.spawningMinionsCurrentCooldownTime < bb.SpawningMinionsSettingsPhase.cooldownTime)
            return false;

        //if conditions met, random chance
        if (Random.Range(0f, 1f) < (float)bb.SpawningMinionsSettingsPhase.chancesOfSpawningMinion / 100)
        {
            bb.spawningMinionsCurrentCooldownTime = 0f;
            return true;
        }
        else
            return false;
    }

    protected void SetHeadUnderground()
    {
        bb.isHeadOverground = false;
        bb.tailReachedMilestone = false;
        bb.FlagCurrentWaypointAsMilestone();
        head.SetVisible(false);
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
        headTrf.position = headTrf.position + (undergroundDirection * bb.WanderingSettingsPhase.wanderingSpeed * Time.deltaTime);
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
        //Search in front of worm
        Vector3 offset = headTrf.forward;
        offset.y = 0;
        offset.Normalize();

        HexagonController result = GetHexagon(offset, hexagonsDistance);

        //if no results, search to left or right
        if(result == null)
        {
            bool right = Random.Range(0f, 1f) > 0.5f;

            if(right)
                offset = Quaternion.Euler(0, 90, 0) * offset;
            else
                offset = Quaternion.Euler(0, -90, 0) * offset;

            result = GetHexagon(offset, hexagonsDistance);

            if(result == null)
            {
                offset = Quaternion.Euler(0, 180, 0) * offset;

                result = GetHexagon(offset, hexagonsDistance);
            }
        }

        return result;
    }

    private HexagonController GetHexagon(Vector3 offset, int hexagonsDistance)
    {
        offset *= (HexagonController.DISTANCE_BETWEEN_HEXAGONS * hexagonsDistance);
        Vector3 position = headTrf.position + offset;
        position.y = 0;

        Collider[] colliders = Physics.OverlapSphere(position, HexagonController.DISTANCE_BETWEEN_HEXAGONS, HexagonController.hexagonLayer);

        if (colliders.Length == 0) return null;

        HexagonController result = null;
        float targetDistance = Mathf.Pow(hexagonsDistance * HexagonController.DISTANCE_BETWEEN_HEXAGONS, 2);
        float distanceDelta = float.MaxValue;

        for (int i = 0; i < colliders.Length; ++i)
        {
            HexagonController candidate = colliders[i].GetComponent<HexagonController>();

            if (candidate.CanExitWorm())
            {
                if (result == null)
                {
                    result = candidate;
                    distanceDelta = (colliders[i].transform.position - position).sqrMagnitude;
                }
                else
                {
                    float newDistance = (colliders[i].transform.position - position).sqrMagnitude;

                    if (newDistance < distanceDelta)
                    {
                        result = candidate;
                        distanceDelta = newDistance;
                    }
                }
            }
        }

        return result;
    }

    protected HexagonController GetHexagonFacingCenter(HexagonController origin, int hexagonsDistance = 5)
    {
        Vector3 offset;

        //Special case if origin hexagon is the center one
        if (origin.transform.position == bb.sceneCenter.transform.position)
        {
            float angle = Random.Range(0f, 365f);
            offset = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            offset = offset * HexagonController.DISTANCE_BETWEEN_HEXAGONS * hexagonsDistance;
        }
        else
        {
            offset = (bb.sceneCenter.transform.position - origin.transform.position);
            offset.y = 0;
            offset = offset.normalized * HexagonController.DISTANCE_BETWEEN_HEXAGONS * hexagonsDistance;
        }

        Vector3 position = origin.transform.position + offset;
        position.y = 0;

        Collider[] colliders = Physics.OverlapSphere(position, HexagonController.DISTANCE_BETWEEN_HEXAGONS, HexagonController.hexagonLayer);

        if (colliders.Length == 0) return null;

        HexagonController result = null;
        float targetDistance = Mathf.Pow(hexagonsDistance * HexagonController.DISTANCE_BETWEEN_HEXAGONS, 2);
        float distanceDelta = targetDistance;

        for (int i = 0; i < colliders.Length; ++i)
        {
            HexagonController candidate = colliders[i].GetComponent<HexagonController>();

            if (candidate.CanExitWorm())
            {
                if (result == null)
                {
                    result = candidate;
                    float distance = (colliders[i].transform.position - position).sqrMagnitude;
                    distanceDelta = Mathf.Abs(targetDistance - distance);
                }
                else
                {
                    float newDistance = (colliders[i].transform.position - position).sqrMagnitude;
                    float newDistanceDelta = Mathf.Abs(targetDistance - newDistance);

                    if (newDistanceDelta < distanceDelta)
                    {
                        result = candidate;
                        distanceDelta = newDistanceDelta;
                    }
                }
            }
        }

        return result;
    }
}

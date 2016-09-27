using UnityEngine;
using System.Collections;


public class HexagonInfectedState : HexagonBaseState
{
    private Vector3 half;
    private float initialTimer;
    private bool spawnChecked;

    public HexagonInfectedState(HexagonController hex) : base(hex)
    {
        half = new Vector3(0.5f, 1f, 0.5f);      
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        initialTimer = hex.AuxTimer;
        spawnChecked = false;

        hex.StartPlaneInfectionAnimation();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        hex.StopPlaneInfectionAnimation();
    }

    public override HexagonBaseState Update()
    {
        ReturnToPlace();

        if(hex.AuxTimer < initialTimer)
        {
            if(!spawnChecked)
            {
                if(rsc.enemyMng.bb.worm != null && rsc.enemyMng.bb.worm.head.CanSpawnMinion())
                {
                    SpiderAIBehaviour enemy = rsc.coloredObjectsMng.GetSpider(rsc.colorMng.GetRandomActiveColor(), hex.spawnPoint.transform.position);

                    if (enemy != null)
                    {
                        enemy.AIInit(SpiderAIBehaviour.SpawnAnimation.FLOOR_FAST, hex.entryActions, hex.attackActions, hex.infectActions);
                        enemy.Spawn(hex.spawnPoint.transform);
                        rsc.enemyMng.AddVortexEnemyInfection(SpiderAIBehaviour.infectionValue);
                    }
                }

                spawnChecked = true;
            }
        }

        if (hex.AuxTimer < hex.AuxHalfTimer)
        {
            hex.plane.transform.localScale = Vector3.Lerp(Vector3.one, half, (hex.AuxHalfTimer - hex.AuxTimer) / hex.AuxHalfTimer);
        }

        if (hex.AuxTimer <= 0f)
        {
            return hex.idleState;
        }

        return null;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        player.ReceiveInfection(hex.infectedCellDamage);
        return null;
    }
}

using UnityEngine;
using System.Collections;

public class HexagonAboveAttackAdjacentState : HexagonBaseState
{
    private enum SubState
    {
        UP,
        DOWN,
        RETURN
    }

    private SubState subState;
    private float totalMovement;
    private float upMovement;
    private float downMovement;
    private float speed;
    private float currentMovement;

    public HexagonAboveAttackAdjacentState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        subState = SubState.UP;
        upMovement = Random.Range(hex.earthquakeMinHeight, hex.earthquakeMaxHeight);
        downMovement = upMovement / 4;

        totalMovement = upMovement;
        currentMovement = 0f;
        speed = hex.earthquakeUpSpeed;

        hex.buffPurpleGO.SetActive(true);
    }

    public override void OnStateExit()
    {
        hex.buffPurpleGO.SetActive(false);
        hex.geometryOffset.transform.position = new Vector3(hex.geometryOffset.transform.position.x, hex.geometryOriginalY, hex.geometryOffset.transform.position.z);
        base.OnStateExit();
    }

    public override HexagonBaseState Update()
    {
        float displacement = speed * Time.deltaTime;

        switch (subState)
        {
            case SubState.UP:
                hex.geometryOffset.transform.position += new Vector3(0f, displacement, 0f);
                currentMovement += displacement;
                if(currentMovement >= totalMovement)
                {
                    totalMovement = upMovement + downMovement;
                    currentMovement = 0;
                    speed = hex.earthquakeDownSpeed;
                    subState = SubState.DOWN;
                }
                break;

            case SubState.DOWN:
                hex.geometryOffset.transform.position -= new Vector3(0f, displacement, 0f);
                currentMovement += displacement;
                if (currentMovement >= totalMovement)
                {
                    totalMovement = downMovement;
                    currentMovement = 0;
                    subState = SubState.RETURN;
                }
                break;

            case SubState.RETURN:
                hex.geometryOffset.transform.position += new Vector3(0f, displacement, 0f);
                currentMovement += displacement;
                if (currentMovement >= totalMovement)
                {
                    hex.currentInfectionDuration = hex.infectionTimeAfterAttack;
                    hex.countingInfectionTime = true;
                    return hex.infectedState;
                }
                break;
            default:
                break;
        }

        return null;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        player.ReceiveInfection(hex.belowAttackDamage, hex.attackCenter, hex.infectionForces);
        return null;
    }
}


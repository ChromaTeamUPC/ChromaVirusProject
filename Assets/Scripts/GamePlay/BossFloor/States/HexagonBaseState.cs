using UnityEngine;
using System.Collections;

public class HexagonBaseState
{
    protected HexagonController hex;

    public HexagonBaseState(HexagonController hex)
    {
        this.hex = hex;
    }

    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }

    public virtual HexagonBaseState Update() { return null; }

    public virtual HexagonBaseState ProbeTouched()
    {
        if (hex.isBorder)
            return hex.borderWallState;

        return null;
    }

    public virtual HexagonBaseState WormHeadEntered() { return null; }
    public virtual HexagonBaseState WormHeadStay(Transform headTrf) { return null; }
    public virtual HexagonBaseState WormHeadExited() { return null; }
    public virtual HexagonBaseState WormTailEntered() { return null; }
    public virtual HexagonBaseState WormTailExited() { return null; }
    public virtual HexagonBaseState PlayerEntered(PlayerController player) { return null; }
    public virtual HexagonBaseState PlayerStay(PlayerController player) { return null; }
    public virtual HexagonBaseState PlayerExited(PlayerController player) { return null; }
    public virtual HexagonBaseState EnemyStay(EnemyBaseAIBehaviour enemy) { return null; }

    protected bool ReturnToPlace()
    {
        if (hex.isMoving && hex.geometryOffset.transform.position.y != hex.geometryOriginalY)
        {
            float displacement = Time.deltaTime * 20f;

            if (hex.geometryOffset.transform.position.y > hex.geometryOriginalY)
            {
                hex.geometryOffset.transform.position -= new Vector3(0f, displacement, 0f);
                if (hex.geometryOffset.transform.position.y < hex.geometryOriginalY)
                {
                    hex.geometryOffset.transform.position = new Vector3(hex.geometryOffset.transform.position.x, hex.geometryOriginalY, hex.geometryOffset.transform.position.z);
                    hex.isMoving = false;
                    return true;
                }
            }
            else
            {
                hex.geometryOffset.transform.position += new Vector3(0f, displacement, 0f);
                if (hex.geometryOffset.transform.position.y > hex.geometryOriginalY)
                {
                    hex.geometryOffset.transform.position = new Vector3(hex.geometryOffset.transform.position.x, hex.geometryOriginalY, hex.geometryOffset.transform.position.z);
                    hex.isMoving = false;
                    return true;
                }
            }

            return false;
        }
        else
        {
            return true;
        }
    }
}

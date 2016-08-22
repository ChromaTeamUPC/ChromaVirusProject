using UnityEngine;
using System.Collections;

public class HexagonMovingState : HexagonBaseState 
{
    private float currentMovement;
    private bool up;
    private float totalMovement;
    private bool returning;

    public HexagonMovingState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.isMoving = true;
        currentMovement = 0f;
        up = Random.Range(0f, 1f) >= 0.5f;
        if (up)
            totalMovement = Random.Range(hex.upMinMovement, hex.upMaxMovement);
        else
            totalMovement = Random.Range(hex.downMinMovement, hex.downMaxMovement);

        returning = false;
    }

    public override HexagonBaseState Update()
    {
        if(!returning)
        {
            if (currentMovement < totalMovement)
            {
                float displacement = Time.deltaTime * hex.movementSpeed;

                if (up)
                    hex.geometryOffset.transform.position += new Vector3(0f, displacement, 0f);
                else
                    hex.geometryOffset.transform.position -= new Vector3(0f, displacement, 0f);
                currentMovement += displacement;
            }
            else
                returning = true;
        }
        else
        {
            if(currentMovement > 0f)
            {
                float displacement = Time.deltaTime * hex.movementSpeed;
                if (up)
                {
                    hex.geometryOffset.transform.position -= new Vector3(0f, displacement, 0f);
                    if (hex.geometryOffset.transform.position.y < hex.geometryOriginalY)
                        hex.geometryOffset.transform.position = new Vector3(hex.geometryOffset.transform.position.x, hex.geometryOriginalY, hex.geometryOffset.transform.position.z);
                }
                else
                {
                    hex.geometryOffset.transform.position += new Vector3(0f, displacement, 0f);
                    if (hex.geometryOffset.transform.position.y > hex.geometryOriginalY)
                        hex.geometryOffset.transform.position = new Vector3(hex.geometryOffset.transform.position.x, hex.geometryOriginalY, hex.geometryOffset.transform.position.z);
                }
                currentMovement -= displacement;
            }
            else
            {
                hex.geometryOffset.transform.position = new Vector3(hex.geometryOffset.transform.position.x, hex.geometryOriginalY, hex.geometryOffset.transform.position.z);
                hex.isMoving = false;

                return hex.idleState;
            }
        }

        return null;
    }

    public override HexagonBaseState ProbeTouched()
    {
        if (hex.isBorder)
            return hex.borderWallState;
        else
            return hex.idleState;
    }
}

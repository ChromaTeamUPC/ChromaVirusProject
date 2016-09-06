using UnityEngine;
using System.Collections;

public class HexagonBorderWallState : HexagonBaseState
{
    private enum SubState
    {
        GOING_UP,
        IDLE,
        GOING_DOWN
    }

    private SubState subState;
    private float totalHeight;

    public HexagonBorderWallState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.navMeshObstacles.SetActive(true);

        subState = SubState.GOING_UP;
        totalHeight = hex.geometryOriginalY + Random.Range(hex.borderMinHeight, hex.borderMaxHeight);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        hex.navMeshObstacles.SetActive(false);

        hex.geometryOffset.transform.position = new Vector3(hex.geometryOffset.transform.position.x, hex.geometryOriginalY, hex.geometryOffset.transform.position.z);
    }

    public override HexagonBaseState Update()
    {
        if (hex.probesInRange == 0) subState = SubState.GOING_DOWN;

        switch (subState)
        {
            case SubState.GOING_UP:
                if (hex.geometryOffset.transform.position.y < totalHeight)
                {
                    float displacement = Time.deltaTime * hex.borderSpeed;
                    hex.geometryOffset.transform.position += new Vector3(0f, displacement, 0f);
                }
                else
                {
                    hex.geometryOffset.transform.position = new Vector3(hex.geometryOffset.transform.position.x, totalHeight, hex.geometryOffset.transform.position.z);
                    subState = SubState.IDLE;
                }
                break;

            case SubState.GOING_DOWN:
                if (hex.geometryOffset.transform.position.y > hex.geometryOriginalY)
                {
                    float displacement = Time.deltaTime * hex.borderSpeed;
                    hex.geometryOffset.transform.position -= new Vector3(0f, displacement, 0f);
                }
                else
                {
                    hex.geometryOffset.transform.position = new Vector3(hex.geometryOffset.transform.position.x, hex.geometryOriginalY, hex.geometryOffset.transform.position.z);
                    return hex.idleState;
                }
                break;
        }

        return null;
    }

    public override HexagonBaseState ProbeTouched()
    {
        return hex.borderWallState;
    }
}

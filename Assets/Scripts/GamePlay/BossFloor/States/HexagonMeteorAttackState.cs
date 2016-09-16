using UnityEngine;
using System.Collections;

public class HexagonMeteorAttackState : HexagonBaseState
{
    private enum SubState
    {
        WAITING,
        WARNING,
        EXPLODING
    }

    private SubState subState;
    private float elapsedTime;
    private float shakeTime;
    private float lastHeightDir;
    private MeteorController meteor;


    public HexagonMeteorAttackState(HexagonController hex) : base(hex) { }

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        hex.planeBlinkController.StopPreviousBlinkings();
        hex.SetPlaneMaterial(hex.planeSecondaryWarningMat);
        hex.plane.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
        //hex.planeBlinkController.BlinkTransparentNoStop(hex.warningBlinkInterval, hex.warningBlinkInterval);

        subState = SubState.WAITING;
        elapsedTime = 0f;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        hex.columnBlinkController.StopPreviousBlinkings();
        hex.continousPurple.Stop();
        hex.StopPlaneInfectionAnimation();

        hex.geometryOffset.transform.position = new Vector3(hex.geometryOffset.transform.position.x, hex.geometryOriginalY, hex.geometryOffset.transform.position.z);
    }

    public override HexagonBaseState Update()
    {
        switch (subState)
        {
            case SubState.WAITING:
                if(elapsedTime >= hex.meteorWaitTime)
                {
                    meteor = rsc.poolMng.meteorPool.GetObject();
                    meteor.transform.position = hex.meteorInitialPosition.position;
                    meteor.transform.rotation = Random.rotation;
                   
                    elapsedTime = 0f;
                    subState = SubState.WARNING;
                }
                else
                {
                    ReturnToPlace();
                    elapsedTime += Time.deltaTime;
                }
                break;
            case SubState.WARNING:
                if(elapsedTime >= hex.meteorWarningTime)
                {
                    hex.plane.transform.localScale = Vector3.one;
                    hex.StartPlaneInfectionAnimation();

                    hex.columnBlinkController.InvalidateMaterials();
                    hex.columnBlinkController.BlinkWhiteNoStop(hex.aboveAttackBlinkInterval, hex.aboveAttackBlinkInterval);

                    if (!hex.continousPurple.isPlaying)
                        hex.continousPurple.Play();

                    meteor.Explode();
                    elapsedTime = 0;
                    shakeTime = 0;
                    lastHeightDir = -1;
                    subState = SubState.EXPLODING;
                }
                else
                {
                    ReturnToPlace();

                    float t = elapsedTime / hex.meteorWarningTime;

                    float newScale = Mathf.Lerp(0.1f, 1f, t);
                    hex.plane.transform.localScale = new Vector3(newScale, 1f, newScale);
                    meteor.transform.position = Vector3.Lerp(hex.meteorInitialPosition.position, hex.transform.position, t);

                    elapsedTime += Time.deltaTime;
                }
                break;


            case SubState.EXPLODING:
                if (elapsedTime >= hex.meteorImpactDuration)
                {
                    return hex.idleState;
                }
                else
                {
                    if (hex.meteorImpactMaxShakeHeight > 0)
                    {
                        if (shakeTime >= hex.meteorImpactShakeInterval)
                        {
                            shakeTime -= hex.meteorImpactShakeInterval;
                            float randHeight = Random.Range(0, hex.meteorImpactMaxShakeHeight) * lastHeightDir;
                            lastHeightDir *= -1;
                            hex.geometryOffset.transform.position = new Vector3(hex.geometryOffset.transform.position.x, hex.geometryOriginalY + randHeight, hex.geometryOffset.transform.position.z);
                        }
                        else
                        {
                            shakeTime += Time.deltaTime;
                        }
                    }

                    elapsedTime += Time.deltaTime;
                }

                break;

            default:
                break;
        }

        return null;
    }

    public override HexagonBaseState PlayerStay(PlayerController player)
    {
        if(subState== SubState.EXPLODING)
            player.ReceiveInfection(hex.meteorAttackDamage, hex.transform.position, hex.infectionForces);
        return null;
    }

    public override HexagonBaseState EnemyStay(EnemyBaseAIBehaviour enemy)
    {
        if (subState == SubState.EXPLODING)
            enemy.ImpactedByHexagon();
        return null;
    }
}

﻿using UnityEngine;
using System.Collections;

public class PlayerBaseState
{   
    protected PlayerBlackboard bb;

    private const float FALLING_GRACE_TIME = 0.2f;
    private float continousFallingTime;

    //Shoot variables
    private float nextFire;
    private const float maxSideOffset = 0.4f;
    private const float minSideOffset = 0.2f;
    private float shotSideOffset = minSideOffset;
    private float sideOffsetVariation = -0.05f;

    private ColoredObjectsManager coloredObjMng;

    public virtual void Init(PlayerBlackboard bb)
    {
        this.bb = bb;

        coloredObjMng = rsc.coloredObjectsMng;
    }

    public virtual void OnStateEnter() { }

    public virtual void OnStateExit() { }

    public virtual void RetrieveInput()
    {
        //Movement
        //float h = Input.GetAxisRaw(blackboard.moveHorizontal);
        //float v = Input.GetAxisRaw(blackboard.moveVertical);
        float h = bb.controller.LeftStickX.Value;
        float v = bb.controller.LeftStickY.Value;

        bb.moveVector = Vector3.zero;

        if (Mathf.Abs(v) > 0 || Mathf.Abs(h) > 0)
        {
            bb.moveVector.x = h;
            bb.moveVector.y = v;
            if (bb.moveVector.magnitude > 1f)
                bb.moveVector.Normalize();

            bb.movePressed = true;
            bb.animator.SetBool("Walking", true);
            bb.KeyPressed = true;
        }

        //Aiming
        //h = Input.GetAxisRaw(blackboard.aimHorizontal);
        //v = Input.GetAxisRaw(blackboard.aimVertical);
        h = bb.controller.RightStickX.Value;
        v = bb.controller.RightStickY.Value;

        bb.aimVector = Vector3.zero;

        if (Mathf.Abs(v) >= bb.player.aimThreshold || Mathf.Abs(h) >= bb.player.aimThreshold)
        {
            bb.aimVector.x = h;
            bb.aimVector.y = v;

            bb.aimPressed = true;
            bb.animator.SetBool("Aiming", true);
            bb.KeyPressed = true;
        }

        //Shoot
        //if (Input.GetAxisRaw(blackboard.fire) > 0.1f)
        if (bb.controller.RightTrigger.Value > 0.1f)
        {
            bb.shootPressed = true;
            bb.KeyPressed = true;
            bb.animator.SetBool("Shooting", true);
        }
        else
        {
            bb.firstShot = true;
            bb.player.StopNoShoot();
        }

        //Dash
        //if (Input.GetButtonDown(blackboard.dash))
        if(bb.controller.LeftBumper.WasPressed)
        {
            bb.dashPressed = true;
            bb.KeyPressed = true;
        }

        //Speed bump
        //if (Input.GetButtonDown(blackboard.dash))
        if (bb.controller.LeftTrigger.WasPressed)
        {
            bb.speedBumpPressed = true;
            bb.KeyPressed = true;
        }

        //Special
        //if (Input.GetButtonDown(blackboard.special))
        if (bb.controller.RightBumper.WasPressed)
        {
            bb.specialPressed = true;
            bb.KeyPressed = true;
        }

        //A Button
        //if (Input.GetButton(blackboard.greenButton))
        if (bb.controller.Action1.IsPressed)
        {
            bb.greenPressed = true;
            bb.colorButtonsPressed = true;
            bb.KeyPressed = true;
        }

        //B Button
        //if (Input.GetButton(blackboard.redButton))
        if (bb.controller.Action2.IsPressed)
        {
            bb.redPressed = true;
            bb.colorButtonsPressed = true;
            bb.KeyPressed = true;
        }

        //X Button
        //if (Input.GetButton(blackboard.blueButton))
        if (bb.controller.Action3.IsPressed)
        {
            bb.bluePressed = true;
            bb.colorButtonsPressed = true;
            bb.KeyPressed = true;
        }

        //Y Button
        //if (Input.GetButton(blackboard.yellowButton))
        if (bb.controller.Action4.IsPressed)
        {
            bb.yellowPressed = true;
            bb.colorButtonsPressed = true;
            bb.KeyPressed = true;
        }
    }

    public virtual PlayerBaseState Update()
    {
        /*actions check list:
        is he grounded?
        is he in a border?
        can he do a special?
        can he do a dash?
        can he charge a capacitor?
        can he disinfect a device?
        can he turn?
        can he shoot?
        can he move?
        */

        return null;
    }

    protected bool ShouldFall()
    {
        if (bb.isGrounded)
        {
            continousFallingTime = 0f;
        }
        else
        {
            continousFallingTime += Time.deltaTime;
            if (continousFallingTime >= FALLING_GRACE_TIME)
                return true;
        }

        return false;
    }

    protected void CapacitorCharge()
    {
        if (bb.capacitor == null || !bb.colorButtonsPressed)
            return;

        if (bb.redPressed)
            bb.capacitor.ManualCharge(ChromaColor.RED, bb.player);
        else if (bb.greenPressed)
            bb.capacitor.ManualCharge(ChromaColor.GREEN, bb.player);
        else if (bb.bluePressed)
            bb.capacitor.ManualCharge(ChromaColor.BLUE, bb.player);
        else if (bb.yellowPressed)
            bb.capacitor.ManualCharge(ChromaColor.YELLOW, bb.player);
    }

    protected void DisinfectDevice()
    {
        if (bb.device == null || !bb.colorButtonsPressed)
            return;

        if (bb.greenPressed)
            bb.device.Disinfect();
    }

    private void LookAt(Vector3 destination)
    {
        Quaternion newRotation = Quaternion.LookRotation(destination);
        newRotation = Quaternion.RotateTowards(bb.player.transform.rotation, newRotation, bb.player.angularSpeed * Time.deltaTime);
        bb.player.transform.rotation = newRotation;
    }

    protected void Turn()
    {
        if (bb.aimPressed)
        {
            bb.aimVector = bb.GetScreenRelativeDirection(bb.aimVector);
            LookAt(bb.aimVector);
        }
    }

    private int AngleBetween360(Vector3 v1, Vector3 v2)
    {
        Vector3 n = new Vector3(0, 1, 0);

        float signedAngle = Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;

        if (signedAngle >= 0)
            return (int)signedAngle;
        else
            return (int)(360 + signedAngle);
    }

    protected bool Move()
    {
        if(bb.movePressed)
        {
            float magnitude = bb.moveVector.magnitude;

            bb.moveVector = bb.GetScreenRelativeDirection(bb.moveVector) * magnitude;

            bb.horizontalDirection = bb.moveVector;

            //If we are not aiming, rotate towards direction
            if (!bb.aimPressed)
            {
                LookAt(bb.moveVector);
            }
            else
            {
                int angleBetweenSticks = AngleBetween360(bb.aimVector, bb.moveVector);

                float angleRad = angleBetweenSticks * Mathf.Deg2Rad;
                float forward = Mathf.Cos(angleRad);
                float lateral = Mathf.Sin(angleRad);
                bb.animator.SetFloat("Forward", forward);
                bb.animator.SetFloat("Lateral", lateral);
            }
        }

        return bb.movePressed;
    }

    protected bool CanDoSpecial()
    {
        return bb.specialPressed &&
            ((bb.currentEnergy >= bb.player.specialAttackNecessaryEnergy) ||
            rsc.debugMng.godMode);
    }

    protected void Shoot()
    {
        if (bb.shootPressed)
        {
            if (bb.canShoot)
            {
                rsc.rumbleMng.AddContinousRumble(RumbleType.PLAYER_SHOOT, bb.player.Id, 0.22f, 0.104f);

                if (Time.time > nextFire)
                {
                    nextFire = Time.time + bb.player.fireRate;

                    // check if it's first shot (single projectile)...
                    if (bb.firstShot)
                    {
                        //Get a shot from pool
                        PlayerShotController shot = coloredObjMng.GetPlayer1Shot();
                        MuzzleController muzzle = coloredObjMng.GetPlayer1Muzzle();

                        if (shot != null && muzzle != null)
                        {
                            Transform shotSpawn = bb.player.shotSpawn;
                            shot.transform.position = shotSpawn.position;
                            shot.transform.rotation = shotSpawn.rotation;
                            shot.damage *= 2;
                            shot.player = bb.player;
                            shot.Shoot();

                            Transform muzzlePoint = bb.player.muzzlePoint;
                            muzzle.transform.SetParent(muzzlePoint);
                            muzzle.transform.position = muzzlePoint.position;
                            muzzle.transform.rotation = muzzlePoint.rotation;
                            muzzle.Play();
                        }
                        bb.firstShot = false;
                    }
                    // ...or not (double projectile)
                    else
                    {
                        //Get two shots from pool
                        PlayerShotController shot1 = coloredObjMng.GetPlayer1Shot();
                        PlayerShotController shot2 = coloredObjMng.GetPlayer1Shot();

                        MuzzleController muzzle1 = coloredObjMng.GetPlayer1Muzzle();
                        MuzzleController muzzle2 = coloredObjMng.GetPlayer1Muzzle();

                        if (shot1 != null && shot2 != null && muzzle1 != null && muzzle2 != null)
                        {
                            Transform shotSpawn = bb.player.shotSpawn;
                            Transform muzzlePoint = bb.player.muzzlePoint;

                            shot1.transform.rotation = shotSpawn.rotation;
                            shot1.transform.position = shotSpawn.position;
                            shot1.transform.Translate(new Vector3(shotSideOffset, 0, 0));
                            shot1.player = bb.player;
                            shot1.Shoot();

                            muzzle1.transform.position = muzzlePoint.position;
                            muzzle1.transform.rotation = muzzlePoint.rotation;
                            muzzle1.transform.SetParent(muzzlePoint);
                            muzzle1.transform.Translate(new Vector3(shotSideOffset, 0, 0));
                            muzzle1.Play();

                            shot2.transform.rotation = shotSpawn.rotation;
                            shot2.transform.position = shotSpawn.position;
                            shot2.transform.Translate(new Vector3(-shotSideOffset, 0, 0));
                            shot2.player = bb.player;
                            shot2.Shoot();

                            muzzle2.transform.position = muzzlePoint.position;
                            muzzle2.transform.rotation = muzzlePoint.rotation;
                            muzzle2.transform.SetParent(muzzlePoint);
                            muzzle2.transform.Translate(new Vector3(-shotSideOffset, 0, 0));
                            muzzle2.Play();

                            if (shotSideOffset <= minSideOffset || shotSideOffset >= maxSideOffset)
                                sideOffsetVariation *= -1;

                            shotSideOffset += sideOffsetVariation;
                        }
                    }
                }
            }
            else
            {
                bb.player.StartNoShoot();
            }
        }
    }

    public virtual PlayerBaseState TakeDamage(float damage, PlayerBaseState nextStateIfDamaged = null, bool whiteBlink = true)
    {
        if (rsc.debugMng.godMode || bb.isInvulnerable) return null;

        bb.blinkController.BlinkWhiteOnce();

        bb.currentHealth -= damage;

        if (bb.currentHealth <= 0) bb.currentHealth = 0;

        //Send event
        PlayerDamagedEventInfo.eventInfo.damage = damage;
        PlayerDamagedEventInfo.eventInfo.currentHealth = bb.currentHealth;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DAMAGED, PlayerDamagedEventInfo.eventInfo);

        if (bb.currentHealth == 0)
        {           
            return bb.dyingState;
        }
        else
        {
            /*if (triggerDamageAnim)
                return bb.receivingDamageState;*/
            return nextStateIfDamaged;
        }
    }

    public virtual PlayerBaseState TakeDamage(float damage, ChromaColor color, PlayerBaseState nextStateIfDamaged = null, bool whiteBlink = true)
    {
        //Check color if needed

        return TakeDamage(damage, nextStateIfDamaged);
    }

    public virtual PlayerBaseState AttackReceived(float damage, ChromaColor color, Vector3 origin)
    {
        if (rsc.debugMng.godMode || bb.isInvulnerable) return null;

        //Shield will be deployed either while shooting or while aiming
        bool isShieldDeployed = bb.shootPressed || bb.aimPressed;

        bool shouldTakeDamage = true;
        bool shouldRechargeEnergy = false;
        float damageRatio = 1f;

        if (isShieldDeployed)
        {
            Vector3 forward = bb.player.transform.forward;
            forward.y = 0;
            Vector3 playerEnemy = origin - bb.player.transform.position;
            playerEnemy.y = 0;

            float angle = Vector3.Angle(forward, playerEnemy);

            //Debug.Log("Angle: " + angle + " // Attack color: " + ChromaColorInfo.GetColorName(color) + " // Current color: " + ChromaColorInfo.GetColorName(blackboard.currentColor));

            if (angle < bb.player.maxAngleToShieldBlocking)
            {
                if (color == bb.currentColor)
                {
                    shouldTakeDamage = false;
                    shouldRechargeEnergy = true;
                }
                else
                {
                    damageRatio = bb.player.damageRatioWhenBlockedWrongColor;
                }
            }
        }

        if (shouldRechargeEnergy)
        {
            bb.player.PlayAttackBlocked();
            bb.player.RechargeEnergy(bb.player.energyIncreaseWhenBlockedCorrectColor);
        }

        if (shouldTakeDamage)
        {
            PlayerBaseState result = TakeDamage((damage * damageRatio), color, bb.receivingDamageState, false);
            if (bb.isAffectedByContact || bb.isContactCooldown)
            {
                bb.player.StopCoroutine(HandleEnemyTouched());
                bb.isAffectedByContact = false;
                bb.isContactCooldown = false;
            }
            bb.player.StartCoroutine(HandleInvulnerabilityTime());
            return result;
        }

        return null;
    }

    public virtual PlayerBaseState InfectionReceived(float damage, Vector3 origin, Vector2 infectionForces)
    {
        if (rsc.debugMng.godMode || bb.isInvulnerable) return null;

        bb.infectionOrigin = origin;
        bb.infectionForces = infectionForces;

        PlayerBaseState result = TakeDamage(damage, bb.pushedState, true);
       
        bb.player.StartCoroutine(HandleInvulnerabilityTime());
        return result;
    }

    public virtual PlayerBaseState InfectionReceived(float damage)
    {
        if (rsc.debugMng.godMode || bb.isInvulnerable) return null;

        PlayerBaseState result = TakeDamage(damage, bb.receivingDamageState, true);

        bb.player.StartCoroutine(HandleInvulnerabilityTime());
        return result;
    }

    public virtual PlayerBaseState EnemyContactOnInvulnerabilityEnd()
    {
        bb.contactFlag = false;

        if (rsc.debugMng.godMode || bb.isInvulnerable) return null;

        PlayerBaseState result = TakeDamage((bb.player.damageAfterInvulnerability), bb.receivingDamageState, false);
        if (bb.isAffectedByContact || bb.isContactCooldown)
        {
            bb.player.StopCoroutine(HandleEnemyTouched());
            bb.isAffectedByContact = false;
            bb.isContactCooldown = false;
        }
        bb.player.StartCoroutine(HandleInvulnerabilityTime());

        return result;
    }

    public void StartInvulnerabilityTime()
    {
        bb.player.StartCoroutine(HandleInvulnerabilityTime());
    }

    private IEnumerator HandleInvulnerabilityTime()
    {
        bb.blinkController.BlinkTransparentMultipleTimes(bb.player.invulnerabilityTimeAfterHit);

        bb.isInvulnerable = true;
        Physics.IgnoreLayerCollision(bb.playerPhysicsLayer, bb.enemyPhysicsPlayer, true);

        yield return new WaitForSeconds(bb.player.invulnerabilityTimeAfterHit);

        bb.isInvulnerable = false;

        //If player are colliding with enemies when finishing invulnerability time, not reactivate collitions and set contact flat
        if (bb.enemiesInRange.Count > 0)
            bb.contactFlag = true;
        else
            Physics.IgnoreLayerCollision(bb.playerPhysicsLayer, bb.enemyPhysicsPlayer, false);
    }


    public virtual PlayerBaseState EnemyTouched()
    {
        if (rsc.debugMng.godMode) return null;

        //If touched by an enemy, speed reduction and damage take
        if (!bb.isAffectedByContact && !bb.isContactCooldown && !bb.isInvulnerable)
        {
            //Debug.Log("Enemy touched!");
            bb.player.StartCoroutine(HandleEnemyTouched());
            return TakeDamage(bb.player.damageOnContact);
        }

        return null;
    }

    private IEnumerator HandleEnemyTouched()
    {
        bb.isAffectedByContact = true;

        yield return new WaitForSeconds(bb.player.speedReductionTimeOnContact);

        bb.isAffectedByContact = false;
        bb.isContactCooldown = true;

        yield return new WaitForSeconds(bb.player.cooldownTime);

        bb.isContactCooldown = false;
    }


    public virtual PlayerBaseState ColorMismatch()
    {
        if (rsc.debugMng.godMode) return null;

        PlayerEventInfo.eventInfo.player = bb.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_COLOR_MISMATCH, PlayerEventInfo.eventInfo);

        if (bb.player.fireSuppresionTimeOnColorMismatch > 0f)
        {
            if (bb.canShoot)
            {
                bb.player.StartCoroutine(ColorMismatchHandle());
            }
        }
        return TakeDamage(bb.player.selfDamageOnColorMismatch);
        
    }

    private IEnumerator ColorMismatchHandle()
    {
        StartColorMismatch();     

        yield return new WaitForSeconds(bb.player.fireSuppresionTimeOnColorMismatch);

        EndColorMismatch();
    }

    protected void StartColorMismatch()
    {
        bb.canShoot = false;
        PlayerEventInfo.eventInfo.player = bb.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_COLOR_MISMATCH_START, PlayerEventInfo.eventInfo);
    }

    protected void EndColorMismatch()
    {
        bb.canShoot = true;
        bb.player.StopNoShoot();
        PlayerEventInfo.eventInfo.player = bb.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_COLOR_MISMATCH_END, PlayerEventInfo.eventInfo);

    }
}

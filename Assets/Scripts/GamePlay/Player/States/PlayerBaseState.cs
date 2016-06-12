using UnityEngine;
using System.Collections;

public class PlayerBaseState
{   
    protected PlayerBlackboard blackboard;

    private int playerRayCastMask;
    private int playerPhysicsLayer;
    private int enemyPhysicsPlayer;

    //Controller mapped attributes
    public string moveHorizontal;
    public string moveVertical;
    public string aimHorizontal;
    public string aimVertical;
    public string fire;
    public string dash;
    public string special;

    //Shoot variables
    private float nextFire;
    private bool isFirstShot = true;
    private const float maxSideOffset = 0.4f;
    private const float minSideOffset = 0.2f;
    private float shotSideOffset = minSideOffset;
    private float sideOffsetVariation = -0.05f;

    private ColoredObjectsManager coloredObjMng;

    public virtual void Init(PlayerBlackboard bb)
    {
        blackboard = bb;

        string playerStr = "";
        switch (blackboard.player.Id)
        {
            case 1: playerStr = "P1"; break;
            case 2: playerStr = "P2"; break;
        }

        moveHorizontal = playerStr + "_Horizontal";
        moveVertical = playerStr + "_Vertical";
        aimHorizontal = playerStr + "_AimHorizontal";
        aimVertical = playerStr + "_AimVertical";
        fire = playerStr + "_Fire";
        dash = playerStr + "_Dash";
        special = playerStr + "_Special";

        playerRayCastMask = LayerMask.GetMask(playerStr + "RayCast");
        playerPhysicsLayer = LayerMask.NameToLayer("Player");
        enemyPhysicsPlayer = LayerMask.NameToLayer("Enemy");

        coloredObjMng = rsc.coloredObjectsMng;
    }

    public virtual void OnStateEnter() { }

    public virtual void OnStateExit() { }

    public virtual PlayerBaseState Update()
    {
        /*actions check list:
        is he grounded?
        is he in a border?
        can he do a special?
        can he do a dash?
        can he turn?
        can he shoot?
        can he move?
        */

        return null;
    }

    private Vector3 GetScreenRelativeDirection(Vector3 direction)
    {
        return rsc.camerasMng.GetDirection(blackboard.player.transform.position, direction, playerRayCastMask);
    }

    protected bool Turn()
    {
        bool result = GetAimingDirectionFromInput();
        UpdateLookAt();

        return result;
    }

    private bool GetAimingDirectionFromInput()
    {
        float h = Input.GetAxisRaw(aimHorizontal);
        float v = Input.GetAxisRaw(aimVertical);

        blackboard.aimingDirection = Vector3.zero;
        bool result = false;

        if (Mathf.Abs(v) >= blackboard.player.aimThreshold || Mathf.Abs(h) >= blackboard.player.aimThreshold)
        {
            blackboard.aimingDirection.x = h;
            blackboard.aimingDirection.y = v;

            blackboard.aimingDirection = GetScreenRelativeDirection(blackboard.aimingDirection);
            blackboard.keyPressed = true;
            blackboard.animator.SetBool("Aiming", true);
            blackboard.aiming = true;
            result = true;
        }

        return result;
    }

    private void UpdateLookAt()
    {
        if (blackboard.aimingDirection != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(blackboard.aimingDirection);
            newRotation = Quaternion.RotateTowards(blackboard.player.transform.rotation, newRotation, blackboard.player.angularSpeed * Time.deltaTime);
            blackboard.player.transform.rotation = newRotation;
        }
    }

    protected bool Move()
    {
        bool result = GetHorizontalDirectionFromInput();
        UpdateMovingAnimatorFlags();

        return result;
    }
    protected bool GetHorizontalDirectionFromInput()
    {
        float h = Input.GetAxisRaw(moveHorizontal);
        float v = Input.GetAxisRaw(moveVertical);

        blackboard.horizontalDirection = Vector3.zero;
        bool result = false;

        if (Mathf.Abs(v) > 0 || Mathf.Abs(h) > 0)
        {
            blackboard.horizontalDirection.x = h;
            blackboard.horizontalDirection.y = v;

            blackboard.horizontalDirection = GetScreenRelativeDirection(blackboard.horizontalDirection);
            blackboard.keyPressed = true;
            result = true;
        }

        return result;
    }

    private void UpdateMovingAnimatorFlags()
    {
        if (blackboard.horizontalDirection != Vector3.zero)
        {
            //If we are not aiming, rotate towards direction
            if (blackboard.aimingDirection == Vector3.zero)
            {
                Quaternion newRotation = Quaternion.LookRotation(blackboard.horizontalDirection);
                newRotation = Quaternion.RotateTowards(blackboard.player.transform.rotation, newRotation, blackboard.player.angularSpeed * Time.deltaTime);
                blackboard.player.transform.rotation = newRotation;
                //blackboard.animator.SetBool("WalkingAiming", false);               
            }
            else
            {
                int angleBetweenSticks = AngleBetween360(blackboard.aimingDirection, blackboard.horizontalDirection);

                float angleRad = angleBetweenSticks * Mathf.Deg2Rad;
                float forward = Mathf.Cos(angleRad);
                float lateral = Mathf.Sin(angleRad);
                blackboard.animator.SetFloat("Forward", forward);
                blackboard.animator.SetFloat("Lateral", lateral);

                blackboard.animator.SetBool("WalkingAiming", true);
                blackboard.walkingAiming = true;

                //Debug.Log("Moving: " + horizontalDirection + " // Aiming: " + aimingDirection);
                //Debug.Log("Angle: " + angleBetweenSticks + " // Forward: " + forward + " // Lateral: " + lateral);
            }
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

    protected bool DashPressed()
    {
        bool result = Input.GetButtonDown(dash);

        if (result)
            blackboard.keyPressed = true;

        return result;
    }

    protected bool SpecialPressed()
    {
        bool result = Input.GetButtonDown(special);

        if (result)
            blackboard.keyPressed = true;

        return result &&
            ((blackboard.currentEnergy >= blackboard.player.specialAttackNecessaryEnergy) ||
            rsc.debugMng.godMode);
    }

    protected bool Shoot()
    {
        bool shooting = false;

        if (Input.GetAxisRaw(fire) > 0.1f)
        {
            shooting = true;
            blackboard.keyPressed = true;

            if (blackboard.canShoot)
            {
                if (Time.time > nextFire)
                {
                    nextFire = Time.time + blackboard.player.fireRate;

                    // check if it's first shot (single projectile)...
                    if (isFirstShot)
                    {
                        //Get a shot from pool
                        PlayerShotController shot = coloredObjMng.GetPlayer1Shot();
                        MuzzleController muzzle = coloredObjMng.GetPlayer1Muzzle();

                        if (shot != null && muzzle != null)
                        {
                            Transform shotSpawn = blackboard.player.shotSpawn;
                            shot.transform.position = shotSpawn.position;
                            shot.transform.rotation = shotSpawn.rotation;
                            shot.damage *= 2;
                            shot.Player = blackboard.player;
                            shot.Shoot();

                            Transform muzzlePoint = blackboard.player.muzzlePoint;
                            muzzle.transform.SetParent(muzzlePoint);
                            muzzle.transform.position = muzzlePoint.position;
                            muzzle.transform.rotation = muzzlePoint.rotation;
                            muzzle.Play();
                        }
                        isFirstShot = false;
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
                            Transform shotSpawn = blackboard.player.shotSpawn;
                            Transform muzzlePoint = blackboard.player.muzzlePoint;

                            shot1.transform.rotation = shotSpawn.rotation;
                            shot1.transform.position = shotSpawn.position;
                            shot1.transform.Translate(new Vector3(shotSideOffset, 0, 0));
                            shot1.Player = blackboard.player;
                            shot1.Shoot();

                            muzzle1.transform.position = muzzlePoint.position;
                            muzzle1.transform.rotation = muzzlePoint.rotation;
                            muzzle1.transform.SetParent(muzzlePoint);
                            muzzle1.transform.Translate(new Vector3(shotSideOffset, 0, 0));
                            muzzle1.Play();

                            shot2.transform.rotation = shotSpawn.rotation;
                            shot2.transform.position = shotSpawn.position;
                            shot2.transform.Translate(new Vector3(-shotSideOffset, 0, 0));
                            shot2.Player = blackboard.player;
                            shot2.Shoot();

                            muzzle2.transform.position = muzzlePoint.position;
                            muzzle2.transform.rotation = muzzlePoint.rotation;
                            muzzle2.transform.SetParent(muzzlePoint);
                            muzzle2.transform.Translate(new Vector3(-shotSideOffset, 0, 0));
                            muzzle2.Play();

                            if (shotSideOffset <= minSideOffset || shotSideOffset >= maxSideOffset)
                                sideOffsetVariation *= -1;

                            shotSideOffset += sideOffsetVariation;

                            blackboard.doubleShooting = true;
                        }
                    }
                }
            }
        }
        else
        {
            isFirstShot = true;
        }

        blackboard.newShootingStatus = shooting;

        return shooting;
    }

    public virtual PlayerBaseState TakeDamage(float damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        if (rsc.debugMng.godMode || blackboard.isInvulnerable) return null;

        blackboard.blinkController.BlinkWhiteOnce();

        blackboard.currentHealth -= damage;

        if (blackboard.currentHealth <= 0) blackboard.currentHealth = 0;

        //Send event
        PlayerDamagedEventInfo.eventInfo.damage = damage;
        PlayerDamagedEventInfo.eventInfo.currentHealth = blackboard.currentHealth;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_DAMAGED, PlayerDamagedEventInfo.eventInfo);

        if (blackboard.currentHealth == 0)
        {           
            return blackboard.dyingState;
        }
        else
        {
            if (triggerDamageAnim)
                return blackboard.receivingDamageState;
        }

        return null;
    }

    public virtual PlayerBaseState TakeDamage(float damage, ChromaColor color, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        //Check color if needed

        return TakeDamage(damage, triggerDamageAnim);
    }

    public virtual PlayerBaseState AttackReceived(float damage, ChromaColor color, Vector3 origin)
    {
        if (rsc.debugMng.godMode || blackboard.isInvulnerable) return null;

        //Shield will be deployed either while shooting or if we are moving and aiming at the same time
        bool isShieldDeployed = blackboard.currentShootingStatus || blackboard.walkingAiming || blackboard.aiming;

        bool shouldTakeDamage = true;
        bool shouldRechargeEnergy = false;
        float damageRatio = 1f;

        if(isShieldDeployed)
        {
            Vector3 forward = blackboard.player.transform.forward;
            forward.y = 0;
            Vector3 playerEnemy = origin - blackboard.player.transform.position;
            playerEnemy.y = 0;

            float angle = Vector3.Angle(forward, playerEnemy);

            if(angle < blackboard.player.maxAngleToShieldBlocking)
            {
                if(color == blackboard.currentColor)
                {
                    shouldTakeDamage = false;
                    shouldRechargeEnergy = true;
                }
                else
                {
                    damageRatio = blackboard.player.damageRatioWhenBlockedWrongColor;
                }
            }
        }

        if (shouldRechargeEnergy)
        {
            blackboard.player.RechargeEnergy(blackboard.player.energyIncreaseWhenBlockedCorrectColor);
        }

        if (shouldTakeDamage)
        {
            PlayerBaseState result = TakeDamage((int)(damage * damageRatio), color, true, false);
            if (blackboard.isAffectedByContact || blackboard.isContactCooldown)
            {
                blackboard.player.StopCoroutine(HandleEnemyTouched());
                blackboard.isAffectedByContact = false;
                blackboard.isContactCooldown = false;
            }
            blackboard.player.StartCoroutine(HandleAttackReceived());
            return result;
        }

        return null;
    }

    private IEnumerator HandleAttackReceived()
    {
        blackboard.blinkController.BlinkTransparentMultipleTimes(blackboard.player.invulnerabilityTimeAfterHit);

        blackboard.isInvulnerable = true;
        Physics.IgnoreLayerCollision(playerPhysicsLayer, enemyPhysicsPlayer, true);

        yield return new WaitForSeconds(blackboard.player.invulnerabilityTimeAfterHit);

        Physics.IgnoreLayerCollision(playerPhysicsLayer, enemyPhysicsPlayer, false);
        blackboard.isInvulnerable = false;
    }


    public virtual void EnemyTouched()
    {
        if (rsc.debugMng.godMode) return;

        //If touched by an enemy, speed reduction and damage take
        if (!blackboard.isAffectedByContact && !blackboard.isContactCooldown && !blackboard.isInvulnerable)
        {
            TakeDamage(blackboard.player.damageOnContact, false);
            blackboard.player.StartCoroutine(HandleEnemyTouched());
        }
    }

    private IEnumerator HandleEnemyTouched()
    {
        blackboard.isAffectedByContact = true;
        Debug.Log("Is slowdown because enemy contact");

        yield return new WaitForSeconds(blackboard.player.speedReductionTimeOnContact);

        blackboard.isAffectedByContact = false;
        blackboard.isContactCooldown = true;
        Debug.Log("Is slowdown cooldown");

        yield return new WaitForSeconds(blackboard.player.cooldownTime);

        blackboard.isContactCooldown = false;
        Debug.Log("Slowdown cooldown finished");
    }


    public virtual void ColorMismatch()
    {
        if (rsc.debugMng.godMode) return;

        if (blackboard.canShoot)
        {
            blackboard.player.StartCoroutine(ColorMismatchHandle());
        }
    }

    private IEnumerator ColorMismatchHandle()
    {
        StartColorMismatch();

        TakeDamage(blackboard.player.selfDamageOnColorMismatch, false);

        yield return new WaitForSeconds(blackboard.player.fireSuppresionTimeOnColorMismatch);

        EndColorMismatch();
    }

    protected void StartColorMismatch()
    {
        blackboard.canShoot = false;
        PlayerEventInfo.eventInfo.player = blackboard.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_COLOR_MISMATCH_START, PlayerEventInfo.eventInfo);
    }

    protected void EndColorMismatch()
    {
        blackboard.canShoot = true;
        PlayerEventInfo.eventInfo.player = blackboard.player;
        rsc.eventMng.TriggerEvent(EventManager.EventType.PLAYER_COLOR_MISMATCH_END, PlayerEventInfo.eventInfo);

    }
}

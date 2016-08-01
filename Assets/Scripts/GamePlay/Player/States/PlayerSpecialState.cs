using UnityEngine;
using System.Collections;

public class PlayerSpecialState : PlayerBaseState {

    private const float EXPLODING_WAIT_TIME = 0.5f;
    private enum State
    {
        MOVING,
        EXPLODING
    }

    private Vector3 currentPos;
    private State state;
    //private float elapsedTime;

    public override void OnStateEnter()
    {
        bb.horizontalDirection = Vector3.zero;
        bb.animationTrigger = false;
        bb.animationEnded = false;
        bb.animator.SetTrigger("SpecialAttack");

        bb.specialAttackDetector.SetActive(true);

        if(!rsc.debugMng.godMode)
            bb.player.SpendEnergy(bb.player.specialAttackNecessaryEnergy);
        bb.player.StartSpecialEnergyCharging();
        state = State.MOVING;
        //elapsedTime = 0f;       
    }

    public override PlayerBaseState Update()
    {
        switch (state)
        {
            case State.MOVING:
                if (bb.animationTrigger)
                {
                    bb.animationTrigger = false;
                    bb.player.StopSpecialEnergyCharging();
                    bb.player.StartSpecial();

                    currentPos = bb.player.transform.position;

                    bb.player.StartCoroutine(SpecialExplosion());

                    state = State.EXPLODING;
                    rsc.rumbleMng.Rumble(bb.player.Id, 1f, 0.25f, 1f);
                }

                return null;

            case State.EXPLODING:
                if (bb.animationEnded)
                {
                    return bb.idleState;
                }
                else
                    return null;
        }

        return null;
    }

    public override PlayerBaseState TakeDamage(float damage, bool triggerDamageAnim = true, bool whiteBlink = true)
    {
        //can not take damage during this state
        return null;
    }

    public override PlayerBaseState AttackReceived(float damage, ChromaColor color, Vector3 origin)
    {
        //can not take damage during this state
        return null;
    }

    public override PlayerBaseState EnemyTouched()
    {
        //can not take damage during this state
        return null;
    }

    public override PlayerBaseState ColorMismatch()
    {
        //can not take damage during this state
        return null;
    }


    //TODO: FPS drop when lots of enemies exploding at once
    private IEnumerator SpecialExplosion()
    {
        yield return new WaitForSeconds(0.1f);
        DamageEnemies();
        bb.specialAttackDetector.SetActive(false);
    }

    private void DamageEnemies()
    {
        float damage = bb.player.specialAttackDamage;
        float baseForceMultiplier = bb.player.forceMultiplierAtDistance1;
        float forceAttenuation = bb.player.forceAttenuationByDistance;

        foreach (EnemyBaseAIBehaviour enemy in bb.enemiesInRange)
        {
            Vector3 direction = enemy.transform.position - currentPos;
            float distance = Vector3.Distance(enemy.transform.position, currentPos);
            if (distance < 1f) distance = 1f;
            direction.y = 0;
            direction.Normalize();
            float totalForceMultiplier = baseForceMultiplier / Mathf.Pow(forceAttenuation, distance - 1);
            direction *= totalForceMultiplier;
            enemy.ImpactedBySpecial(damage, direction, bb.player);
        }

        foreach (EnemyShotControllerBase shot in bb.shotsInRange)
        {
            shot.Impact();
        }
    }
}

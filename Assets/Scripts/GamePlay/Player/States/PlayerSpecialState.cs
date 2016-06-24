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
    private float elapsedTime;

    public override void OnStateEnter()
    {
        blackboard.horizontalDirection = Vector3.zero;
        blackboard.animationEnded = false;
        blackboard.animator.SetTrigger("SpecialAttack");

        blackboard.specialAttackCollider.enabled = true;

        if(!rsc.debugMng.godMode)
            blackboard.player.SpendEnergy(blackboard.player.specialAttackNecessaryEnergy);
        blackboard.player.StartSpecialEnergyCharging();
        state = State.MOVING;
        elapsedTime = 0f;       
    }

    public override PlayerBaseState Update()
    {
        switch (state)
        {
            case State.MOVING:
                if (blackboard.animationEnded)
                {
                    blackboard.player.StopSpecialEnergyCharging();
                    blackboard.player.StartSpecial();

                    currentPos = blackboard.player.transform.position;

                    blackboard.player.StartCoroutine(SpecialExplosion());

                    state = State.EXPLODING;
                    rsc.rumbleMng.Rumble(blackboard.player.Id, 1f, 0.25f, 1f);
                }

                return null;

            case State.EXPLODING:
                elapsedTime += Time.deltaTime;

                if (elapsedTime > EXPLODING_WAIT_TIME)
                    return blackboard.idleState;
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
        blackboard.specialAttackCollider.enabled = false;
        blackboard.enemiesInRange.Clear();
    }

    private void DamageEnemies()
    {
        float damage = blackboard.player.specialAttackDamage;
        float baseForceMultiplier = blackboard.player.forceMultiplierAtDistance1;
        float forceAttenuation = blackboard.player.forceAttenuationByDistance;

        foreach (EnemyBaseAIBehaviour enemy in blackboard.enemiesInRange)
        {
            Vector3 direction = enemy.transform.position - currentPos;
            float distance = Vector3.Distance(enemy.transform.position, currentPos);
            if (distance < 1f) distance = 1f;
            direction.y = 0;
            direction.Normalize();
            float totalForceMultiplier = baseForceMultiplier / Mathf.Pow(forceAttenuation, distance - 1);
            direction *= totalForceMultiplier;
            enemy.ImpactedBySpecial(damage, direction);
        }
    }
}

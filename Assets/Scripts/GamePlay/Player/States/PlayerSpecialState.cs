using UnityEngine;
using System.Collections;

public class PlayerSpecialState : PlayerBaseState {

    private Vector3 currentPos;

    public override void OnStateEnter()
    {
        blackboard.horizontalDirection = Vector3.zero;
        blackboard.animationEnded = false;
        blackboard.animator.SetTrigger("SpecialAttack");

        blackboard.specialAttackCollider.enabled = true;

        if(!rsc.debugMng.godMode)
            blackboard.player.SpendEnergy(blackboard.player.specialAttackNecessaryEnergy);
        blackboard.player.StartSpecialEnergyCharging();
    }

    public override PlayerBaseState Update()
    {
        if(blackboard.animationEnded)
        {
            blackboard.player.StopSpecialEnergyCharging();
            //play explosion

            currentPos = blackboard.player.transform.position;

            blackboard.player.StartCoroutine(SpecialExplosion());

            return blackboard.idleState;
        }
        else
        {
            return null;
        }
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

    public override void EnemyTouched()
    {
        //can not take damage during this state
        return;
    }

    public override void ColorMismatch()
    {
        //can not take damage during this state
        return;
    }

    private IEnumerator SpecialExplosion()
    {
        //yield return new WaitForSeconds(0.1f);
        DamageEnemies();
        blackboard.specialAttackCollider.enabled = false;
        blackboard.enemiesInRange.Clear();
        yield return null;
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

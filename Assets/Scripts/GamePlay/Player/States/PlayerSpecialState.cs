using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSpecialState : PlayerBaseState {

    private enum State
    {
        MOVING,
        EXPLODING
    }

    private Vector3 currentPos;
    private SortedList<float, EnemyBaseAIBehaviour> enemies = new SortedList<float, EnemyBaseAIBehaviour>();
    private State state;
    //private float elapsedTime;

    public override void OnStateEnter()
    {
        bb.horizontalDirection = Vector3.zero;
        bb.animationTrigger = false;
        bb.animationEnded = false;
        bb.animator.SetTrigger("SpecialAttack");

        bb.specialAttackDetector.SetActive(true);
        enemies.Clear();

        if(!rsc.debugMng.godMode)
            bb.player.SpendEnergy(bb.player.specialAttackNecessaryEnergy);
        bb.player.StartSpecialEnergyCharging();
        state = State.MOVING;
        //elapsedTime = 0f;       
    }

    public override void OnStateExit()
    {
        bb.specialAttackDetector.SetActive(false);
        bb.player.StopSpecialEnergyCharging();
    }

    public override PlayerBaseState Update()
    {
        switch (state)
        {
            case State.MOVING:
                if(ShouldFall())
                {
                    return bb.fallingState;
                }

                Turn();

                Move();

                if (bb.animationTrigger)
                {
                    bb.horizontalDirection = Vector3.zero;
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

    //TODO: FPS drop when lots of enemies exploding at once
    private IEnumerator SpecialExplosion()
    {
        //Add enemies to sorted list
        foreach(EnemyBaseAIBehaviour enemy in bb.enemiesInRange)
        {
            float distance = Vector3.Distance(enemy.transform.position, currentPos);
            enemies.Add(distance, enemy);
        }
        bb.specialAttackDetector.SetActive(false);

        yield return new WaitForSeconds(0.1f);


        float damage = bb.player.specialAttackDamage;
        float baseForceMultiplier = bb.player.forceMultiplierAtDistance1;
        float forceAttenuation = bb.player.forceAttenuationByDistance;
        float elapsedTime = 0f;
        int i = 0;

        foreach (EnemyShotControllerBase shot in bb.shotsInRange)
        {
            shot.Impact();
        }

        if (bb.worm != null)
        {
            bb.worm.ImpactedBySpecial(damage, bb.player);
        }

        while (i < enemies.Count)
        {
            float currentDistance = bb.player.specialAttackAffectationRadius / elapsedTime;

            float enemyDistance = enemies.Keys[i];
            EnemyBaseAIBehaviour enemy = enemies.Values[i];

            while (enemyDistance <= currentDistance && i < enemies.Count)
            {
                Vector3 direction = enemy.transform.position - currentPos;
                if (enemyDistance < 1f) enemyDistance = 1f;
                direction.y = 0;
                direction.Normalize();
                float totalForceMultiplier = baseForceMultiplier / Mathf.Pow(forceAttenuation, enemyDistance - 1);
                direction *= totalForceMultiplier;
                enemy.ImpactedBySpecial(damage, direction, bb.player);

                ++i;
                if(i < enemies.Count)
                {
                    enemyDistance = enemies.Keys[i];
                    enemy = enemies.Values[i];
                }
            }

            yield return null;
            elapsedTime += Time.deltaTime;
        }      

       // DamageEnemies();
    }

    private void DamageEnemies()
    {
        float damage = bb.player.specialAttackDamage;
        float baseForceMultiplier = bb.player.forceMultiplierAtDistance1;
        float forceAttenuation = bb.player.forceAttenuationByDistance;

        foreach (EnemyBaseAIBehaviour enemy in enemies.Values)
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

        if (bb.worm != null)
        {
            bb.worm.ImpactedBySpecial(damage, bb.player);
        }
    }

    public override PlayerBaseState TakeDamage(float damage, PlayerBaseState nextStateIfDamaged = null, bool whiteBlink = true)
    {
        //can not take damage during this state
        return null;
    }

    public override PlayerBaseState AttackReceived(float damage, ChromaColor color, Vector3 origin)
    {
        //can not take damage during this state
        return null;
    }

    public override PlayerBaseState InfectionReceived(float damage, Vector3 origin, Vector2 infectionForces)
    {
        //can not take damage during this state
        return null;
    }

    public override PlayerBaseState InfectionReceived(float damage)
    {
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
}

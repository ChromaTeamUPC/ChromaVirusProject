using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BarrelController : MonoBehaviour {

    private enum State
    {
        NOT_STARTED,
        IDLE,
        CHARGED,
        EXPLODING
    }

    public int maxCharge = 100;
    public int chargePerShot = 10;
    public float timeToExplode = 5f;
    public int damage = 50;

    public SphereCollider attractingCollider;
    public float attractingColliderRange;
    public SphereCollider damageCollider;
    public float damageColliderRange;

    private State state;

    [HideInInspector]
    public ChromaColor currentColor;
    private int currentCharge;
    private float elapsedTime;

    private HashSet<EnemyBaseAIBehaviour> enemiesInRange;

    void Awake()
    {
        attractingCollider.radius = attractingColliderRange;
        damageCollider.radius = damageColliderRange;

        state = State.NOT_STARTED;
        currentCharge = 0;
        elapsedTime = 0f;

        enemiesInRange = new HashSet<EnemyBaseAIBehaviour>();
    }

    void Update()
    {
        switch (state)
        {
            case State.IDLE:
                if(currentCharge == maxCharge)
                {
                    attractingCollider.enabled = true;
                    damageCollider.enabled = true;
                    state = State.CHARGED;
                    //TODO: start "warning" animation
                    Debug.Log("Moving to Charged state");
                }
                break;
            case State.CHARGED:
                elapsedTime += Time.deltaTime;
                if(elapsedTime >= timeToExplode)
                {
                    state = State.EXPLODING;
                    //TODO: start explosion animation
                    Debug.Log("Moving to Exploding state");
                    StartCoroutine(Exploding());
                }
                break;
            case State.EXPLODING:
                DestroyObject(gameObject, 3f); //TODO: Adjust time to allow exploding animation to finish;
                break;
            default:
                break;
        }
    }

    public void ImpactedByShot(ChromaColor shotColor)
    {
        switch (state)
        {
            case State.NOT_STARTED:
                state = State.IDLE;               
                currentCharge += chargePerShot;
                currentColor = shotColor;
                Debug.Log("Received first shot.");
                Debug.Log("Current Color = " + ChromaColorInfo.GetColorName(currentColor) + ". Current charge = " + currentCharge);
                Debug.Log("Moving to Idle state");
                break;

            case State.IDLE:
                if (currentColor == shotColor)
                {
                    if (currentCharge < maxCharge)
                    {
                        currentCharge += chargePerShot;
                        if (currentCharge > maxCharge)
                            currentCharge = maxCharge;
                    }
                }
                else
                {
                    currentCharge = 0;
                    currentColor = shotColor;
                }
                Debug.Log("Current Color = " + ChromaColorInfo.GetColorName(currentColor) + ". Current charge = " + currentCharge);
                break;
        }
    }

    public void EnemyInRange(EnemyBaseAIBehaviour enemy)
    {
        enemiesInRange.Add(enemy);
    }

    public void EnemyOutOfRange(EnemyBaseAIBehaviour enemy)
    {
        enemiesInRange.Remove(enemy);
    }

    private IEnumerator Exploding()
    {
        yield return new WaitForSeconds(0.5f);
        DamageEnemies();
    }

    private void DamageEnemies()
    {
        foreach(EnemyBaseAIBehaviour enemy in enemiesInRange)
        {
            enemy.ImpactedByBarrel(currentColor, damage);
        }

        attractingCollider.enabled = false;
        damageCollider.enabled = false;
    }
}

using UnityEngine;
using System.Collections;

public class PlayerSpecialAttackDetector : MonoBehaviour 
{
    private PlayerBlackboard blackboard = null;

    public PlayerBlackboard Blackboard { set { blackboard = value; } }

    void OnEnable()
    {
        if(blackboard != null)
            blackboard.enemiesInRange.Clear();
    }

    void OnDisable()
    {
        if (blackboard != null)
            blackboard.enemiesInRange.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            EnemyBaseAIBehaviour enemy = other.GetComponent<EnemyBaseAIBehaviour>();

            //Mosquito has the collider in a children object so we need to search for script in parent
            if (enemy == null)
                enemy = other.GetComponentInParent<EnemyBaseAIBehaviour>();

            if (enemy != null)
                blackboard.enemiesInRange.Add(enemy);
        }       
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            EnemyBaseAIBehaviour enemy = other.GetComponent<EnemyBaseAIBehaviour>();

            //Mosquito has the collider in a children object so we need to search for script in parent
            if (enemy == null)
                enemy = other.GetComponentInParent<EnemyBaseAIBehaviour>();

            if (enemy != null)
                blackboard.enemiesInRange.Remove(enemy);
        }
    }
}

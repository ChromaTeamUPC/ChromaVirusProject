using UnityEngine;
using System.Collections;

public class PlayerSpecialAttackDetector : MonoBehaviour 
{
    private PlayerBlackboard bb = null;

    public PlayerBlackboard Blackboard { set { bb = value; } }

    void OnEnable()
    {
        if (bb != null)
        {
            bb.worm = null;
            bb.enemiesInRange.Clear();
            bb.shotsInRange.Clear();
        }
    }

    void OnDisable()
    {
        if (bb != null)
        {
            bb.worm = null;
            bb.enemiesInRange.Clear();
            bb.shotsInRange.Clear();
        }
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
                bb.enemiesInRange.Add(enemy);
        }  
        else if (other.tag == "Shot")
        {
            EnemyShotControllerBase shot = other.GetComponent<EnemyShotControllerBase>();

            if (shot != null)
                bb.shotsInRange.Add(shot);
        } 
        else if (other.tag == "WormHead")
        {
            WormAIBehaviour worm = other.GetComponent<WormAIBehaviour>();

            if (worm != null)
                bb.worm = worm;
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
                bb.enemiesInRange.Remove(enemy);
        }
        else if (other.tag == "Shot")
        {
            EnemyShotControllerBase shot = other.GetComponent<EnemyShotControllerBase>();

            if (shot != null)
                bb.shotsInRange.Remove(shot);
        }
        else if (other.tag == "WormHead")
        {
            WormAIBehaviour worm = other.GetComponent<WormAIBehaviour>();

            if (worm != null)
                bb.worm = null;
        }
    }
}

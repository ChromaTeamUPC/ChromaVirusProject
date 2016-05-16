using UnityEngine;
using System.Collections;

public class BarrelDamage : MonoBehaviour {

    public BarrelController controller;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {
            controller.EnemyInRange(other.GetComponent<EnemyBaseAIBehaviour>());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            controller.EnemyOutOfRange(other.GetComponent<EnemyBaseAIBehaviour>());
        }
    }
}

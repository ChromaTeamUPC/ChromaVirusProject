using UnityEngine;
using System.Collections;

public class DyingCollider : MonoBehaviour {

    private EnemyBaseAIBehaviour agent;

    void Awake()
    {
        agent = GetComponentInParent<EnemyBaseAIBehaviour>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Stage" || other.tag == "Hexagon")
        {
            agent.CollitionOnDie();
        }
    }
}

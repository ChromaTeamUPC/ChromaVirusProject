using UnityEngine;
using System.Collections;

public class SpiderAttack : MonoBehaviour {

    private SpiderAIBehaviour spider;
	// Use this for initialization
	void Start () {
        spider = GetComponentInParent<SpiderAIBehaviour>();
	}
	
	void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player1" || other.tag == "Player2")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.TakeDamage(spider.biteDamage, spider.color);
        }
    }
}

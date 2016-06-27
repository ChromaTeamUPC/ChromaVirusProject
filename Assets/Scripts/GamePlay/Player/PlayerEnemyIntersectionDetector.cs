using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerEnemyIntersectionDetector : MonoBehaviour 
{
    private List<GameObject> enemies = new List<GameObject>();

    public bool IsIntersectingEnemies { get { return enemies.Count > 0; } }

	void OnTriggerEnter(Collider other)
    {
        enemies.Add(other.gameObject);
        Debug.Log("New enemy added. Total: " + enemies.Count);
    }

    void OnTriggerExit(Collider other)
    {
        enemies.Remove(other.gameObject);
        Debug.Log("Enemy removed. Total: " + enemies.Count);
    }
}

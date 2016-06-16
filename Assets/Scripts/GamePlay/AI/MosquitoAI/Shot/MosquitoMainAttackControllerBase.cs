using UnityEngine;
using System.Collections;

public class MosquitoMainAttackControllerBase : MonoBehaviour {

    protected Transform source;
    protected PlayerController player;

    public GameObject projectilePrefab;

    public int damage = 10;
    public int speed = 20;
    public float forceMultiplier = 5f;

    protected bool active;

    public virtual void Shoot(Transform s, PlayerController p)
    {
        source = s;
        player = p;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    void OnDisable()
    {
        source = null;
        player = null;
    }
}

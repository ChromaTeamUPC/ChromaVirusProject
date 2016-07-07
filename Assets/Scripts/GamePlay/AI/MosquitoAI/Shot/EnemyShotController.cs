using UnityEngine;
using System.Collections;

public class EnemyShotController : EnemyShotControllerBase
{
    public bool homing = false;
    public float maxHomingDuration = 0f;
    public float torque = 5f;
    public Transform target;

    private float homingDuration;

    // Use this for initialization
    public override void Shoot()
    {
        base.Shoot();
        homingDuration = 0f;
    }

    void FixedUpdate()
    {
        if(Active && homing && shotCollider.enabled && target != null)
        {
            homingDuration += Time.fixedDeltaTime; 
            if(homingDuration <= maxHomingDuration)
            {
                rigidBody.velocity = transform.forward * speed;

                Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
                rigidBody.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, torque));
            }
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
        target = null;      
    }
}

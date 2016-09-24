using UnityEngine;
using System.Collections;

public class LifeChipController : MonoBehaviour 
{
    public bool extraLife = true;
    public bool heal = true;
    private bool animationEnded;
    private Animator anim;
    public ParticleSystem fx;
    private SphereCollider sphereCollider;
    private PlayerController player;
    private AudioSource audioSource;
    private float attractionSpeed = 6;


    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        animationEnded = false;
        player = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player1" || other.tag == "Player2")
        {
            sphereCollider.enabled = false;

            player = other.GetComponent<PlayerController>();
            player.RechargeLife(extraLife, heal);

            audioSource.Play();

            anim.SetTrigger("Fade");
            fx.Stop();
        }
    }

    void Update()
    {
        if(player != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, Time.deltaTime * attractionSpeed);
        }

        if(animationEnded && fx.isStopped && !audioSource.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    public void AnimationEnded()
    {
        animationEnded = true;
    }
}

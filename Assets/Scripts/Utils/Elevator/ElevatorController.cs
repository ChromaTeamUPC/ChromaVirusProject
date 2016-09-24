using UnityEngine;
using System.Collections;

public class ElevatorController : MonoBehaviour 
{
    public GameObject colliders;
    private Animator animator;
    private AudioSource audioSource;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        colliders.SetActive(true);
    }

    public void Activate()
    {
        animator.SetFloat("AnimationSpeed", 1f);
        audioSource.Play();
    }

    public void Deactivate()
    {
        colliders.SetActive(false);
    }
}

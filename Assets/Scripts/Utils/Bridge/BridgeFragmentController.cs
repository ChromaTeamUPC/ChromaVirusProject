using UnityEngine;
using System.Collections;

public class BridgeFragmentController : MonoBehaviour 
{
    private Animator animator;
    [SerializeField]
    private AudioSource enterSoundFx;
    [SerializeField]
    private AudioSource exitSoundFx;

    private BoxCollider col;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        col = GetComponentInChildren<BoxCollider>();
    }
     
	public void Enter()
    {
        col.enabled = true;
        animator.SetTrigger("EnterNow");
        enterSoundFx.Play();
    }

    public void Wait()
    {
        animator.SetTrigger("WaitNow");
    }

    public void Exit()
    {
        col.enabled = false;
        animator.SetTrigger("ExitNow");
        exitSoundFx.Play();
    }
}

using UnityEngine;
using System.Collections;

public class BridgeFragmentController : MonoBehaviour 
{
    private Animator animator;
    [SerializeField]
    private AudioSource enterSoundFx;
    [SerializeField]
    private AudioSource exitSoundFx;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }
     
	public void Enter()
    {
        animator.SetTrigger("EnterNow");
        enterSoundFx.Play();
    }

    public void Wait()
    {
        animator.SetTrigger("WaitNow");
    }

    public void Exit()
    {
        animator.SetTrigger("ExitNow");
        exitSoundFx.Play();
    }
}

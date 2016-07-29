using UnityEngine;
using System.Collections;
using InControl;

public class EntryCameraController : MonoBehaviour 
{
    public bool skippeable = true;
    private bool skipped = false;
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        CutSceneEventInfo.eventInfo.skippeable = skippeable;
        rsc.eventMng.TriggerEvent(EventManager.EventType.START_CUT_SCENE, CutSceneEventInfo.eventInfo);
    }

    public void SetLevelAnimation(int levelAnimation)
    {
        anim.SetInteger("LevelAnim", levelAnimation);
    }

	public void AnimationEnded()
    {
        if(!skipped)
            rsc.eventMng.TriggerEvent(EventManager.EventType.CAMERA_ANIMATION_ENDED, EventInfo.emptyInfo);
    }

    void Update()
    {
        if (rsc.gameMng.State == GameManager.GameState.STARTED)
        {        
            if (skippeable && InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action2))
            {
                skipped = true;
                rsc.eventMng.TriggerEvent(EventManager.EventType.CAMERA_ANIMATION_ENDED, EventInfo.emptyInfo);
            }
        }
    }
}

using UnityEngine;
using System.Collections;
using InControl;

public class EntryCameraController : MonoBehaviour 
{
    public bool skippeable = true;
    private bool skipped = false;
    private Animator anim;

    public float defaultShakeMaximum = 0.3f;
    private float shakeDuration;
    private float currentShakeMaximum;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        rsc.eventMng.StartListening(EventManager.EventType.WORM_ATTACK, WormAttack);

        CutSceneEventInfo.eventInfo.skippeable = skippeable;
        rsc.eventMng.TriggerEvent(EventManager.EventType.START_CUT_SCENE, CutSceneEventInfo.eventInfo);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.WORM_ATTACK, WormAttack);
        }
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

    private void WormAttack(EventInfo eventInfo)
    {
        WormEventInfo info = (WormEventInfo)eventInfo;

        shakeDuration = info.wormBb.belowAttackRumbleDuration;
        rsc.rumbleMng.Rumble(0, shakeDuration);
    }

    void LateUpdate()
    {
        if (shakeDuration > 0)
        {
            transform.position = transform.position + Random.insideUnitSphere * (currentShakeMaximum > 0 ? currentShakeMaximum : defaultShakeMaximum);

            shakeDuration -= Time.deltaTime;

            if (shakeDuration <= 0)
            {
                shakeDuration = 0f;
                currentShakeMaximum = 0f;
            }
        }     	
    }
}

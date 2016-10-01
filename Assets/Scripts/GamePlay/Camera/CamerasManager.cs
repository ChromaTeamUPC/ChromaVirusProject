using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct Effects
{
    public const int NONE = 0;
    public const int GLITCH = 1;
    public const int MOTION_BLUR = 2;
    public const int BN = 4;
    public const int NOISE = 8;
}

public enum EffectId
{
    TEST,
    PLAYER_DASH,
    PLAYER_SPECIAL_CHARGE,
    DEVICE_INFECTION
}

public class CamerasManager : MonoBehaviour
{

    [HideInInspector]
    public GameObject currentCameraObj;
    [HideInInspector]
    public Camera currentCamera;

    public float xRotationMinThreshold = 25;

    public GameObject mainCameraObj;
    public GameObject entryCameraObj;
    public GameObject godCameraObj;

    private float camRayLength = 100f;

    //Effects control
    [HideInInspector]
    public bool effectsActive;

    private bool pauseEffects;

    private CameraController mainCameraController;
    private CameraController entryCameraController;
    private CameraController godCameraController;

    public class CameraEffectsInfo
    {
        public float shakeIntensity;
        public int effectSet;

        public void Reset()
        {
            shakeIntensity = 0;
            effectSet = 0;
        }

        public void SetMax(CameraEffectsInfo e1, CameraEffectsInfo e2)
        {
            shakeIntensity = Mathf.Max(e1.shakeIntensity, e2.shakeIntensity);

            effectSet = e1.effectSet | e2.effectSet;
        }

        public void SetMax(EffectInfo e)
        {
            shakeIntensity = e.GetMaxShakeValue(shakeIntensity);

            effectSet |= e.effectSet;
        }

        public void Copy(CameraEffectsInfo e)
        {
            shakeIntensity = e.shakeIntensity;
            effectSet = e.effectSet;
        }
    }

    public class EffectInfo
    {
        public int player = 0;
        public int effectSet = 0;
        public float shakeIntensity = 0f;

        public EffectInfo(int pl, int set, float shake)
        {
            player = pl;
            effectSet = set;
            shakeIntensity = shake;
        }

        public virtual float GetMaxShakeValue(float reference)
        {
            return Mathf.Max(shakeIntensity, reference);
        }
    }

    private class TemporalEffectInfo : EffectInfo
    {
        public float duration = 0f;
        public float startFadingTime = 0f;

        private float powerFactor;

        public TemporalEffectInfo(int pl, int set, float shake, float dur, float fade) : base(pl, set, shake)
        {
            duration = dur;
            startFadingTime = fade;
            if (startFadingTime <= 0)
                startFadingTime = 0.1f;
        }

        public void Update()
        {
            duration -= Time.deltaTime;
            powerFactor = Mathf.Clamp(duration / startFadingTime, 0f, 1f);
        }

        public override float GetMaxShakeValue(float reference)
        {
            return Mathf.Max(shakeIntensity * powerFactor, reference);
        }
    }

    private class ContinousEffectInfo : EffectInfo
    {
        public EffectId id;

        public ContinousEffectInfo(int pl, int set, float shake, EffectId effectId) : base(pl, set, shake)
        {
            id = effectId;
        }
    }

    private List<TemporalEffectInfo> temporalEffectList = new List<TemporalEffectInfo>();
    private Dictionary<string, ContinousEffectInfo> continousEffectList = new Dictionary<string, ContinousEffectInfo>();

    private CameraEffectsInfo p0Effects = new CameraEffectsInfo();
    private CameraEffectsInfo p1Effects = new CameraEffectsInfo();
    private CameraEffectsInfo p2Effects = new CameraEffectsInfo();

    private CameraEffectsInfo currentPlayerEffects;
    private CameraEffectsInfo finalEffects = new CameraEffectsInfo();

    void Awake()
    {
        effectsActive = true;

        currentCameraObj = mainCameraObj;
        currentCamera = currentCameraObj.GetComponent<Camera>();

        mainCameraController = mainCameraObj.GetComponent<CameraController>();
        entryCameraController = entryCameraObj.GetComponent<CameraController>();
        godCameraController = godCameraObj.GetComponent<CameraController>();

        //We are sure rsc is created because we forced script execution order from unity editor - project settings
        rsc.camerasMng = this;
    }

    void Start()
    { 
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESET, ClearAllEffects);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_LOADED, ClearAllEffects);
        rsc.eventMng.StartListening(EventManager.EventType.LEVEL_UNLOADED, ClearAllEffects);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_OPENED, TutorialOpened);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_CLOSED, TutorialClosed);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_PAUSED, GamePaused);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESUMED, GameResumed);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESET, ClearAllEffects);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_LOADED, ClearAllEffects);
            rsc.eventMng.StopListening(EventManager.EventType.LEVEL_UNLOADED, ClearAllEffects);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_OPENED, TutorialOpened);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_CLOSED, TutorialClosed);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_PAUSED, GamePaused);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESUMED, GameResumed);
        }

        rsc.camerasMng = null;
    }

    private void ClearAllEffects(EventInfo eventInfo)
    {
        pauseEffects = false;
        temporalEffectList.Clear();
        continousEffectList.Clear();

        finalEffects.Reset();
        mainCameraController.SetEffects(finalEffects);
        entryCameraController.SetEffects(finalEffects);
        godCameraController.SetEffects(finalEffects);
    }

    private void TutorialOpened(EventInfo eventInfo)
    {
        pauseEffects = true;
    }

    private void TutorialClosed(EventInfo eventInfo)
    {
        pauseEffects = false;
    }

    private void GamePaused(EventInfo eventInfo)
    {
        pauseEffects = true;
    }

    private void GameResumed(EventInfo eventInfo)
    {
        pauseEffects = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (temporalEffectList.Count > 0 || continousEffectList.Count > 0)
        {
            p0Effects.Reset();
            p1Effects.Reset();
            p2Effects.Reset();

            finalEffects.Reset();

            if (!pauseEffects)
            {
                for (int i = temporalEffectList.Count - 1; i >= 0; --i)
                {
                    TemporalEffectInfo effect = temporalEffectList[i];
                    effect.Update();

                    if (effect.duration > 0)
                    {
                        switch (effect.player)
                        {
                            case 0:
                                currentPlayerEffects = p0Effects;
                                break;

                            case 1:
                                currentPlayerEffects = p1Effects;
                                break;

                            case 2:
                                currentPlayerEffects = p2Effects;
                                break;
                        }

                        currentPlayerEffects.SetMax(effect);
                    }
                    else
                    {
                        temporalEffectList.RemoveAt(i);
                    }
                }


                foreach (ContinousEffectInfo effect in continousEffectList.Values)
                {
                    switch (effect.player)
                    {
                        case 0:
                            currentPlayerEffects = p0Effects;
                            break;

                        case 1:
                            currentPlayerEffects = p1Effects;
                            break;

                        case 2:
                            currentPlayerEffects = p2Effects;
                            break;
                    }
                    currentPlayerEffects.SetMax(effect);
                }

                if (rsc.gameInfo.numberOfPlayers == 1)
                {
                    finalEffects.SetMax(p0Effects, p1Effects);
                }
                else
                {
                    if (rsc.gameInfo.player1Controller.IsPlaying && rsc.gameInfo.player2Controller.IsPlaying)
                    {
                        finalEffects.Copy(p0Effects);
                    }
                    else if (rsc.gameInfo.player1Controller.IsPlaying)
                    {
                        finalEffects.SetMax(p0Effects, p1Effects);
                    }
                    else if (rsc.gameInfo.player2Controller.IsPlaying)
                    {
                        finalEffects.SetMax(p0Effects, p2Effects);
                    }
                }
            }
            //If effects paused should stop only shake
            else
            {
                finalEffects.shakeIntensity = 0f;
            }

            mainCameraController.SetEffects(finalEffects);
            entryCameraController.SetEffects(finalEffects);
            godCameraController.SetEffects(finalEffects);
        }          
    }


    public void PlayEffect(int player = 0, float duration = 0.5f, float shake = 0f, int effects = Effects.NONE, float startFading = -1f)
    {
        if (!effectsActive) return;

        //Player 0 means both players
        TemporalEffectInfo effect = new TemporalEffectInfo(player, effects, shake, duration, (startFading == -1f ? duration * 0.75f : startFading));
        temporalEffectList.Add(effect);
    }

    public void AddContinousEffect(EffectId effectId, int player = 0, float shake = 0f, int effects = Effects.NONE)
    {
        if (!effectsActive) return;

        string key = effectId.ToString() + player.ToString();

        if (!continousEffectList.ContainsKey(key))
        {
            ContinousEffectInfo effect = new ContinousEffectInfo(player, effects, shake, effectId);
            continousEffectList.Add(key, effect);
        }
    }

    public void RemoveContinousEffect(EffectId effectId, int player)
    {
        string key = effectId.ToString() + player.ToString();

        continousEffectList.Remove(key);

        //If that was the last effect, stop all effects
        if (temporalEffectList.Count == 0 && continousEffectList.Count == 0)
        {
            finalEffects.Reset();
            mainCameraController.SetEffects(finalEffects);
            entryCameraController.SetEffects(finalEffects);
            godCameraController.SetEffects(finalEffects);
        }
    }

    public void SetEntryCameraLevelAnimation(int levelAnimation)
    {
        entryCameraObj.GetComponent<CameraController>().SetLevelAnimation(levelAnimation);
    }

    public void SetMainCameraPositionToEntryCameraPosition()
    {
        mainCameraObj.transform.position = entryCameraObj.transform.position;
    }

    public void ChangeCamera(int cameraIndex)
    {
        //Disable all cameras
        mainCameraObj.SetActive(false);
        if (entryCameraObj != null) entryCameraObj.SetActive(false);
        if (godCameraObj != null) godCameraObj.SetActive(false);

        currentCameraObj = mainCameraObj;
        //Find selected camera
        switch (cameraIndex)
        {
            case 1:
                if (entryCameraObj != null) currentCameraObj = entryCameraObj;
                break;
            case 2:
                if (godCameraObj != null) currentCameraObj = godCameraObj;
                break;
        }

        //Enable it and send event
        currentCameraObj.SetActive(true);
        currentCamera = currentCameraObj.GetComponent<Camera>();

        rsc.eventMng.TriggerEvent(EventManager.EventType.CAMERA_CHANGED, new CameraEventInfo { newCamera = currentCamera });
    }

    public void PositionCamera(int cameraIndex, Transform trf)
    {
        //Find selected camera
        switch (cameraIndex)
        {
            case 0:
                if (mainCameraObj != null)
                {
                    mainCameraController.UpdatePosition(trf);
                }
                break;
            case 1:
                if (entryCameraObj != null)
                {
                    entryCameraController.UpdatePosition(trf);
                }
                break;
            case 2:
                if (godCameraObj != null)
                {
                    godCameraController.UpdatePosition(trf);
                }
                break;
        }
    }

    public void ToggleCameraFollowPlayers()
    {
        mainCameraController.enabled = !mainCameraController.enabled;
    }

    public void SetCameraFollowPlayers()
    {
        mainCameraController.enabled = true;
    }

    public Vector3 GetDirection(Vector3 originalPosition, Vector3 displacement, int rayCastMask)
    {
        if (currentCameraObj == null) return Vector3.zero;

        if (currentCameraObj.transform.localEulerAngles.x > xRotationMinThreshold)
        {
            return CalculateDirectionRaycasting(originalPosition, displacement, rayCastMask);
        }
        else
        {
            return CalculateDirectionTransforming(displacement);
        }
    }

    private Vector3 CalculateDirectionRaycasting(Vector3 originalPosition, Vector3 displacement, int rayCastMask)
    {
        displacement = displacement * 100;

        Vector3 screenOriginalPosition = currentCamera.WorldToScreenPoint(originalPosition);
        Vector3 screenDestinationPosition = screenOriginalPosition + displacement;

        Ray camRay = currentCamera.ScreenPointToRay(screenDestinationPosition);
        RaycastHit raycastHit;

        if (Physics.Raycast(camRay, out raycastHit, camRayLength, rayCastMask))
        {
            Vector3 direction = raycastHit.point - originalPosition;
            direction.y = 0;
            direction.Normalize();
            return direction;
        }
        else
            return CalculateDirectionTransforming(displacement);

    }

    private Vector3 CalculateDirectionTransforming(Vector3 displacement)
    {
        Vector3 result = new Vector3(displacement.x, 0, displacement.y);
        result = currentCameraObj.transform.TransformDirection(result);
        result.y = 0f;
        result.Normalize();
        return result;
    }

    public bool IsObjectVisible(GameObject gameObject)
    {
        Vector3 screenPoint = currentCamera.WorldToViewportPoint(gameObject.transform.position);
        return screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }
}

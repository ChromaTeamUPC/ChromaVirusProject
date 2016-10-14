using UnityEngine;
using System.Collections;
using InControl;
using UnityStandardAssets.ImageEffects;
using VideoGlitches;

public class CameraController : MonoBehaviour 
{
    public enum CameraType
    {
        ENTRY,
        NORMAL,
        GOD
    }

    public CameraType type;

    [Header("Entry Camera Settings")]
    public bool skippeable = true;
    private bool skipped = false;
    private Animator anim;

    [Header("Main Camera Settings")]
    public bool smoothMovement = true;
    public float smoothing = 5f;
    public float topViewportLimit = 0.9f;
    public float bottomViewportLimit = 0.1f;
    public float leftViewportLimit = 0.1f;
    public float rightViewportLimit = 0.9f;
    private float maxHorizontalDistance;
    private float maxVerticalDistance;
    private float camRayLength = 100f;

    private Vector3 smoothedPosition;

    private Vector3 offset;
    private Camera thisCamera;
    private enum Horizontal
    {
        LEFT,
        RIGHT
    }

    private enum Vertical
    {
        TOP,
        BOTTOM
    }

    private Transform target1;
    private Transform target2;
    private PlayerController player1;
    private PlayerController player2;
    private Vector3 target1LastPos;
    private Vector3 target2LastPos;

    [Header("God Camera Settings")]
    public bool applyEffectsInGodCamera = true;
    public int speed = 10;

    public float minY = -90.0f;
    public float maxY = 90.0f;

    public float sens = 50.0f;

    float rotationY = 0.0f;
    float rotationX = 0.0f;   

    //Effects control
    private float shakeIntensity;
    private BlurOptimized blur;
    private VideoGlitchSpectrumOffset glitch;
    private MotionBlur motionBlur;
    private NoiseAndGrain noise;
    private Grayscale grayScale;

    void Awake()
    {
        anim = GetComponent<Animator>();
        blur = GetComponent<BlurOptimized>();
        glitch = GetComponent<VideoGlitchSpectrumOffset>();
        motionBlur = GetComponent<MotionBlur>();
        noise = GetComponent<NoiseAndGrain>();
        grayScale = GetComponent<Grayscale>();

        maxHorizontalDistance = 1 - leftViewportLimit - (1 - rightViewportLimit);
        maxVerticalDistance = 1 - bottomViewportLimit - (1 - topViewportLimit);
    }

    void Start () 
	{
        thisCamera = gameObject.GetComponent<Camera>();
        offset = rsc.gameInfo.gameCameraOffset;
        smoothedPosition = transform.position;

        target1 = rsc.gameInfo.player1.transform;
        player1 = rsc.gameInfo.player1Controller;

        target2 = rsc.gameInfo.player2.transform;
        player2 = rsc.gameInfo.player2Controller;

        rsc.eventMng.StartListening(EventManager.EventType.GAME_PAUSED, GamePaused);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESUMED, GameResumed);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_OPENED, GamePaused);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_CLOSED, GameResumed);
        rsc.eventMng.StartListening(EventManager.EventType.SCORE_OPENING, GamePaused);
        //rsc.eventMng.StartListening(EventManager.EventType.SCORE_CLOSED, GameResumed);

        if (type == CameraType.ENTRY)
        {
            CutSceneEventInfo.eventInfo.skippeable = skippeable;
            rsc.eventMng.TriggerEvent(EventManager.EventType.START_CUT_SCENE, CutSceneEventInfo.eventInfo);
        }
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.GAME_PAUSED, GamePaused);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESUMED, GameResumed);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_OPENED, GamePaused);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_CLOSED, GameResumed);
            rsc.eventMng.StopListening(EventManager.EventType.SCORE_OPENING, GamePaused);
            //rsc.eventMng.StopListening(EventManager.EventType.SCORE_CLOSED, GameResumed);
        }
    }

    void OnPreRender()
    {
        if (rsc.debugMng.viewWireFrame)
            GL.wireframe = true;
    }

    void OnPostRender()
    {
        GL.wireframe = false;
    }

    public void UpdatePosition(Transform trf)
    {
        transform.position = trf.position;
        transform.rotation = trf.rotation;
        smoothedPosition = transform.position;
    }

    private void GamePaused(EventInfo eventInfo)
    {
        if(rsc.debugMng.UIVisible)
            blur.enabled = true;
    }

    private void GameResumed(EventInfo evetInfo)
    {
        blur.enabled = false;
    }

    public void SetEffects(CamerasManager.CameraEffectsInfo effects)
    {
        if (type == CameraType.GOD && !applyEffectsInGodCamera) return;

        shakeIntensity = effects.shakeIntensity;

        if (glitch != null) glitch.enabled = ((effects.effectSet & Effects.GLITCH) > 0);
        if (rsc.gameMng.motionBlur && motionBlur != null) motionBlur.enabled = ((effects.effectSet & Effects.MOTION_BLUR) > 0);
        if (grayScale != null) grayScale.enabled = ((effects.effectSet & Effects.BN) > 0);
        if (noise != null) noise.enabled = ((effects.effectSet & Effects.NOISE) > 0);
    }
  
    void Update () 
	{
        switch (type)
        {
            case CameraType.ENTRY:
                if (rsc.gameMng.State == GameManager.GameState.STARTED)
                {
                    if (skippeable && InputManager.GetAnyControllerButtonWasPressed(InputControlType.Action2))
                    {
                        skipped = true;
                        rsc.eventMng.TriggerEvent(EventManager.EventType.CAMERA_ANIMATION_ENDED, EventInfo.emptyInfo);
                    }
                }
                break;

            case CameraType.NORMAL:
                break;

            case CameraType.GOD:               
                break;

            default:
                break;
        }
    }

    void LateUpdate()
    {
        switch (type)
        {
            case CameraType.ENTRY:
                if (shakeIntensity > 0)
                {
                    transform.position = transform.position + Random.insideUnitSphere * shakeIntensity;
                }
                break;

            case CameraType.NORMAL:
                Vector3 newCamPos = GetCamTargetPosition();
                if (smoothMovement)
                    smoothedPosition = Vector3.Lerp(smoothedPosition, newCamPos, smoothing * Time.deltaTime);
                else
                    smoothedPosition = newCamPos;

                //Save players position for the next frame
                target1LastPos = target1.position;
                target2LastPos = target2.position;

                if (shakeIntensity > 0)
                    transform.position = smoothedPosition + Random.insideUnitSphere * shakeIntensity;
                else
                    transform.position = smoothedPosition;
                break;

            case CameraType.GOD:
                if (Input.GetKey(rsc.debugMng.keys.godCameraRight))
                {
                    Vector3 displacement = transform.right * Time.unscaledDeltaTime * speed;
                    transform.position = transform.position + displacement;
                }
                else if (Input.GetKey(rsc.debugMng.keys.godCameraLeft))
                {
                    Vector3 displacement = transform.right * Time.unscaledDeltaTime * speed * -1;
                    transform.position = transform.position + displacement;
                }
                if (Input.GetKey(rsc.debugMng.keys.godCameraForward))
                {
                    Vector3 displacement = transform.forward * Time.unscaledDeltaTime * speed;
                    transform.position = transform.position + displacement;
                }
                else if (Input.GetKey(rsc.debugMng.keys.godCameraBackward))
                {
                    Vector3 displacement = transform.forward * Time.unscaledDeltaTime * speed * -1;
                    transform.position = transform.position + displacement;
                }

                // camera rotation with mouse coordinates
                rotationX += Input.GetAxis("Mouse X") * sens * Time.unscaledDeltaTime;
                rotationY += Input.GetAxis("Mouse Y") * sens * Time.unscaledDeltaTime;
                rotationY = Mathf.Clamp(rotationY, minY, maxY);
                transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
                break;
        }
    }

    #region ENTRY CAMERA METHODS
    public void SetLevelAnimation(int levelAnimation)
    {
        if (type == CameraType.ENTRY)
            anim.SetInteger("LevelAnim", levelAnimation);
    }

    public void AnimationEnded()
    {
        if (!skipped)
            rsc.eventMng.TriggerEvent(EventManager.EventType.CAMERA_ANIMATION_ENDED, EventInfo.emptyInfo);
    }
    #endregion

    #region NORMAL CAMERA METHODS
    public Vector3 GetCamTargetPosition()
    {
        //1 player: follow him until last life lost
        if (rsc.gameInfo.numberOfPlayers == 1)
        {
            if (player1.IsPlaying)
                return target1.position + offset;
        }
        //2 players
        else
        {
            if (player1.IsPlaying || player2.IsPlaying)
            {
                if (player1.IsPlaying && !player2.IsPlaying)
                {
                    return target1.position + offset;
                }

                if (!player1.IsPlaying && player2.IsPlaying)
                {
                    return target2.position + offset;
                }

                if (!player1.IsFalling || !player2.IsFalling)
                {
                    if (player1.IsFalling && !player2.IsFalling)
                    {
                        return target2.position + offset;
                    }

                    if (!player1.IsFalling && player2.IsFalling)
                    {
                        return target1.position + offset;
                    }

                    return CheckPlayersAndCameraPosition();
                }
            }
        }

        return transform.position; //Not moving
    }

    private Vector3 CheckPlayersAndCameraPosition()
    {
        Vector3 p1NewViewportPos = thisCamera.WorldToViewportPoint(target1.position);
        Vector3 p2NewViewportPos = thisCamera.WorldToViewportPoint(target2.position);

        //If both players inside new limits: no problem
        if (IsPositionInsideLimits(p1NewViewportPos) && IsPositionInsideLimits(p2NewViewportPos))
        {
            return GetNewCamPosition();
        }
        //Uh oh. Someone is off the limits
        else
        {
            Vector3 p1LastViewportPos = thisCamera.WorldToViewportPoint(target1LastPos);
            Vector3 p2LastViewportPos = thisCamera.WorldToViewportPoint(target2LastPos);

            //Horizontal calculations
            PlayerController leftPlayer;
            Vector3 leftNewViewportPos;
            Vector3 leftLastViewportPos;
            bool leftMoved;

            PlayerController rightPlayer;
            Vector3 rightNewViewportPos;
            Vector3 rightLastViewportPos;
            bool rightMoved;

            Horizontal p1Horizontal;

            if (p1NewViewportPos.x < p2NewViewportPos.x)
            {
                p1Horizontal = Horizontal.LEFT;
                leftPlayer = player1;
                leftNewViewportPos = p1NewViewportPos;
                leftLastViewportPos = p1LastViewportPos;
                leftMoved = leftPlayer.bb.screenVector.x != 0;

                rightPlayer = player2;
                rightNewViewportPos = p2NewViewportPos;
                rightLastViewportPos = p2LastViewportPos;
                rightMoved = rightPlayer.bb.screenVector.x != 0;
            }
            else
            {
                p1Horizontal = Horizontal.RIGHT;
                rightPlayer = player1;
                rightNewViewportPos = p1NewViewportPos;
                rightLastViewportPos = p1LastViewportPos;
                rightMoved = rightPlayer.bb.screenVector.x != 0;

                leftPlayer = player2;
                leftNewViewportPos = p2NewViewportPos;
                leftLastViewportPos = p2LastViewportPos;
                leftMoved = leftPlayer.bb.screenVector.x != 0;
            }

            //If both players are outside horizontal limits simply restrict to current limit
            if (leftNewViewportPos.x < leftViewportLimit && rightNewViewportPos.x > rightViewportLimit)
            {
                if (leftMoved)
                {
                    leftNewViewportPos.x = Mathf.Max(leftViewportLimit, leftNewViewportPos.x);
                }

                if (rightMoved)
                {
                    rightNewViewportPos.x = Mathf.Min(rightViewportLimit, rightNewViewportPos.x);
                }
            }
            //If left is outside but right is inside
            else if (leftNewViewportPos.x < leftViewportLimit)
            {
                float leftTotalOffset = Mathf.Abs(leftViewportLimit - leftNewViewportPos.x);
                float rightTotalOffset = Mathf.Abs(rightViewportLimit - rightNewViewportPos.x);

                if (rightTotalOffset < leftTotalOffset)
                {
                    float newLeftLimit = leftNewViewportPos.x + (leftTotalOffset - rightTotalOffset);

                    if (leftMoved)
                    {
                        if (leftNewViewportPos.x != leftLastViewportPos.x)
                        {
                            if (leftNewViewportPos.x < leftLastViewportPos.x)
                                leftNewViewportPos.x = Mathf.Min(leftLastViewportPos.x, newLeftLimit);
                            else
                                leftNewViewportPos.x = Mathf.Max(leftNewViewportPos.x, newLeftLimit);
                        }
                    }

                    if (rightMoved)
                    {
                        if (rightNewViewportPos.x - leftNewViewportPos.x > maxHorizontalDistance)
                        {
                            rightNewViewportPos.x = leftNewViewportPos.x + maxHorizontalDistance;
                        }
                    }
                }
            }
            //If right is outside but left is inside
            else if (rightNewViewportPos.x > rightViewportLimit)
            {
                float leftTotalOffset = Mathf.Abs(leftViewportLimit - leftNewViewportPos.x);
                float rightTotalOffset = Mathf.Abs(rightViewportLimit - rightNewViewportPos.x);

                if (leftTotalOffset < rightTotalOffset)
                {
                    float newRightLimit = rightNewViewportPos.x - (rightTotalOffset - leftTotalOffset);

                    if (rightMoved)
                    {
                        if (rightNewViewportPos.x != rightLastViewportPos.x)
                        {
                            if (rightNewViewportPos.x > rightLastViewportPos.x)
                                rightNewViewportPos.x = Mathf.Max(rightLastViewportPos.x, newRightLimit);
                            else
                                rightNewViewportPos.x = Mathf.Min(rightNewViewportPos.x, newRightLimit);
                        }
                    }

                    if (leftMoved)
                    {
                        if (rightNewViewportPos.x - leftNewViewportPos.x > maxHorizontalDistance)
                        {
                            leftNewViewportPos.x = rightNewViewportPos.x - maxHorizontalDistance;
                        }
                    }
                }
            }
            //Else both players are in horitzontal limits do nothing

            //Vertical calculations
            PlayerController bottomPlayer;
            Vector3 bottomNewViewportPos;
            Vector3 bottomLastViewportPos;
            bool bottomMoved;

            PlayerController topPlayer;
            Vector3 topNewViewportPos;
            Vector3 topLastViewportPos;
            bool topMoved;

            Vertical p1Vertical;

            if (p1NewViewportPos.y < p2NewViewportPos.y)
            {
                p1Vertical = Vertical.BOTTOM;
                bottomPlayer = player1;
                bottomNewViewportPos = p1NewViewportPos;
                bottomLastViewportPos = p1LastViewportPos;
                bottomMoved = bottomPlayer.bb.screenVector.y != 0;

                topPlayer = player2;
                topNewViewportPos = p2NewViewportPos;
                topLastViewportPos = p2LastViewportPos;
                topMoved = topPlayer.bb.screenVector.y != 0;
            }
            else
            {
                p1Vertical = Vertical.TOP;
                topPlayer = player1;
                topNewViewportPos = p1NewViewportPos;
                topLastViewportPos = p1LastViewportPos;
                topMoved = topPlayer.bb.screenVector.y != 0;

                bottomPlayer = player2;
                bottomNewViewportPos = p2NewViewportPos;
                bottomLastViewportPos = p2LastViewportPos;
                bottomMoved = bottomPlayer.bb.screenVector.y != 0;
            }

            //If both players are outside vertical limits simply restrict to current limit
            if (bottomNewViewportPos.y < bottomViewportLimit && topNewViewportPos.y > topViewportLimit)
            {
                if (bottomMoved)
                {
                    bottomNewViewportPos.y = Mathf.Max(bottomViewportLimit, bottomNewViewportPos.y);
                }

                if (topMoved)
                {
                    topNewViewportPos.y = Mathf.Min(topViewportLimit, topNewViewportPos.y);
                }
            }
            //If bottom is outside but top is inside
            else if (bottomNewViewportPos.y < bottomViewportLimit)
            {
                float bottomTotalOffset = Mathf.Abs(bottomViewportLimit - bottomNewViewportPos.y);
                float topTotalOffset = Mathf.Abs(topViewportLimit - topNewViewportPos.y);

                if (topTotalOffset < bottomTotalOffset)
                {
                    float newBottomLimit = bottomNewViewportPos.y + (bottomTotalOffset - topTotalOffset);

                    if (bottomMoved)
                    {
                        if (bottomNewViewportPos.y != bottomLastViewportPos.y)
                        {
                            if (bottomNewViewportPos.y < bottomLastViewportPos.y)
                                bottomNewViewportPos.y = Mathf.Min(bottomLastViewportPos.y, newBottomLimit);
                            else
                                bottomNewViewportPos.y = Mathf.Max(bottomNewViewportPos.y, newBottomLimit);
                        }
                    }

                    if (topMoved)
                    {
                        if (topNewViewportPos.y - bottomNewViewportPos.y > maxVerticalDistance)
                        {
                            topNewViewportPos.y = bottomNewViewportPos.y + maxVerticalDistance;
                        }
                    }
                }
            }
            //If top is outside but bottom is inside
            else if (topNewViewportPos.y > topViewportLimit)
            {
                float bottomTotalOffset = Mathf.Abs(bottomViewportLimit - bottomNewViewportPos.y);
                float topTotalOffset = Mathf.Abs(topViewportLimit - topNewViewportPos.y);

                if (bottomTotalOffset < topTotalOffset)
                {
                    float newTopLimit = topNewViewportPos.y - (topTotalOffset - bottomTotalOffset);

                    if (topMoved)
                    {
                        if (topNewViewportPos.y != topLastViewportPos.y)
                        {
                            if (topNewViewportPos.y > topLastViewportPos.y)
                                topNewViewportPos.y = Mathf.Max(topLastViewportPos.y, newTopLimit);
                            else
                                topNewViewportPos.y = Mathf.Min(topNewViewportPos.y, newTopLimit);
                        }
                    }

                    if (bottomMoved)
                    {
                        if (topNewViewportPos.y - bottomNewViewportPos.y > maxVerticalDistance)
                        {
                            bottomNewViewportPos.y = topNewViewportPos.y - maxVerticalDistance;
                        }
                    }
                }
            }
            //Else both players are in vertical limits do nothing

            //Check if players need move
            bool movePlayer1H = false;
            bool movePlayer1V = false;
            Vector3 newPlayer1Pos = new Vector3(0, 0, 0);
            bool movePlayer2H = false;
            bool movePlayer2V = false;
            Vector3 newPlayer2Pos = new Vector3(0, 0, 0);

            //Check new positions against last ones. Player can not be pushed back
            if (p1Horizontal == Horizontal.LEFT)
            {
                newPlayer1Pos.x = leftNewViewportPos.x;
                newPlayer2Pos.x = rightNewViewportPos.x;
            }
            else
            {
                newPlayer1Pos.x = rightNewViewportPos.x;
                newPlayer2Pos.x = leftNewViewportPos.x;
            }

            movePlayer1H = newPlayer1Pos.x != p1NewViewportPos.x;
            movePlayer2H = newPlayer2Pos.x != p2NewViewportPos.x;

            if (p1Vertical == Vertical.BOTTOM)
            {
                newPlayer1Pos.y = bottomNewViewportPos.y;
                newPlayer2Pos.y = topNewViewportPos.y;
            }
            else
            {
                newPlayer1Pos.y = topNewViewportPos.y;
                newPlayer2Pos.y = bottomNewViewportPos.y;
            }

            movePlayer1V = newPlayer1Pos.y != p1NewViewportPos.y;
            movePlayer2V = newPlayer2Pos.y != p2NewViewportPos.y;

            if (movePlayer1H || movePlayer1V)
            {
                Ray camRay = thisCamera.ViewportPointToRay(newPlayer1Pos);

                RaycastHit raycastHit;

                if (Physics.Raycast(camRay, out raycastHit, camRayLength, player1.bb.playerRayCastMask))
                {
                    Vector3 newPos = raycastHit.point;
                    player1.ForcePosition(newPos);
                }
            }

            if (movePlayer2H || movePlayer2V)
            {
                Ray camRay = thisCamera.ViewportPointToRay(newPlayer2Pos);
                RaycastHit raycastHit;

                if (Physics.Raycast(camRay, out raycastHit, camRayLength, player2.bb.playerRayCastMask))
                {
                    Vector3 newPos = raycastHit.point;
                    player2.ForcePosition(newPos);
                }
            }

            return GetNewCamPosition();
        }
    }

    private Vector3 GetNewCamPosition()
    {
        //Method 1
        Vector3 p1NewViewportPos = thisCamera.WorldToViewportPoint(target1.position);
        Vector3 p2NewViewportPos = thisCamera.WorldToViewportPoint(target2.position);

        Vector3 newCamViewportPos = (p1NewViewportPos + p2NewViewportPos) / 2;
        Vector3 newCamWorldPos = thisCamera.ViewportToWorldPoint(newCamViewportPos);
        newCamWorldPos += offset;

        return newCamWorldPos;

        //Method 2
        //return ((target1.position + target2.position) / 2) + offset;
    }

    private bool IsPositionInsideLimits(Vector3 viewportPosition)
    {
        if (viewportPosition.x < leftViewportLimit) return false;
        if (viewportPosition.x > rightViewportLimit) return false;
        if (viewportPosition.y < bottomViewportLimit) return false;
        if (viewportPosition.y > topViewportLimit) return false;

        return true;
    }

    void OnGUI()
    {
        if (type == CameraType.NORMAL && rsc.debugMng.showPlayerLimits)
        {
            GUI.color = Color.red;

            //Vertical center
            Vector3 screenPoint1 = thisCamera.ViewportToScreenPoint(new Vector3(0.5f, 1f, 0f));
            Vector3 screenPoint2 = thisCamera.ViewportToScreenPoint(new Vector3(0.5f, 0f, 0f));
            Rect rect = new Rect();
            rect.xMin = screenPoint1.x;
            rect.xMax = screenPoint1.x + 1;
            rect.yMin = screenPoint2.y;
            rect.yMax = screenPoint1.y;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);

            //Horizontal center
            screenPoint1 = thisCamera.ViewportToScreenPoint(new Vector3(0f, 0.5f, 0f));
            screenPoint2 = thisCamera.ViewportToScreenPoint(new Vector3(1f, 0.5f, 0f));
            rect = new Rect();
            rect.xMin = screenPoint1.x;
            rect.xMax = screenPoint2.x;
            rect.yMin = screenPoint2.y;
            rect.yMax = screenPoint1.y + 1;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);

            GUI.color = Color.green;

            //Left
            screenPoint1 = thisCamera.ViewportToScreenPoint(new Vector3(leftViewportLimit, 1f, 0f));
            screenPoint2 = thisCamera.ViewportToScreenPoint(new Vector3(leftViewportLimit, 0f, 0f));
            rect = new Rect();
            rect.xMin = screenPoint1.x;
            rect.xMax = screenPoint1.x + 1;
            rect.yMin = screenPoint2.y;
            rect.yMax = screenPoint1.y;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);

            //Right
            screenPoint1 = thisCamera.ViewportToScreenPoint(new Vector3(rightViewportLimit, 1f, 0f));
            screenPoint2 = thisCamera.ViewportToScreenPoint(new Vector3(rightViewportLimit, 0f, 0f));
            rect = new Rect();
            rect.xMin = screenPoint1.x;
            rect.xMax = screenPoint1.x + 1;
            rect.yMin = screenPoint2.y;
            rect.yMax = screenPoint1.y;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);

            //Bottom
            screenPoint1 = thisCamera.ViewportToScreenPoint(new Vector3(0f, bottomViewportLimit, 0f));
            screenPoint2 = thisCamera.ViewportToScreenPoint(new Vector3(1f, bottomViewportLimit, 0f));
            rect = new Rect();
            rect.xMin = screenPoint1.x;
            rect.xMax = screenPoint2.x;
            rect.yMin = Screen.height - screenPoint2.y;
            rect.yMax = Screen.height - screenPoint1.y + 1;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);

            //Top
            screenPoint1 = thisCamera.ViewportToScreenPoint(new Vector3(0f, topViewportLimit, 0f));
            screenPoint2 = thisCamera.ViewportToScreenPoint(new Vector3(1f, topViewportLimit, 0f));
            rect = new Rect();
            rect.xMin = screenPoint1.x;
            rect.xMax = screenPoint2.x;
            rect.yMin = Screen.height - screenPoint2.y;
            rect.yMax = Screen.height - screenPoint1.y + 1;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);

            GUI.color = Color.magenta;

            screenPoint1 = thisCamera.WorldToScreenPoint(player1.transform.position);
            rect = new Rect();
            rect.xMin = screenPoint1.x - 2;
            rect.xMax = screenPoint1.x + 2;
            rect.yMin = Screen.height - screenPoint1.y - 2;
            rect.yMax = Screen.height - screenPoint1.y + 2;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);

            screenPoint1 = thisCamera.WorldToScreenPoint(player2.transform.position);
            rect = new Rect();
            rect.xMin = screenPoint1.x - 2;
            rect.xMax = screenPoint1.x + 2;
            rect.yMin = Screen.height - screenPoint1.y - 2;
            rect.yMax = Screen.height - screenPoint1.y + 2;

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);

        }
    }
    #endregion
}

using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using VideoGlitches;

public class MainCameraController : MonoBehaviour {

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

    public bool smoothMovement = false;
    public float smoothing = 5f;
    public float defaultShakeMaximum = 0.3f;
    public bool showPlayerLimits = true;
    public float topViewportLimit = 0.9f;
    public float bottomViewportLimit = 0.1f;
    public float leftViewportLimit = 0.1f;
    public float rightViewportLimit = 0.9f;
    private float maxHorizontalDistance;
    private float maxVerticalDistance;
    private float camRayLength = 100f;

    private Vector3 smoothedPosition;

    private MotionBlur motionBlur;
    private VideoGlitchSpectrumOffset glitch;
    private NoiseAndGrain noise;
    private Grayscale grayScale;

    private Vector3 offset;
    private Camera thisCamera;
    private float cameraBorderMargin = 50f;
    private float maxYPosition;
    private float maxXPosition;
    private float minYPosition;
    private float minXPosition;

    private float colorMismatchDuration;
    private float shakeDuration;
    private float currentShakeMaximum;

    private bool pauseEffects;



    void Awake()
    {
        motionBlur = GetComponent<MotionBlur>();
        glitch = GetComponent<VideoGlitchSpectrumOffset>();
        noise = GetComponent<NoiseAndGrain>();
        grayScale = GetComponent<Grayscale>();

        maxHorizontalDistance = 1 - leftViewportLimit - (1 - rightViewportLimit);
        maxVerticalDistance = 1 - bottomViewportLimit - (1 - topViewportLimit);
    }

    void Start()
    {
        thisCamera = gameObject.GetComponent<Camera>();
        offset = rsc.gameInfo.gameCameraOffset;
        smoothedPosition = transform.position;
        //gameObject.transform.rotation = rsc.gameInfo.gameCameraRotation;

        //playerRayCastMask = LayerMask.GetMask("PlayerRayCast");
            
        maxYPosition = thisCamera.pixelHeight - cameraBorderMargin;
        maxXPosition = thisCamera.pixelWidth - cameraBorderMargin;
        minYPosition = cameraBorderMargin;
        minXPosition = cameraBorderMargin;

        target1 = rsc.gameInfo.player1.transform;
        player1 = rsc.gameInfo.player1Controller;

        target2 = rsc.gameInfo.player2.transform;
        player2 = rsc.gameInfo.player2Controller;

        colorMismatchDuration = 0f;
        shakeDuration = 0f;
        currentShakeMaximum = 0f;
        pauseEffects = false;

        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DASHING, PlayerStartDash);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DASHED, PlayerEndDash);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_COLOR_MISMATCH, PlayerColorMismatch);
        rsc.eventMng.StartListening(EventManager.EventType.DEVICE_INFECTION_LEVEL_CHANGED, DeviceInfectionChanged);
        rsc.eventMng.StartListening(EventManager.EventType.WORM_ATTACK, WormAttack);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_OPENED, GamePaused);
        rsc.eventMng.StartListening(EventManager.EventType.TUTORIAL_CLOSED, GameResumed);
        rsc.eventMng.StartListening(EventManager.EventType.SCORE_OPENING, GamePaused);
        //rsc.eventMng.StartListening(EventManager.EventType.SCORE_CLOSED, GameResumed);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_PAUSED, GamePaused);
        rsc.eventMng.StartListening(EventManager.EventType.GAME_RESUMED, GameResumed);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DASHING, PlayerStartDash);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DASHED, PlayerEndDash);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_COLOR_MISMATCH, PlayerColorMismatch);
            rsc.eventMng.StopListening(EventManager.EventType.DEVICE_INFECTION_LEVEL_CHANGED, DeviceInfectionChanged);
            rsc.eventMng.StopListening(EventManager.EventType.WORM_ATTACK, WormAttack);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_OPENED, GamePaused);
            rsc.eventMng.StopListening(EventManager.EventType.TUTORIAL_CLOSED, GameResumed);
            rsc.eventMng.StopListening(EventManager.EventType.SCORE_OPENING, GamePaused);
            //rsc.eventMng.StopListening(EventManager.EventType.SCORE_CLOSED, GameResumed);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_PAUSED, GamePaused);
            rsc.eventMng.StopListening(EventManager.EventType.GAME_RESUMED, GameResumed);
        }
    }

    private void PlayerStartDash(EventInfo eventInfo)
    {
        if(rsc.gameMng.motionBlur)
            motionBlur.enabled = true;
    }

    private void PlayerEndDash(EventInfo eventInfo)
    {
        if (rsc.gameMng.motionBlur)
            motionBlur.enabled = false;
    }

    private void PlayerColorMismatch(EventInfo eventInfo)
    {
        PlayerEventInfo info = (PlayerEventInfo)eventInfo;

        colorMismatchDuration = info.player.effectDurationOnColorMismatch;
        shakeDuration = info.player.effectDurationOnColorMismatch;

        if (colorMismatchDuration > 0)
            glitch.enabled = true;

        rsc.rumbleMng.Rumble(0, shakeDuration);
    }

    private void WormAttack(EventInfo eventInfo)
    {
        WormEventInfo info = (WormEventInfo)eventInfo;

        shakeDuration = info.wormBb.attackRumbleDuration;
        rsc.rumbleMng.Rumble(0, shakeDuration);
    }

    private void GamePaused(EventInfo eventInfo)
    {
        pauseEffects = true;
    }

    private void GameResumed(EventInfo eventInfo)
    {
        pauseEffects = false;
    }

    private void DeviceInfectionChanged(EventInfo eventInfo)
    {
        DeviceEventInfo info = (DeviceEventInfo)eventInfo;
        if (info.device.type != DeviceController.Type.VIDEO)
            return;

        StopAllCoroutines();
        StopInfection();

        switch (info.device.CurrentInfectionLevel)
        {
            case DeviceController.InfectionLevel.LEVEL1:
                StartCoroutine(InfectionLevel01());
                break;
            case DeviceController.InfectionLevel.LEVEL2:
                StartCoroutine(InfectionLevel02());
                break;
            case DeviceController.InfectionLevel.LEVEL3:
                StartCoroutine(InfectionLevel03());
                break;
            default:
                break;
        }
    }

    private void StopInfection()
    {
        noise.enabled = false;
        grayScale.enabled = false;
    }

    private IEnumerator InfectionLevel01()
    {
        while (true)
        {
            noise.enabled = true;
            grayScale.enabled = true;
            yield return new WaitForSeconds(1f);
            noise.enabled = false;
            grayScale.enabled = false;
            yield return new WaitForSeconds(3f);
        }
    }

    private IEnumerator InfectionLevel02()
    {
        while (true)
        {
            noise.enabled = true;
            grayScale.enabled = true;
            yield return new WaitForSeconds(2f);
            noise.enabled = false;
            grayScale.enabled = false;
            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator InfectionLevel03()
    {
        noise.enabled = true;

        while (true)
        {
            grayScale.enabled = true;
            yield return new WaitForSeconds(3f);
            grayScale.enabled = false;
            yield return new WaitForSeconds(1f);
        }
    }

    public Vector3 GetCamTargetPosition()
    {
        //1 player: follow him until last life lost
        if (rsc.gameInfo.numberOfPlayers == 1)
        {
            if(player1.IsPlaying)
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
            Transform leftTransform;
            PlayerController leftPlayer;
            Vector3 leftNewViewportPos;
            Vector3 leftLastViewportPos;
            bool leftMoved;

            Transform rightTransform;
            PlayerController rightPlayer;
            Vector3 rightNewViewportPos;
            Vector3 rightLastViewportPos;
            bool rightMoved;

            Horizontal p1Horizontal;

            if (p1NewViewportPos.x < p2NewViewportPos.x)
            {
                p1Horizontal = Horizontal.LEFT;
                leftTransform = target1;
                leftPlayer = player1;
                leftNewViewportPos = p1NewViewportPos;
                leftLastViewportPos = p1LastViewportPos;
                leftMoved = leftPlayer.bb.screenVector.x != 0;

                rightTransform = target2;
                rightPlayer = player2;
                rightNewViewportPos = p2NewViewportPos;
                rightLastViewportPos = p2LastViewportPos;
                rightMoved = rightPlayer.bb.screenVector.x != 0;
            }
            else
            {
                p1Horizontal = Horizontal.RIGHT;
                rightTransform = target1;
                rightPlayer = player1;
                rightNewViewportPos = p1NewViewportPos;
                rightLastViewportPos = p1LastViewportPos;
                rightMoved = rightPlayer.bb.screenVector.x != 0;

                leftTransform = target2;
                leftPlayer = player2;
                leftNewViewportPos = p2NewViewportPos;
                leftLastViewportPos = p2LastViewportPos;
                leftMoved = leftPlayer.bb.screenVector.x != 0;
            }

            //If both players are outside horizontal limits simply restrict to current limit
            if (leftNewViewportPos.x < leftViewportLimit && rightNewViewportPos.x > rightViewportLimit)
            {
                if(leftMoved)
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
            Transform bottomTransform;
            PlayerController bottomPlayer;
            Vector3 bottomNewViewportPos;
            Vector3 bottomLastViewportPos;
            bool bottomMoved;

            Transform topTransform;
            PlayerController topPlayer;
            Vector3 topNewViewportPos;
            Vector3 topLastViewportPos;
            bool topMoved;

            Vertical p1Vertical;

            if (p1NewViewportPos.y < p2NewViewportPos.y)
            {
                p1Vertical = Vertical.BOTTOM;
                bottomTransform = target1;
                bottomPlayer = player1;
                bottomNewViewportPos = p1NewViewportPos;
                bottomLastViewportPos = p1LastViewportPos;
                bottomMoved = bottomPlayer.bb.screenVector.y != 0;

                topTransform = target2;
                topPlayer = player2;
                topNewViewportPos = p2NewViewportPos;
                topLastViewportPos = p2LastViewportPos;
                topMoved = topPlayer.bb.screenVector.y != 0;
            }
            else
            {
                p1Vertical = Vertical.TOP;
                topTransform = target1;
                topPlayer = player1;
                topNewViewportPos = p1NewViewportPos;
                topLastViewportPos = p1LastViewportPos;
                topMoved = topPlayer.bb.screenVector.y != 0;

                bottomTransform = target2;
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
                Vector3 screenPoint = thisCamera.ViewportToScreenPoint(newPlayer1Pos);

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
                Vector3 screenPoint = thisCamera.ViewportToScreenPoint(newPlayer2Pos);

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

    public void SetCamPosition()
    {
        transform.position = GetCamTargetPosition();
    }

    void OnGUI()
    {
        if (showPlayerLimits)
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

    void Update()
    {
        if (colorMismatchDuration > 0)
        {
            colorMismatchDuration -= Time.deltaTime; 
            
            if(colorMismatchDuration <= 0)
            {
                colorMismatchDuration = 0f;
                glitch.enabled = false;
            }      
        }
    }
	
	// Update is called once per frame
	void LateUpdate () 
    {
        if (!pauseEffects)
        {
            Vector3 newCamPos = GetCamTargetPosition();
            if (smoothMovement)
                smoothedPosition = Vector3.Lerp(smoothedPosition, newCamPos, smoothing * Time.deltaTime);
            else
                smoothedPosition = newCamPos;

            if (shakeDuration > 0)
            {
                transform.position = smoothedPosition + Random.insideUnitSphere * (currentShakeMaximum > 0 ? currentShakeMaximum : defaultShakeMaximum);

                shakeDuration -= Time.deltaTime;

                if (shakeDuration <= 0)
                {
                    shakeDuration = 0f;
                    currentShakeMaximum = 0f;
                }
            }
            else
            {
                transform.position = smoothedPosition;
            }
            //transform.position = smoothedPosition; 
        }

        //Save players position for the next frame
        target1LastPos = target1.position;
        target2LastPos = target2.position;      	
	}

    
}

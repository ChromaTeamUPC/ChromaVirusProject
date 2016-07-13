using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using VideoGlitches;

public class MainCameraController : MonoBehaviour {

    private Transform target1;
    private Transform target2;
    private PlayerController player1;
    private PlayerController player2;
    //private int playerRayCastMask;

    public float smoothing = 5f;
    public float defaultShakeMaximum = 0.3f;

    private Vector3 smoothedPosition;

    private BlurOptimized blur;
    private MotionBlur motionBlur;
    private VideoGlitchSpectrumOffset glitch;
    private NoiseAndGrain noise;
    private Grayscale grayScale;

    private Vector3 offset;
    private Camera thisCamera;
    private float cameraBorderMargin = 50f;
    float maxYPosition;
    float maxXPosition;
    float minYPosition;
    float minXPosition;

    private float colorMismatchDuration;
    private float shakeDuration;
    private float currentShakeMaximum;

    void Awake()
    {
        blur = GetComponent<BlurOptimized>();
        motionBlur = GetComponent<MotionBlur>();
        glitch = GetComponent<VideoGlitchSpectrumOffset>();
        noise = GetComponent<NoiseAndGrain>();
        grayScale = GetComponent<Grayscale>();
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

        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DASHING, PlayerStartDash);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_DASHED, PlayerEndDash);
        rsc.eventMng.StartListening(EventManager.EventType.PLAYER_COLOR_MISMATCH, PlayerColorMismatch);
        rsc.eventMng.StartListening(EventManager.EventType.DEVICE_INFECTION_LEVEL_CHANGED, DeviceInfectionChanged);
    }

    void OnDestroy()
    {
        if (rsc.eventMng != null)
        {
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DASHING, PlayerStartDash);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_DASHED, PlayerEndDash);
            rsc.eventMng.StopListening(EventManager.EventType.PLAYER_COLOR_MISMATCH, PlayerColorMismatch);
            rsc.eventMng.StopListening(EventManager.EventType.DEVICE_INFECTION_LEVEL_CHANGED, DeviceInfectionChanged);
        }
    }

    private void PlayerStartDash(EventInfo eventInfo)
    {
        motionBlur.enabled = true;
    }

    private void PlayerEndDash(EventInfo eventInfo)
    {
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
        if (player1.Active && player2.Active)
        {
            /*Vector3 p1ScreenPos = thisCamera.WorldToScreenPoint(target1.position);
            Vector3 p2ScreenPos = thisCamera.WorldToScreenPoint(target2.position);
            Vector3 targetCamPos = ((p1ScreenPos + p2ScreenPos) / 2);            

            Ray camRay = thisCamera.ScreenPointToRay(targetCamPos);
            RaycastHit playerRaycastHit;

            if (Physics.Raycast(camRay, out playerRaycastHit, 100, playerRayCastMask))
            {
                return playerRaycastHit.point + offset;
            } */
            return ((target1.position + target2.position) / 2) + offset;

        }
        else if (player1.Active)
        {
            return target1.position + offset;
        }
        else if (player2.Active)
        {
            return target2.position + offset;
        }

        return transform.position; //Not moving
    }

    public void SetCamPosition()
    {
        transform.position = GetCamTargetPosition();
    }

    void Update()
    {
        if(colorMismatchDuration > 0)
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
        smoothedPosition = Vector3.Lerp(smoothedPosition, GetCamTargetPosition(), smoothing * Time.deltaTime);

        if(shakeDuration > 0)
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

    public Vector3 GetPosition(Vector3 originalPosition, Vector3 displacement)
    {
        if (player1.Active && player2.Active)
        {
            Vector3 p1ScreenPos = thisCamera.WorldToScreenPoint(target1.position);
            Vector3 p2ScreenPos = thisCamera.WorldToScreenPoint(target2.position);

            float yMargin = Mathf.Min(maxYPosition - p1ScreenPos.y, maxYPosition - p2ScreenPos.y) +
                Mathf.Min(p1ScreenPos.y - minYPosition, p2ScreenPos.y - minYPosition);

            float xMargin = Mathf.Min(maxXPosition - p1ScreenPos.x, maxXPosition - p2ScreenPos.x) +
                Mathf.Min(p1ScreenPos.x - minXPosition, p2ScreenPos.x - minXPosition);


            Vector3 finalPosition = originalPosition + displacement;

            if (finalPosition.y < minYPosition)
            {
                finalPosition.y = Mathf.Max(finalPosition.y, originalPosition.y - yMargin);
            }
            else if (finalPosition.y > maxYPosition)
            {
                finalPosition.y = Mathf.Min(finalPosition.y, originalPosition.y + yMargin);
            }

            if (finalPosition.x < minXPosition)
            {
                finalPosition.x = Mathf.Max(finalPosition.x, originalPosition.x - xMargin);
            }
            else if (finalPosition.x > maxXPosition)
            {
                finalPosition.x = Mathf.Min(finalPosition.x, originalPosition.x + xMargin);
            }

            return finalPosition;
        }
        else
        {
            return originalPosition + displacement;
        }
    }
}

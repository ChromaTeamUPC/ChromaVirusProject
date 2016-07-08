using UnityEngine;
using System.Collections;

public class CamerasManager : MonoBehaviour {

    [HideInInspector]
    public GameObject currentCameraObj;
    [HideInInspector]
    public Camera currentCamera;

    public float xRotationMinThreshold = 25;

    public GameObject mainCameraObj;
    public GameObject entryCameraObj;
    public GameObject godCameraObj;
    public GameObject staticCamera1Obj;
    public GameObject staticCamera2Obj;
    public GameObject staticCamera3Obj;

    private DebugKeys keys;

    private float camRayLength = 100f;
    /*private Transform target1;
    private Transform target2;
    private PlayerController player1;
    private PlayerController player2;

    private float cameraBorderMargin = 50f;
    private float maxYPosition;
    private float maxXPosition;
    private float minYPosition;
    private float minXPosition;*/

    void Awake()
    {
        currentCamera = mainCameraObj.GetComponent<Camera>();

        //We are sure rsc is created because we forced script execution order from unity editor - project settings
        rsc.camerasMng = this;
    }

    void OnDestroy()
    {
        rsc.camerasMng = null;
    }

    void Start()
    { 
        keys = rsc.debugMng.keys;
        /*target1 = rsc.gameInfo.player1.transform;
        player1 = rsc.gameInfo.player1Controller;

        target2 = rsc.gameInfo.player2.transform;
        player2 = rsc.gameInfo.player2Controller;

        minYPosition = cameraBorderMargin;
        minXPosition = cameraBorderMargin;*/

        //Set main camera
        //ChangeCamera(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keys.mainCameraActivationKey))
            ChangeCamera(0);
        else if (Input.GetKeyDown(keys.godCameraActivationKey))
            ChangeCamera(2);
        else if (Input.GetKeyDown(keys.staticCamera1ActivationKey))
            ChangeCamera(3);
        else if (Input.GetKeyDown(keys.staticCamera2ActivationKey))
            ChangeCamera(4);
        else if (Input.GetKeyDown(keys.staticCamera3ActivationKey))
            ChangeCamera(5);
        else if (Input.GetKeyDown(keys.mainCameraFollowPlayersKey))
            ToggleCameraFollowPlayers();
    }

    public void ChangeCamera(int cameraIndex)
    {
        //Disable all cameras
        mainCameraObj.SetActive(false);
        if (entryCameraObj != null) entryCameraObj.SetActive(false);
        if (godCameraObj != null) godCameraObj.SetActive(false);
        if (staticCamera1Obj != null) staticCamera1Obj.SetActive(false);
        if (staticCamera2Obj != null) staticCamera2Obj.SetActive(false);
        if (staticCamera3Obj != null) staticCamera3Obj.SetActive(false);

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
            case 3:
                if (staticCamera1Obj != null) currentCameraObj = staticCamera1Obj;
                break;
            case 4:
                if (staticCamera2Obj != null) currentCameraObj = staticCamera2Obj;
                break;
            case 5:
                if (staticCamera3Obj != null) currentCameraObj = staticCamera3Obj;
                break;
        }

        //Enable it and send event
        currentCameraObj.SetActive(true);
        currentCamera = currentCameraObj.GetComponent<Camera>();
        /*maxYPosition = currentCamera.pixelHeight - cameraBorderMargin;
        maxXPosition = currentCamera.pixelWidth - cameraBorderMargin;*/

        rsc.eventMng.TriggerEvent(EventManager.EventType.CAMERA_CHANGED, new CameraEventInfo { newCamera = currentCamera });
    }

    void ToggleCameraFollowPlayers()
    {
        MainCameraController script = mainCameraObj.GetComponent<MainCameraController>();
        script.enabled = !script.enabled;
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
}

using UnityEngine;
using System.Collections;

public class CamerasManager : MonoBehaviour {

    [HideInInspector]
    public GameObject currentCameraObj;
    [HideInInspector]
    public Camera currentCamera;

    public float xRotationMinThreshold = 25;

    public GameObject mainCameraObj;
    public GameObject leftCameraObj;
    public GameObject rightCameraObj;
    public GameObject godCameraObj;
    public GameObject staticCamera1Obj;
    public GameObject staticCamera2Obj;
    public GameObject staticCamera3Obj;

    private DebugKeys keys;

    private Transform target1;
    private Transform target2;
    private PlayerController player1;
    private PlayerController player2;
    private float camRayLength = 100f;

    private float cameraBorderMargin = 50f;
    private float maxYPosition;
    private float maxXPosition;
    private float minYPosition;
    private float minXPosition;

    void Awake()
    {
        currentCamera = mainCameraObj.GetComponent<Camera>();

        //We are sure rsc is created because we forced script execution order from unity editor - project settings
        rsc.camerasMng = this;
    }

    void Start()
    { 
        keys = rsc.debugMng.keys;
        target1 = rsc.gameInfo.player1.transform;
        player1 = rsc.gameInfo.player1Controller;

        target2 = rsc.gameInfo.player2.transform;
        player2 = rsc.gameInfo.player2Controller;

        minYPosition = cameraBorderMargin;
        minXPosition = cameraBorderMargin;

        leftCameraObj.GetComponent<SinglePlayerCameraController>().SetTarget(rsc.gameInfo.player1);
        rightCameraObj.GetComponent<SinglePlayerCameraController>().SetTarget(rsc.gameInfo.player2);

        //Set main camera
        ChangeCamera(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keys.mainCameraActivationKey))
            ChangeCamera(0);
        else if (Input.GetKeyDown(keys.godCameraActivationKey))
            ChangeCamera(1);
        else if (Input.GetKeyDown(keys.staticCamera1ActivationKey))
            ChangeCamera(2);
        else if (Input.GetKeyDown(keys.staticCamera2ActivationKey))
            ChangeCamera(3);
        else if (Input.GetKeyDown(keys.staticCamera3ActivationKey))
            ChangeCamera(4);
        else if (Input.GetKeyDown(keys.mainCameraFollowPlayersKey))
            ToggleCameraFollowPlayers();
    }

    void ChangeCamera(int cameraIndex)
    {
        //Disable all cameras
        mainCameraObj.SetActive(false);
        if (leftCameraObj != null) leftCameraObj.SetActive(false);
        if (rightCameraObj != null) rightCameraObj.SetActive(false);
        if (godCameraObj != null) godCameraObj.SetActive(false);
        if (staticCamera1Obj != null) staticCamera1Obj.SetActive(false);
        if (staticCamera2Obj != null) staticCamera2Obj.SetActive(false);
        if (staticCamera3Obj != null) staticCamera3Obj.SetActive(false);

        currentCameraObj = mainCameraObj;
        //Find selected camera
        switch (cameraIndex)
        {
            case 1:
                if (godCameraObj != null) currentCameraObj = godCameraObj;
                break;
            case 2:
                if (leftCameraObj != null) leftCameraObj.SetActive(true);
                if (rightCameraObj != null) rightCameraObj.SetActive(true);
                leftCameraObj.GetComponent<SinglePlayerCameraController>().SetTarget(rsc.gameInfo.player1);
                rightCameraObj.GetComponent<SinglePlayerCameraController>().SetTarget(rsc.gameInfo.player2);

                //if (staticCamera1Obj != null) currentCameraObj = staticCamera1Obj;
                break;
            case 3:
                if (staticCamera2Obj != null) currentCameraObj = staticCamera2Obj;
                break;
            case 4:
                if (staticCamera3Obj != null) currentCameraObj = staticCamera3Obj;
                break;
        }

        //Enable it and send event
        currentCameraObj.SetActive(true);
        currentCamera = currentCameraObj.GetComponent<Camera>();
        maxYPosition = currentCamera.pixelHeight - cameraBorderMargin;
        maxXPosition = currentCamera.pixelWidth - cameraBorderMargin;

        rsc.eventMng.TriggerEvent(EventManager.EventType.CAMERA_CHANGED, new CameraEventInfo { newCamera = currentCamera });
    }

    void ToggleCameraFollowPlayers()
    {
        MainCameraController script = mainCameraObj.GetComponent<MainCameraController>();
        script.enabled = !script.enabled;
    }

    public Vector3 GetDirection(Vector3 originalPosition, Vector3 displacement, int rayCastMask)
    {
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
        float magnitude = displacement.magnitude;
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
            direction *= magnitude;
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

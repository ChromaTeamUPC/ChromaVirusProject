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

    private DebugKeys keys;

    private float camRayLength = 100f;

    void Awake()
    {
        currentCameraObj = mainCameraObj;
        currentCamera = currentCameraObj.GetComponent<Camera>();

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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keys.mainCameraActivationKey))
            ChangeCamera(0);
        else if (Input.GetKeyDown(keys.godCameraActivationKey))
            ChangeCamera(2);
        else if (Input.GetKeyDown(keys.mainCameraFollowPlayersKey))
            ToggleCameraFollowPlayers();
    }

    public void SetEntryCameraLevelAnimation(int levelAnimation)
    {
        entryCameraObj.GetComponent<EntryCameraController>().SetLevelAnimation(levelAnimation);
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

    public bool IsObjectVisible(GameObject gameObject)
    {
        Vector3 screenPoint = currentCamera.WorldToViewportPoint(gameObject.transform.position);
        return screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }
}

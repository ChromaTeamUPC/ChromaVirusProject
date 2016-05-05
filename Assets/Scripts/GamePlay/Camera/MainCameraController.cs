using UnityEngine;
using System.Collections;

public class MainCameraController : MonoBehaviour {

    private Transform target1;
    private Transform target2;
    private PlayerController player1;
    private PlayerController player2;
    private int playerRayCastMask;

    public float smoothing = 5f;

    private Vector3 offset;
    private Camera thisCamera;
    private float cameraBorderMargin = 50f;
    float maxYPosition;
    float maxXPosition;
    float minYPosition;
    float minXPosition;

    void Start()
    {
        thisCamera = gameObject.GetComponent<Camera>();
        offset = rsc.gameInfo.gameCameraOffset;
        //gameObject.transform.rotation = rsc.gameInfo.gameCameraRotation;

        playerRayCastMask = LayerMask.GetMask("PlayerRayCast");
            
        maxYPosition = thisCamera.pixelHeight - cameraBorderMargin;
        maxXPosition = thisCamera.pixelWidth - cameraBorderMargin;
        minYPosition = cameraBorderMargin;
        minXPosition = cameraBorderMargin;

        target1 = rsc.gameInfo.player1.transform;
        player1 = rsc.gameInfo.player1Controller;

        target2 = rsc.gameInfo.player2.transform;
        player2 = rsc.gameInfo.player2Controller;      
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
	
	// Update is called once per frame
	void LateUpdate () 
    {
        transform.position = Vector3.Lerp(transform.position, GetCamTargetPosition(), smoothing * Time.deltaTime);       	
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

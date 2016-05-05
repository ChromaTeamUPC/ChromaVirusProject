using UnityEngine;
using System.Collections;

public class SinglePlayerCameraController : MonoBehaviour {

    public float smoothing = 5f;

    private Transform target;
    private PlayerController player;

    private Vector3 offset;

    // Use this for initialization
    void Start ()
    {
        offset = rsc.gameInfo.gameCameraOffset;
    }
	
    public void SetTarget(GameObject targetToFollow)
    {
        target = targetToFollow.transform;
        player = targetToFollow.GetComponent<PlayerController>();
        //transform.position = target.position + offset;
    }

	// Update is called once per frame
	void LateUpdate ()
    {
        if(player.Active)
            transform.position = Vector3.Lerp(transform.position, target.position + offset, smoothing * Time.deltaTime);
    }
}

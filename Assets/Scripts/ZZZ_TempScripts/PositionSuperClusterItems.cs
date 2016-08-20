using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class PositionSuperClusterItems : MonoBehaviour 
{
    public bool action = false;
    private Transform center;
    private Transform[] neigbours = new Transform[6]; 

	// Use this for initialization
	void Start () 
	{
        if (EditorApplication.isPlaying) return;
        Debug.Log("Script started");
        action = false;       
    }

    void Update()
    {
        if (EditorApplication.isPlaying) return;

        if(action)
        {
            center = transform.Find("ClusterCenter");

            neigbours[0] = transform.Find("ClusterTopLeft");
            neigbours[1] = transform.Find("ClusterTopRight");
            neigbours[2] = transform.Find("ClusterRight");
            neigbours[3] = transform.Find("ClusterBottomRight");
            neigbours[4] = transform.Find("ClusterBottomLeft");
            neigbours[5] = transform.Find("ClusterLeft");

            Debug.Log("Before positioning");
            Debug.Log("Center: " + center.localPosition);
            for (int i = 0; i < 6; ++i)
                Debug.Log(neigbours[i].name + ": " + neigbours[i].localPosition);


            Vector3 direction = Vector3.forward * HexagonController.DISTANCE_BETWEEN_HEXAGONS *2;
            Vector3 secondaryDirection = Vector3.forward * HexagonController.DISTANCE_BETWEEN_HEXAGONS;
            secondaryDirection = Quaternion.Euler(0, -60, 0) * secondaryDirection;
            direction += secondaryDirection;

            for (int i = 0; i < 3; ++i)
            {
                neigbours[i].localPosition = direction;

                direction = Quaternion.Euler(0, 60, 0) * direction;
            }

            direction = Vector3.forward * HexagonController.DISTANCE_BETWEEN_HEXAGONS * -2;
            secondaryDirection = Vector3.forward * HexagonController.DISTANCE_BETWEEN_HEXAGONS * -1;
            secondaryDirection = Quaternion.Euler(0, -60, 0) * secondaryDirection;
            direction += secondaryDirection;

            for (int i = 3; i < 6; ++i)
            {
                neigbours[i].localPosition = direction;

                direction = Quaternion.Euler(0, 60, 0) * direction;
            }

            Debug.Log("After positioning");
            Debug.Log("Center: " + center.localPosition);
            for (int i = 0; i < 6; ++i)
                Debug.Log(neigbours[i].name + ": " + neigbours[i].localPosition);

            Debug.Log("Done!");
            action = false;
        }
    }
}

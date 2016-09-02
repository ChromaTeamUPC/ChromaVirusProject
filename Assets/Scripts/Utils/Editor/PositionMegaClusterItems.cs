using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class PositionMegaClusterItems : MonoBehaviour 
{
    public bool action = false;
    private Transform center;
    private Transform[] neigbours = new Transform[6]; 

    void Update()
    {
        if (EditorApplication.isPlaying) return;

        if(action)
        {
            center = transform.Find("SuperClusterCenter");

            neigbours[0] = transform.Find("SuperClusterTop");
            neigbours[1] = transform.Find("SuperClusterTopRight");
            neigbours[2] = transform.Find("SuperClusterBottomRight");
            neigbours[3] = transform.Find("SuperClusterBottom");
            neigbours[4] = transform.Find("SuperClusterBottomLeft");
            neigbours[5] = transform.Find("SuperClusterTopLeft");

            Debug.Log("Before positioning");
            Debug.Log("Center: " + center.localPosition);
            for (int i = 0; i < 6; ++i)
                Debug.Log(neigbours[i].name + ": " + neigbours[i].localPosition);


            Vector3 direction = Vector3.forward * HexagonController.DISTANCE_BETWEEN_HEXAGONS * 7;

            for (int i = 0; i < 3; ++i)
            {
                neigbours[i].localPosition = direction;

                direction = Quaternion.Euler(0, 60, 0) * direction;
            }

            direction = Vector3.forward * HexagonController.DISTANCE_BETWEEN_HEXAGONS * -7;

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

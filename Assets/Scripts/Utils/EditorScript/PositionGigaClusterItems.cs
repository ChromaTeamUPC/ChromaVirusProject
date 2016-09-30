using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PositionGigaClusterItems : MonoBehaviour 
{
    public bool position = false;
    public bool set = false;
    public bool reset = false;
    public bool setHeight = false;

    public float minDistanceHexagons = 9;
    public float maxDistanceHexagons = 18;
    private float minDistance;
    private float maxDistance;
    public float probabilityAtStart = 40f;
    public float probabilityAtEnd = 5f;

    public float minHeight = -20;
    public float maxHeight = 20;

    private Transform center;
    private Transform[] neigbours = new Transform[6];

    void Update()
    {
        if (EditorApplication.isPlaying) return;

        if (position)
        {
            center = transform.Find("MegaClusterCenter");

            neigbours[0] = transform.Find("MegaClusterTopLeft");
            neigbours[1] = transform.Find("MegaClusterTopRight");
            neigbours[2] = transform.Find("MegaClusterRight");
            neigbours[3] = transform.Find("MegaClusterBottomRight");
            neigbours[4] = transform.Find("MegaClusterBottomLeft");
            neigbours[5] = transform.Find("MegaClusterLeft");

            Debug.Log("Before positioning");
            Debug.Log("Center: " + center.localPosition);
            for (int i = 0; i < 6; ++i)
                Debug.Log(neigbours[i].name + ": " + neigbours[i].localPosition);


            Vector3 direction = Vector3.forward * HexagonController.DISTANCE_BETWEEN_HEXAGONS * 7;
            Vector3 secondaryDirection = Vector3.forward * HexagonController.DISTANCE_BETWEEN_HEXAGONS * 14;
            secondaryDirection = Quaternion.Euler(0, -60, 0) * secondaryDirection;
            direction += secondaryDirection;

            for (int i = 0; i < 3; ++i)
            {
                neigbours[i].localPosition = direction;

                direction = Quaternion.Euler(0, 60, 0) * direction;
            }

            direction = Vector3.forward * HexagonController.DISTANCE_BETWEEN_HEXAGONS * -7;
            secondaryDirection = Vector3.forward * HexagonController.DISTANCE_BETWEEN_HEXAGONS * -14;
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
            position = false;
        }

        if(set)
        {
            DecorativeHexagonController[] hexagons = gameObject.GetComponentsInChildren<DecorativeHexagonController>(true);
            Debug.Log("Found hexagons: " + hexagons.Length);

            System.Random rand = new System.Random();

            minDistance = minDistanceHexagons * HexagonController.DISTANCE_BETWEEN_HEXAGONS;
            maxDistance = maxDistanceHexagons * HexagonController.DISTANCE_BETWEEN_HEXAGONS;
            Debug.Log("Min distance: " + minDistance);
            Debug.Log("Max distance: " + maxDistance);

            float newDistance = 0;
            float factor = 0;
            float activeChances = 0;
            double dice = 0;

            //First disable all hexagons
            for (int i = 0; i < hexagons.Length; ++i)
            {
                hexagons[i].isStatic = false;
                hexagons[i].gameObject.SetActive(false);
            }

            //Second set
            for (int i = 0; i < hexagons.Length; ++i)
            {
                newDistance = Vector3.Distance(transform.position, hexagons[i].transform.position);

                if (newDistance > maxDistance)
                {
                    activeChances = probabilityAtEnd;              
                }
                else if (newDistance > minDistance)
                {
                    factor = (newDistance - minDistance) / (maxDistance - minDistance);
                    activeChances = Mathf.Lerp(probabilityAtStart, probabilityAtEnd, factor);                   
                }
                else
                {
                    activeChances = 0;
                }

                if (activeChances > 0)
                {
                    dice = rand.NextDouble() * 100;

                    if (dice <= activeChances)
                    {
                        hexagons[i].gameObject.SetActive(true);                      
                    }
                }
            }

            set = false;
        }

        if (reset)
        {
            DecorativeHexagonController[] hexagons = gameObject.GetComponentsInChildren<DecorativeHexagonController>(true);

            for (int i = 0; i < hexagons.Length; ++i)
            {
                hexagons[i].gameObject.SetActive(true);
            }

            reset = false;
        }

        if (setHeight && false)
        {
            DecorativeHexagonController[] hexagons = gameObject.GetComponentsInChildren<DecorativeHexagonController>(true);

            System.Random rand = new System.Random();
            double newY = 0;
            float amplitude = maxHeight - minHeight;

            for (int i = 0; i < hexagons.Length; ++i)
            {
                newY = (rand.NextDouble() * amplitude) + minHeight;

                Vector3 newModelPos = new Vector3(0, (float)newY, 0);
                hexagons[i].column.transform.localPosition = newModelPos;
            }

            setHeight = false;
        }
    }
}

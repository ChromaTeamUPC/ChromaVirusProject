using UnityEngine;
using System.Collections;

public class HexagonController : MonoBehaviour 
{
    private enum Neighbour
    {
        TOP,
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM,
        BOTTOM_LEFT,
        BOTTOM_RIGHT
    }
    private const float DISTANCE_BETWEEN_HEXAGONS = 7.225f;
    private static int hexagonLayer = LayerMask.NameToLayer("Hexagon");

    private HexagonController[] Neighbours = new HexagonController[6];

    // Use this for initialization
    void Start () 
	{
        CheckNeighbours();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

    private void CheckNeighbours()
    {
        Vector3 direction = Vector3.forward * DISTANCE_BETWEEN_HEXAGONS;
        Collider[] colliders;

        if (Neighbours[(int)Neighbour.TOP] == null)
        {
            colliders = Physics.OverlapSphere(transform.position + direction, 1f, hexagonLayer);
            for (int i = 0; i < colliders.Length; ++i)
            {
                if (colliders[i].tag == "Hexagon")
                {
                    HexagonController neighbour = colliders[i].GetComponent<HexagonController>();
                    Neighbours[(int)Neighbour.TOP] = neighbour;

                    neighbour.SetNeighbour(this, Neighbour.BOTTOM);

                    break;
                }
            }
        }

        direction = Quaternion.Euler(0, 60, 0) * direction;

        if (Neighbours[(int)Neighbour.TOP_RIGHT] == null)
        {
            colliders = Physics.OverlapSphere(transform.position + direction, 1f, hexagonLayer);
            for (int i = 0; i < colliders.Length; ++i)
            {
                if (colliders[i].tag == "Hexagon")
                {
                    HexagonController neighbour = colliders[i].GetComponent<HexagonController>();
                    Neighbours[(int)Neighbour.TOP_RIGHT] = neighbour;

                    neighbour.SetNeighbour(this, Neighbour.BOTTOM_LEFT);

                    break;
                }
            }
        }

        direction = Quaternion.Euler(0, 60, 0) * direction;

        if (Neighbours[(int)Neighbour.BOTTOM_RIGHT] == null)
        {
            colliders = Physics.OverlapSphere(transform.position + direction, 1f, hexagonLayer);
            for (int i = 0; i < colliders.Length; ++i)
            {
                if (colliders[i].tag == "Hexagon")
                {
                    HexagonController neighbour = colliders[i].GetComponent<HexagonController>();
                    Neighbours[(int)Neighbour.BOTTOM_RIGHT] = neighbour;

                    neighbour.SetNeighbour(this, Neighbour.TOP_LEFT);

                    break;
                }
            }
        }
    }

    private void SetNeighbour(HexagonController neighbour, Neighbour position)
    {
        Neighbours[(int)position] = neighbour;
    }
}

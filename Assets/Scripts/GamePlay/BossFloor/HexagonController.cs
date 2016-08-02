using UnityEngine;
using System.Collections;

public class HexagonController : MonoBehaviour 
{
    private enum MovementState
    {
        IDLE,
        MOVING,
        RETURNING,
        FAST_RETURNING
    }

    private enum InfectionState
    {
        NORMAL,
        INFECTED
    }

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

    //Movement variables
    private MovementState movementState;
    public float movementMinWaitTime = 1f;
    public float movementMaxWaitTime = 3f;
    private bool up;
    private float totalMovement;
    private float currentMovement;
    public float upMinMovement = 1f;
    public float upMaxMovement = 5f;
    public float downMinMovement = 1f;
    public float downMaxMovement = 2f;
    public float movementSpeed = 5f;
    private bool canMove;

    private GameObject model;
    private float modelOriginalY;

    //Infection variables
    private InfectionState infectionState;
    public float infectionDuration = 5f;

    private Renderer rend;

    private Material originalMat;
    public Material testMat;
    public Material infectedMat;

    void Awake()
    {
        model = transform.FindDeepChild("Model").gameObject;
        modelOriginalY = model.transform.position.y;
        rend = GetComponentInChildren<Renderer>();
        originalMat = rend.sharedMaterial;
        canMove = true;
    }

    // Use this for initialization
    void Start () 
	{
        CheckNeighbours();
        StartCoroutine(TestMovement());
    }
	
	// Update is called once per frame
	void Update () 
	{
        //Movement control
        switch (movementState)
        {
            case MovementState.MOVING:
                if (currentMovement < totalMovement)
                {
                    float displacement = Time.deltaTime * movementSpeed;
                    if (up)
                        model.transform.position += new Vector3(0f, displacement, 0f);
                    else
                        model.transform.position -= new Vector3(0f, displacement, 0f);
                    currentMovement += displacement;
                }
                else
                    movementState = MovementState.RETURNING;
                break;

            case MovementState.RETURNING:
                if (currentMovement > 0f)
                {
                    float displacement = Time.deltaTime * movementSpeed;
                    if (up)
                    {
                        model.transform.position -= new Vector3(0f, displacement, 0f);
                        if(model.transform.position.y < modelOriginalY)
                            model.transform.position = new Vector3(model.transform.position.x, modelOriginalY, model.transform.position.z);
                    }
                    else
                    {
                        model.transform.position += new Vector3(0f, displacement, 0f);
                        if (model.transform.position.y > modelOriginalY)
                            model.transform.position = new Vector3(model.transform.position.x, modelOriginalY, model.transform.position.z);
                    }
                    currentMovement -= displacement;
                }
                else
                {
                    model.transform.position = new Vector3(model.transform.position.x, modelOriginalY, model.transform.position.z);
                    movementState = MovementState.IDLE;
                    StartCoroutine(TestMovement());
                }

                break;

            case MovementState.FAST_RETURNING:
                if (currentMovement > 0f)
                {
                    float displacement = Time.deltaTime * 20f;
                    if (up)
                    {
                        model.transform.position -= new Vector3(0f, displacement, 0f);
                        if (model.transform.position.y < modelOriginalY)
                            model.transform.position = new Vector3(model.transform.position.x, modelOriginalY, model.transform.position.z);
                    }
                    else
                    {
                        model.transform.position += new Vector3(0f, displacement, 0f);
                        if (model.transform.position.y > modelOriginalY)
                            model.transform.position = new Vector3(model.transform.position.x, modelOriginalY, model.transform.position.z);
                    }
                    currentMovement -= displacement;
                }
                else
                {
                    Vector3 original = new Vector3(model.transform.position.x, modelOriginalY, model.transform.position.z);
                    model.transform.position = original;
                    movementState = MovementState.IDLE;
                    //StartCoroutine(TestMovement());

                }
                break;
        }
    }

    //Test
    private IEnumerator TestMovement()
    {
        yield return new WaitForSeconds(Random.Range(movementMinWaitTime, movementMaxWaitTime));
        if(canMove && infectionState == InfectionState.NORMAL)
            StartMovement();
        else
            StartCoroutine(TestMovement());
    }

    private void SetMat()
    {
        rend.sharedMaterial = testMat;
    }

    //Movement methods
    public void StartMovement()
    {
        //Reset variables
        currentMovement = 0f;

        up = Random.Range(0f, 1f) >= 0.5f;
        if (up)
            totalMovement = Random.Range(upMinMovement, upMaxMovement);
        else
            totalMovement = Random.Range(downMinMovement, downMaxMovement);

        movementState = MovementState.MOVING;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "HexagonProbe")
        {
            canMove = false;
            if (movementState == MovementState.MOVING || movementState == MovementState.RETURNING)
            {
                movementState = MovementState.FAST_RETURNING;
                SetMat();
                Debug.Log("Going to fast return");
            }
        }
        else if (other.tag == "WormHead")
        {
            StartCoroutine(Infect());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "HexagonProbe")
        {
            canMove = true;
            if (infectionState == InfectionState.NORMAL)
            {
                rend.sharedMaterial = originalMat;
                StartCoroutine(TestMovement());
            }
        }
    }

    private IEnumerator Infect()
    {
        infectionState = InfectionState.INFECTED;
        rend.material = infectedMat;

        yield return new WaitForSeconds(infectionDuration);

        rend.material = originalMat;
        infectionState = InfectionState.NORMAL;
    }

    //Neighbour control
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

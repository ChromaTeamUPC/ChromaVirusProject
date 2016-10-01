using UnityEngine;
using System.Collections;

public class DecorativeHexagonController : MonoBehaviour 
{
    private static System.Random rand = new System.Random();

    private enum State
    {
        STATIC,
        IDLE, 
        MOVING,
        FLOATING
    }

    private State state;

    public bool isStatic = false;
    public bool forceStatic = false;

    //Movement variables
    [Header("Movement Settings")]
    public float chancesOfStatic = 15f;

    private float continousHMin = 0.5f;
    private float continouHsMax = 1.5f;
    private float speedMin = 1f;
    private float speedMax = 1.5f;

    private float continousH;
    private float speed;
    private float direction;


    public float movementMinWaitTime = 1f;
    public float movementMaxWaitTime = 3f;
    public float upMinMovement = 1f;
    public float upMaxMovement = 5f;
    public float downMinMovement = 1f;
    public float downMaxMovement = 2f;
    public float movementSpeed = 5f;

    [Header("Materials")]
    public Material floorStaticMat;
    public Material floorNonStaticMat;

    [Header("Objects")]
    public GameObject column;
    public Renderer columnRend;
    private GameObject geometryOffset;
    private float geometryOriginalY;

    private float moveWaitTime;
    private float elapsedTime;

    private float currentMovement;
    private bool up;
    private float totalMovement;
    private bool returning;

    void Awake()
    {
        geometryOffset = transform.FindDeepChild("GeometryOffset").gameObject;
        geometryOriginalY = geometryOffset.transform.position.y;        
    }

    // Use this for initialization
    void Start () 
	{
        if (forceStatic)
            isStatic = true;
        else
        {
            double dice;
            dice = rand.NextDouble() * 100;

            if (dice < chancesOfStatic)
                isStatic = true;
            else
                isStatic = false;
        }


        if (isStatic)
        {
            columnRend.sharedMaterial = floorStaticMat;
            //ResetMovement();

            /*if (up)
                geometryOffset.transform.position += new Vector3(0f, totalMovement, 0f);
            else
                geometryOffset.transform.position -= new Vector3(0f, totalMovement, 0f);*/

            state = State.STATIC;
        }
        /*else
        {
            //columnRend.sharedMaterial = floorNonStaticMat;
            columnRend.sharedMaterial = rsc.coloredObjectsMng.GetHexagonMaterial();
            ResetTime();
            state = State.IDLE;
        }*/
        else
        {
            columnRend.sharedMaterial = rsc.coloredObjectsMng.GetHexagonMaterial();

            float amplitude = continouHsMax - continousHMin;
            continousH = (float)(rand.NextDouble() * amplitude) + continousHMin;

            amplitude = speedMax - speedMin;
            speed = (float)(rand.NextDouble() * amplitude) + speedMin;

            direction = (rand.NextDouble() >= 0.5f? 1f: -1f);

            state = State.FLOATING;
        }
    }

    private void ResetTime()
    {
        moveWaitTime = Random.Range(movementMinWaitTime, movementMaxWaitTime);
        elapsedTime = 0f;
    }

    private void ResetMovement()
    {
        currentMovement = 0f;
        up = Random.Range(0f, 1f) >= 0.5f;
        if (up)
            totalMovement = Random.Range(upMinMovement, upMaxMovement);
        else
            totalMovement = Random.Range(downMinMovement, downMaxMovement);

        returning = false;
    }

    // Update is called once per frame
    void Update () 
	{
        switch (state)
        {
            case State.IDLE:
                if (elapsedTime >= moveWaitTime)
                {
                    ResetMovement();

                    state = State.MOVING;
                }
                else
                {
                    elapsedTime += Time.deltaTime;
                }
                break;

            case State.MOVING:
                if (!returning)
                {
                    if (currentMovement < totalMovement)
                    {
                        float displacement = Time.deltaTime * movementSpeed;

                        if (up)
                            geometryOffset.transform.position += new Vector3(0f, displacement, 0f);
                        else
                            geometryOffset.transform.position -= new Vector3(0f, displacement, 0f);
                        currentMovement += displacement;
                    }
                    else
                        returning = true;
                }
                else
                {
                    if (currentMovement > 0f)
                    {
                        float displacement = Time.deltaTime * movementSpeed;
                        if (up)
                        {
                            geometryOffset.transform.position -= new Vector3(0f, displacement, 0f);
                            if (geometryOffset.transform.position.y < geometryOriginalY)
                                geometryOffset.transform.position = new Vector3(geometryOffset.transform.position.x, geometryOriginalY, geometryOffset.transform.position.z);
                        }
                        else
                        {
                            geometryOffset.transform.position += new Vector3(0f, displacement, 0f);
                            if (geometryOffset.transform.position.y > geometryOriginalY)
                                geometryOffset.transform.position = new Vector3(geometryOffset.transform.position.x, geometryOriginalY, geometryOffset.transform.position.z);
                        }
                        currentMovement -= displacement;
                    }
                    else
                    {
                        geometryOffset.transform.position = new Vector3(geometryOffset.transform.position.x, geometryOriginalY, geometryOffset.transform.position.z);

                        ResetTime();

                        state = State.IDLE;
                    }
                }
                break;

            case State.FLOATING:

                float currHeight = Mathf.Sin(Time.time * speed) * continousH * direction;
                geometryOffset.transform.position = new Vector3(geometryOffset.transform.position.x, currHeight, geometryOffset.transform.position.z);

                break;
        }
    }
}

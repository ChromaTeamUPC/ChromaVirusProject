using UnityEngine;
using System.Collections;

public class MeteorController : MonoBehaviour 
{
    public enum SubState
    {
        GOING_UP,
        FALLING,
        EXPLODING
    }

    public float upSpeed = 30f;
    public float upDuration = 3f;

    private SubState subState;
    private float elapsedTime;

	// Update is called once per frame
	void Update () 
	{
        switch (subState)
        {
            case SubState.GOING_UP:
                if(elapsedTime >= upDuration)
                {
                    rsc.poolMng.meteorPool.AddObject(this);
                }
                else
                {
                    transform.Translate(0f, upSpeed * Time.deltaTime, 0f, Space.World);
                    elapsedTime += Time.deltaTime;
                }
                break;

            case SubState.FALLING:
                //Do nothing. Position controlled by hexagon
                break;

            case SubState.EXPLODING:
                //TODO: FX, animations
                rsc.poolMng.meteorPool.AddObject(this);
                break;
            default:
                break;
        }
    }

    void OnEnable()
    {
        //Default state is falling
        subState = SubState.FALLING;
    }


    public void GoUp(float speed)
    {
        upSpeed = speed;
        elapsedTime = 0f;
        subState = SubState.GOING_UP;
    }

    public void Explode()
    {
        subState = SubState.EXPLODING;
    }
}

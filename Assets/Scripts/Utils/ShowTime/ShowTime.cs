using UnityEngine;
using System.Collections;

public class ShowTime : MonoBehaviour
{

    [Header("ShowTime")]
    public float showTimeMin = 15f;
    public float showTimeMax = 25f;
    private float showTime = 10f;
    private bool showTimeEnabled = true;
    private float showElapsedTime = 0f;
    private float elapsedTime;
    public Transform kdtHandle;
    public Transform kdt;
    public Animator kdtAnimator;
    public Material matColor;
    public Material matFace;
    public Material matShield;
    public GameObject kdtShield;
    private ChromaColor currentColor;
    private float totalChances;

    [Header("ShowTime Fall")]
    public float fallChances = 33f;
    public float fallInitialXRange = 7f;
    public float fallInitialY = 8f;
    public float fallFinalY = -8f;
    public float fallMinYRotation = 0f;
    public float fallMaxYRotation = 360f;
    public float fallMinSpeed = 3f;
    public float fallMaxSpeed = 5f;

    [Header("ShowTime Pass")]
    public float passChances = 33f;
    public float passInitialMinY = -3;
    public float passInitialMaxY = 1.25f;
    public float passLeftX = -10f;
    public float passRightX = 10f;
    public float passRotation = 115f;
    public float passMinSpeed = 3f;
    public float passMaxSpeed = 6f;
    public float passWaveMinCycleTime = 1f;
    public float passWaveMaxCycleTime = 3f;
    private float passWaveCycleTime = 2f;
    public float passWaveMinAmplitude = 0.4f;
    public float passWaveMaxAmplitude = 1.25f;
    private float passWaveAmplitude = 0.5f;
    public float passDizzyWaveMinCycleTime = 1f;
    public float passDizzyWaveMaxCycleTime = 3f;
    private float passDizzyWaveCycleTime = 2f;
    public float passDizzyWaveMinAmplitude = 0.4f;
    public float passDizzyWaveMaxAmplitude = 1.25f;
    private float passDizzyWaveAmplitude = 0.5f;
    public float chancesToBackwards = 40f;
    public float chancesToSpin = 100f;
    public float spinMinX = -7f;
    public float spinMaxX = 7f;

    [Header("ShowTime Swim")]
    public float swimChances = 33f;
    public float swimInitialMinY = -3;
    public float swimInitialMaxY = 1.25f;
    public float swimLeftX = -10f;
    public float swimRightX = 10f;
    public float swimRotation = 115f;
    public float swimMinSpeed = 3f;
    public float swimMaxSpeed = 6f;
    public float swimCrawlAngle = -55f;
    public float swimBackstrokeAngle = 90f;
    public float swimChangesToReturn = 50f;

    [Header("Showtime Fly")]
    public float flyChances = 33f;
    public float flyInitialXRange = 7f;
    public float flyInitialY = -8f;
    public float flyFinalY = 8f;
    public float flyMinYRotation = 170f;
    public float flyMaxYRotation = 190f;
    public float flyMinSpeed = 3f;
    public float flyMaxSpeed = 5f;
    public float chancesToBlink = 100f;
    public float blinkMinY = -1.75f;
    public float blinkMaxY = 1.4f;
    public float blinkStoppingTime = 0.25f;
    public float blinkStartingTime = 0.25f;
    public float blinkTime = 0.25f;

    // Use this for initialization
    void Start()
    {
        showTimeEnabled = true;
        showElapsedTime = 0f;
        showTime = Random.Range(showTimeMin / 2, showTimeMax / 2); //First time shows earlier
        currentColor = ChromaColorInfo.Random;
        totalChances = fallChances + passChances + swimChances + flyChances;
    }

    // Update is called once per frame
    void Update()
    {
        if (showTimeEnabled)
        {
            showElapsedTime += Time.deltaTime;
            if (showElapsedTime >= showTime)
            {
                //Pick a random color
                ChromaColor newColor = ChromaColorInfo.Random;
                //Force new color every time
                while (currentColor == newColor)
                    newColor = ChromaColorInfo.Random;

                currentColor = newColor;

                Color color = rsc.coloredObjectsMng.GetPlayerColor(newColor);

                matColor.SetColor("_EmissionColor", color);
                matShield.SetColor("_EmissionColor", color);

                showElapsedTime = 0f;
                showTimeEnabled = false;
                showTime = Random.Range(showTimeMin, showTimeMax);

                //Pick a random effect
                float dice = Random.Range(0f, totalChances);

                if (dice <= fallChances)
                    StartCoroutine(KDTFall());
                else if (dice <= fallChances + passChances)
                    StartCoroutine(KDTPass());
                else if (dice <= fallChances + passChances + swimChances)
                    StartCoroutine(KDTSwim());
                else
                    StartCoroutine(KDTFly());

            }
        }
    }

    IEnumerator KDTFall()
    {
        kdtHandle.gameObject.SetActive(true);
        float speed = Random.Range(fallMinSpeed, fallMaxSpeed);

        //Position
        float x = Random.Range(fallInitialXRange * -1, fallInitialXRange);
        kdtHandle.position = new Vector3(x, fallInitialY, 0f);

        //Rotation
        kdtHandle.localEulerAngles = new Vector3(0f, 0f, 0f);
        float yRot = Random.Range(fallMinYRotation, fallMaxYRotation);
        kdt.localEulerAngles = new Vector3(0, yRot, 0);

        //Face
        matFace.mainTextureOffset = new Vector2(0.33f, 0);

        //Shield
        kdtShield.SetActive(false);

        //Animation
        kdtAnimator.SetInteger("Animation", 1);

        yield return null;

        while (kdt.position.y > fallFinalY)
        {
            kdtHandle.Translate(0f, speed * Time.deltaTime * -1, 0f, Space.World);
            yield return null;
        }

        kdtHandle.gameObject.SetActive(false);
        showTimeEnabled = true;
    }

    private enum PassStates
    {
        WALKING_1,
        SPINNING,
        HIT,
        WALKING_2
    }

    IEnumerator KDTPass()
    {
        kdtHandle.gameObject.SetActive(true);
        float speed = Random.Range(passMinSpeed, passMaxSpeed);

        //Face
        matFace.mainTextureOffset = new Vector2(0f, 0f);

        //Shield
        kdtShield.SetActive(true);

        float maxDistance = swimRightX - swimLeftX;
        float totalDistance = 0f;

        float y = Random.Range(passInitialMinY, passInitialMaxY);

        int speedDirection;
        float speedFactor;
        int animation;
        float kdtBodyAngle;
        float initialXPos;


        bool leftToRight = Random.value > 0.5f;
        if (leftToRight)
        {
            speedDirection = 1;
            initialXPos = passLeftX;
        }
        else
        {
            speedDirection = -1;
            initialXPos = passRightX;
        }

        bool forward = Random.Range(0f, 100f) > chancesToBackwards;
        if (forward)
        {
            animation = 2;
            speedFactor = 1f;
            kdtBodyAngle = passRotation * speedDirection;
        }
        else
        {
            animation = 6;
            speedFactor = 0.75f;
            kdtBodyAngle = passRotation * speedDirection * -1;
        }

        //Position & Rotation
        kdtHandle.localEulerAngles = new Vector3(0f, 0f, 0f);
        kdtHandle.position = new Vector3(initialXPos, y, 0f);
        kdt.localEulerAngles = new Vector3(0, kdtBodyAngle, 0);

        //Animation
        kdtAnimator.SetInteger("Animation", animation);

        //Spin
        bool spin = forward && (Random.Range(0, 100) <= chancesToSpin);
        float spinX = Random.Range(spinMinX, spinMaxX) - passLeftX;
        PassStates state = PassStates.WALKING_1;

        //Wave
        passWaveCycleTime = Random.Range(passWaveMinCycleTime, passWaveMaxCycleTime);
        passWaveAmplitude = Random.Range(passWaveMinAmplitude, passWaveMaxAmplitude);

        passDizzyWaveCycleTime = Random.Range(passDizzyWaveMinCycleTime, passDizzyWaveMaxCycleTime);
        passDizzyWaveAmplitude = Random.Range(passDizzyWaveMinAmplitude, passDizzyWaveMaxAmplitude);

        yield return null;

        float displacement;
        float elapsedTime = 0f;
        float newX = 0f, newY = 0f;

        while (totalDistance < maxDistance)
        {
            switch (state)
            {
                case PassStates.WALKING_1:
                    displacement = speed * speedFactor * Time.deltaTime;
                    totalDistance += displacement;

                    newX = kdtHandle.position.x + (displacement * speedDirection);
                    newY = y + (Mathf.Sin(elapsedTime / passWaveCycleTime * Mathf.PI * 2) * passWaveAmplitude);
                    kdtHandle.position = new Vector3(newX, newY, kdtHandle.position.z);

                    elapsedTime += Time.deltaTime;

                    if (spin && totalDistance >= spinX)
                    {
                        state = PassStates.SPINNING;
                        kdtAnimator.SetInteger("Animation", 3);
                        matFace.mainTextureOffset = new Vector2(0.33f, 0.71f);
                        elapsedTime = 0;
                    }
                    break;

                case PassStates.SPINNING:

                    if (elapsedTime >= 2.5f)
                    {
                        state = PassStates.HIT;
                        kdtAnimator.SetInteger("Animation", 7);
                        matFace.mainTextureOffset = new Vector2(0f, 0.7f);
                        elapsedTime = 0f;
                        kdtShield.SetActive(false);
                    }
                    else
                    {
                        elapsedTime += Time.deltaTime;
                    }
                    break;

                case PassStates.HIT:

                    if (elapsedTime >= 0.5f)
                    {
                        state = PassStates.WALKING_2;
                        kdtAnimator.SetInteger("Animation", 10);
                        matFace.mainTextureOffset = new Vector2(0f, 0.41f);
                        kdtShield.SetActive(true);
                        elapsedTime = 0f;
                        y = kdtHandle.position.y;
                    }
                    else
                    {
                        elapsedTime += Time.deltaTime;
                    }
                    break;

                case PassStates.WALKING_2:
                    displacement = speed * Time.deltaTime * 0.5f;
                    totalDistance += displacement;

                    newX = kdtHandle.position.x + (displacement * speedDirection);
                    newY = y + (Mathf.Sin(elapsedTime / passDizzyWaveCycleTime * Mathf.PI * 2) * passDizzyWaveAmplitude);
                    kdtHandle.position = new Vector3(newX, newY, kdtHandle.position.z);

                    elapsedTime += Time.deltaTime;

                    //kdtHandle.Translate(displacement * speedDirection, Mathf.Sin(totalDistance) * Time.deltaTime * 3, 0f, Space.World);
                    break;
                default:
                    break;
            }

            yield return null;
        }

        kdtHandle.gameObject.SetActive(false);
        showTimeEnabled = true;
    }

    IEnumerator KDTSwim()
    {
        kdtHandle.gameObject.SetActive(true);
        float speed = Random.Range(swimMinSpeed, swimMaxSpeed);

        //Face
        matFace.mainTextureOffset = new Vector2(0.66f, 0f);

        //Shield
        kdtShield.SetActive(false);

        float maxDistance = swimRightX - swimLeftX;
        float totalDistance = 0f;

        float y = Random.Range(swimInitialMinY, swimInitialMaxY);

        int speedDirection;
        float speedFactor;
        int animation;
        float kdtHandleAngle;
        float kdtBodyAngle;
        float initialXPos;


        bool leftToRight = Random.value > 0.5f;
        if (leftToRight)
        {
            speedDirection = 1;
            initialXPos = swimLeftX;
        }
        else
        {
            speedDirection = -1;
            initialXPos = swimRightX;
        }

        bool crawl = Random.value > 0.5f;
        if (crawl)
        {
            animation = 4;
            speedFactor = 1f;
            kdtHandleAngle = swimCrawlAngle * speedDirection;
            kdtBodyAngle = swimRotation * speedDirection;
        }
        else
        {
            animation = 5;
            speedFactor = 0.75f;
            kdtHandleAngle = swimBackstrokeAngle * speedDirection;
            kdtBodyAngle = swimRotation * speedDirection * -1;
        }


        //Position & Rotation
        kdtHandle.localEulerAngles = new Vector3(0f, 0f, kdtHandleAngle);
        kdtHandle.position = new Vector3(initialXPos, y, 0f);
        kdt.localEulerAngles = new Vector3(0, kdtBodyAngle, 0);

        //Animation
        kdtAnimator.SetInteger("Animation", animation);

        yield return null;

        float displacement;

        while (totalDistance < maxDistance)
        {
            displacement = speed * speedFactor * Time.deltaTime;
            totalDistance += displacement;
            kdtHandle.Translate(displacement * speedDirection, 0f, 0f, Space.World);

            yield return null;
        }

        if (Random.Range(0f, 100f) <= swimChangesToReturn)
        {
            totalDistance = 0f;

            leftToRight = !leftToRight;
            if (leftToRight)
            {
                speedDirection = 1;
                initialXPos = swimLeftX;
            }
            else
            {
                speedDirection = -1;
                initialXPos = swimRightX;
            }

            crawl = Random.value > 0.5f;
            if (crawl)
            {
                animation = 4;
                kdtHandleAngle = swimCrawlAngle * speedDirection;
                kdtBodyAngle = swimRotation * speedDirection;
            }
            else
            {
                animation = 5;
                kdtHandleAngle = swimBackstrokeAngle * speedDirection;
                kdtBodyAngle = swimRotation * speedDirection * -1;
            }


            //Position & Rotation
            kdtHandle.localEulerAngles = new Vector3(0f, 0f, kdtHandleAngle);
            kdtHandle.position = new Vector3(initialXPos, y, 0f);
            kdt.localEulerAngles = new Vector3(0, kdtBodyAngle, 0);

            //Animation
            kdtAnimator.SetInteger("Animation", animation);

            while (totalDistance < maxDistance)
            {
                displacement = speed * speedFactor * Time.deltaTime;
                totalDistance += displacement;
                kdtHandle.Translate(displacement * speedDirection, 0f, 0f, Space.World);

                yield return null;
            }
        }

        kdtHandle.gameObject.SetActive(false);
        showTimeEnabled = true;
    }

    private enum FlyStates
    {
        FLYING_1,
        STOPPING,
        BLINK,
        STARTING,
        FLYING_2
    }

    IEnumerator KDTFly()
    {
        kdtHandle.gameObject.SetActive(true);
        float maxSpeed = Random.Range(flyMinSpeed, flyMaxSpeed);
        float currentSpeed = maxSpeed;

        //Position
        float x = Random.Range(flyInitialXRange * -1, flyInitialXRange);
        kdtHandle.position = new Vector3(x, flyInitialY, 0f);

        //Rotation
        kdtHandle.localEulerAngles = new Vector3(0f, 0f, 0f);
        float yRot = Random.Range(flyMinYRotation, flyMaxYRotation);
        kdt.localEulerAngles = new Vector3(0, yRot, 0);

        //Face
        matFace.mainTextureOffset = new Vector2(0f, 0);

        //Shield
        kdtShield.SetActive(false);

        //Animation
        kdtAnimator.SetInteger("Animation", 8);

        bool blink = Random.Range(0f, 100f) <= chancesToBlink;
        float blinkY = Random.Range(blinkMinY, blinkMaxY);

        FlyStates state = FlyStates.FLYING_1;

        yield return null;

        float elapsedTime = 0f;

        while (kdt.position.y < flyFinalY)
        {
            kdtHandle.Translate(0f, maxSpeed * Time.deltaTime, 0f, Space.World);
            yield return null;

            switch (state)
            {
                case FlyStates.FLYING_1:
                    kdtHandle.Translate(0f, maxSpeed * Time.deltaTime, 0f, Space.World);

                    if (blink && kdtHandle.position.y >= blinkY)
                    {
                        state = FlyStates.STOPPING;

                        //matFace.mainTextureOffset = new Vector2(-0.34f, -0.3f);
                        elapsedTime = 0f;
                    }
                    break;

                case FlyStates.STOPPING:
                    if (elapsedTime < blinkStoppingTime)
                    {
                        elapsedTime += Time.deltaTime;

                        currentSpeed = maxSpeed * Mathf.Cos((elapsedTime / blinkStoppingTime) * (Mathf.PI / 2));

                        kdtHandle.Translate(0f, currentSpeed * Time.deltaTime, 0f, Space.World);
                    }
                    else
                    {
                        kdtAnimator.SetInteger("Animation", 9);
                        state = FlyStates.BLINK;
                    }
                    break;

                case FlyStates.BLINK:
                    yield return new WaitForSeconds(0.25f);
                    matFace.mainTextureOffset = new Vector2(-0.34f, -0.3f);
                    yield return new WaitForSeconds(blinkTime);
                    matFace.mainTextureOffset = new Vector2(0f, 0f);
                    yield return new WaitForSeconds(0.25f);
                    state = FlyStates.STARTING;
                    kdtAnimator.SetInteger("Animation", 8);
                    elapsedTime = 0f;
                    break;

                case FlyStates.STARTING:
                    if (elapsedTime < blinkStartingTime)
                    {
                        elapsedTime += Time.deltaTime;

                        currentSpeed = maxSpeed * Mathf.Sin((elapsedTime / blinkStartingTime) * (Mathf.PI / 2));

                        kdtHandle.Translate(0f, currentSpeed * Time.deltaTime, 0f, Space.World);
                    }
                    else
                    {
                        state = FlyStates.FLYING_2;
                    }
                    break;

                case FlyStates.FLYING_2:
                    kdtHandle.Translate(0f, maxSpeed * Time.deltaTime, 0f, Space.World);
                    break;

                default:
                    break;
            }

            yield return null;
        }

        kdtHandle.gameObject.SetActive(false);
        showTimeEnabled = true;
    }
}

using UnityEngine;
using System.Collections;

public class BackgroundAnimationScript : MonoBehaviour
{

    private float elapsedTime;

    [Header("Background")]
    public RectTransform background;
    public bool bkgScale = true;
    public float bkgMinScale = 1.0f;
    public float bkgMaxScale = 1.0f;
    public float bkgScaleFullCicleTime = 10.0f;
    private float bkgScaleMiddleValue;
    private float bkgScaleHalfRange;
    private float bkgScaleInitialTime;

    [Header("Left Triangles")]
    public RectTransform leftTriangles;
    public bool ltScale = true;
    public float ltMinScale = 1.0f;
    public float ltMaxScale = 1.0f;
    public float ltScaleFullCycleTime = 20.0f;
    private float ltScaleMiddleValue;
    private float ltScaleHalfRange;
    private float ltScaleInitialTime;

    public bool ltRotate = true;
    public bool ltRotateClockWise = false;
    public float ltRotateMinDPS = 3f;
    public float ltRotateMaxDPS = 5f;
    private float ltRotateDPS;

    public bool ltTranslateVert = true;
    public float ltTranslateVertMaxDisplacement = 0.5f;
    public float ltTranslateVertFullCycleTime = 7f;
    private float ltTransVertHalfDisplacement;
    private float ltTransVertOriginalY;
    private float ltTransVertInitialTime;

    [Header("Right Triangles")]
    public RectTransform rightTriangles;
    public bool rtScale = true;
    public float rtMinScale = 1.0f;
    public float rtMaxScale = 1.0f;
    public float rtScaleFullCycleTime = 18.0f;
    private float rtScaleMiddleValue;
    private float rtScaleHalfRange;
    private float rtScaleInitialTime;

    public bool rtRotate = true;
    public bool rtRotateClockWise = false;
    public float rtRotateMinDPS = 3f;
    public float rtRotateMaxDPS = 5f;
    private float rtRotateDPS;

    public bool rtTranslateVert = true;
    public float rtTranslateVertMaxDisplacement = 0.5f;
    public float rtTranslateVertFullCycleTime = 8f;
    private float rtTransVertHalfDisplacement;
    private float rtTransVertOriginalY;
    private float rtTransVertInitialTime;

    // Use this for initialization
    void Start()
    {
        elapsedTime = 0f;

        //Background
        bkgScaleMiddleValue = (bkgMinScale + bkgMaxScale) / 2;
        bkgScaleHalfRange = (bkgMaxScale - bkgMinScale) / 2; ;
        bkgScaleInitialTime = Random.Range(0f, bkgScaleFullCicleTime);

        //Left triangles
        ltScaleMiddleValue = (ltMinScale + ltMaxScale) / 2;
        ltScaleHalfRange = (ltMaxScale - ltMinScale) / 2; ;
        ltScaleInitialTime = Random.Range(0f, ltScaleFullCycleTime);

        ltRotateDPS = Random.Range(ltRotateMinDPS, ltRotateMaxDPS) * (ltRotateClockWise ? -1 : 1);

        ltTransVertOriginalY = leftTriangles.position.y;
        ltTransVertHalfDisplacement = ltTranslateVertMaxDisplacement / 2;
        ltTransVertInitialTime = Random.Range(0f, ltTranslateVertFullCycleTime);

        //Right triangles
        rtScaleMiddleValue = (rtMinScale + rtMaxScale) / 2;
        rtScaleHalfRange = (rtMaxScale - rtMinScale) / 2; ;
        rtScaleInitialTime = Random.Range(0f, rtScaleFullCycleTime);

        rtRotateDPS = Random.Range(rtRotateMinDPS, rtRotateMaxDPS) * (rtRotateClockWise ? -1 : 1);

        rtTransVertOriginalY = rightTriangles.position.y;
        rtTransVertHalfDisplacement = rtTranslateVertMaxDisplacement / 2;
        rtTransVertInitialTime = Random.Range(0f, rtTranslateVertFullCycleTime);
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (bkgScale)
        {
            float factor = Mathf.Sin((elapsedTime + bkgScaleInitialTime) / bkgScaleFullCicleTime * Mathf.PI * 2);
            float newBkgScale = bkgScaleMiddleValue + (bkgScaleHalfRange * factor);
            background.localScale = new Vector3(newBkgScale, newBkgScale, 1f);
        }

        if (ltScale)
        {
            float factor = Mathf.Sin((elapsedTime + ltScaleInitialTime) / ltScaleFullCycleTime * Mathf.PI * 2);
            float newBkgScale = ltScaleMiddleValue + (ltScaleHalfRange * factor);
            leftTriangles.localScale = new Vector3(newBkgScale, newBkgScale, 1f);
        }

        if (ltRotate)
        {
            leftTriangles.Rotate(0f, 0f, ltRotateDPS * Time.deltaTime, Space.Self);
        }

        if(ltTranslateVert)
        {
            float factor = Mathf.Sin((elapsedTime + ltTransVertInitialTime) / ltTranslateVertFullCycleTime * Mathf.PI * 2);
            float newLtVertPos = ltTransVertOriginalY + (ltTransVertHalfDisplacement * factor);
            leftTriangles.position = new Vector3(leftTriangles.position.x, newLtVertPos, leftTriangles.position.z);
        }

        if (rtScale)
        {
            float factor = Mathf.Sin((elapsedTime + rtScaleInitialTime) / rtScaleFullCycleTime * Mathf.PI * 2);
            float newBkgScale = rtScaleMiddleValue + (rtScaleHalfRange * factor);
            rightTriangles.localScale = new Vector3(newBkgScale, newBkgScale, 1f);
        }

        if (rtRotate)
        {
            rightTriangles.Rotate(0f, 0f, rtRotateDPS * Time.deltaTime, Space.Self);
        }

        if (rtTranslateVert)
        {
            float factor = Mathf.Sin((elapsedTime + rtTransVertInitialTime) / rtTranslateVertFullCycleTime * Mathf.PI * 2);
            float newRtVertPos = rtTransVertOriginalY + (rtTransVertHalfDisplacement * factor);
            rightTriangles.position = new Vector3(rightTriangles.position.x, newRtVertPos, rightTriangles.position.z);
        }

    }
}

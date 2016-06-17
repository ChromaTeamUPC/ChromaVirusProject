using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundImgScript : MonoBehaviour {

    public Color[] colors;

    public float changeEverySeconds = 3f;
    public float transitionDuration = 0.5f;

    private float changeTime = 0f;
    private int currentColor;
    private Image img;

    /*public float speed;
    public bool direction;
    private float currentHueValue;
    private float saturation;
    private float brigtness;*/

	// Use this for initialization
	void Start () {
        img = gameObject.GetComponent<Image>();
        changeTime = 0f;

        currentColor = Random.Range(0, colors.Length);
        img.color = colors[currentColor];   

        //Color.RGBToHSV(img.color, out currentHueValue, out saturation, out brigtness);
    }
	
	// Update is called once per frame
	void Update () {

        if (changeTime >= changeEverySeconds)
        {
            ++currentColor;
            if (currentColor >= colors.Length)
                currentColor = 0;

            StartCoroutine(ChangeColor(colors[currentColor]));
            changeTime = 0f;
        }
        else
            changeTime += Time.deltaTime;


        /*float deltaHue = speed * Time.deltaTime;

        if(direction)
        {
            currentHueValue += deltaHue;

            //To loop
            while (currentHueValue > 1.0f)
                currentHueValue -= 1.0f;
        } 
        else
        {
            currentHueValue -= deltaHue;

            //To loop
            while (currentHueValue < 0.0f)
                currentHueValue += 1.0f;
        }

        img.color = Color.HSVToRGB(currentHueValue, saturation, brigtness);*/
	}

    private IEnumerator ChangeColor(Color to)
    {
        float elapsedTime = 0f;
        Color from = img.color;

        while (elapsedTime <= transitionDuration)
        {
            img.color = Color.Lerp(from, to, elapsedTime * (1/transitionDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        img.color = to;
    }
}

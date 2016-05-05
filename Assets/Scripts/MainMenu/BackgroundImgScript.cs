using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundImgScript : MonoBehaviour {

    public float speed;
    public bool direction;
    private Image img;
    private float currentHueValue;
    private float saturation;
    private float brigtness;

	// Use this for initialization
	void Start () {
        img = gameObject.GetComponent<Image>();
        Color.RGBToHSV(img.color, out currentHueValue, out saturation, out brigtness);
	}
	
	// Update is called once per frame
	void Update () {
        float deltaHue = speed * Time.deltaTime;

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

        img.color = Color.HSVToRGB(currentHueValue, saturation, brigtness);
	}
}

using UnityEngine;
using System.Collections;

public class floatingTextController : MonoBehaviour {

    private static floatingText popupText;
    private static GameObject canvas;

    public static void init()
    {
        canvas = GameObject.Find("Canvas");
        popupText = Resources.Load<floatingText>("popupTextParent");             
    }

    public static void createFloatingText(string text, Transform location)
    {
        floatingText instance = Instantiate(popupText);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(location.position);
        instance.transform.SetParent(canvas.transform, false);
        instance.transform.position = screenPosition;
        instance.setText(text);
    }

}

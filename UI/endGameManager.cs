using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class endGameManager : MonoBehaviour {

    public static string message = "";
    public Text msgText;

	void Start () {
        msgText.text = message;
	}
	

    public void returnClick()
    {
        SceneManager.LoadScene("Menu");
    }

}

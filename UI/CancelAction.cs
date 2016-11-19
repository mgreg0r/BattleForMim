using UnityEngine;
using System.Collections;

public class CancelAction : MonoBehaviour {

	public void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {

        }
        else if(Input.GetMouseButtonDown(1))
        {
            GameEngine.cancelAction();
        }
    }
}

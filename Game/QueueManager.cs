using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class QueueManager : MonoBehaviour {

	void Update () {
        string msg = NetworkEngine.messageQueue.get();
        if (msg.Equals("2"))
        {
            MainMenu.gameFound();
            SceneManager.LoadScene("gameRoom");
        }
    }
}

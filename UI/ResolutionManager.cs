using UnityEngine;
using System.Collections;

public class ResolutionManager : MonoBehaviour {

    public float screenHeight = 1920f;
    public float screenWidth = 1080f;
    public float targetAspect = 9f / 16f;
    public float orthographicSize;
    private Camera mainCamera;

    void Start()
    {

        mainCamera = Camera.main;
        orthographicSize = mainCamera.orthographicSize;

        float orthoWidth = orthographicSize / screenHeight * screenWidth;
        orthoWidth = orthoWidth / (targetAspect / mainCamera.aspect);
        Camera.main.orthographicSize = (orthoWidth / Screen.width * Screen.height);
    }
}

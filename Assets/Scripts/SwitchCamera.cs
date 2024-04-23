using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCamera : MonoBehaviour
{

    public GameObject mainCamera;
    public GameObject secondaryCamera;
    public bool switchCam;

    void Start()
    {
        mainCamera.SetActive(true);
        secondaryCamera.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (switchCam)
            {
                secondaryCamera.SetActive(true);
                mainCamera.SetActive(false);
                switchCam = false;
            }
            else
            {
                secondaryCamera.SetActive(false);
                mainCamera.SetActive(true);
                switchCam = true;
            }
    }
}
}

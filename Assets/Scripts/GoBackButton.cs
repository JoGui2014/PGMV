using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GoBackButton : MonoBehaviour
{
    public GameObject mainCamera;
    public Button button;

    void Start() {
        button.onClick.AddListener(ChangeCamera);
    }

    void Update() {
       if(Input.GetKeyDown(KeyCode.Escape))
         ChangeCamera();
    }

    void ChangeCamera() {
        Camera[] cameras = Camera.allCameras;
        foreach (Camera cam in cameras)
        {
            cam.gameObject.SetActive(false);
        }
        mainCamera.SetActive(true);
    }
}
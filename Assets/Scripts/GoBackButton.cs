using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GoBackButton : MonoBehaviour
{
    // Reference to the main camera GameObject (first camera displaced)
    public GameObject mainCamera;
    // Reference to the Button UI component
    public Button button;

    void Start() {
        // Add a listener to the button to call the ChangeCamera method when clicked
        button.onClick.AddListener(ChangeCamera);
    }

    // Update is called once per frame
    void Update() {
         // Check if the Escape key is pressed
       if(Input.GetKeyDown(KeyCode.Escape))
        // If pressed, invokes the ChangeCamera method
         ChangeCamera();
    }

    
    // Method to change the active camera, deactivating all the active ones
    void ChangeCamera() {
        // Get all cameras in the scene and deactivates each one
        Camera[] cameras = Camera.allCameras;
        foreach (Camera cam in cameras)
        {
            cam.gameObject.SetActive(false);
        }
        // Activate the main camera
        mainCamera.SetActive(true);
    }
}
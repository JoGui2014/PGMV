using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactersCamera : MonoBehaviour
{
    private Camera mainCamera;
    public GameObject secondaryCamera;

    void Start() {
        // Initially set the secondary camera to inactive (board camera)
        secondaryCamera.SetActive(false);
         // Get the parent transform of the current object
        Transform parent = transform.parent;
        // Find and assign the main camera from the clicked character
        mainCamera = parent.GetComponentInChildren<Camera>();                
    }

     // Update is called once per frame
    void Update() {
        // Check if the X key is pressed down
        if(Input.GetKeyDown(KeyCode.X))
            // If pressed, deactivate the characters camera
            secondaryCamera.SetActive(false);
    }

    void OnMouseUp()
    {
        // Activate the character camera once the mouse button is released
        secondaryCamera.SetActive(true);       
    }
}


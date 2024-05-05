using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactersCamera : MonoBehaviour
{
    private Camera mainCamera;
    public GameObject secondaryCamera;

    void Start() {
        secondaryCamera.SetActive(false);
        Transform parent = transform.parent;
        mainCamera = parent.GetComponentInChildren<Camera>();                
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.X))
            secondaryCamera.SetActive(false);
    }

    void OnMouseUp()
    {
        secondaryCamera.SetActive(true);       
    }
}


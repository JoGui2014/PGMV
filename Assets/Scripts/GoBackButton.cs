using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GoBackButton : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject secondaryCamera;
    public Button button;

    void Start() {
        button.onClick.AddListener(changeCamera);

    }
   void changeCamera() {
        secondaryCamera.SetActive(false);
        mainCamera.SetActive(true);
   }
}

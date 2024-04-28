using UnityEngine;
using UnityEngine.UI;
using UnityEditor;


public class FileExplorer : MonoBehaviour
{
    public Button yourButton; // Reference to your button in the scene
    public string xmlFilePath; // Path to store the selected XML file
    private XMLReader xmlReader; // Reference to your XML reader script
    public GameObject mainCamera;
    public GameObject secondaryCamera;
    public bool switchCam;

    void Start()
    {
        //Calls the TaskOnClick/TaskWithParameters/ButtonClicked method when you click the Button
        xmlReader = GameObject.FindWithTag("board").GetComponent<XMLReader>();
        yourButton.onClick.AddListener(SwitchToGameCamera);
        mainCamera.SetActive(true);
        secondaryCamera.SetActive(false);
    }

    
    void OpenExplorer()
    {
        // Open file explorer
        string path = EditorUtility.OpenFilePanel("Open XML File", "", "xml");

        // Check if a file was selected
        if (!string.IsNullOrEmpty(path))
        {
            // Store the path
            xmlFilePath = path;

            // Call the XML reader function
            

            // Switch camera to game camera
            SwitchToGameCamera();
        }
    }
    

    void SwitchToGameCamera()
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
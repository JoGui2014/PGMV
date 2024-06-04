using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System;
using System.Xml;

public class FileExplorer : MonoBehaviour
{
    // Reference to the XMLReader script
    private XMLReader xmlReader;
    // Reference to the main camera and the board camera
    public GameObject mainCamera;
    public GameObject secondaryCamera;
    // Reference to the button UI
    public Button button; 
    // Path to store the selected XML file
    public string xmlFilePath; 
  

    void Start()
    {
        // Get the XMLReader component
        xmlReader = GetComponent<XMLReader>();
        // Add a listener to the button to call the OnClick method when clicked
        button.onClick.AddListener(OnClick);
        // Assures that when the game starts the main camera it's on and the board one it's off
        mainCamera.SetActive(true);
        secondaryCamera.SetActive(false);
    }

    void OnClick()
    {
        // If xmlFilePath is null, allow selecting XML file

        if (xmlFilePath == "")
        {
            OpenExplorer();
        } else
        {
            // If xmlFilePath is not null, switch cameras
            SwitchToGameCamera();
        }
    }

    void OpenExplorer()
    {
        // Open file explorer
        string path = UnityEditor.EditorUtility.OpenFilePanel("Open XML File", "", "xml");

        // Check if a file was selected
        if (!string.IsNullOrEmpty(path) && IsXmlFile(path) && ValidateXmlWithDtd(path))
        {
            // Store the path
            xmlFilePath = path;
            xmlReader.StartReadingXML(xmlFilePath);
            SwitchToGameCamera(); // Switch camera after XML file is selected
        }
        else {
            EditorUtility.DisplayDialog("Aviso", "O arquivo fornecido não é um arquivo XML válido.", "OK");
        }

    }

    bool IsXmlFile(string filePath)
    {
        // Verifies the extension of the file to determine if it's a XML
        return filePath.ToLower().EndsWith(".xml");
    }

    // Method to validate the XML file using its DTD
    bool ValidateXmlWithDtd(string xmlFilePath)
    {
        try
        {
            // Open the XML file and create an XmlReader with the specified settings
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.ValidationType = ValidationType.DTD;
            settings.XmlResolver = new XmlUrlResolver();

            using (FileStream xmlStream = new FileStream(xmlFilePath, FileMode.Open))
            using (XmlReader reader = XmlReader.Create(xmlStream, settings))
            {
                // Read the XML file to validate it
                while (reader.Read()) { }
            }

            Debug.Log("XML file is valid.");
            return true;
        }
        catch (XmlException ex)
        {
            Debug.LogError("XML file is not valid: " + ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError("An error occurred: " + ex.Message);
            return false;
        }
    }

    // Method to switch from the main camera to the board camera
    void SwitchToGameCamera()
    {
        mainCamera.SetActive(false);
        secondaryCamera.SetActive(true);
    }
}
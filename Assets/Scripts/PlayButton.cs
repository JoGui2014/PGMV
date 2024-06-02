using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System;
using System.Xml;

public class FileExplorer : MonoBehaviour
{
    public Button button; // Reference to your button in the scene
    public string xmlFilePath; // Path to store the selected XML file
    private XMLReader xmlReader; // Reference to your XML reader script
    public GameObject mainCamera;
    public GameObject secondaryCamera;

    void Start()
    {
        xmlReader = GetComponent<XMLReader>();
        button.onClick.AddListener(OnClick);
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
            EditorUtility.DisplayDialog("Aviso", "O arquivo fornecido nÃ£o Ã© um arquivo XML vÃ¡lido.", "OK");
            // Abre um painel de diÃ¡logo para escolher outro arquivo
            //xmlFilePath = EditorUtility.OpenFilePanel("Selecionar arquivo XML", "", "xml");
        }

    }

    bool IsXmlFile(string filePath)
    {
        // Verifica a extensÃ£o do arquivo para determinar se Ã© XML
        return filePath.ToLower().EndsWith(".xml");
    }

    bool ValidateXmlWithDtd(string xmlFilePath)
    {
        try
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            settings.ValidationType = ValidationType.DTD;
            settings.XmlResolver = new XmlUrlResolver();

            using (FileStream xmlStream = new FileStream(xmlFilePath, FileMode.Open))
            using (XmlReader reader = XmlReader.Create(xmlStream, settings))
            {
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

    void SwitchToGameCamera()
    {
        mainCamera.SetActive(false);
        secondaryCamera.SetActive(true);
    }
}
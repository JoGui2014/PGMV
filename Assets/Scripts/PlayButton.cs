using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

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
        if (!string.IsNullOrEmpty(path) && IsXmlFile(path))
        {
            // Store the path
            xmlFilePath = path;
            xmlReader.StartReadingXML(xmlFilePath);
            SwitchToGameCamera(); // Switch camera after XML file is selected
        }
        else {
            EditorUtility.DisplayDialog("Aviso", "O arquivo fornecido não é um arquivo XML válido.", "OK");
            // Abre um painel de diálogo para escolher outro arquivo
            //xmlFilePath = EditorUtility.OpenFilePanel("Selecionar arquivo XML", "", "xml");
        }
        
    }

    bool IsXmlFile(string filePath)
    {
        // Verifica a extensão do arquivo para determinar se é XML
        return filePath.ToLower().EndsWith(".xml");
    }

    void SwitchToGameCamera()
    {
        secondaryCamera.SetActive(true);
        mainCamera.SetActive(false);
    }
}
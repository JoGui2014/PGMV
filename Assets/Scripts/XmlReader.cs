using UnityEngine;
using System.Xml;
using System;
using System.Collections;

public class XMLReader : MonoBehaviour
{
public GameObject catapult;
public GameObject soldier;
public GameObject archer;
public GameObject village;
public GameObject forest;
public GameObject mountain;
public GameObject plain;
public GameObject sea;
public GameObject desert;
public int width;
public int height;

private bool waitForInput = true;
[SerializeField] GameObject gameBoard;

void Start()
{
StartCoroutine(ReadXML());
}

void boardCreation(){
GameObject board = Instantiate(gameBoard);
}

IEnumerator ReadXML()
{
// Load the XML file
TextAsset xmlFile = Resources.Load<TextAsset>("game__horizontal-island__war-2");
XmlDocument xmlDoc = new XmlDocument();

xmlDoc.LoadXml(xmlFile.text);

// Extract width and height of the gaming board
XmlNode boardNode = xmlDoc.SelectSingleNode("//board");
if (boardNode != null)
{
    width = int.Parse(boardNode.Attributes["width"].Value);
    height = int.Parse(boardNode.Attributes["height"].Value);
    XmlNodeList fields = boardNode.ChildNodes;
    buildBoard(fields, width, height);
}

// Extract and instantiate GameObjects based on XML data
XmlNodeList turnNodes = xmlDoc.SelectNodes("//turn");
foreach (XmlNode turnNode in turnNodes)
{

    // Wait until P key is pressed
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.P));
    XmlNodeList unitNodes = turnNode.SelectNodes("./unit");
    foreach (XmlNode unitNode in unitNodes)
    {
        string type = unitNode.Attributes["type"].Value;
        float x = float.Parse(unitNode.Attributes["x"].Value);
        float y = float.Parse(unitNode.Attributes["y"].Value);

        string action = unitNode.Attributes["action"].Value;

        GameObject prefab = null;

        if (action == "spawn")
        {
            switch (type)
            {
                case "soldier":
                    break;
                case "archer":
                    break;
                case "catapult":
                    prefab = catapult;
                    break;
                default:
                    Debug.LogWarning("Unknown unit type: " + type);
                    break;
            }
        }
        if (prefab != null)
        {
            GameObject instance = Instantiate(prefab, gameBoard.transform);
            instance.transform.localPosition = new Vector3(0, 0, 0);
            instance.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }
    }
}

// Now you can use boardWidth and boardHeight as needed
Debug.Log("Board Width: " + width);
Debug.Log("Board Height: " + height);
}
int i = 0;
int j = 0;

void buildBoard(XmlNodeList fields, int width, int heigth){
// Extract and instantiate GameObjects based on XML data
foreach (XmlNode field in fields)
{
    string fieldType = field.Name;

    GameObject prefab = null;

    switch (fieldType)
    {
        case "village":
            prefab = village;
            break;
        case "forest":
            prefab = forest;
            break;
        case "sea":
            prefab = sea;
            break;
        case "plain":
            prefab = plain;
            break;
        case "mountain":
            prefab = mountain;
            break;
        case "desert":
            prefab = desert;
            break;
        default:
            Debug.LogWarning("Unknown unit type: " + fieldType);
            break;
    }
    if (prefab != null)
    {
        // Instantiate the prefab with the game board as its parent
        GameObject instance = Instantiate(prefab, gameBoard.transform);

        // Get the height of the game board
        MeshFilter meshFilter = gameBoard.GetComponent<MeshFilter>();
        float boardDepth = 0f;
        float boardWidth = 0f;
        float boardHeight = 0f;

        if (meshFilter != null)
        {
            
            boardDepth = meshFilter.sharedMesh.bounds.size.z;
            boardWidth = meshFilter.sharedMesh.bounds.size.x;
            boardHeight = meshFilter.sharedMesh.bounds.size.y;
            
        }
        
        float prefabWidth = boardWidth/width;
        float prefabHeight = boardHeight/height;
        float x = prefabHeight * ((height-1f)/2);
        float y = prefabWidth * ((width-1f)/2);
        

        Quaternion targetRotation = Quaternion.Euler(0, 90, 0);
        instance.transform.rotation = targetRotation;
        
        float newX = -x + (j * prefabHeight);
        float newY = -y + (i * prefabWidth);
        
       
        instance.transform.localPosition = new Vector3(newX, newY, -boardDepth/2);
        
        // Set the scale of the instance
        MeshFilter meshFilterPrefab = prefab.GetComponent<MeshFilter>();

        float prefabScaleX = prefabWidth / meshFilterPrefab.sharedMesh.bounds.size.z;
        float prefabScaleZ = prefabHeight / meshFilterPrefab.sharedMesh.bounds.size.x;

        instance.transform.localScale = new Vector3(prefabScaleX, 1f, prefabScaleZ);
        
        
        i++;
        if(i == width) {
            i = 0;
            j++;    
        }
    }
}
}
}
using UnityEngine;
using System.Xml;
using System;
using System.Collections;

public class XMLReader : MonoBehaviour{
    public GameObject catapult;
    public GameObject soldier;
    public GameObject archer;
    public GameObject mage;
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

    public void StartReadingXML(string xmlFilePath){
        StartCoroutine(ReadXML(xmlFilePath));
    }

    IEnumerator ReadXML(string xmlFilePath){
        
        // Load the XML file
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlFilePath);

        // Extract width and height of the gaming board
        XmlNode boardNode = xmlDoc.SelectSingleNode("//board");
        if (boardNode != null){
            width = int.Parse(boardNode.Attributes["width"].Value);
            height = int.Parse(boardNode.Attributes["height"].Value);
            XmlNodeList fields = boardNode.ChildNodes;
            buildBoard(fields, width, height);
        }

        // Extract and instantiate GameObjects based on XML data
        // Wait until P key is pressed
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.P));
        XmlNodeList turnNodes = xmlDoc.SelectNodes("//turn");
        summonPieces(turnNodes);
    }

    void summonPieces(XmlNodeList turnNodes){
        foreach (XmlNode turnNode in turnNodes){
            XmlNodeList unitNodes = turnNode.SelectNodes("./unit");
            foreach (XmlNode unitNode in unitNodes){
                string type = unitNode.Attributes["type"].Value;
                float x = float.Parse(unitNode.Attributes["x"].Value);
                float y = float.Parse(unitNode.Attributes["y"].Value);

                string action = unitNode.Attributes["action"].Value;

                GameObject prefab = null;
                
                float depth = 0f;

                Vector3 scale = new Vector3(0.0001f, 0.0001f, 0.0001f);

                if (action == "spawn"){
                    switch (type){
                        case "soldier":
                            prefab = soldier;
                            break;
                        case "archer":
                            prefab = archer;
                            break;
                        case "mage":
                            prefab = mage;
                            break;
                        case "catapult":
                            prefab = catapult;
                            scale = new Vector3(0.00005f, 0.00005f, 0.00005f);
                            depth = -0.00015f;
                            break;
                        default:
                            Debug.LogWarning("Unknown unit type: " + type);
                            break;
                    }
                }
                if (prefab != null){
                    Transform terrain = gameBoard.transform.Find($"{x},{y}");
                    Vector3 position = terrain.localPosition;
                    GameObject instance = Instantiate(prefab, gameBoard.transform);
                    //METER ALEATORIDADE NA POSIÇAO               
                    instance.transform.localPosition = new Vector3(position.x, position.y, position.z + depth);
                    Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
                    instance.transform.rotation = targetRotation;
                    instance.transform.localScale = scale;
                    instance.name = unitNode.Attributes["id"].Value;
                }
            }
        }
    }


    void buildBoard(XmlNodeList fields, int width, int heigth){
        int i = 0;
        int j = 0;
        // Extract and instantiate GameObjects based on XML data
        foreach (XmlNode field in fields){
            string fieldType = field.Name;

            GameObject prefab = null;

            switch (fieldType){
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
            if (prefab != null){
                // Instantiate the prefab with the game board as its parent
                GameObject instance = Instantiate(prefab, gameBoard.transform);

                // Get the height of the game board
                MeshFilter meshFilter = gameBoard.GetComponent<MeshFilter>();
                float boardDepth = 0f;
                float boardWidth = 0f;
                float boardHeight = 0f;

                if (meshFilter != null){
    
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
                instance.name = $"{i+1},{j+1}";

                i++;

                if(i == width) {
                    i = 0;
                    j++;    
                }
            }
        }
    }
}
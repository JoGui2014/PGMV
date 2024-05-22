using UnityEngine;
using System.Xml;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class XMLReader : MonoBehaviour {
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
    public TMP_Text player;
    public TMP_Text turns;

    private bool waitForInput = true;
    [SerializeField] GameObject gameBoard;
    private XmlDocument xmlDoc = new XmlDocument();
    private XmlNode currTurn;

    [SerializeField] Button buttonPause;
    [SerializeField] Button buttonFoward;
    [SerializeField] Button buttonBack;

    private bool isRunning = false;
    private bool lastTurn = false;
    private bool hasBoard = false;

    private int numberTurns = 0;
    private int numPieces;
    private string playerName;
    private float boardDepth;

    public void StartReadingXML(string xmlFilePath) {
        ReadXML(xmlFilePath);
        numberTurns = 0;
    }

    void Start() {
        buttonPause.onClick.AddListener(OnClickPause);
        buttonFoward.onClick.AddListener(OnClickForward);
        buttonBack.onClick.AddListener(OnClickBack);
    }

    void Update() {
        turns.text = $"Nº Turns: {numberTurns}";
    }

    void ReadXML(string xmlFilePath) {
        try {
            xmlDoc.Load(xmlFilePath);
            XmlNode turnNodes = xmlDoc.SelectSingleNode("//turns");
            currTurn = turnNodes?.FirstChild;

            XmlNode boardNode = xmlDoc.SelectSingleNode("//board");
            if (boardNode != null) {
                width = int.Parse(boardNode.Attributes["width"].Value);
                height = int.Parse(boardNode.Attributes["height"].Value);
                XmlNodeList fields = boardNode.ChildNodes;
                buildBoard(fields, width, height);
            }
            hasBoard = true;
        } catch (Exception ex) {
            Debug.LogError($"Error reading XML file: {ex.Message}");
        }
    }

    IEnumerator ProcessActions(XmlNodeList unitNodes) {
        foreach (XmlNode unitNode in unitNodes) {
            string action = unitNode.Attributes["action"].Value;
            playerName = unitNode.Attributes["role"].Value;

            // Update player name UI
            player.text = $"Playing: {playerName}";

            switch (action) {
                case "spawn":
                    summonPieces(unitNode);
                    break;
                case "move_to":
                    move(unitNode);
                    break;
                case "attack":
                    attack(unitNode);
                    break;
                case "hold":
                    Debug.Log("hold");
                    break;
                default:
                    Debug.LogWarning("Unknown action: " + action);
                    break;
            }

            // Wait for a short period before processing the next action
            yield return new WaitForSeconds(2); // Adjust the delay as needed
        }
    }

    void move(XmlNode unit) {
        string id = unit.Attributes["id"].Value;

        GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;

        if (piece != null) {
            float x = float.Parse(unit.Attributes["x"].Value);
            float y = float.Parse(unit.Attributes["y"].Value);
            Transform terrain = gameBoard.transform.Find($"{x},{y}");
            Vector3 move_to_position = terrain.position;
            CharacterIdleMacro cim = piece.GetComponent<CharacterIdleMacro>();
            cim.spawnGhost();
            cim.SetTarget(move_to_position);
        }
    }

    void attack(XmlNode unit) {
        string id = unit.Attributes["id"].Value;
        GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;
        float x = float.Parse(unit.Attributes["x"].Value);
        float y = float.Parse(unit.Attributes["y"].Value);
        string type = unit.Attributes["type"].Value;
        string team = unit.Attributes["role"].Value;
        CharacterIdleMacro piece_attacking = piece.GetComponent<CharacterIdleMacro>();
        Transform terrain = gameBoard.transform.Find($"{x},{y}");
        Vector3 attacked_position = terrain.position;
        List<string> charsToAttack = getEnemies(team, getCharactersInTerrain(attacked_position));
        if (type == "soldier"){
            foreach (string loopie in charsToAttack){
                GameObject piece_attacked = gameBoard.transform.Find($"{loopie}")?.gameObject;
                CharacterIdleMacro aux = piece_attacked.GetComponent<CharacterIdleMacro>();
                aux.Died();
            }
        } else {
            foreach (string loopie in charsToAttack){
                GameObject piece_attacked = gameBoard.transform.Find($"{loopie}")?.gameObject;
                piece_attacking.KillCharacter(piece_attacked);
            }
        }

    }

    public List<string> getCharactersInTerrain(Vector3 terrainPos){
        List<string> CharsInTerrain = new List<string>();
        for (int i = 1; i <= numPieces; i++)
        {
            string id = i.ToString();
            GameObject c = gameBoard.transform.Find($"{id}")?.gameObject;
            if (c != null && Vector3.Distance(c.transform.position, terrainPos) <= 0.1f){
                CharsInTerrain.Add(id);
            }
        }
        return CharsInTerrain;
    }

    public List<string> getEnemies(string team, List<string> characters){
        List<string> enemies = new List<string>();
        foreach(string entity in characters){
            GameObject character = gameBoard.transform.Find($"{entity}")?.gameObject;
            CharacterIdleMacro aux = character.GetComponent<CharacterIdleMacro>();
            string entityTeam = aux.getTeam();
            if (entityTeam != team){
                enemies.Add(entity);
            }
        }
        return enemies;
    }


   void summonPieces(XmlNode unit) {
        numPieces++;
        string type = unit.Attributes["type"].Value;
        float x = float.Parse(unit.Attributes["x"].Value);
        float y = float.Parse(unit.Attributes["y"].Value);
        string team = unit.Attributes["role"].Value;
        GameObject prefab = GetPrefabByType(type);

        if (prefab != null) {
            Transform terrain = gameBoard.transform.Find($"{x},{y}");
            if (terrain != null) {
                Vector3 terrainSize = terrain.GetComponent<Renderer>().bounds.size;
                Vector3 terrainPosition = terrain.position;


                // Calculate the size of the piece's bounding box
                GameObject tempInstance = Instantiate(prefab);
                Renderer pieceRenderer = tempInstance.GetComponentInChildren<Renderer>();
                if (pieceRenderer == null) {
                    Debug.LogWarning($"No Renderer found for the prefab type '{type}'.");
                    Destroy(tempInstance);
                    return;
                }
                Vector3 pieceSize = pieceRenderer.bounds.size;
                Destroy(tempInstance);

                // Generate random positions within the terrain bounds, adjusted for the piece size
                float halfPieceWidth = pieceSize.x / 2;
                float halfPieceDepth = pieceSize.y / 2;
                float randomX = UnityEngine.Random.Range(terrainPosition.x - terrainSize.x / 2 + halfPieceWidth, terrainPosition.x + terrainSize.x / 2 - halfPieceWidth);
                float randomY = terrainPosition.y + GetDepthByType(type);
                float randomZ = UnityEngine.Random.Range(terrainPosition.z - terrainSize.z / 2 + halfPieceDepth, terrainPosition.z + terrainSize.z / 2 - halfPieceDepth);

                Vector3 randomPosition = new Vector3(randomX, randomY, randomZ);

                GameObject instance = Instantiate(prefab, gameBoard.transform);
                CharacterIdleMacro character = instance.GetComponent<CharacterIdleMacro>();
                character.SetTeam(team);
                if (type == "catapult"){
                    character.SetCatapult();
                }
                instance.transform.position = randomPosition;
                instance.transform.rotation = Quaternion.Euler(0, 0, 0);
                instance.transform.localScale = GetScaleByType(type);
                instance.name = unit.Attributes["id"].Value;
            } else {
                Debug.LogWarning($"Terrain at position ({x},{y}) not found.");
            }
        } else {
            Debug.LogWarning($"Prefab for type '{type}' not found.");
        }
    }

    GameObject GetPrefabByType(string type) {
        switch (type) {
            case "soldier":
                return soldier;
            case "archer":
                return archer;
            case "mage":
                return mage;
            case "catapult":
                return catapult;
            default:
                Debug.LogWarning("Unknown unit type: " + type);
                return null;
        }
    }

    float GetDepthByType(string type) {
        return type == "catapult" ? 0.15f : 0f;
    }

    Vector3 GetScaleByType(string type) {
        return type == "catapult" ? new Vector3(0.00005f, 0.00005f, 0.00005f) : new Vector3(0.00025f, 0.00025f, 0.00025f);
    }

    void buildBoard(XmlNodeList fields, int width, int height) {
        int i = 0, j = 0;
        MeshFilter meshFilter = gameBoard.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        float boardWidth = meshFilter.sharedMesh.bounds.size.x;
        float boardHeight = meshFilter.sharedMesh.bounds.size.y;
        boardDepth = meshFilter.sharedMesh.bounds.size.z;

        float prefabWidth = boardWidth / width;
        float prefabHeight = boardHeight / height;
        float xOffset = prefabHeight * ((height - 1f) / 2);
        float yOffset = prefabWidth * ((width - 1f) / 2);

        foreach (XmlNode field in fields) {
            GameObject prefab = GetPrefabByFieldType(field.Name);
            if (prefab != null) {
                GameObject instance = Instantiate(prefab, gameBoard.transform);
                float terrainX = -xOffset + (j * prefabHeight);
                float terrainY = -yOffset + (i * prefabWidth);
                instance.transform.localPosition = new Vector3(terrainX, terrainY, -boardDepth / 2);
                instance.transform.localScale = GetScaledPrefab(prefab, prefabWidth, prefabHeight);
                instance.transform.rotation = Quaternion.Euler(0, gameBoard.transform.rotation.eulerAngles.y + 90, 0);
                instance.name = $"{i + 1},{j + 1}";

                i++;
                if (i == width) {
                    i = 0;
                    j++;
                }
            }
        }
    }

    GameObject GetPrefabByFieldType(string fieldType) {
        switch (fieldType) {
            case "village":
                return village;
            case "forest":
                return forest;
            case "sea":
                return sea;
            case "plain":
                return plain;
            case "mountain":
                return mountain;
            case "desert":
                return desert;
            default:
                Debug.LogWarning("Unknown field type: " + fieldType);
                return null;
        }
    }

    Vector3 GetScaledPrefab(GameObject prefab, float prefabWidth, float prefabHeight) {
        MeshFilter meshFilterPrefab = prefab.GetComponent<MeshFilter>();
        float prefabScaleX = prefabWidth / meshFilterPrefab.sharedMesh.bounds.size.z;
        float prefabScaleZ = prefabHeight / meshFilterPrefab.sharedMesh.bounds.size.x;
        return new Vector3(prefabScaleX, 1f, prefabScaleZ);
    }

    private IEnumerator playLoop() {
        isRunning = true;
        lastTurn = false;

        while (!lastTurn && isRunning) {
            if (currTurn != null) {
                yield return StartCoroutine(ProcessActions(currTurn.SelectNodes("./unit")));
                if (currTurn.NextSibling != null) {
                    currTurn = currTurn.NextSibling;
                    numberTurns += 1;
                } else {
                    lastTurn = true;
                }
            } else {
                lastTurn = true;
            }
            yield return new WaitForSeconds(10); // Adjust if needed
        }
    }

    private void OnClickPause() {
        Debug.Log("Pause clicked");
        if (hasBoard) {
            if (!isRunning) {
                StartCoroutine(playLoop());
            } else {
                isRunning = false;
            }
        }
    }

    private void OnClickForward() {
        Debug.Log("Forward clicked");
        if (hasBoard) {
            isRunning = false;
            if (currTurn != null) {
                StartCoroutine(ProcessActions(currTurn.SelectNodes("./unit")));
                if (currTurn.NextSibling != null) {
                    currTurn = currTurn.NextSibling;
                    numberTurns += 1;
                } else {
                    lastTurn = true;
                }
            }
        }
    }

    private void OnClickBack() {
        Debug.Log("Back clicked");
        if (hasBoard) {
            isRunning = false;
            XmlNode previousTurn = currTurn?.PreviousSibling;
            if (previousTurn != null) {
                currTurn = previousTurn;
                numberTurns = Math.Max(0, numberTurns - 1);
                StartCoroutine(ProcessActions(currTurn.SelectNodes("./unit")));
            }
        }
    }
}
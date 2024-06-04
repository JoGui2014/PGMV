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
    public TMP_Text gameOverText;
    [SerializeField] GameObject gameBoard;
    private XmlDocument xmlDoc = new XmlDocument();
    private XmlNode currTurn;
    private int line = 0;
    private XmlNode firstTurn;
    [SerializeField] Button buttonPause;
    [SerializeField] Button buttonFoward;
    [SerializeField] Button buttonBack;
    [SerializeField] Button buttonRestart;
    public Button changeSceneButton;

    public AudioSource ArcherAttack;
    public AudioSource SoldierAttack;
    public AudioSource MageAttack;
    public AudioSource CatapultAttack;
    public AudioSource Ambient;

    private Dictionary<string,(float, float)> CharToTerrain = new Dictionary<string, (float, float)>();

    private bool isRunning = false;
    private bool lastTurn = false;
    private bool hasBoard = false;
    private bool onPlay = false;

    private int numberTurns = 0;
    private int numPieces;
    private string playerName;
    private float boardDepth;
    private List<GameObject> gameObjects = new List<GameObject>();
    private XmlNodeList turnsList;


    public void StartReadingXML(string xmlFilePath) {
        ReadXML(xmlFilePath);
    }

    void OnEnable(){
        
        if (isRunning && !lastTurn){
            Ambient.Play();
            StartCoroutine(PlayLoop());
        }
    }

    void Start() {
        gameOverText.gameObject.SetActive(false);
        changeSceneButton.gameObject.SetActive(false);
        buttonPause.onClick.AddListener(OnClickPause);
        buttonFoward.onClick.AddListener(OnClickForward);
        buttonBack.onClick.AddListener(OnClickBack);
        buttonRestart.onClick.AddListener(RestartGame);
    }

    void Update() {
         CheckInput(KeyCode.RightArrow, OnClickForward);
         CheckInput(KeyCode.LeftArrow, OnClickBack);
         CheckInput(KeyCode.Space, OnClickPause);
         CheckInput(KeyCode.R, RestartGame); 
    }

    void CheckInput(KeyCode key, System.Action action) {
         if (Input.GetKeyDown(key)) {
            action.Invoke();
         }
    }

    void ReadXML(string xmlFilePath) {
        try {
            xmlDoc.Load(xmlFilePath);
            XmlNode turnNodes = xmlDoc.SelectSingleNode("//turns");
            turnsList = turnNodes.ChildNodes;
            firstTurn = turnNodes?.FirstChild;
            currTurn = firstTurn;

            XmlNode boardNode = xmlDoc.SelectSingleNode("//board");
            if (boardNode != null) {
                width = int.Parse(boardNode.Attributes["width"].Value);
                height = int.Parse(boardNode.Attributes["height"].Value);
                XmlNodeList fields = boardNode.ChildNodes;
                BuildBoard(fields, width, height);
            }
            hasBoard = true;
        } catch (Exception ex) {
            Debug.LogError($"Error reading XML file: {ex.Message}");
        }
    }

    IEnumerator ProcessActions(XmlNodeList unitNodes, bool skip, int initialLine) {
        int count = 0;
        turns.text = $"Nº Turns: {numberTurns}";
        foreach (XmlNode unitNode in unitNodes) {
            string action = unitNode.Attributes["action"].Value;
            playerName = unitNode.Attributes["role"].Value;
            // Update player name UI
            player.text = $"Playing: {playerName}";
            if (count < initialLine){
                action = "skip";
                count += 1;
            }
            switch (action) {
                case "spawn":
                    SummonPieces(unitNode);
                    line += 1;
                    break;
                case "move_to":
                    Move(unitNode, skip);
                    line += 1;
                    break;
                case "attack":
                    yield return StartCoroutine(Attack(unitNode, skip));
                    line += 1;
                    break;
                case "hold":
                    Hold(unitNode);
                    line += 1;
                    break;
                case "skip":
                    break;
                default:
                    Debug.LogWarning("Unknown action: " + action);
                    break;
            }
            if(!skip){
                yield return new WaitForSeconds(2);
            }
        }
        line = 0;
    }

    void Hold(XmlNode unit) {
        string id = unit.Attributes["id"].Value;

        GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;

        if (piece != null) {
           CharacterIdleMacro cim = piece.GetComponent<CharacterIdleMacro>();
           cim.setHold(true);
        }
    }


    void Move(XmlNode unit, bool skip) {
        string id = unit.Attributes["id"].Value;
        float originalX = CharToTerrain[id].Item1;
        float originalY = CharToTerrain[id].Item2;
        GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;

        if (piece != null) {
            float x = float.Parse(unit.Attributes["x"].Value);
            float y = float.Parse(unit.Attributes["y"].Value);
            int pieceCount = GetCharactersInTerrain(x, y).Count;
            CharToTerrain[id] = (x,y);
            Transform terrain = gameBoard.transform.Find($"{x},{y}");
            Vector3 move_to_position = terrain.position;
            CharacterIdleMacro cim = piece.GetComponent<CharacterIdleMacro>();
            if(skip) {
                piece.transform.position = move_to_position;
                cim.SetCurPos(move_to_position);
            } else {
                if(unit.Attributes["type"].Value == "mage"){
                    cim.setHold(false);
                    cim.spawnGhost();
                    StartCoroutine(MoveWithJump(piece, move_to_position, 1.0f));
                }else{
                    cim.setHold(false);
                    cim.spawnGhost();
                    StartCoroutine(MoveToTarget(piece, move_to_position, originalX, originalY, pieceCount));
                }
            }
        }
    }

    IEnumerator MoveToTarget(GameObject piece, Vector3 targetPosition, float originalX, float originalY, int pieceCount) {
        CharacterIdleMacro cim = piece.GetComponent<CharacterIdleMacro>();
        float x = originalX;
        float y = originalY;
        Transform terrain = gameBoard.transform.Find($"{x},{y}");
        while (Vector3.Distance(piece.transform.position, terrain.position) > 0.01f){
            cim.SetTarget(terrain.position);
            yield return null;
        }
        Vector3 targetLatitude = new Vector3(piece.transform.position.x, piece.transform.position.y, terrain.position.z);
        while (Vector3.Distance(piece.transform.position, targetLatitude) > 0.01f){
            cim.SetTarget(targetLatitude);
            yield return null;
        }
        while (Vector3.Distance(piece.transform.position, targetPosition) > 0.01f){
            cim.SetTarget(targetPosition);
            yield return null;
        }
        Vector3 terrainSize = terrain.GetComponent<Renderer>().bounds.size;
        float halfWidth = terrainSize.x / 2;
        float halfDepth = terrainSize.z / 2;
        float offsetX = halfWidth / 2;
        float offsetZ = halfDepth / 2;
        // Determine the position within the quadrant
        print(pieceCount);
        Vector3[] quadrantPositions = new Vector3[] {
            new Vector3(targetPosition.x - offsetX, targetPosition.y, targetPosition.z - offsetZ),
            new Vector3(targetPosition.x + offsetX, targetPosition.y, targetPosition.z - offsetZ),
            new Vector3(targetPosition.x - offsetX, targetPosition.y, targetPosition.z + offsetZ),
            new Vector3(targetPosition.x + offsetX, targetPosition.y, targetPosition.z + offsetZ)
        };
        if (pieceCount < 4) {
            Vector3 movePosition = quadrantPositions[pieceCount];
            while (Vector3.Distance(piece.transform.position, movePosition) > 0.01f){
                cim.SetTarget(movePosition);
                yield return null;
            }
        }
    }

    IEnumerator MoveWithJump(GameObject piece, Vector3 targetPosition, float speed) {
        BoxCollider boxCollider = piece.GetComponent<BoxCollider>();
        CharacterIdleMacro cim = piece.GetComponent<CharacterIdleMacro>();
        Vector3 startPosition = piece.transform.position;
        Vector3 midPosition = new Vector3(startPosition.x, startPosition.y + (boxCollider.size.y*0.5f), startPosition.z);
        Vector3 targetMidPosition = new Vector3(targetPosition.x, midPosition.y, targetPosition.z);
        while (Vector3.Distance(piece.transform.position, midPosition) > 0.01f) {
            piece.transform.position = Vector3.MoveTowards(piece.transform.position, midPosition, speed * Time.deltaTime);
            yield return null;
        }
        while (Vector3.Distance(piece.transform.position, targetMidPosition) > 0.01f) {
            cim.SetTarget(targetMidPosition);
            yield return null;
        }
        while (Vector3.Distance(piece.transform.position, targetPosition) > 0.01f) {
            piece.transform.position = Vector3.MoveTowards(piece.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        piece.transform.position = targetPosition;
    }



  IEnumerator Attack(XmlNode unit, bool skip) {
        string id = unit.Attributes["id"].Value;
        GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;
        float x = float.Parse(unit.Attributes["x"].Value);
        float y = float.Parse(unit.Attributes["y"].Value);
        string type = unit.Attributes["type"].Value;
        string team = unit.Attributes["role"].Value;
        CharacterIdleMacro piece_attacking = piece.GetComponent<CharacterIdleMacro>();
        piece_attacking.setHold(false);
        Transform terrain = gameBoard.transform.Find($"{x},{y}");
        Vector3 attacked_position = terrain.position;
        List<string> charsToAttack = GetEnemies(team, GetCharactersInTerrain(x, y));
        foreach (string loopie in charsToAttack) {
            GameObject piece_attacked = gameBoard.transform.Find($"{loopie}")?.gameObject;
            if (skip) {
                piece_attacked.SetActive(false);
            } else {
                if (type == "soldier") {
                    if(IsTerrainAdjacent(id, x, y)){
                        if (piece_attacked.tag == "soldier") {
                            CharacterIdleMacro piece_attacked_macro = piece_attacked.GetComponent<CharacterIdleMacro>();
                            if (piece_attacked_macro.isHolding()) {
                                GameObject terrainObject = gameBoard.transform.Find($"{x},{y}")?.gameObject;
                                PlayerPrefs.SetString("TerrainType", terrainObject.tag);
                                changeSceneButton.gameObject.SetActive(true);
                                yield return new WaitForSeconds(10);
                                changeSceneButton.gameObject.SetActive(false);
                            }
                            }
                            CharacterIdleMacro aux = piece_attacked.GetComponent<CharacterIdleMacro>();
                            SoldierAttack.Play();
                            aux.DeadSound();
                            aux.Smoke();
                            aux.Died();
                    }
                } else {
                    PlaySoundByType(type);
                    piece_attacking.KillCharacter(piece_attacked);
                }
            }
        }
    }

    bool IsTerrainAdjacent(string id, float posx, float posy){
        (float, float) currentTerrain = CharToTerrain[id];
        if((currentTerrain.Item1 + 1 == posx || currentTerrain.Item1 - 1 == posx || currentTerrain.Item1 == posx) && (currentTerrain.Item2 + 1 == posy || currentTerrain.Item2 - 1 == posy || currentTerrain.Item2 == posy)){
            return true;
        }
        return false;
    }



    void PlaySoundByType(string type){
        if (type == "archer"){
            ArcherAttack.Play();
        }
        if (type == "catapult"){
            CatapultAttack.Play();
        }
        if (type == "mage"){
            MageAttack.Play();
        }
    }

    public List<string> GetCharactersInTerrain(float Posx, float Posy){
        List<string> CharsInTerrain = new List<string>();
        for (int i = 1; i <= numPieces; i++)
        {
            string id = i.ToString();
            if (CharToTerrain.ContainsKey(id)){
                (float, float) terrain = CharToTerrain[id];
                 GameObject c = gameBoard.transform.Find($"{id}")?.gameObject;
                 if (c != null && terrain.Item1 == Posx && terrain.Item2 == Posy){
                     CharsInTerrain.Add(id);
                 }
            }
            
        }
        return CharsInTerrain;
    }

    public List<string> GetEnemies(string team, List<string> characters){
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


  void SummonPieces(XmlNode unit) {
        numPieces++;
        string id = unit.Attributes["id"].Value;
        string type = unit.Attributes["type"].Value;
        float x = float.Parse(unit.Attributes["x"].Value);
        float y = float.Parse(unit.Attributes["y"].Value);
        string team = unit.Attributes["role"].Value;
        GameObject prefab = GetPrefabByType(type);

        if (prefab != null) {
            Transform terrain = gameBoard.transform.Find($"{x},{y}");
            if (terrain != null) {
                if (TryPlacePiece(terrain, prefab, type, team, id, x, y)) {
                    CharToTerrain.Add(id, (x, y));
                }
            } else {
                Debug.LogWarning($"Terrain at position ({x},{y}) not found.");
            }
        } else {
            Debug.LogWarning($"Prefab for type '{type}' not found.");
        }
    }

    // New TryPlacePiece method
    bool TryPlacePiece(Transform terrain, GameObject prefab, string type, string team, string id, float x, float y) {
        Vector3 terrainPosition = terrain.position;
        Vector3 terrainSize = terrain.GetComponent<Renderer>().bounds.size;
        // Calculate the size of the four slices of the tile's terrain
        float halfWidth = terrainSize.x / 2;
        float halfDepth = terrainSize.z / 2;
        float offsetX = halfWidth / 2;
        float offsetZ = halfDepth / 2;
        float terrainY = terrainPosition.y + GetDepthByType(type);
        // Determine the position within the quadrant
        int pieceCount = GetCharactersInTerrain(x, y).Count;
        Vector3[] quadrantPositions = new Vector3[] {
            new Vector3(terrainPosition.x - offsetX, terrainY, terrainPosition.z - offsetZ),
            new Vector3(terrainPosition.x + offsetX, terrainY, terrainPosition.z - offsetZ),
            new Vector3(terrainPosition.x - offsetX, terrainY, terrainPosition.z + offsetZ),
            new Vector3(terrainPosition.x + offsetX, terrainY, terrainPosition.z + offsetZ)
        };

        if (pieceCount < 4) {
            Vector3 spawnPosition = quadrantPositions[pieceCount];

            GameObject instance = Instantiate(prefab, gameBoard.transform);
            CharacterIdleMacro character = instance.GetComponent<CharacterIdleMacro>();
            character.SetTeam(team);
            if (type == "catapult") {
                character.SetCatapult();
            }
            instance.transform.position = spawnPosition;
            instance.transform.rotation = Quaternion.Euler(0, 0, 0);
            instance.transform.localScale = GetScaleByType(type);
            instance.name = id;
            instance.tag = type;
            gameObjects.Add(instance);
            return true;
        } else {
            Debug.LogWarning($"Cannot place more than 4 characters on terrain at position ({x},{y}).");
            return false;
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

    void BuildBoard(XmlNodeList fields, int width, int height) {
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
                instance.tag = field.Name;

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

    private IEnumerator PlayLoop() {
        isRunning = true;

        while (!lastTurn && isRunning) {
            onPlay = true;
            if (currTurn != null) {
                numberTurns = GetTurnNumber(currTurn);
                yield return StartCoroutine(ProcessActions(currTurn.SelectNodes("./unit"), false, line));
                if (currTurn.NextSibling != null) {
                    currTurn = currTurn.NextSibling;
                } else {
                    lastTurn = true;
                    StartCoroutine(GameOver());                   
                }
            } else {
                lastTurn = true;
                StartCoroutine(GameOver());
                
            }
            onPlay = false;
        }
        
        
    }

    private void RestartGame() {
        if (hasBoard) {
            isRunning = false;
            if(!onPlay) {
                lastTurn = false;
                foreach (GameObject game in gameObjects) {
                    Destroy(game);
                }
                currTurn = firstTurn;
                numberTurns = 0;
                StartCoroutine(PlayLoop());
            }
        }
    }

    private IEnumerator GameOver() {
        gameOverText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        gameOverText.gameObject.SetActive(false);
    }

    private void OnClickPause() {
        if (hasBoard) {
            if (!isRunning && !onPlay && !lastTurn) {
                Ambient.Play();
                StartCoroutine(PlayLoop());
            } else {
                isRunning = false;
            }
        }
    }

    private void OnClickForward() {
        if (hasBoard) {
            isRunning = false;
            if(!onPlay){
                if (currTurn != null && !lastTurn) {
                    numberTurns = GetTurnNumber(currTurn);
                    StartCoroutine(ProcessActions(currTurn.SelectNodes("./unit"), true, line));
                    if (currTurn.NextSibling != null) {
                        currTurn = currTurn.NextSibling;
                    } else {
                        lastTurn = true;
                        StartCoroutine(GameOver());
                    }
                }
            }
        }
    }

    private void OnClickBack() {
        if (hasBoard) {
            isRunning = false;
            if(!onPlay){
                if(lastTurn){
                    ProcessActionsBackwards(currTurn);
                    lastTurn = false;
                }else{
                    XmlNode previousTurn = currTurn?.PreviousSibling;
                    if (previousTurn.PreviousSibling != null) {
                        currTurn = previousTurn;
                        ProcessActionsBackwards(currTurn);
                    }
                }
                numberTurns = GetTurnNumber(currTurn)-1; 
                turns.text = $"Nº Turns: {numberTurns}";
            }
        }
    }

    void ProcessActionsBackwards(XmlNode turn) {
        XmlNodeList unitNodes = turn.SelectNodes("./unit");
        foreach (XmlNode unitNode in unitNodes) {
            string action = unitNode.Attributes["action"].Value;
            switch (action) {
                case "spawn":
                    break;
                case "move_to":
                    MoveBack(turn, unitNode);
                    break;
                case "attack":
                    break;
                case "hold":
                    string id = unitNode.Attributes["id"].Value;
                    GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;
                    CharacterIdleMacro macro = piece.GetComponent<CharacterIdleMacro>();
                    macro.Revive();
                    piece.SetActive(true);
                    break;
                default:
                    Debug.LogWarning("Unknown action: " + action);
                    break;
            }
        }
    }

    void MoveBack(XmlNode turn, XmlNode unit){
        string id = unit.Attributes["id"].Value;
        GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;
        XmlNodeList unitNodes = turn.PreviousSibling.SelectNodes("./unit");
        Vector3 move_to_position = new Vector3(0,0,0);
        foreach (XmlNode unitNode in unitNodes){
            string idAux = unitNode.Attributes["id"].Value;
            if(idAux == id){
                float x = float.Parse(unitNode.Attributes["x"].Value);
                float y = float.Parse(unitNode.Attributes["y"].Value);
                Transform terrain = gameBoard.transform.Find($"{x},{y}");
                move_to_position = terrain.position;
            }
        }
        CharacterIdleMacro cim = piece.GetComponent<CharacterIdleMacro>();
        piece.transform.position = move_to_position;
        cim.SetCurPos(move_to_position);
    }

    int GetTurnNumber(XmlNode thisTurn){
        for (int i = 0; i < turnsList.Count; i++)
        {
            if (turnsList[i] == thisTurn)
            {
                return i+1;
            }
        }
        return -1;
    }
}
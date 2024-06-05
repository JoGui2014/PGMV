using UnityEngine;
using System.Xml;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class XMLReader : MonoBehaviour {

    // Prefab references for different game objects of the game
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

    // instance for the board dimensions
    public int width;
    public int height;

    // Instance for the HUD elements
    public TMP_Text player;
    public TMP_Text turns;
    public TMP_Text gameOverText;
    [SerializeField] GameObject gameBoard;
    [SerializeField] Button buttonPause;
    [SerializeField] Button buttonFoward;
    [SerializeField] Button buttonBack;
    [SerializeField] Button buttonRestart;
    public Button changeSceneButton;   
    
    // Audio sources for different actions of the characters
    public AudioSource ArcherAttack;
    public AudioSource SoldierAttack;
    public AudioSource MageAttack;
    public AudioSource CatapultAttack;
    public AudioSource Ambient;

    // XML parsing variables
    private XmlDocument xmlDoc = new XmlDocument();
    private XmlNode currTurn;
    private XmlNode firstTurn;
    private XmlNodeList turnsList;
    private int line = 0;

    // Game state variables
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


    // Method to start reading the XML file invoking the readXML method
    public void StartReadingXML(string xmlFilePath) {
        ReadXML(xmlFilePath);
    }

    // Unity method called when the script instance is being loaded
    void OnEnable(){
        // if the game is running and it's not the last turn it will play the audio and will run the game (play loop method)
        if (isRunning && !lastTurn){
            Ambient.Play();
            StartCoroutine(PlayLoop());
        }
    }


    void Start() {
        // Hide the game over text at the start of the game
        gameOverText.gameObject.SetActive(false);
        // Hide the change scene button at the start of the game
        changeSceneButton.gameObject.SetActive(false);
        // Add an event listener to all the buttons
        buttonPause.onClick.AddListener(OnClickPause);
        buttonFoward.onClick.AddListener(OnClickForward);
        buttonBack.onClick.AddListener(OnClickBack);
        buttonRestart.onClick.AddListener(RestartGame);
    }

    // This function is called once per frame.
    void Update() {
        // Check if the Right Arrow key is pressed and call OnClickForward if it is
        CheckInput(KeyCode.RightArrow, OnClickForward);
        // Check if the Left Arrow key is pressed and call OnClickBack if it is
        CheckInput(KeyCode.LeftArrow, OnClickBack);
        // Check if the Space key is pressed and call OnClickPause if it is
        CheckInput(KeyCode.Space, OnClickPause);
        // Check if the R key is pressed and call RestartGame if it is
        CheckInput(KeyCode.R, RestartGame); 
    }

    // This function checks if a specific key is pressed and, if so, invokes a given action
    void CheckInput(KeyCode key, System.Action action) {
         if (Input.GetKeyDown(key)) {
            action.Invoke();
         }
    }

    // This function reads and processes an XML file that was given by the user
    void ReadXML(string xmlFilePath) {
        try {
            // Load the XML document from the provided file path
            xmlDoc.Load(xmlFilePath);
            // Select the <turns> node from the XML document
            XmlNode turnNodes = xmlDoc.SelectSingleNode("//turns");
            // Store all child nodes of <turns> in turnsList and the first turn that is the current turn in this phase
            turnsList = turnNodes.ChildNodes;
            firstTurn = turnNodes?.FirstChild;
            currTurn = firstTurn;
            // Select the <board> node from the XML document
            XmlNode boardNode = xmlDoc.SelectSingleNode("//board");
            // creates the board following the info in the xml about the widht and weight for the board
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

    // Function to build the game board based on provided fields, width, and height
    void BuildBoard(XmlNodeList fields, int width, int height) {
        int i = 0, j = 0;
        
        // Get the MeshFilter component of the game board
        MeshFilter meshFilter = gameBoard.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        // Get the dimensions of the board
        float boardWidth = meshFilter.sharedMesh.bounds.size.x;
        float boardHeight = meshFilter.sharedMesh.bounds.size.y;
        boardDepth = meshFilter.sharedMesh.bounds.size.z;

        // Calculate the width and height of each field
        float prefabWidth = boardWidth / width;
        float prefabHeight = boardHeight / height;

        // Distance so the fields can be next to each other
        float xOffset = prefabHeight * ((height - 1f) / 2);
        float yOffset = prefabWidth * ((width - 1f) / 2);

        // Iterate through each field
        foreach (XmlNode field in fields) {
            // Get the prefab for the field type
            GameObject prefab = GetPrefabByFieldType(field.Name);
            if (prefab != null) {
                // Instantiate the prefab
                GameObject instance = Instantiate(prefab, gameBoard.transform);

                // Calculate position of the field
                float terrainX = -xOffset + (j * prefabHeight);
                float terrainY = -yOffset + (i * prefabWidth);

                // Set position, rotation, scale, name, and tag of the field instance
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

    // Coroutine to process a list of actions defined in XML nodes
    IEnumerator ProcessActions(XmlNodeList unitNodes, bool skip, int initialLine) {
        int count = 0;

        // Update the UI element showing the number of turns
        turns.text = $"Nº Turns: {numberTurns}";

        // Iterate through each XML node in the provided list of unit nodes
        foreach (XmlNode unitNode in unitNodes) {
            // Retrieve the action and role (player name) attributes from the current unit node
            string action = unitNode.Attributes["action"].Value;
            playerName = unitNode.Attributes["role"].Value;

            // Update player name UI
            player.text = $"Playing: {playerName}";

            // If the count of processed actions is less than the initial line, set the action to "skip"
            if (count < initialLine){
                action = "skip";
                count += 1;
            }

            // Perform the action specified by the unit node
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

            // If not skipping actions (Forward button not pressed), wait for 2 seconds before processing the next action.
            if(!skip){
                yield return new WaitForSeconds(2);
            }
        }
        // Reset the line counter after processing all actions
        line = 0;
    }

    // This function handles the hold action
    void Hold(XmlNode unit) {

        // Retrieve the "id" attribute from the unit XML node and find the game object on the game board
        string id = unit.Attributes["id"].Value;
        GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;

        // If the game object is found
        if (piece != null) {
            // Get the CharacterIdleMacro component attached to the game object and set the hold state to true
           CharacterIdleMacro cim = piece.GetComponent<CharacterIdleMacro>();
           cim.SetHold(true);
        }
    }

    // Method to handle the movement
    void Move(XmlNode unit, bool skip) {
        // Retrieve the unique identifier ("id") of the unit
        string id = unit.Attributes["id"].Value;

        // Retrieve the original coordinates of the unit
        float originalX = CharToTerrain[id].Item1;
        float originalY = CharToTerrain[id].Item2;

        // Find the game object representing the unit on the game board
        GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;

        // If the character is found
        if (piece != null) {
            // Retrieve the target coordinates ("x" and "y") from the XML node
            float x = float.Parse(unit.Attributes["x"].Value);
            float y = float.Parse(unit.Attributes["y"].Value);

            // Determine the number of pieces currently occupying the target terrain
            int pieceCount = GetCharactersInTerrain(x, y).Count;
            // Update the CharToTerrain dictionary with the new coordinates of the character
            CharToTerrain[id] = (x,y);

            // Find the terrain game object corresponding to the target coordinates and calculate the position to move the character to
            Transform terrain = gameBoard.transform.Find($"{x},{y}");
            Vector3 move_to_position = terrain.position;

            // Get the CharacterIdleMacro component attached to the unit game object
            CharacterIdleMacro cim = piece.GetComponent<CharacterIdleMacro>();

            //Get the positions of each quadrant
            Vector3[] quadrantPositions = GetQuadrant(terrain, piece.tag);
            
            // If skipping animations (forward button is clicked)
            if(skip) {
                // If there are less than 4 pieces in the terrain, move the character to a position within the quadrant
                if (pieceCount < 4) {
                    Vector3 movePosition = quadrantPositions[pieceCount]; 
                    piece.transform.position = movePosition;
                    cim.SetCurPos(movePosition);
                }
            } else {
                // If the unit is of type "mage", perform a special movement animation (he will fly)
                if(unit.Attributes["type"].Value == "mage") {
                    // Release the hold state and spawn a ghost for the mage
                    cim.SetHold(false);
                    cim.SpawnGhost();

                    // If there are less than 4 pieces in the terrain, move the character to a position within the quadrant
                    if (pieceCount < 4) {
                        Vector3 movePosition = quadrantPositions[pieceCount]; 
                        // Start a coroutine to move the character with a jump animation
                        StartCoroutine(MoveWithJump(piece, movePosition, 1.0f));
                    }
                } else {
                    // Release the hold state and spawn a ghost for the unit
                    cim.SetHold(false);
                    cim.SpawnGhost();
                    // Start a coroutine to move the character to the target position
                    StartCoroutine(MoveToTarget(piece, move_to_position, originalX, originalY, pieceCount));
                }
            }
        }
    }

    // Coroutine to move the characters to its target position with smooth movement
    IEnumerator MoveToTarget(GameObject piece, Vector3 targetPosition, float originalX, float originalY, int pieceCount) {
        // Get the CharacterIdleMacro component attached to the unit game object
        CharacterIdleMacro cim = piece.GetComponent<CharacterIdleMacro>();

        // Initialize variables for the original coordinates of the character
        float x = originalX;
        float y = originalY;
        // Find the terrain game object
        Transform terrain = gameBoard.transform.Find($"{x},{y}");

        // Move the character towards the terrain position until it's close enough
        while (Vector3.Distance(piece.transform.position, terrain.position) > 0.01f){
            cim.SetTarget(terrain.position);
            yield return null;
        }

        // Move the chartacter to the same latitude as the target position
        Vector3 targetLatitude = new Vector3(piece.transform.position.x, piece.transform.position.y, terrain.position.z);
        while (Vector3.Distance(piece.transform.position, targetLatitude) > 0.01f){
            cim.SetTarget(targetLatitude);
            yield return null;
        }

        // Move the character towards the final target position until it's close enough
        while (Vector3.Distance(piece.transform.position, targetPosition) > 0.01f){
            cim.SetTarget(targetPosition);
            yield return null;
        }

        //Get the positions of each quadrant
        Vector3[] quadrantPositions = GetQuadrant(terrain, piece.tag);

        // If there are less than 4 pieces in the terrain, move the character to a position within the quadrant
        if (pieceCount < 4) {
            Vector3 movePosition = quadrantPositions[pieceCount];
            while (Vector3.Distance(piece.transform.position, movePosition) > 0.01f){
                cim.SetTarget(movePosition);
                yield return null;
            }
        }
    }

    //Get the positions of each quadrant
    Vector3[] GetQuadrant(Transform terrain, string type){
        // Get the size of the terrain to determine the position within the quadrant
        Vector3 terrainSize = terrain.GetComponent<Renderer>().bounds.size;
        float halfWidth = terrainSize.x / 2;
        float halfDepth = terrainSize.z / 2;
        float offsetX = halfWidth / 2;
        float offsetZ = halfDepth / 2;

        // Get the position the terrain
        Vector3 terrainPosition = terrain.position;

        // Calculate the y-coordinate of the terrain
        float terrainY = terrainPosition.y + GetDepthByType(type);

        // Determine the position within the quadrant
        Vector3[] quadrantPositions = new Vector3[] {
            new Vector3(terrainPosition.x - offsetX, terrainY, terrainPosition.z - offsetZ),
            new Vector3(terrainPosition.x + offsetX, terrainY, terrainPosition.z - offsetZ),
            new Vector3(terrainPosition.x - offsetX, terrainY, terrainPosition.z + offsetZ),
            new Vector3(terrainPosition.x + offsetX, terrainY, terrainPosition.z + offsetZ)
        };
        
        return quadrantPositions;
    }

    // Coroutine to move the mage to its target position with a jumping animation (flying)
    IEnumerator MoveWithJump(GameObject piece, Vector3 targetPosition, float speed) {
        // Get the BoxCollider component attached to the mage
        BoxCollider boxCollider = piece.GetComponent<BoxCollider>();

        // Get the CharacterIdleMacro component attached to the mage
        CharacterIdleMacro cim = piece.GetComponent<CharacterIdleMacro>();

        // Get the starting, mid and target mid-position position of the mage
        Vector3 startPosition = piece.transform.position;
        Vector3 midPosition = new Vector3(startPosition.x, startPosition.y + (boxCollider.size.y*0.5f), startPosition.z);
        Vector3 targetMidPosition = new Vector3(targetPosition.x, midPosition.y, targetPosition.z);

        // Move the character towards the mid-position for the jump until it's close enough
        while (Vector3.Distance(piece.transform.position, midPosition) > 0.01f) {
            piece.transform.position = Vector3.MoveTowards(piece.transform.position, midPosition, speed * Time.deltaTime);
            yield return null;
        }

        // Move the character towards the target mid-position until it's close enough
        while (Vector3.Distance(piece.transform.position, targetMidPosition) > 0.01f) {
            cim.SetTarget(targetMidPosition);
            yield return null;
        }

        // Move the character towards the final target position until it's close enough
        while (Vector3.Distance(piece.transform.position, targetPosition) > 0.01f) {
            piece.transform.position = Vector3.MoveTowards(piece.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        // Set the mage's position to the final target position to ensure accuracy
        piece.transform.position = targetPosition;
    }

    // Coroutine to perform an attack action
    IEnumerator Attack(XmlNode unit, bool skip) {
        // Retrieve the unique identifier ("id") of the attacking character
        string id = unit.Attributes["id"].Value;
        // Find the game object on the gameboard
        GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;

        // Retrieve the target coordinates ("x" and "y") and other attributes from the XML node
        float x = float.Parse(unit.Attributes["x"].Value);
        float y = float.Parse(unit.Attributes["y"].Value);
        string type = unit.Attributes["type"].Value;
        string team = unit.Attributes["role"].Value;

        // Get the CharacterIdleMacro component attached to the attacking character and release his state
        CharacterIdleMacro piece_attacking = piece.GetComponent<CharacterIdleMacro>();
        piece_attacking.SetHold(false);

        // Find the terrain game object corresponding to the target coordinates and calculate the position of the terrain
        Transform terrain = gameBoard.transform.Find($"{x},{y}");
        Vector3 attacked_position = terrain.position;

        // Get a list of enemy characters in the target terrain
        List<string> charsToAttack = GetEnemies(team, GetCharactersInTerrain(x, y));

        // Iterate through each enemy character in the target terrain
        foreach (string loopie in charsToAttack) {
            // Find the character attacked on the gameboard
            GameObject piece_attacked = gameBoard.transform.Find($"{loopie}")?.gameObject;

            // If skipping animations, deactivate the attacked character
            if (skip) {
                piece_attacked.SetActive(false);
            } else {
                // If the attacking character is a soldier and the target terrain is adjacent is also a soldier and his is holding, trigger the 3D Model Scene
                if (type == "soldier") {
                    if(IsTerrainAdjacent(id, x, y)){
                        if (piece_attacked.tag == "soldier") {
                            CharacterIdleMacro piece_attacked_macro = piece_attacked.GetComponent<CharacterIdleMacro>();
                            if (piece_attacked_macro.IsHolding()) {
                                GameObject terrainObject = gameBoard.transform.Find($"{x},{y}")?.gameObject;
                                PlayerPrefs.SetString("TerrainType", terrainObject.tag);
                                changeSceneButton.gameObject.SetActive(true);
                                yield return new WaitForSeconds(10);
                                changeSceneButton.gameObject.SetActive(false);
                            }
                        }

                        // Trigger specific actions for soldier-soldier combat when he dies (sound, smoke and animation of the character dying)
                        CharacterIdleMacro aux = piece_attacked.GetComponent<CharacterIdleMacro>();
                        SoldierAttack.Play();
                        aux.DeadSound();
                        aux.Smoke();
                        aux.Died();
                    }
                } else {
                    // If the attacking character is not a soldier or the attack is not soldier-soldier, proceed with regular attack actions
                    PlaySoundByType(type);
                    piece_attacking.KillCharacter(piece_attacked, piece);
                }
            }
        }
    }

    // Function to check if the given terrain position is adjacent to the current terrain position of a character
    bool IsTerrainAdjacent(string id, float posx, float posy) {
        // Get the current terrain position of the character with the specified ID
        (float, float) currentTerrain = CharToTerrain[id];

        /* 
            Check if the given position is adjacent to the current terrain position.
            Two positions are considered adjacent if they share the same x or y coordinate and differ by at most 1 in the other coordinate.
         */
        if ((currentTerrain.Item1 + 1 == posx || currentTerrain.Item1 - 1 == posx || currentTerrain.Item1 == posx)
        && (currentTerrain.Item2 + 1 == posy || currentTerrain.Item2 - 1 == posy || currentTerrain.Item2 == posy)) {
            return true;
        }
        return false;
    }


    // Method to play a sound effect based on the type of character involved in the action
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

    // Function to retrieve a list of IDs of characters located in a specific terrain position
    public List<string> GetCharactersInTerrain(float Posx, float Posy){
        List<string> CharsInTerrain = new List<string>();

        // Iterate through each possible character ID
        for (int i = 1; i <= numPieces; i++)
        {
            string id = i.ToString();

            // Check if the character ID exists in the CharToTerrain dictionary
            if (CharToTerrain.ContainsKey(id)) {
                // Get the terrain position of the character
                (float, float) terrain = CharToTerrain[id];
                // Find the game object representing the character on the game board
                GameObject c = gameBoard.transform.Find($"{id}")?.gameObject;
                
                // If the character exists and its terrain position matches the specified position, add its ID to the list
                if (c != null && terrain.Item1 == Posx && terrain.Item2 == Posy) {
                     CharsInTerrain.Add(id);
                }
            }
            
        }
        // Return the list of character IDs in the specified terrain position
        return CharsInTerrain;
    }


    // Function to retrieve a list of IDs of enemy characters from a given list of characters
    public List<string> GetEnemies(string team, List<string> characters) {

        // Creates an empty list to store the IDs of enemy characters
        List<string> enemies = new List<string>();

        // Iterate through each character ID in the given list
        foreach(string entity in characters){
            // Find the game object representing the character on the game board
            GameObject character = gameBoard.transform.Find($"{entity}")?.gameObject;
            // Get the CharacterIdleMacro component attached to the character
            CharacterIdleMacro aux = character.GetComponent<CharacterIdleMacro>();

            // Get the team affiliation of the character (one of the players team)
            string entityTeam = aux.GetTeam();

            // If the character's team is opposite from the specified team, add its ID to the list of enemies
            if (entityTeam != team){
                enemies.Add(entity);
            }
        }
        return enemies;
    }


    // Function to summon new game pieces onto the game board
    void SummonPieces(XmlNode unit) {
        numPieces++;

        // Retrieve necessary attributes from the XML node
        string id = unit.Attributes["id"].Value;
        string type = unit.Attributes["type"].Value;
        float x = float.Parse(unit.Attributes["x"].Value);
        float y = float.Parse(unit.Attributes["y"].Value);
        string team = unit.Attributes["role"].Value;

        // Get the prefab corresponding to the type of the character
        GameObject prefab = GetPrefabByType(type);

        if (prefab != null) {
            // Find the terrain game object corresponding to the specified coordinates
            Transform terrain = gameBoard.transform.Find($"{x},{y}");
            if (terrain != null) {
                // Try to place the piece on the terrain
                if (TryPlacePiece(terrain, prefab, type, team, id, x, y)) {
                    // If successful, add the piece's ID and terrain coordinates to the CharToTerrain dictionary
                    CharToTerrain.Add(id, (x, y));
                }
            } else {
                Debug.LogWarning($"Terrain at position ({x},{y}) not found.");
            }
        } else {
            Debug.LogWarning($"Prefab for type '{type}' not found.");
        }
    }

    // Function to try placing a new game piece onto a terrain tile
    bool TryPlacePiece(Transform terrain, GameObject prefab, string type, string team, string id, float x, float y) {
        // Determine the position within the quadrant
        int pieceCount = GetCharactersInTerrain(x, y).Count;

        //Get the positions of each quadrant
        Vector3[] quadrantPositions = GetQuadrant(terrain, type);

        // Check if there is room to place the piece on the terrain
        if (pieceCount < 4) {
            // Calculate the spawn position within the quadrant and instanciate the prefab
            Vector3 spawnPosition = quadrantPositions[pieceCount];
            GameObject instance = Instantiate(prefab, gameBoard.transform);

            // Get the CharacterIdleMacro component attached to the new instance and set the team affilliation of the new instance
            CharacterIdleMacro character = instance.GetComponent<CharacterIdleMacro>();
            character.SetTeam(team);

            // If the type is catapult, set additional properties
            if (type == "catapult") {
                character.SetCatapult();
            }

            // Set the position, rotation, scale, name, and tag of the new instance
            instance.transform.position = spawnPosition;
            instance.transform.rotation = Quaternion.Euler(0, 0, 0);
            instance.transform.localScale = GetScaleByType(type);
            instance.name = id;
            instance.tag = type;

            // Add the new instance to the list of game objects
            gameObjects.Add(instance);

            return true;
        } else {
            Debug.LogWarning($"Cannot place more than 4 characters on terrain at position ({x},{y}).");
            return false;
        }
    }

    // Function to retrieve the prefab game object based on the provided unit type
    GameObject GetPrefabByType(string type) {
        // Use a switch statement to determine the appropriate prefab based on the unit type
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


    // Function to retrieve the depth of a game object based on its type
    float GetDepthByType(string type) {
        return type == "catapult" ? 0.15f : 0f;
    }

    // Function to retrieve the scale of a game object based on its type
    Vector3 GetScaleByType(string type) {
        return type == "catapult" ? new Vector3(0.00005f, 0.00005f, 0.00005f) : new Vector3(0.00025f, 0.00025f, 0.00025f);
    }

    // Function to retrieve the prefab based on the field type
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

    // Function to calculate the scale of a prefab based on its width and height
    Vector3 GetScaledPrefab(GameObject prefab, float prefabWidth, float prefabHeight) {
        // Get the MeshFilter component of the prefab
        MeshFilter meshFilterPrefab = prefab.GetComponent<MeshFilter>();
        // Calculate the scale based on prefab's mesh bounds
        float prefabScaleX = prefabWidth / meshFilterPrefab.sharedMesh.bounds.size.z;
        float prefabScaleZ = prefabHeight / meshFilterPrefab.sharedMesh.bounds.size.x;
        return new Vector3(prefabScaleX, 1f, prefabScaleZ);
    }


    // Function to control the game loop, executing actions for each turn until the game ends or is stopped
    private IEnumerator PlayLoop() {
        isRunning = true;

        // Continuously loop until it reaches the last turn or is stopped
        while (!lastTurn && isRunning) {
            onPlay = true;
            // Execute actions for the current turn
            if (currTurn != null) {
                numberTurns = GetTurnNumber(currTurn);
                yield return StartCoroutine(ProcessActions(currTurn.SelectNodes("./unit"), false, line));

                // Move to the next turn if available, otherwise trigger game over
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

    // Method to restart the game when restart button is clicked
    private void RestartGame() {
        // Restart the game if there's a board and the game it's not currently playing
        if (hasBoard) {
            isRunning = false;
            if(!onPlay) {
                lastTurn = false;

                // Destroy all game objects, so it can reset and generate new ones
                foreach (GameObject game in gameObjects) {
                    Destroy(game);
                }

                // Reset to the first turn and start playing loop again 
                currTurn = firstTurn;
                line = 0;
                CharToTerrain.Clear();
                numberTurns = 0;
                StartCoroutine(PlayLoop());
            }
        }
    }

    // Method to display the game over text
    private IEnumerator GameOver() {
        gameOverText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        gameOverText.gameObject.SetActive(false);
    }

    // Pauses or resumes the game based on the current state
    private void OnClickPause() {
        if (hasBoard) {
            // If the game is not running, not currently playing, and not in the last turn, plays the ambient sound and resumes the game again
            if (!isRunning && !onPlay && !lastTurn) {
                Ambient.Play();
                StartCoroutine(PlayLoop());
            } else {
                // Otherwise, pause the game by setting the isRunning flag to false
                isRunning = false; 
            }
        }
    }

    // When the forward button is clicked, it goes to the next turn
    private void OnClickForward() {
        if (hasBoard) {
            isRunning = false;
            if(!onPlay) {
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

    // When the back button is clicked, it goes to the previous turn
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
                // Update the number of turns displayed
                numberTurns = GetTurnNumber(currTurn)-1; 
                turns.text = $"Nº Turns: {numberTurns}";
            }
        }
    }

    // Processes actions backward for the given turn
    void ProcessActionsBackwards(XmlNode turn) {
        XmlNodeList unitNodes = turn.SelectNodes("./unit");

        // Iterate through each unit node
        foreach (XmlNode unitNode in unitNodes) {
            // Get the action attribute value
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
                    string id = unitNode.Attributes["id"].Value; // Get the ID of the character
                    GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject; // Find the game object by ID
                    CharacterIdleMacro macro = piece.GetComponent<CharacterIdleMacro>(); // Get the CharacterIdleMacro component
                    macro.Revive(); // Revive the character
                    piece.SetActive(true); // Activate the game object
                    break;
                default:
                    Debug.LogWarning("Unknown action: " + action);
                    break;
            }
        }
    }

    // Moves the specified piece back to its previous position
    void MoveBack(XmlNode turn, XmlNode unit){
        string id = unit.Attributes["id"].Value;
        GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;
        XmlNodeList unitNodes = turn.PreviousSibling.SelectNodes("./unit");
        Vector3 move_to_position = new Vector3(0,0,0);

        // Iterate through each unit node of the previous turn
        foreach (XmlNode unitNode in unitNodes){
            string idAux = unitNode.Attributes["id"].Value;

            // If the ID matches the current unit's ID
            if(idAux == id){
                // Get the x and y coordinates of the unit from the previous turn
                float x = float.Parse(unitNode.Attributes["x"].Value);
                float y = float.Parse(unitNode.Attributes["y"].Value);

                // Determine the number of pieces currently occupying the target terrain
                int pieceCount = GetCharactersInTerrain(x, y).Count;

                // Update the CharToTerrain dictionary with the new coordinates of the character
                CharToTerrain[id] = (x,y);

                // Find the terrain transform by coordinates and set the move-to position to the terrain position
                Transform terrain = gameBoard.transform.Find($"{x},{y}");
                move_to_position = terrain.position;

                // Get the CharacterIdleMacro component attached to the unit game object
                CharacterIdleMacro cim = piece.GetComponent<CharacterIdleMacro>();

                //Get the positions of each quadrant
                Vector3[] quadrantPositions = GetQuadrant(terrain, piece.tag);

                // If there are less than 4 pieces in the terrain, move the character to a position within the quadrant
                if (pieceCount < 4) {
                    // Get the correct quadrant
                    Vector3 movePosition = quadrantPositions[pieceCount]; 
                    // Move the piece back to its previous position
                    piece.transform.position = movePosition;
                    // Update the current position in the CharacterIdleMacro component
                    cim.SetCurPos(movePosition);
                }
            }
        }
    }

    // Get the turn number of the specified turn node
    int GetTurnNumber(XmlNode thisTurn){
        // Iterate through the list of turns
        for (int i = 0; i < turnsList.Count; i++)
        {
            // Check if the current turn node matches the specified turn and returns the turn number
            if (turnsList[i] == thisTurn)
            {
                return i+1;
            }
        }
        return -1;
    }
}
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
    public CharacterIdleMacro cim;

    private bool waitForInput = true;
    [SerializeField] GameObject gameBoard;
    XmlDocument xmlDoc = new XmlDocument();
    XmlNode currTurn;

    public void StartReadingXML(string xmlFilePath){
        ReadXML(xmlFilePath);
<<<<<<< Updated upstream
        XmlNode turnNodes = xmlDoc.SelectSingleNode("//turns");
        currTurn = turnNodes.FirstChild;
        StartCoroutine(play(currTurn));
=======
        numberTurns = 0;
    }

    void OnEnable(){

        if (isRunning){
            Ambient.Play();
            StartCoroutine(PlayLoop());
        }
>>>>>>> Stashed changes
    }

<<<<<<< Updated upstream
    void ReadXML(string xmlFilePath){
        xmlDoc.Load(xmlFilePath);
=======
    void Start() {
        DontDestroyOnLoad(this);
        changeSceneButton.gameObject.SetActive(false);
        buttonPause.onClick.AddListener(OnClickPause);
        buttonFoward.onClick.AddListener(OnClickForward);
        buttonBack.onClick.AddListener(OnClickBack);
    }
>>>>>>> Stashed changes

        XmlNode boardNode = xmlDoc.SelectSingleNode("//board");
        if (boardNode != null){
            width = int.Parse(boardNode.Attributes["width"].Value);
            height = int.Parse(boardNode.Attributes["height"].Value);
            XmlNodeList fields = boardNode.ChildNodes;
            buildBoard(fields, width, height);
        }
    }

    void Update(){

    }

    IEnumerator play(XmlNode turn){
        XmlNodeList unitNodes = turn.SelectNodes("./unit");
        foreach (XmlNode unitNode in unitNodes){
            string action = unitNode.Attributes["action"].Value;
            switch (action){
                case "spawn":
                    summonPieces(unitNode);
                    break;
                case "move_to":
                    move(unitNode);
                    break;
                case "attack":
                    attack(unitNode);
                    //  só animação de atacar e remoção das personagens adversárias que se encontram na casa atacada
                    break;
                case "hold":
                    break;
                default:
                    Debug.LogWarning("Unknown action: " + action);
                    break;
            }
        }
        yield return new WaitForSeconds(5);
        if(turn.NextSibling != null){
            currTurn = turn.NextSibling;
            StartCoroutine(play(currTurn));
        }
        /*
        if(PlayButton.isActive){
            yield return new WaitForSeconds(5);
            if(turn.NextSibling != null){
                currTurn = turn.NextSibling;
                StartCoroutine(play(currTurn));
            }
        }else if(NextButton.onClick){
            if(turn.NextSibling != null){
                currTurn = turn.NextSibling;
                StartCoroutine(play(currTurn));
            }
        }else if(PreviousButton.onClick){
            if(turn.PreviousSibling != null){
                currTurn = turn.PreviousSibling;
                StartCoroutine(play(currTurn));
            }
        }
        */
    }

    void move(XmlNode unit){
        string id = unit.Attributes["id"].Value;
        GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;
        if (piece != catapult){
            float x = float.Parse(unit.Attributes["x"].Value);
            float y = float.Parse(unit.Attributes["y"].Value);
            Transform terrain = gameBoard.transform.Find($"{x},{y}");
            Vector3 position = terrain.localPosition;
            cim.SetCharacter(piece);
            cim.SetTarget(position);
            cim.Update();
        }
    }

    void attack(XmlNode unit){
        string id = unit.Attributes["id"].Value;
        GameObject piece = gameBoard.transform.Find($"{id}")?.gameObject;
        float x = float.Parse(unit.Attributes["x"].Value);
        float y = float.Parse(unit.Attributes["y"].Value);
<<<<<<< Updated upstream
        print($"{id} atacou");
    }

    void summonPieces(XmlNode unit){
        string type = unit.Attributes["type"].Value;
        float x = float.Parse(unit.Attributes["x"].Value);
        float y = float.Parse(unit.Attributes["y"].Value);
        GameObject prefab = null;
        float depth = 0f;
        Vector3 scale = new Vector3(0.0001f, 0.0001f, 0.0001f);
        switch (type){
=======
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
                    if(isTerrainAdjacent(id, x, y)){
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

    bool isTerrainAdjacent(string id, float posx, float posy){
        (float, float) currentTerrain = CharToTerrain[id];
        if((currentTerrain.Item1 + 1 == posx || currentTerrain.Item1 - 1 == posx || currentTerrain.Item1 == posx) && (currentTerrain.Item2 + 1 == posy || currentTerrain.Item2 - 1 == posy || currentTerrain.Item2 == posy)){
            return true;
        }
        return false;
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
                List<string> charsinthisterrain = GetCharactersInTerrain(x,y);
                if(charsinthisterrain.Count < 4){
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
                    CharToTerrain.Add(id, (x,y));
                    instance.transform.position = randomPosition;
                    instance.transform.rotation = Quaternion.Euler(0, 0, 0);
                    instance.transform.localScale = GetScaleByType(type);
                    instance.name = unit.Attributes["id"].Value;
                    instance.tag = type;
                    gameObjects.Add(instance);
                }else{
                    Debug.LogWarning($"Terrain at position ({x},{y}) has already 4 pieces.");
                }

            } else {
                Debug.LogWarning($"Terrain at position ({x},{y}) not found.");
            }
        } else {
            Debug.LogWarning($"Prefab for type '{type}' not found.");
        }
    }

    GameObject GetPrefabByType(string type) {
        switch (type) {
>>>>>>> Stashed changes
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
        if (prefab != null){
            Transform terrain = gameBoard.transform.Find($"{x},{y}");
            Vector3 position = terrain.localPosition;
            GameObject instance = Instantiate(prefab, gameBoard.transform);
            //METER ALEATORIDADE NA POSIÇAO
            instance.transform.localPosition = new Vector3(position.x, position.y, position.z + depth);
            Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
            instance.transform.rotation = targetRotation;
            instance.transform.localScale = scale;
            instance.name = unit.Attributes["id"].Value;
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
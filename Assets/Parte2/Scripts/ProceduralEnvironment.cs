using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using UnityEngine;

public class ProceduralTerrainGenerator : MonoBehaviour
{

    // Prefabs for different environmental elements
    public GameObject treePrefab;
    public GameObject rockPrefab;
    public GameObject housePrefab;

    // Terrain parameters
    public int terrainResolution = 1024;
    private float terrainSize = 200;

    // Materials for different terrain types
    public Material mountain;
    public Material village;
    public Material plain;
    public Material desert;
    public Material forest;

    // Parent object to hold generated objects
    public GameObject parent;

    // Terrain and terrain data
    private Terrain terrain;
    private TerrainData terrainData;
    private string terrainType;

    // Dictionary to map terrain types to materials
    private Dictionary<string, Material> terrainMaterials;

    void Start()
    {
        // Retrieve the terrain type from player preferences or use "default"
        terrainType = PlayerPrefs.GetString("TerrainType", "default");
        // Invoke different methods
        InitializeTerrainMaterials();
        GenerateTerrain();
        LoadEnvironmentFromXML();
    }

    // Initialize terrain materials dictionary
    void InitializeTerrainMaterials()
    {
        terrainMaterials = new Dictionary<string, Material>
        {
            { "mountain", mountain },
            { "village", village },
            { "plain", plain },
            { "desert", desert },
            { "forest", forest }
        };
    }

    // Generate terrain
    void GenerateTerrain()
    {
        // Create a new GameObject to hold the terrain
        GameObject terrainObject = new GameObject("ProceduralTerrain");
        terrainObject.transform.parent = parent.transform;

        // Add Terrain component to the terrain GameObject
        terrain = terrainObject.AddComponent<Terrain>();

        // Create new TerrainData and set its properties
        terrainData = new TerrainData
        {
            heightmapResolution = terrainResolution,
            size = new Vector3(terrainSize, terrainSize, terrainSize)
        };

        // Assign TerrainData to the Terrain component
        terrain.terrainData = terrainData;

        // Add TerrainCollider to the terrain GameObject
        TerrainCollider terrainCollider = terrainObject.AddComponent<TerrainCollider>();
        terrainCollider.terrainData = terrainData;

        // Set terrain position to zero
        terrainObject.transform.position = Vector3.zero;

        // Assign material based on terrain type
        if (terrainMaterials.ContainsKey(terrainType))
        {
            terrain.materialTemplate = terrainMaterials[terrainType];
        }
        else
        {
            Debug.LogError($"Terrain type '{terrainType}' not found in terrainMaterials dictionary.");
        }

    }

    // Load environment data from XML file
    void LoadEnvironmentFromXML()
    {
        // Load XML document
        TextAsset xmlTextAsset = Resources.Load<TextAsset>("environment_part_2");
        if (xmlTextAsset == null)
        {
            Debug.LogError($"Failed to load XML file");
            return;
        }

        // Load XML document from the text
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlTextAsset.text);
        
        // Get list of "square" nodes from XML
        XmlNodeList squares = xmlDoc.GetElementsByTagName("square");

        // Iterate through each square node
        foreach (XmlNode square in squares)
        {
            string type = square.Attributes["type"].Value;

            // Process data if it matches the current terrain type
            if (type == terrainType) {
                // Extract maximum elevation for the square
                float maxElevation = ParseFloat(square.Attributes["maximum_elevation"].Value);

                // List to store object data
                List<ObjectData> objects = new List<ObjectData>();

                // Iterate through child nodes to get object data
                foreach (XmlNode obj in square.ChildNodes)
                {
                    // Extract object type and densities for low and high altitudes
                    string objType = obj.Attributes["type"].Value;
                    float densityLow = ParseFloat(obj.Attributes["density_low_altitute"].Value);
                    float densityHigh = ParseFloat(obj.Attributes["density_high_altitute"].Value);
                    objects.Add(new ObjectData(objType, densityLow, densityHigh));
                }

                // Generate terrain heights based on the current type and maximum elevation and Place objects on the terrain
                GenerateTerrainHeights(type, maxElevation);
                PlaceObjects(objects, maxElevation);
            }
        }
    }

    // Generate terrain heights based on type and maximum elevation
    void GenerateTerrainHeights(string type, float maxElevation)
    {
        if (terrainData == null)
        {
            Debug.LogError("TerrainData is not initialized.");
            return;
        }
        // Initialize a 2D array to store terrain heights
        float[,] heights = new float[terrainResolution, terrainResolution];

        // Calculate center coordinates of the terrain
        float centerX = terrainResolution / 2;
        float centerY = terrainResolution / 2;
        float flatRadius = terrainResolution / 8; // Radius of the fully flat area
        float blendRadius = terrainResolution / 2; // Radius of the blending area
        float flatHeight = 0f; // Height of the flat area

        // Loop through each point in the terrain
        for (int x = 0; x < terrainResolution; x++)
        {
            for (int y = 0; y < terrainResolution; y++)
            {
                // Calculate Perlin noise to generate terrain height
                float perlinHeight = (Mathf.PerlinNoise(x * 0.01f, y * 0.01f) +
                                  Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * 0.5f) * maxElevation / terrainData.size.y;

                // Calculate the distance from the center
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));

                if (distance < flatRadius)
                {
                    // Fully flat area
                    heights[x, y] = flatHeight;
                } else if (distance < blendRadius) {
                    // Blending area
                    float t = (distance - flatRadius) / (blendRadius - flatRadius); // Normalized distance within the blending area
                    float blendHeight = Mathf.Lerp(flatHeight, perlinHeight, t);
                    heights[x, y] = blendHeight;
                } else {
                    // Outside blending area
                    heights[x, y] = perlinHeight;
                }
            }
        }
        // Set generated heights to the terrain data
        terrainData.SetHeights(0, 0, heights);
    }

    void PlaceObjects(List<ObjectData> objects, float maxElevation)
    {
        // Define altitude limits for object placement
        float lowAltitudeLimit = maxElevation * 0.2f;
        float highAltitudeLimit = maxElevation * 0.8f;

        int maxObjectsPerType = 1500; // Maximum number of objects per type
        int placementStep = 35; // Controls spacing between placement checks

        // Calculate center coordinates of the terrain
        float centerX = terrainResolution / 2;
        float centerY = terrainResolution / 2;
        float flatRadius = terrainResolution / 8;

        // Dictionary to keep track of the number of placed objects per type
        Dictionary<string, int> placedObjectsCount = new Dictionary<string, int>();
        // List to track positions of placed objects
        List<Vector3> placedPositions = new List<Vector3>();

        // Initialize counts for each object type
        foreach (var objData in objects)
        {
            placedObjectsCount[objData.type] = 0;
        }

        // Loop through terrain to place objects
        for (int x = 0; x < terrainResolution; x += placementStep)
        {
            for (int y = 0; y < terrainResolution; y += placementStep)
            {
                // Get normalized height at current position and calculate the distance from the center
                float normalizedHeight = terrainData.GetHeight(x, y) / maxElevation;
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));

                // Place objects outside flat area
                if (distance > flatRadius) {
                    foreach (var objData in objects)
                    {
                        // Skip object type if the maximum count has been reached
                        if (placedObjectsCount[objData.type] >= maxObjectsPerType)
                        {
                            continue; // Skip this type if the maximum count has been reached
                        }

                        // Determine object density based on terrain altitude
                        float density = (normalizedHeight < 0.2f) ? objData.densityLow :
                                    (normalizedHeight > 0.8f) ? objData.densityHigh : 0f;

                        // Place object randomly based on density
                        if (Random.value < density)
                        {
                            float posX = x * (terrainSize / terrainResolution);
                            float posZ = y * (terrainSize / terrainResolution);
                            float posY = terrainData.GetHeight(x, y);

                            Vector3 position = new Vector3(posX, posY, posZ);

                            // Check for overlap before placing object
                            if (!IsOverlapping(position, placedPositions, 2.0f)) // Check for overlap within 2 units
                            {
                                // Instantiate object with random rotation
                                Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                                Instantiate(GetPrefab(objData.type), position, rotation, parent.transform);

                                // Update placed objects count and positions list
                                placedPositions.Add(position);
                                placedObjectsCount[objData.type]++;
                            }
                        }
                    }
                }
            }
        }
    }

    bool IsOverlapping(Vector3 position, List<Vector3> placedPositions, float minDistance)
    {
        foreach (var placedPosition in placedPositions)
        {
            if (Vector3.Distance(position, placedPosition) < minDistance)
            {
                return true; // The new position is too close to an existing object
            }
        }
        return false;
    }
    GameObject GetPrefab(string type)
    {
        switch (type)
        {
            case "tree":
                return treePrefab;
            case "rock":
                return rockPrefab;
            case "house":
                return housePrefab;
            default:
                return null;
        }
    }

    float ParseFloat(string value)
    {
        float result;
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
        {
            return result;
        }
        else
        {
            Debug.LogError($"Failed to parse float from string: {value}");
            return 0f; // Retorne um valor padrão ou decida como tratar o erro
        }
    }

    class ObjectData
    {
        public string type;
        public float densityLow;
        public float densityHigh;

        public ObjectData(string type, float densityLow, float densityHigh)
        {
            this.type = type;
            this.densityLow = densityLow;
            this.densityHigh = densityHigh;
        }
    }
}
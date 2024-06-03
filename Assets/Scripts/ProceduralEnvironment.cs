using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using UnityEngine;

public class ProceduralTerrainGenerator : MonoBehaviour
{
    public string xmlFilePath = "Assets/Resources/environment_part_2.xml";
    public GameObject treePrefab;
    public GameObject rockPrefab;
    public GameObject housePrefab;
    public int terrainResolution = 1024;
    private float terrainSize = 200;

    public Material mountain;
    public Material village;
    public Material plain;
    public Material desert;
    public Material forest;
    public GameObject parent;

    private Terrain terrain;
    private TerrainData terrainData;
    private string terrainType;

    private Dictionary<string, Material> terrainMaterials;

    void Start()
    {
        //terrainType = PlayerPrefs.GetString("TerrainType", "default");
        terrainType = "desert";
        InitializeTerrainMaterials();
        GenerateTerrain();
        LoadEnvironmentFromXML(xmlFilePath);
    }

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

    void GenerateTerrain()
    {
        GameObject terrainObject = new GameObject("ProceduralTerrain");
        terrainObject.transform.parent = parent.transform;
        terrain = terrainObject.AddComponent<Terrain>();
        terrainData = new TerrainData
        {
            heightmapResolution = terrainResolution,
            size = new Vector3(terrainSize, terrainSize, terrainSize)
        };
        terrain.terrainData = terrainData;

        TerrainCollider terrainCollider = terrainObject.AddComponent<TerrainCollider>();
        terrainCollider.terrainData = terrainData;

        terrainObject.transform.position = Vector3.zero;

        if (terrainMaterials.ContainsKey(terrainType))
        {
            terrain.materialTemplate = terrainMaterials[terrainType];
        }
        else
        {
            Debug.LogError($"Terrain type '{terrainType}' not found in terrainMaterials dictionary.");
        }

    }

    void LoadEnvironmentFromXML(string path)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);

        XmlNodeList squares = xmlDoc.GetElementsByTagName("square");

        foreach (XmlNode square in squares)
        {
            string type = square.Attributes["type"].Value;
            if (type == terrainType){
                float maxElevation = ParseFloat(square.Attributes["maximum_elevation"].Value);

                List<ObjectData> objects = new List<ObjectData>();
                foreach (XmlNode obj in square.ChildNodes)
                {
                    string objType = obj.Attributes["type"].Value;
                    float densityLow = ParseFloat(obj.Attributes["density_low_altitute"].Value);
                    float densityHigh = ParseFloat(obj.Attributes["density_high_altitute"].Value);
                    objects.Add(new ObjectData(objType, densityLow, densityHigh));
                }


                GenerateTerrainHeights(type, maxElevation);
                PlaceObjects(objects, maxElevation);
            }
        }
    }

    void GenerateTerrainHeights(string type, float maxElevation)
    {
    float[,] heights = new float[terrainResolution, terrainResolution];
    float centerX = terrainResolution / 2;
    float centerY = terrainResolution / 2;
    float flatRadius = terrainResolution / 8; // Radius of the fully flat area
    float blendRadius = terrainResolution / 2; // Radius of the blending area
    float flatHeight = 0f; // Height of the flat area

    for (int x = 0; x < terrainResolution; x++)
    {
        for (int y = 0; y < terrainResolution; y++)
        {
            float perlinHeight = (Mathf.PerlinNoise(x * 0.01f, y * 0.01f) +
                                  Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * 0.5f) * maxElevation / terrainData.size.y;

            // Calculate the distance from the center
            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));

            if (distance < flatRadius)
            {
                // Fully flat area
                heights[x, y] = flatHeight;
            }
            else if (distance < blendRadius)
            {
                // Blending area
                float t = (distance - flatRadius) / (blendRadius - flatRadius); // Normalized distance within the blending area
                float blendHeight = Mathf.Lerp(flatHeight, perlinHeight, t);
                heights[x, y] = blendHeight;
            }
            else
            {
                // Outside blending area
                heights[x, y] = perlinHeight;
            }
        }
    }
    terrainData.SetHeights(0, 0, heights);
    }

    void PlaceObjects(List<ObjectData> objects, float maxElevation)
{
    float lowAltitudeLimit = maxElevation * 0.2f;
    float highAltitudeLimit = maxElevation * 0.8f;

    int maxObjectsPerType = 1500; // Maximum number of objects per type
    int placementStep = 35; // Controls spacing between placement checks

    float centerX = terrainResolution / 2;
    float centerY = terrainResolution / 2;
    float flatRadius = terrainResolution / 8;

    // Dictionary to keep track of the number of placed objects per type
    Dictionary<string, int> placedObjectsCount = new Dictionary<string, int>();
    // List to track positions of placed objects
    List<Vector3> placedPositions = new List<Vector3>();

    foreach (var objData in objects)
    {
        placedObjectsCount[objData.type] = 0;
    }

    for (int x = 0; x < terrainResolution; x += placementStep)
    {
        for (int y = 0; y < terrainResolution; y += placementStep)
        {
            float normalizedHeight = terrainData.GetHeight(x, y) / maxElevation;
            // Calculate the distance from the center
            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));

            if (distance > flatRadius)
            {
                foreach (var objData in objects)
                {
                    if (placedObjectsCount[objData.type] >= maxObjectsPerType)
                    {
                        continue; // Skip this type if the maximum count has been reached
                    }

                    float density = (normalizedHeight < 0.2f) ? objData.densityLow :
                                    (normalizedHeight > 0.8f) ? objData.densityHigh : 0f;

                    if (Random.value < density)
                    {
                        float posX = x * (terrainSize / terrainResolution);
                        float posZ = y * (terrainSize / terrainResolution);
                        float posY = terrainData.GetHeight(x, y);

                        Vector3 position = new Vector3(posX, posY, posZ);

                        if (!IsOverlapping(position, placedPositions, 2.0f)) // Check for overlap within 2 units
                        {
                            Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                            Instantiate(GetPrefab(objData.type), position, rotation, parent.transform);

                            placedPositions.Add(position); // Add the new position to the list
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
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
    public int terrainResolution = 512;
    public float terrainSize = 500;

    private Terrain terrain;
    private TerrainData terrainData;

    void Start()
    {
        GenerateTerrain();
        LoadEnvironmentFromXML(xmlFilePath);
    }

    void GenerateTerrain()
    {
        GameObject terrainObject = new GameObject("ProceduralTerrain");
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
    }

    void LoadEnvironmentFromXML(string path)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);

        XmlNodeList squares = xmlDoc.GetElementsByTagName("square");

        foreach (XmlNode square in squares)
        {
            string type = square.Attributes["type"].Value;
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

    void GenerateTerrainHeights(string type, float maxElevation)
    {
        float[,] heights = new float[terrainResolution, terrainResolution];
        for (int x = 0; x < terrainResolution; x++)
        {
            for (int y = 0; y < terrainResolution; y++)
            {
                float height = (Mathf.PerlinNoise(x * 0.01f, y * 0.01f) +
                                Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * 0.5f) * maxElevation / terrainData.size.y;
                heights[x, y] = height;
            }
        }
        terrainData.SetHeights(0, 0, heights);
    }

    void PlaceObjects(List<ObjectData> objects, float maxElevation)
    {
        float lowAltitudeLimit = maxElevation * 0.2f;
        float highAltitudeLimit = maxElevation * 0.8f;

        for (int x = 0; x < terrainResolution; x++)
        {
            for (int y = 0; y < terrainResolution; y++)
            {
                float normalizedHeight = terrainData.GetHeight(x, y) / maxElevation;
                foreach (var objData in objects)
                {
                    float density = (normalizedHeight < 0.2f) ? objData.densityLow :
                                    (normalizedHeight > 0.8f) ? objData.densityHigh : 0f;

                    if (Random.value < density)
                    {
                        float posX = x * (terrainSize / terrainResolution);
                        float posZ = y * (terrainSize / terrainResolution);
                        float posY = terrainData.GetHeight(x, y);

                        Vector3 position = new Vector3(posX, posY, posZ);
                        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                        Instantiate(GetPrefab(objData.type), position, rotation);
                    }
                }
            }
        }
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
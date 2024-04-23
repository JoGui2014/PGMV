using UnityEngine;
using System.Xml;
using System;
using System.Collections.Generic;


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

     void Start()
    {
        // Load the XML file
        TextAsset xmlFile = Resources.Load<TextAsset>("game__enunciado_statement");
        XmlDocument xmlDoc = new XmlDocument();
        
        xmlDoc.LoadXml(xmlFile.text);

        // Extract width and height of the gaming board
        XmlNode boardNode = xmlDoc.SelectSingleNode("//board");
        if (boardNode != null)
        {
            width = int.Parse(boardNode.Attributes["width"].Value);
            height = int.Parse(boardNode.Attributes["height"].Value);
        }

        // Extract and instantiate GameObjects based on XML data
        XmlNodeList turnNodes = xmlDoc.SelectNodes("//turn");
        foreach (XmlNode turnNode in turnNodes)
        {
            XmlNodeList unitNodes = turnNode.SelectNodes("./unit");
            foreach (XmlNode unitNode in unitNodes)
            {
                string type = unitNode.Attributes["type"].Value;
                float x = float.Parse(unitNode.Attributes["x"].Value);
                float y = float.Parse(unitNode.Attributes["y"].Value);

                string action = unitNode.Attributes["action"].Value;

                GameObject prefab = null;

                if (action == "spawn") {
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
                    GameObject instance = Instantiate(prefab, new Vector3(-6, 8, 0), Quaternion.identity);
                }
            }
        }

        // Now you can use boardWidth and boardHeight as needed
        Debug.Log("Board Width: " + width);
        Debug.Log("Board Height: " + height);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Xml;

public class XMLReader : MonoBehaviour
{
    static void Main(string[] args)
    {
        // Provide the path to your XML file
        string filePath = "C://Users//guiva//OneDrive//Documents//ISCTE//Primeiro ano Mestrado ISCTE//Segundo Semestre//PGMV//Projeto//Projeto1//PGMV//Assets//Scripts//XML//game__enunciado_statement.xml";

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found.");
            return;
        }

        // Read the XML file line by line
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Process each line as needed
                Console.WriteLine(line);
            }
        }
    }
}


using UnityEngine;
using UnityEditor;
using System.IO;

public class EmptyJsonFileCreator : ScriptableObject
{
    [MenuItem("Assets/Create/Empty JSON File")]
    public static void CreateEmptyJsonFile()
    {
        // Get the currently selected folder in the Project window
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        // If no folder is selected or if a file is selected, use the Assets folder
        if (string.IsNullOrEmpty(selectedPath) || File.Exists(selectedPath))
        {
            selectedPath = "Assets";
        }

        // Ensure the path is a directory
        if (!Directory.Exists(selectedPath))
        {
            selectedPath = Path.GetDirectoryName(selectedPath);
        }

        // Create a unique filename
        string fileName = "EmptyJsonFile";
        string filePath = Path.Combine(selectedPath, fileName + ".json");

        // Ensure the filename is unique
        int counter = 1;
        while (File.Exists(filePath))
        {
            filePath = Path.Combine(selectedPath, fileName + counter + ".json");
            counter++;
        }

        // Create the empty JSON file with a basic structure
        string jsonContent = "{\n    \n}";
        File.WriteAllText(filePath, jsonContent);

        // Refresh the Asset Database to show the new file
        AssetDatabase.Refresh();

        // Select the newly created file
        Object newAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(Object));
        Selection.activeObject = newAsset;
        EditorGUIUtility.PingObject(newAsset);

        Debug.Log($"Created empty JSON file: {filePath}");
    }
}

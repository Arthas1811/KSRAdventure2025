// Script taken from KSRAdventure2025

using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;

// Handles saving and loading the player's progress using a JSON file.
public static class SaveDataManager
{
    // OS-safe path that persists across game updates
    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "SaveData.json");

    public static JObject ReadData()
    {
        // Return existing save file if available
        if (File.Exists(SaveFilePath))
        {
            string json = File.ReadAllText(SaveFilePath);
            return JObject.Parse(json);
        }

        // Fallback to default starting state for new games
        TextAsset defaultSave = Resources.Load<TextAsset>("SaveFile/SaveData");
        if (defaultSave == null)
        {
            Debug.LogError("SaveDataManager: Kein Default-SaveFile unter Resources/SaveFile/saveData gefunden.");
            return new JObject(); // Return empty object to prevent crashes
        }

        return JObject.Parse(defaultSave.text);
    }

    public static void SaveData(JObject saveData)
    {
        if (saveData == null)
        {
            Debug.LogWarning("SaveDataManager: SaveData ist null, nichts gespeichert.");
            return;
        }

        File.WriteAllText(SaveFilePath, saveData.ToString());
    }
}
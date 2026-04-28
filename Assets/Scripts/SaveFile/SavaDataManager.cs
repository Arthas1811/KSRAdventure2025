using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance { get; private set; }

    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "saveData.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public JObject readData()
    {
        if (File.Exists(SaveFilePath))
        {
            return JObject.Parse(File.ReadAllText(SaveFilePath));
        }

        // First launch: copy default from Resources
        TextAsset defaultSave = Resources.Load<TextAsset>("SaveFile/saveData");
        if (defaultSave == null)
        {
            Debug.LogError("Default saveData not found at Resources/SaveFile/saveData.json");
            return new JObject();
        }
        return JObject.Parse(defaultSave.text);
    }

    public void saveData(JObject saveData)
    {
        File.WriteAllText(SaveFilePath, saveData.ToString());
    }
}
using UnityEngine;
using Newtonsoft.Json.Linq;
using AG_NameSpace;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance { get; private set; }

    // Static so the loaded/imported save survives a scene reload. This script's
    // GameObject is NOT a root object, so DontDestroyOnLoad does not actually
    // preserve the instance across SceneManager.LoadScene. A static field instead
    // persists for the whole play session and only resets on app/page restart,
    // which is exactly the lifetime we want for the in-memory save.
    private static JObject inMemorySaveData;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public JObject readData()
    {
        if (inMemorySaveData != null)
        {
            return inMemorySaveData;
        }

        TextAsset defaultSave = Resources.Load<TextAsset>("SaveFile/saveData");
        if (defaultSave == null)
        {
            Debug.LogError("Default saveData not found at Resources/SaveFile/saveData.json");
            return new JObject();
        }
        
        inMemorySaveData = JObject.Parse(defaultSave.text);
        return inMemorySaveData;
    }

    public void saveData(JObject saveData)
    {
        inMemorySaveData = saveData;
    }

    public string GetExportString()
    {
        if (inMemorySaveData == null)
        {
            readData();
        }
        return StringCipher.Encrypt(inMemorySaveData.ToString());
    }

    public bool ImportSaveString(string encryptedSave)
    {
        try
        {
            string decryptedJson = StringCipher.Decrypt(encryptedSave);
            inMemorySaveData = JObject.Parse(decryptedJson);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
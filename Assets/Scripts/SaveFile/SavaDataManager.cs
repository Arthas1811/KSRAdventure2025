using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

public class SaveDataManager : MonoBehaviour
{
    public JObject readData()
    {
        string savePath = Path.Combine(Application.dataPath, "Scripts/SaveFile/saveData.json");
        string saveJson = File.ReadAllText(savePath);
        return JObject.Parse(saveJson);
    }
    public void saveData(JObject saveData)
    {
        string savePath = Path.Combine(Application.dataPath, "Scripts/SaveFile/saveData.json");
        string saveJson = saveData.ToString();
        File.WriteAllText(savePath, saveJson);
    }

}




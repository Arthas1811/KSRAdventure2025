using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

public class Inventory : MonoBehaviour
{
    private JObject saveData;
    private string[] allItems;

    void Start()
    {
        string savePath = Path.Combine(Application.dataPath, "Scripts/SaveFile/saveData.json");
        string saveJson = File.ReadAllText(savePath);

        saveData = JObject.Parse(saveJson);
        allItems = saveData["itemsOwned"].ToObject<string[]>();


        foreach (var id in allItems)
        {
            InventoryState.Instance.ReceiveItem(id);
        }

        Object.FindAnyObjectByType<InventoryManager>().UpdateInventoryUI();
    }

    public void add(string id)
    {
        InventoryState.Instance.ReceiveItem(id);
        Object.FindAnyObjectByType<InventoryManager>().UpdateInventoryUI();
        JArray itemsArray = (JArray)saveData["itemsOwned"];
        if (!itemsArray.Contains(id))
        {
            itemsArray.Add(id);
            saveData["itemsOwned"] = itemsArray;
            saveDataInFile(saveData);
        }

    }

    void saveDataInFile(JObject saveData)
    {
        string savePath = Path.Combine(Application.dataPath, "Scripts/SaveFile/saveData.json");
        string saveJson = saveData.ToString();
        File.WriteAllText(savePath, saveJson);
    }

}

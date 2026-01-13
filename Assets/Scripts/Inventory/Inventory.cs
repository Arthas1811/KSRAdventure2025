using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

public class Inventory : MonoBehaviour
{   
    JObject saveData;
    private string[] allItems;

    public SaveDataManager saveDataManager;

    void Start()
    {
        saveData = saveDataManager.readData();
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
            saveDataManager.saveData(saveData);
        }

    }
}

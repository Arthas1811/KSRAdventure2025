using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

public class Inventory : MonoBehaviour
{
    JObject saveData;
    public SaveDataManager saveDataManager;
    private string[] allItems;

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
        saveData = saveDataManager.readData(); // ensure we use the latest file state
        JArray itemsArray = (JArray)saveData["itemsOwned"];
        if (!itemsArray.Contains(id))
        {
            itemsArray.Add(id);
            saveData["itemsOwned"] = itemsArray;
            saveDataManager.saveData(saveData);
        }

    }
    public void remove(string id)
    {
        InventoryState.Instance.RemoveItem(id);
        Object.FindAnyObjectByType<InventoryManager>().UpdateInventoryUI();
        saveData = saveDataManager.readData(); // ensure we use the latest file state
        JArray itemsArray = (JArray)saveData["itemsOwned"];
        if (itemsArray.Contains(id))
        {
            itemsArray.Remove(id);
            saveData["itemsOwned"] = itemsArray;
            saveDataManager.saveData(saveData);
        }
    }
}
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    JObject saveData;
    private string[] allItems;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        saveData = SaveDataManager.Instance.readData();
        allItems = saveData["itemsOwned"].ToObject<string[]>();

        foreach (var id in allItems)
        {
            InventoryState.Instance.ReceiveItem(id);
        }

        RefreshInventoryUI();
    }

    public void add(string id)
    {
        InventoryState.Instance.ReceiveItem(id);
        RefreshInventoryUI();
        saveData = SaveDataManager.Instance.readData(); // ensure we use the latest file state
        JArray itemsArray = (JArray)saveData["itemsOwned"];
        if (!itemsArray.Contains(id))
        {
            itemsArray.Add(id);
            saveData["itemsOwned"] = itemsArray;
            SaveDataManager.Instance.saveData(saveData);
        }

    }
    public void remove(string id)
    {
        InventoryState.Instance.RemoveItem(id);
        RefreshInventoryUI();
        saveData = SaveDataManager.Instance.readData(); // ensure we use the latest file state
        JArray itemsArray = (JArray)saveData["itemsOwned"];
        if (itemsArray.Contains(id))
        {
            itemsArray.Remove(id);
            saveData["itemsOwned"] = itemsArray;
            SaveDataManager.Instance.saveData(saveData);
        }
    }

    private void RefreshInventoryUI()
    {
        InventoryManager inventoryManager = Object.FindAnyObjectByType<InventoryManager>();
        if (inventoryManager != null)
            inventoryManager.UpdateInventoryUI();
    }
}
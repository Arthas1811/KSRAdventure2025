using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

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
        LoadFromSave();
    }

    public void LoadFromSave()
    {
        saveData = SaveDataManager.Instance.readData();
        allItems = saveData["itemsOwned"].ToObject<string[]>();

        InventoryState.Instance.Clear();

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
        saveData = SaveDataManager.Instance.readData();
        JArray itemsArray = (JArray)saveData["itemsOwned"];
        if (itemsArray != null && !itemsArray.Any(t => t.ToString() == id))
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
        saveData = SaveDataManager.Instance.readData();
        JArray itemsArray = (JArray)saveData["itemsOwned"];
        if (itemsArray != null)
        {
            var tokenToRemove = itemsArray.FirstOrDefault(t => t.ToString() == id);
            if (tokenToRemove != null)
            {
                itemsArray.Remove(tokenToRemove);
                saveData["itemsOwned"] = itemsArray;
                SaveDataManager.Instance.saveData(saveData);
            }
        }
    }

    public void RefreshInventoryUI()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.UpdateInventoryUI();
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemData
{
    public string name;
    public string description;
    public string image;
    public Sprite sprite;
}

[Serializable]
public class ItemEntry
{
    public string id;
    public ItemData data;
}

[Serializable]
public class ItemListWrapper
{
    public List<ItemEntry> entries;
}

public static class InventoryDatabase
{
    public static TextAsset jsonFile;
    private static Dictionary<string, ItemData> _items;

    public static void Load()
    {
        if (_items != null)
            return;

        if (jsonFile == null)
        {
            Debug.LogError("InventoryDatabase: JSON file not assigned!");
            _items = new Dictionary<string, ItemData>();
            return;
        }
        ItemListWrapper wrapper = JsonUtility.FromJson<ItemListWrapper>(jsonFile.text);

        _items = new Dictionary<string, ItemData>();

        foreach (var entry in wrapper.entries)
        {
            ItemData data = entry.data;

            if (!string.IsNullOrEmpty(data.image))
            {
                data.sprite = Resources.Load<Sprite>(data.image);

                if (data.sprite == null)
                    Debug.LogWarning($"InventoryDatabase: No sprite found for image path '{data.image}'");
            }

            _items[entry.id] = data;
        }

        Debug.Log($"InventoryDatabase: Loaded {_items.Count} items.");
    }
    public static ItemData GetItem(string id)
    {
        if (_items == null)
            Load();

        _items.TryGetValue(id, out ItemData data);
        return data;
    }
}
using System.Collections.Generic;
using UnityEngine;

public class InventoryState : MonoBehaviour
{
    public static InventoryState Instance;

    private HashSet<string> receivedItems = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ReceiveItem(string id)
    {
        if (!string.IsNullOrEmpty(id))
            receivedItems.Add(id);
    }

    public void RemoveItem(string id)
    {
        if (receivedItems.Contains(id))
            receivedItems.Remove(id);
    }

    public IEnumerable<string> GetReceivedItems()
    {
        return receivedItems;
    }
}
using UnityEngine;

public class Inventory : MonoBehaviour
{
    void Start()
    {
        string[] allItems = new string[]
        {
            "frog", "glycerin"
        };

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
    }

}

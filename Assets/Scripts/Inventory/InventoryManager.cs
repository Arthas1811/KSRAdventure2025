using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Transform inventoryPanel;
    public GameObject inventoryItemPrefab;

    void Start()
    {
        InventoryDatabase.Load();
        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
        foreach (Transform child in inventoryPanel)
            Destroy(child.gameObject);

        foreach (var itemId in InventoryState.Instance.GetReceivedItems())
        {
            ItemData data = InventoryDatabase.GetItem(itemId);
            if (data == null) continue;

            GameObject item = Instantiate(inventoryItemPrefab, inventoryPanel);
            item.GetComponent<InventoryItemUI>().Setup(data);
        }
    }
}

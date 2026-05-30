using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public Transform inventoryPanel;
    public GameObject inventoryItemPrefab;

    void Awake()
    {
        // Mirror the singleton guard used by Inventory/InventoryState. When "main"
        // is re-entered (returning from a minigame, or after a save-string import —
        // both call LoadScene("main")), a freshly-loaded duplicate must not clobber
        // Instance with an object that is about to be destroyed.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InventoryDatabase.Load();
        if (Inventory.Instance != null)
            Inventory.Instance.LoadFromSave();
        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
        if (inventoryPanel == null)
        {
            Debug.LogWarning("InventoryManager: inventoryPanel is null, can't update UI.");
            return;
        }

        // Destroy old item icons
        foreach (Transform child in inventoryPanel)
        {
            child.gameObject.SetActive(false); // remove from layout immediately
            Destroy(child.gameObject);
        }

        if (InventoryState.Instance == null) return;

        // Rebuild from InventoryState
        int count = 0;
        foreach (var itemId in InventoryState.Instance.GetReceivedItems())
        {
            ItemData data = InventoryDatabase.GetItem(itemId);
            if (data == null)
            {
                Debug.LogWarning($"InventoryManager: item '{itemId}' not found in database, skipping.");
                continue;
            }

            GameObject item = Instantiate(inventoryItemPrefab, inventoryPanel);
            item.transform.localScale = Vector3.one;
            item.GetComponent<InventoryItemUI>().Setup(data);
            count++;
        }

        // GridLayoutGroup (and layout groups in general) do NOT arrange children
        // that were added while the panel was inactive. That's the only reason a
        // full scene reload used to be needed: it rebuilt the UI while everything
        // was freshly active. Instead of reloading, just force the layout to run
        // once the panel is active — this positions the freshly-spawned items.
        if (inventoryPanel is RectTransform panelRect && inventoryPanel.gameObject.activeInHierarchy)
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        Debug.Log($"InventoryManager: rebuilt UI — {count} item(s) shown.");
    }
}

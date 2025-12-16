using UnityEngine;

public class TestInventory : MonoBehaviour
{
    void Start()
    {
        string[] allItems = new string[]
        {
            "frog", "glycerin", "coffeemug", "goldkey", "key2", "keys",
            "nitric_acid", "nitro_beaker", "nitrogly_beaker", "pipette",
            "redbull", "sulfuric_acid", "surfacecharger"
        };

        foreach (var id in allItems)
        {
            InventoryState.Instance.ReceiveItem(id);
        }

        Object.FindAnyObjectByType<InventoryManager>().UpdateInventoryUI();
    }
}

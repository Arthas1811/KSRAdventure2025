using UnityEngine;

public class InventoryDatabaseLoader : MonoBehaviour
{
    public TextAsset itemsJson;

    private void Awake()
    {
        InventoryDatabase.jsonFile = itemsJson;
    }
}

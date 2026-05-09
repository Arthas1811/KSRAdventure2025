using UnityEngine;
using UnityEngine.UI;

public class TwoSlotInventory : MonoBehaviour
{
    public Image slot1;
    public Image slot2;

    private bool slot1Filled = false;
    private bool slot2Filled = false;

    // Call this when clicking an ingredient
    public void AddIngredient(Sprite ingredientSprite)
    {
        if (!slot1Filled)
        {
            slot1.sprite = ingredientSprite;
            slot1.enabled = true;
            slot1Filled = true;
        }
        else if (!slot2Filled)
        {
            slot2.sprite = ingredientSprite;
            slot2.enabled = true;
            slot2Filled = true;
        }
        else
        {
            Debug.Log("Both slots are full!");
        }
    }
}
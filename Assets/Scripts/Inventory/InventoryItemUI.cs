using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemImage;
    public TMP_Text itemName;
    [TextArea] public string description;

    public void Setup(ItemData data)
    {
        //itemName.text = data.name;
        name = data.name;
        description = data.description;
        itemImage.sprite = data.sprite;
    }

    // show hover window
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HoverWindow.Instance != null)
        {
            HoverWindow.Instance.Show(name, description);
        }
    }

    // hide hover window
    public void OnPointerExit(PointerEventData eventData)
    {
        if (HoverWindow.Instance != null)
        {
            HoverWindow.Instance.Hide();
        }
    }
}

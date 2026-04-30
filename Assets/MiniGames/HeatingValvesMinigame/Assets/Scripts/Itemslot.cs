using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    public string requiredTag;
    public bool locked = false;
    public string partID;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            if (eventData.pointerDrag.CompareTag(requiredTag))
            {
                RectTransform itemRT = eventData.pointerDrag.GetComponent<RectTransform>();
                itemRT.position = GetComponent<RectTransform>().position;

                Debug.Log("Snappt auf Position");

                locked = true;

                DragDrop itemScript = eventData.pointerDrag.GetComponent<DragDrop>();
                if (itemScript != null)
                {
                    itemScript.isLocked = true;                   
                }

                InstructionsManager.Instance.PartCompleted(partID);

                RectTransform slotRT = GetComponent<RectTransform>();
       
                itemRT.position = slotRT.position;
                itemRT.SetAsLastSibling();

                Debug.Log("Dropped");
            }
        }
    }
}


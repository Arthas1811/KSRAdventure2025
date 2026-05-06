using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    public ItemSlot fest;
    
    private Vector2 startPosition;
    public bool isLocked = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }
    public void OnPointerDown(PointerEventData eventData) { }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked)
        {
            return;
        }
        startPosition = rectTransform.anchoredPosition;
        
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked)
        {
            return;
        }
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (!isLocked)
        {
            rectTransform.anchoredPosition = startPosition;
        }
    }   
    public void ResetPosition()
    {
        rectTransform.anchoredPosition = startPosition;
    }
    public string displayText;
}
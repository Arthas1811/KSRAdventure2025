using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//handles dragged block shape
public class DraggableBlockShape : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private const float AvailableAlpha = 1f;
    private const float UnavailableAlpha = 0.38f;
    private const float MaximumTrayScale = 0.72f;
    private static readonly Color UnavailableTint = new Color(0.52f, 0.55f, 0.62f, 1f);

    private BlockBlastGame game;
    private RectTransform rectTransform;
    private RectTransform homeSlot;
    private CanvasGroup canvasGroup;
    private Image[] blockImages;
    private Color[] blockBaseColors;
    private Vector2 pointerOffset;

    public BlockBlastShape Shape { get; private set; }

    public RectTransform ShapeRect => rectTransform;

    //sets start data
    public void Initialize(BlockBlastGame game, BlockBlastShape shape, RectTransform homeSlot)
    {
        this.game = game;
        Shape = shape;
        this.homeSlot = homeSlot;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        CacheVisibleBlockImages();
        ReturnToSlot();
    }

    //sets shape availability
    public void SetAvailability(bool canBePlaced)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = canBePlaced ? AvailableAlpha : UnavailableAlpha;
        }

        ApplyAvailabilityTint(canBePlaced);
    }

    //starts drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (game.IsGameOver)
        {
            return;
        }

        transform.SetParent(game.DragLayer, true);
        transform.SetAsLastSibling();
        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition = game.ClampDragPosition(rectTransform, rectTransform.anchoredPosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(game.DragLayer, eventData.position, eventData.pressEventCamera, out Vector2 pointerLocalPosition);
        pointerOffset = rectTransform.anchoredPosition - pointerLocalPosition;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }

    //moves drag
    public void OnDrag(PointerEventData eventData)
    {
        if (game.IsGameOver)
        {
            return;
        }

        MoveToPointer(eventData);
        game.UpdatePlacementPreview(this);
    }

    //ends drag
    public void OnEndDrag(PointerEventData eventData)
    {
        if (game.IsGameOver)
        {
            ReturnToSlot();
            return;
        }

        MoveToPointer(eventData);
        game.ClearPlacementPreview();

        if (!game.TryPlaceDraggedShape(this))
        {
            ReturnToSlot();
        }
    }

    //returns shape to slot
    public void ReturnToSlot()
    {
        if (homeSlot == null || rectTransform == null)
        {
            return;
        }

        transform.SetParent(homeSlot, false);
        transform.SetAsLastSibling();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one * GetSlotFitScale();

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }

    //gets slot scale
    private float GetSlotFitScale()
    {
        if (homeSlot == null || rectTransform == null || rectTransform.rect.width <= 0f || rectTransform.rect.height <= 0f)
        {
            return 1f;
        }

        const float padding = 12f;
        float widthScale = Mathf.Max(0.1f, (homeSlot.rect.width - padding) / rectTransform.rect.width);
        float heightScale = Mathf.Max(0.1f, (homeSlot.rect.height - padding) / rectTransform.rect.height);
        return Mathf.Clamp(Mathf.Min(widthScale, heightScale, MaximumTrayScale), 0.1f, MaximumTrayScale);
    }

    //stores child block images
    private void CacheVisibleBlockImages()
    {
        List<Image> visibleImages = new List<Image>();
        foreach (Image candidate in GetComponentsInChildren<Image>(true))
        {
            if (candidate.transform != transform)
            {
                visibleImages.Add(candidate);
            }
        }

        blockImages = visibleImages.ToArray();
        blockBaseColors = new Color[blockImages.Length];
        for (int i = 0; i < blockImages.Length; i++)
        {
            blockBaseColors[i] = blockImages[i].color;
        }
    }

    //sets shape tint
    private void ApplyAvailabilityTint(bool canBePlaced)
    {
        if (blockImages == null || blockBaseColors == null)
        {
            return;
        }

        for (int i = 0; i < blockImages.Length; i++)
        {
            if (blockImages[i] == null)
            {
                continue;
            }

            blockImages[i].color = canBePlaced ? blockBaseColors[i] : Color.Lerp(blockBaseColors[i], UnavailableTint, 0.62f);
        }
    }

    //moves shape to pointer
    private void MoveToPointer(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(game.DragLayer, eventData.position, eventData.pressEventCamera, out Vector2 pointerLocalPosition);
        rectTransform.anchoredPosition = game.ClampDragPosition(rectTransform, pointerLocalPosition + pointerOffset);
    }
}

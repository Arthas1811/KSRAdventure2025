using UnityEngine;
using UnityEngine.UI;

//shows block blast cell
public class BlockBlastCell : MonoBehaviour
{
    private Image image;
    private Color emptyColor;
    private Color occupiedColor;
    private Sprite occupiedSprite;
    private bool occupied;

    //sets start data
    public void Initialize(Image targetImage, Color emptyCellColor)
    {
        image = targetImage;
        emptyColor = emptyCellColor;
        SetState(false, Color.clear, null);
    }

    //sets cell state
    public void SetState(bool isOccupied, Color color)
    {
        SetState(isOccupied, color, null);
    }

    //sets cell state
    public void SetState(bool isOccupied, Color color, Sprite sprite)
    {
        occupied = isOccupied;
        occupiedColor = color;
        occupiedSprite = sprite;

        if (image != null)
        {
            ApplyStoredVisual();
        }
    }

    //sets cell preview
    public void SetPreview(Color previewColor)
    {
        SetPreview(previewColor, null);
    }

    //sets cell preview
    public void SetPreview(Color previewColor, Sprite previewSprite)
    {
        if (image != null)
        {
            HandyTextureProvider.ApplyTintedSprite(image, previewSprite, previewColor, true);
        }
    }

    //clears cell preview
    public void ClearPreview()
    {
        if (image != null)
        {
            ApplyStoredVisual();
        }
    }

    //sets stored cell look
    private void ApplyStoredVisual()
    {
        if (occupied)
        {
            Color color = occupiedSprite != null ? Color.white : occupiedColor;
            HandyTextureProvider.ApplyTintedSprite(image, occupiedSprite, color, true);
            return;
        }

        HandyTextureProvider.ApplyTintedSprite(image, null, emptyColor, false);
    }
}

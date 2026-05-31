using UnityEngine;
using UnityEngine.UI;

//loads handy textures
public static class HandyTextureProvider
{
    private const string CatalogResourcePath = "HandyTextureCatalog";

    private static HandyTextureCatalog cachedCatalog;
    private static bool loggedMissingCatalog;
    private static Sprite roundedAppIconMask;
    private static Sprite roundedPhoneScreenMask;
    private static Sprite roundedPanelMask;
    private static Sprite circleMask;

    public static HandyTextureCatalog Catalog
    {
        get
        {
            if (cachedCatalog == null)
            {
                cachedCatalog = Resources.Load<HandyTextureCatalog>(CatalogResourcePath);
                if (cachedCatalog == null && !loggedMissingCatalog)
                {
                    Debug.LogWarning($"HandyTextureProvider: Could not load Resources/{CatalogResourcePath}.");
                    loggedMissingCatalog = true;
                }
            }

            return cachedCatalog;
        }
    }

    public static Sprite Background => Catalog != null ? Catalog.background : null;
    public static Sprite PhoneFrame => Catalog != null ? Catalog.phoneFrame : null;
    public static Sprite PhoneWallpaper => Catalog != null ? Catalog.phoneWallpaper : null;
    public static Sprite CrackedDisplay => Catalog != null ? Catalog.crackedDisplay : null;
    public static Sprite HomeIcon => Catalog != null ? Catalog.homeIcon : null;
    public static Sprite MailboxIcon => Catalog != null ? Catalog.mailboxIcon : null;
    public static Sprite BlockBlastIcon => Catalog != null ? Catalog.blockBlastIcon : null;
    public static Sprite TetrosIcon => Catalog != null ? Catalog.tetrosIcon : null;
    public static Sprite RoundedAppIconMask
    {
        get
        {
            if (roundedAppIconMask == null)
            {
                roundedAppIconMask = CreateRoundedRectangleSprite(96, 96, 22f, "RoundedAppIconMask");
            }

            return roundedAppIconMask;
        }
    }

    public static Sprite RoundedPhoneScreenMask
    {
        get
        {
            if (roundedPhoneScreenMask == null)
            {
                roundedPhoneScreenMask = CreateRoundedRectangleSprite(128, 128, 58f, "RoundedPhoneScreenMask");
            }

            return roundedPhoneScreenMask;
        }
    }

    public static Sprite RoundedPanelMask
    {
        get
        {
            if (roundedPanelMask == null)
            {
                roundedPanelMask = CreateRoundedRectangleSprite(32, 32, 7f, "RoundedPanelMask");
            }

            return roundedPanelMask;
        }
    }

    public static Sprite CircleMask
    {
        get
        {
            if (circleMask == null)
            {
                circleMask = CreateCircleSprite(64, "CircleMask");
            }

            return circleMask;
        }
    }

    //gets block blast sprite
    public static Sprite GetBlockBlastSprite(HandyBlockTextureKey textureKey)
    {
        return Catalog != null ? Catalog.GetBlockBlastSprite(textureKey) : null;
    }

    //gets tetris sprite
    public static Sprite GetTetrisSprite(TetrisTetromino tetromino)
    {
        return Catalog != null ? Catalog.GetTetrisSprite(tetromino) : null;
    }

    //sets sprite image
    public static void ApplySprite(Image image, Sprite sprite, Color fallbackColor, bool preserveAspect)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = sprite != null && preserveAspect;
        image.color = sprite != null ? Color.white : fallbackColor;
    }

    //sets tinted sprite
    public static void ApplyTintedSprite(Image image, Sprite sprite, Color color, bool preserveAspect)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = sprite != null && preserveAspect;
        image.color = color;
    }

    //sets mask sprite
    public static void ApplyMaskSprite(Image image, Sprite maskSprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = maskSprite;
        image.type = Image.Type.Sliced;
        image.preserveAspect = false;
        image.color = Color.white;
        image.raycastTarget = false;
    }

    //creates rounded sprite
    private static Sprite CreateRoundedRectangleSprite(int width, int height, float radius, string spriteName)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = spriteName + "Texture",
            hideFlags = HideFlags.DontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float alpha = GetRoundedRectangleAlpha(x, y, width, height, radius);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        sprite.name = spriteName;
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }

    //creates circle sprite
    private static Sprite CreateCircleSprite(int size, string spriteName)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = spriteName + "Texture",
            hideFlags = HideFlags.DontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        float radius = size * 0.5f - 1f;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pixelCenter = new Vector2(x + 0.5f, y + 0.5f);
                float signedDistance = Vector2.Distance(pixelCenter, center) - radius;
                float alpha = Mathf.Clamp01(0.5f - signedDistance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f);
        sprite.name = spriteName;
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }

    //gets rounded alpha
    private static float GetRoundedRectangleAlpha(int x, int y, int width, int height, float radius)
    {
        Vector2 pixelCenter = new Vector2(x + 0.5f, y + 0.5f);
        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
        Vector2 halfSizeWithoutCorners = new Vector2(width * 0.5f - radius, height * 0.5f - radius);
        Vector2 distanceFromCenter = new Vector2(
            Mathf.Abs(pixelCenter.x - center.x),
            Mathf.Abs(pixelCenter.y - center.y));
        Vector2 cornerDistance = distanceFromCenter - halfSizeWithoutCorners;

        float outsideX = Mathf.Max(cornerDistance.x, 0f);
        float outsideY = Mathf.Max(cornerDistance.y, 0f);
        float outsideDistance = Mathf.Sqrt(outsideX * outsideX + outsideY * outsideY);
        float insideDistance = Mathf.Min(Mathf.Max(cornerDistance.x, cornerDistance.y), 0f);
        float signedDistance = outsideDistance + insideDistance - radius;

        return Mathf.Clamp01(0.5f - signedDistance);
    }
}

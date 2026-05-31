using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InventoryLauncherButton : MonoBehaviour
{
    private const string ButtonName = "GeneratedInventoryLauncherButton";
    private const string IconResourcePath = "Images/Inventory/InventoryButtonIcon";
    private const int ButtonSiblingIndex = 1;

    private static readonly Vector2 ButtonPosition = new Vector2(240f, 185f);
    private static readonly Vector2 ButtonSize = new Vector2(50f, 50f);

    [SerializeField] private InventoryAnimatedToggle inventoryToggle = null;

    private Sprite inventoryIconSprite;

    private void Start()
    {
        RectTransform root = transform as RectTransform;
        if (root == null)
        {
            Debug.LogWarning("InventoryLauncherButton must be attached to a UI RectTransform.");
            enabled = false;
            return;
        }

        if (inventoryToggle == null)
        {
            inventoryToggle = FindFirstObjectByType<InventoryAnimatedToggle>();
        }

        BuildButton(root);
    }

    private void BuildButton(RectTransform root)
    {
        Transform existingButton = root.Find(ButtonName);
        if (existingButton != null)
        {
            Destroy(existingButton.gameObject);
        }

        RectTransform buttonRect = HandyUIFactory.CreatePanel(
            ButtonName,
            root,
            Color.white,
            ButtonPosition,
            ButtonSize);

        Image image = buttonRect.GetComponent<Image>();
        Sprite iconSprite = GetInventoryIconSprite();
        if (iconSprite != null)
        {
            HandyTextureProvider.ApplySprite(image, iconSprite, Color.white, false);
        }
        else
        {
            image.color = Color.clear;
        }

        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = CreateButtonColors(iconSprite != null ? Color.white : Color.clear);
        button.onClick.AddListener(ToggleInventory);

        AddFallbackText(buttonRect, iconSprite == null);
        MenuLayerManager.ConfigureLauncherButton(buttonRect, GetHotkeyLabel());
        buttonRect.SetSiblingIndex(Mathf.Min(ButtonSiblingIndex, root.childCount - 1));
    }

    private void AddFallbackText(RectTransform parent, bool visible)
    {
        TextMeshProUGUI text = HandyUIFactory.CreateText(
            "FallbackText",
            parent,
            "I",
            Vector2.zero,
            ButtonSize,
            25f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            Color.white,
            true,
            13f);
        text.raycastTarget = false;
        text.gameObject.SetActive(visible);
    }

    private Sprite GetInventoryIconSprite()
    {
        if (inventoryIconSprite != null)
        {
            return inventoryIconSprite;
        }

        Texture2D iconTexture = Resources.Load<Texture2D>(IconResourcePath);
        if (iconTexture == null)
        {
            Debug.LogWarning($"InventoryLauncherButton: Could not load Resources/{IconResourcePath}.png.");
            return null;
        }

        inventoryIconSprite = Sprite.Create(
            iconTexture,
            new Rect(0f, 0f, iconTexture.width, iconTexture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        inventoryIconSprite.name = "GeneratedInventoryButtonIconSprite";
        inventoryIconSprite.hideFlags = HideFlags.DontSave;
        return inventoryIconSprite;
    }

    private void ToggleInventory()
    {
        if (inventoryToggle == null)
        {
            inventoryToggle = FindFirstObjectByType<InventoryAnimatedToggle>();
        }

        if (inventoryToggle == null)
        {
            Debug.LogWarning("InventoryLauncherButton: Could not find InventoryAnimatedToggle.");
            return;
        }

        inventoryToggle.ToggleInventory();
    }

    private string GetHotkeyLabel()
    {
        if (inventoryToggle == null || inventoryToggle.toggleKey == UnityEngine.InputSystem.Key.None)
        {
            return string.Empty;
        }

        return inventoryToggle.toggleKey.ToString();
    }

    private static ColorBlock CreateButtonColors(Color normalColor)
    {
        return HandyUIFactory.CreateButtonColors(
            normalColor,
            Color.Lerp(normalColor, Color.white, 0.14f),
            Color.Lerp(normalColor, Color.black, 0.18f),
            Color.Lerp(normalColor, Color.white, 0.08f),
            new Color(0.18f, 0.18f, 0.18f, 0.55f));
    }
}

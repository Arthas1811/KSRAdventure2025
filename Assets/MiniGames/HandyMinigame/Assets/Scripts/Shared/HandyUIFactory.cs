using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

//creates UI parts
public static class HandyUIFactory
{
    public const int UiLayer = 5;

    public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

    private static readonly Color MainSceneOverlayBackdropColor = new Color(0f, 0f, 0f, 0.42f);

    //creates overlay canvas
    public static RectTransform CreateOverlayCanvas(Transform parent, string objectName, int sortingOrder)
    {
        Transform existingRoot = parent.Find(objectName);
        if (existingRoot != null)
        {
            UnityEngine.Object.Destroy(existingRoot.gameObject);
        }

        GameObject canvasObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.layer = UiLayer;
        canvasObject.transform.SetParent(parent, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        StretchToParent(canvasRect);
        MenuLayerManager.BringToFront(canvasObject);
        return canvasRect;
    }

    public static void ConfigureBackdrop(Image image, Color fallbackColor)
    {
        if (PhoneSceneNavigation.IsPhoneOverlayOpen)
        {
            image.sprite = null;
            image.color = MainSceneOverlayBackdropColor;
            image.type = Image.Type.Simple;
            image.raycastTarget = true;
            return;
        }

        HandyTextureProvider.ApplySprite(image, HandyTextureProvider.Background, fallbackColor, false);
    }

    //creates UI object
    public static RectTransform CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.layer = UiLayer;
        uiObject.transform.SetParent(parent, false);
        return uiObject.GetComponent<RectTransform>();
    }

    //creates UI panel
    public static RectTransform CreatePanel(string objectName, Transform parent, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = CreateUIObject(objectName, parent);
        SetCentered(rect, anchoredPosition, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = true;
        return rect;
    }

    //creates stretch panel
    public static RectTransform CreateStretchPanel(string objectName, Transform parent, Color color)
    {
        RectTransform rect = CreatePanel(objectName, parent, color, Vector2.zero, Vector2.zero);
        StretchToParent(rect);
        return rect;
    }

    //creates rounded panel
    public static RectTransform CreateRoundedPanel(string objectName, Transform parent, Color color, Vector2 anchoredPosition, Vector2 size, Sprite sprite)
    {
        RectTransform rect = CreatePanel(objectName, parent, color, anchoredPosition, size);
        Image image = rect.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        return rect;
    }

    //creates circle panel
    public static RectTransform CreateCirclePanel(string objectName, Transform parent, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = CreatePanel(objectName, parent, color, anchoredPosition, size);
        Image image = rect.GetComponent<Image>();
        image.sprite = HandyTextureProvider.CircleMask;
        image.type = Image.Type.Simple;
        return rect;
    }

    //creates sliced panel
    public static RectTransform CreateSlicedPanel(string objectName, Transform parent, Color color, Vector2 anchoredPosition, Vector2 size, Sprite sprite)
    {
        RectTransform rect = CreatePanel(objectName, parent, color, anchoredPosition, size);
        Image image = rect.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.raycastTarget = color.a > 0f;
        return rect;
    }

    //creates accent band
    public static RectTransform CreateAccentBand(string objectName, Transform parent, Color color, Vector2 anchoredPosition, Vector2 size, float rotation)
    {
        RectTransform band = CreatePanel(objectName, parent, color, anchoredPosition, size);
        band.localEulerAngles = new Vector3(0f, 0f, rotation);
        return band;
    }

    //creates UI button
    public static RectTransform CreateButton(
        string objectName,
        Transform parent,
        string label,
        Vector2 anchoredPosition,
        Vector2 size,
        Color normalColor,
        UnityAction onClick,
        ColorBlock colors,
        Sprite iconSprite = null,
        float fontSize = 21f,
        Color? textColor = null,
        Vector2? textSize = null,
        float iconInset = 10f)
    {
        RectTransform rect = CreatePanel(objectName, parent, normalColor, anchoredPosition, size);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.transition = Selectable.Transition.ColorTint;
        button.colors = colors;
        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }

        if (iconSprite != null)
        {
            Vector2 iconSize = new Vector2(size.y - iconInset, size.y - iconInset);
            RectTransform icon = CreatePanel($"{objectName}Icon", rect, Color.white, Vector2.zero, iconSize);
            Image iconImage = icon.GetComponent<Image>();
            HandyTextureProvider.ApplySprite(iconImage, iconSprite, Color.white, true);
            iconImage.raycastTarget = false;
        }
        else if (!string.IsNullOrEmpty(label))
        {
            CreateText(
                $"{objectName}Text",
                rect,
                label,
                Vector2.zero,
                textSize ?? size,
                fontSize,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                textColor ?? Color.white);
        }

        return rect;
    }

    //creates clear button
    public static RectTransform CreateTransparentButton(
        string objectName,
        Transform parent,
        Vector2 anchoredPosition,
        Vector2 size,
        UnityAction onClick)
    {
        RectTransform rect = CreatePanel(objectName, parent, Color.clear, anchoredPosition, size);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(onClick);
        return rect;
    }

    //adds button action
    public static Button AddButton(RectTransform rect, ColorBlock colors, UnityAction onClick)
    {
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        button.transition = Selectable.Transition.ColorTint;
        button.colors = colors;
        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }

        return button;
    }

    //creates button colors
    public static ColorBlock CreateButtonColors(
        Color normalColor,
        Color highlightedColor,
        Color pressedColor,
        Color selectedColor,
        Color disabledColor,
        float fadeDuration = 0.1f)
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = normalColor;
        colors.highlightedColor = highlightedColor;
        colors.pressedColor = pressedColor;
        colors.selectedColor = selectedColor;
        colors.disabledColor = disabledColor;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = fadeDuration;
        return colors;
    }

    //creates UI text
    public static TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string text,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize,
        FontStyles fontStyle,
        TextAlignmentOptions alignment,
        Color? color = null,
        bool autoSize = true,
        float minFontSize = -1f,
        TextWrappingModes wrapping = TextWrappingModes.Normal,
        TextOverflowModes overflow = TextOverflowModes.Overflow)
    {
        RectTransform rect = CreateUIObject(objectName, parent);
        SetCentered(rect, anchoredPosition, size);

        TextMeshProUGUI textComponent = rect.gameObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.alignment = alignment;
        textComponent.color = color ?? Color.white;
        textComponent.textWrappingMode = wrapping;
        textComponent.overflowMode = overflow;
        textComponent.enableAutoSizing = autoSize;
        textComponent.fontSizeMin = minFontSize >= 0f ? minFontSize : Mathf.Max(10f, fontSize * 0.55f);
        textComponent.fontSizeMax = fontSize;
        textComponent.raycastTarget = false;
        return textComponent;
    }

    //sets rounded screen
    public static void ConfigureRoundedScreen(RectTransform screen, Color fallbackColor, Sprite backgroundSprite = null)
    {
        HandyTextureProvider.ApplyMaskSprite(screen.GetComponent<Image>(), HandyTextureProvider.RoundedPhoneScreenMask);
        Mask screenMask = screen.gameObject.AddComponent<Mask>();
        screenMask.showMaskGraphic = false;

        RectTransform background = CreateStretchPanel("PhoneScreenBackground", screen, fallbackColor);
        Image backgroundImage = background.GetComponent<Image>();
        HandyTextureProvider.ApplySprite(backgroundImage, backgroundSprite, fallbackColor, false);
        backgroundImage.raycastTarget = false;
        background.SetAsFirstSibling();
    }

    //creates phone frame overlay
    public static void CreatePhoneFrameOverlay(Transform parent, Color fallbackColor)
    {
        RectTransform overlay = CreatePanel("PhoneFrameOverlay", parent, Color.white, Vector2.zero, new Vector2(560f, 1040f));
        Image overlayImage = overlay.GetComponent<Image>();
        HandyTextureProvider.ApplySprite(overlayImage, HandyTextureProvider.PhoneFrame, fallbackColor, true);
        overlayImage.raycastTarget = false;
        overlay.SetAsLastSibling();
    }

    //stretches to parent
    public static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    //sets centered rect
    public static void SetCentered(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }
}

public static class MenuLayerManager
{
    private const int MenuSortingOrderBase = 1000;
    private const int LauncherSortingOrder = 5000;
    private const string HotkeyBadgeName = "HotkeyBadge";

    private static int nextMenuSortingOrder = MenuSortingOrderBase;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        nextMenuSortingOrder = MenuSortingOrderBase;
    }

    public static void BringToFront(GameObject menuRoot)
    {
        if (menuRoot == null)
        {
            return;
        }

        Canvas canvas = GetCanvasFor(menuRoot);
        canvas.overrideSorting = true;
        canvas.sortingOrder = ++nextMenuSortingOrder;
    }

    public static void ConfigureLauncherButton(RectTransform buttonRect, string hotkeyText)
    {
        if (buttonRect == null)
        {
            return;
        }

        Canvas canvas = buttonRect.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = buttonRect.gameObject.AddComponent<Canvas>();
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = LauncherSortingOrder;

        if (buttonRect.GetComponent<GraphicRaycaster>() == null)
        {
            buttonRect.gameObject.AddComponent<GraphicRaycaster>();
        }

        AddHotkeyBadge(buttonRect, hotkeyText);
    }

    public static bool IsTextInputFocused()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null || eventSystem.currentSelectedGameObject == null)
        {
            return false;
        }

        GameObject selected = eventSystem.currentSelectedGameObject;
        return selected.GetComponent<TMP_InputField>() != null
            || selected.GetComponent<InputField>() != null;
    }

    private static Canvas GetCanvasFor(GameObject menuRoot)
    {
        Canvas canvas = menuRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            return canvas;
        }

        canvas = menuRoot.GetComponentInParent<Canvas>(true);
        if (canvas != null)
        {
            return canvas;
        }

        canvas = menuRoot.AddComponent<Canvas>();
        if (menuRoot.GetComponent<GraphicRaycaster>() == null)
        {
            menuRoot.AddComponent<GraphicRaycaster>();
        }

        return canvas;
    }

    private static void AddHotkeyBadge(RectTransform parent, string hotkeyText)
    {
        Transform existingBadge = parent.Find(HotkeyBadgeName);
        if (existingBadge != null)
        {
            Object.Destroy(existingBadge.gameObject);
        }

        if (string.IsNullOrWhiteSpace(hotkeyText))
        {
            return;
        }

        GameObject badgeObject = new GameObject(HotkeyBadgeName, typeof(RectTransform), typeof(Image));
        badgeObject.layer = parent.gameObject.layer;
        badgeObject.transform.SetParent(parent, false);

        RectTransform badge = badgeObject.GetComponent<RectTransform>();
        badge.anchorMin = new Vector2(1f, 0f);
        badge.anchorMax = new Vector2(1f, 0f);
        badge.pivot = new Vector2(1f, 0f);
        badge.anchoredPosition = new Vector2(-2f, 2f);
        badge.sizeDelta = new Vector2(Mathf.Max(22f, hotkeyText.Length * 12f), 16f);

        Image badgeImage = badgeObject.GetComponent<Image>();
        badgeImage.color = new Color(0f, 0f, 0f, 0.68f);
        badgeImage.raycastTarget = false;

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.layer = parent.gameObject.layer;
        textObject.transform.SetParent(badge, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = hotkeyText;
        text.fontSize = 11f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HandyLauncherButton : MonoBehaviour
{
    private const string ButtonName = "GeneratedHandyLauncherButton";
    private const string NotificationDotName = "NotificationDot";
    private const int ButtonSiblingIndex = 1;

    private static readonly Vector2 ButtonPosition = new Vector2(300f, 185f);
    private static readonly Vector2 ButtonSize = new Vector2(50f, 50f);
    private static readonly Vector2 DotSize = new Vector2(14f, 14f);

    [SerializeField] private Texture2D mobileIconTexture = null;
    [SerializeField] private Key hotkey = PhoneSceneNavigation.PhoneToggleKey;

    private GameObject notificationDot;
    private Sprite mobileIconSprite;

    private void Start()
    {
        RectTransform root = transform as RectTransform;
        if (root == null)
        {
            Debug.LogWarning("HandyLauncherButton must be attached to a UI RectTransform.");
            enabled = false;
            return;
        }

        BuildButton(root);
        RefreshNotificationDot();
    }

    private void OnEnable()
    {
        if (notificationDot != null)
        {
            RefreshNotificationDot();
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || hotkey == Key.None || MenuLayerManager.IsTextInputFocused())
        {
            return;
        }

        if (keyboard[hotkey].wasPressedThisFrame)
        {
            ToggleHandy();
        }
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

        Image background = buttonRect.GetComponent<Image>();
        Sprite iconSprite = GetMobileIconSprite();
        if (iconSprite != null)
        {
            HandyTextureProvider.ApplySprite(background, iconSprite, Color.white, false);
        }
        else
        {
            background.color = Color.clear;
        }

        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = CreateButtonColors(iconSprite != null ? Color.white : Color.clear);
        button.onClick.AddListener(ToggleHandy);

        AddFallbackText(buttonRect, iconSprite == null);
        notificationDot = CreateNotificationDot(buttonRect).gameObject;
        MenuLayerManager.ConfigureLauncherButton(buttonRect, GetHotkeyLabel());

        buttonRect.SetSiblingIndex(Mathf.Min(ButtonSiblingIndex, root.childCount - 1));
    }

    private void AddFallbackText(RectTransform parent, bool visible)
    {
        TextMeshProUGUI text = HandyUIFactory.CreateText(
            "FallbackText",
            parent,
            "H",
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

    private Sprite GetMobileIconSprite()
    {
        if (mobileIconTexture == null)
        {
            return null;
        }

        if (mobileIconSprite != null)
        {
            return mobileIconSprite;
        }

        mobileIconSprite = Sprite.Create(
            mobileIconTexture,
            new Rect(0f, 0f, mobileIconTexture.width, mobileIconTexture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        mobileIconSprite.name = "GeneratedMobileIconSprite";
        mobileIconSprite.hideFlags = HideFlags.DontSave;
        return mobileIconSprite;
    }

    private RectTransform CreateNotificationDot(RectTransform parent)
    {
        RectTransform dot = HandyUIFactory.CreateCirclePanel(
            NotificationDotName,
            parent,
            new Color(0.94f, 0.08f, 0.08f, 1f),
            Vector2.zero,
            DotSize);

        dot.anchorMin = new Vector2(1f, 1f);
        dot.anchorMax = new Vector2(1f, 1f);
        dot.pivot = new Vector2(0.5f, 0.5f);
        dot.anchoredPosition = new Vector2(-3f, -3f);

        Image dotImage = dot.GetComponent<Image>();
        dotImage.raycastTarget = false;

        return dot;
    }

    private void RefreshNotificationDot()
    {
        SaveDataManager saveDataManager = HandySceneServices.EnsureSaveDataManager();
        PhoneSaveState phoneSaveState = new PhoneSaveState(saveDataManager);
        phoneSaveState.Load();

        MailboxMessageCatalog mailboxCatalog = MailboxMessageCatalog.LoadFromResources();
        phoneSaveState.EnsureMailStates(mailboxCatalog.MailIds, true, false);

        bool hasUnreadMail = phoneSaveState.HasUnreadMailFor(mailboxCatalog.MailIds);
        notificationDot.SetActive(hasUnreadMail && !phoneSaveState.PhoneDestroyed);
    }

    private void ToggleHandy()
    {
        PhoneSceneNavigation.TogglePhoneOverlay();
    }

    private string GetHotkeyLabel()
    {
        return hotkey == Key.None ? string.Empty : hotkey.ToString();
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

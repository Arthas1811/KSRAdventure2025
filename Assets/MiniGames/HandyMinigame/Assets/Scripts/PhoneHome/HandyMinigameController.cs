using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//runs phone home app
[DisallowMultipleComponent]
public class HandyMinigameController : MonoBehaviour
{
    private PhoneSaveState phoneSaveState;
    private MailboxMessageCatalog mailboxCatalog;

    private PhoneSceneNavigation navigation;

    private GameObject phoneHomePanel;
    private GameObject notificationDot;
    private GameObject destroyedPhonePanel;

    //starts game screen
    private void Start()
    {
        navigation = HandySceneServices.EnsureNavigation(this);

        phoneSaveState = new PhoneSaveState(HandySceneServices.EnsureSaveDataManager());
        phoneSaveState.Load();
        mailboxCatalog = MailboxMessageCatalog.LoadFromResources();
        phoneSaveState.EnsureMailStates(mailboxCatalog.MailIds, true, false);

        HandySceneServices.EnsureEventSystem();

        BuildInterface();

        ApplySaveState();
    }

    //builds UI
    private void BuildInterface()
    {
        RectTransform canvasRect = HandyUIFactory.CreateOverlayCanvas(transform, "GeneratedHandyStartScreen", 20);

        Color backdropFallbackColor = new Color(0.07f, 0.08f, 0.1f, 1f);
        RectTransform backdrop = CreatePanel("Backdrop", canvasRect, backdropFallbackColor, Vector2.zero, Vector2.zero);
        HandyUIFactory.ConfigureBackdrop(backdrop.GetComponent<Image>(), backdropFallbackColor);
        if (HandyTextureProvider.Background == null && !PhoneSceneNavigation.IsPhoneOverlayOpen)
        {
            CreateBackdropBands(canvasRect);
        }

        Color phoneFrameFallbackColor = new Color(0.015f, 0.018f, 0.025f, 1f);
        RectTransform phoneFrame = CreatePanel(
            "PhoneFrame",
            canvasRect,
            phoneFrameFallbackColor,
            Vector2.zero,
            new Vector2(560f, 1040f));
        Image phoneFrameImage = phoneFrame.GetComponent<Image>();
        if (HandyTextureProvider.PhoneFrame != null)
        {
            phoneFrameImage.color = Color.clear;
            phoneFrameImage.raycastTarget = false;
        }

        if (HandyTextureProvider.PhoneFrame == null)
        {
            CreatePanel(
                "PhoneInnerBezel",
                phoneFrame,
                new Color(0.06f, 0.065f, 0.08f, 1f),
                Vector2.zero,
                new Vector2(456f, 940f));
        }

        Color screenFallbackColor = new Color(0.11f, 0.14f, 0.18f, 1f);
        RectTransform screen = CreatePanel(
            "PhoneScreen",
            phoneFrame,
            Color.white,
            new Vector2(0f, -2f),
            new Vector2(512f, 1000f));
        HandyUIFactory.ConfigureRoundedScreen(screen, screenFallbackColor, HandyTextureProvider.PhoneWallpaper);

        CreateStatusBar(screen);
        if (HandyTextureProvider.PhoneFrame == null)
        {
            CreateSpeakerAndHomeBar(phoneFrame);
        }

        phoneHomePanel = CreatePhoneHomePanel(screen).gameObject;
        destroyedPhonePanel = CreateDestroyedPhonePanel(screen).gameObject;

        if (HandyTextureProvider.PhoneFrame != null)
        {
            HandyUIFactory.CreatePhoneFrameOverlay(phoneFrame, phoneFrameFallbackColor);
        }
    }

    //creates back bands
    private void CreateBackdropBands(RectTransform parent)
    {
        RectTransform leftBand = CreatePanel(
            "BackdropLeftBand",
            parent,
            new Color(0.02f, 0.16f, 0.18f, 0.72f),
            new Vector2(-620f, 0f),
            new Vector2(420f, 1080f));
        leftBand.localEulerAngles = new Vector3(0f, 0f, -8f);

        RectTransform rightBand = CreatePanel(
            "BackdropRightBand",
            parent,
            new Color(0.22f, 0.1f, 0.04f, 0.62f),
            new Vector2(640f, 0f),
            new Vector2(360f, 1080f));
        rightBand.localEulerAngles = new Vector3(0f, 0f, 7f);

        RectTransform lowerBand = CreatePanel(
            "BackdropLowerBand",
            parent,
            new Color(0.14f, 0.12f, 0.24f, 0.55f),
            new Vector2(0f, -505f),
            new Vector2(1920f, 150f));
        lowerBand.localEulerAngles = new Vector3(0f, 0f, 2f);
    }

    //creates status bar
    private void CreateStatusBar(RectTransform screen)
    {
        RectTransform statusBar = CreatePanel(
            "StatusBar",
            screen,
            new Color(0.07f, 0.09f, 0.12f, 1f),
            new Vector2(0f, 399f),
            new Vector2(392f, 52f));

        CreateText(
            "NetworkText",
            statusBar,
            "KSR",
            20f,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineLeft,
            new Color(0.74f, 0.82f, 0.88f, 1f),
            new Vector2(-124f, 0f),
            new Vector2(110f, 34f));

        CreateText(
            "TimeText",
            statusBar,
            "09:41",
            21f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            new Color(0.92f, 0.96f, 0.98f, 1f),
            Vector2.zero,
            new Vector2(120f, 34f));

        CreateText(
            "BatteryText",
            statusBar,
            "100%",
            18f,
            FontStyles.Bold,
            TextAlignmentOptions.MidlineRight,
            new Color(0.74f, 0.82f, 0.88f, 1f),
            new Vector2(124f, 0f),
            new Vector2(110f, 34f));
    }

    //creates speaker and home bar
    private void CreateSpeakerAndHomeBar(RectTransform phoneFrame)
    {
        CreatePanel(
            "Speaker",
            phoneFrame,
            new Color(0.18f, 0.19f, 0.22f, 1f),
            new Vector2(0f, 432f),
            new Vector2(98f, 12f));

        CreatePanel(
            "HomeIndicator",
            phoneFrame,
            new Color(0.36f, 0.37f, 0.4f, 1f),
            new Vector2(0f, -430f),
            new Vector2(116f, 8f));
    }

    //creates phone home panel
    private RectTransform CreatePhoneHomePanel(RectTransform screen)
    {
        RectTransform homePanel = HandyUIFactory.CreateUIObject("PhoneHomePanel", screen);
        HandyUIFactory.StretchToParent(homePanel);

        CreateText(
            "Title",
            homePanel,
            "Handy",
            48f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            new Color(0.96f, 0.98f, 1f, 1f),
            new Vector2(0f, 326f),
            new Vector2(420f, 70f));

        CreateText(
            "Subtitle",
            homePanel,
            "Choose an app",
            22f,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            new Color(0.68f, 0.76f, 0.82f, 1f),
            new Vector2(0f, 274f),
            new Vector2(420f, 42f));

        RectTransform mailButton = CreateAppButton(
            "MailAppButton",
            homePanel,
            "MAIL",
            "Mailbox",
            new Color(0.08f, 0.43f, 0.76f, 1f),
            new Vector2(-118f, 82f),
            HandyTextureProvider.MailboxIcon,
            navigation.LoadMailbox);

        notificationDot = CreateNotificationDot(mailButton).gameObject;

        CreateAppButton(
            "BlockBlastAppButton",
            homePanel,
            "BB",
            "Block Blast",
            new Color(0.85f, 0.38f, 0.13f, 1f),
            new Vector2(118f, 82f),
            HandyTextureProvider.BlockBlastIcon,
            navigation.LoadBlockBlast);

        CreateAppButton(
            "TetrisAppButton",
            homePanel,
            "T",
            "Tetris",
            new Color(0.2f, 0.57f, 0.32f, 1f),
            new Vector2(0f, -132f),
            HandyTextureProvider.TetrosIcon,
            navigation.LoadTetris);

        return homePanel;
    }

    //creates broken phone panel
    private RectTransform CreateDestroyedPhonePanel(RectTransform screen)
    {
        Color destroyedFallbackColor = new Color(0.045f, 0.048f, 0.055f, 0.98f);
        RectTransform panel = CreatePanel(
            "DestroyedPhonePanel",
            screen,
            destroyedFallbackColor,
            Vector2.zero,
            Vector2.zero);
        HandyTextureProvider.ApplySprite(panel.GetComponent<Image>(), HandyTextureProvider.CrackedDisplay, destroyedFallbackColor, false);

        if (HandyTextureProvider.CrackedDisplay == null)
        {
            CreatePanel(
                "BrokenScreenGlow",
                panel,
                new Color(0.17f, 0.03f, 0.04f, 0.7f),
                new Vector2(0f, 38f),
                new Vector2(410f, 520f));

            CreateCrack(panel, "CrackMain", new Vector2(-15f, 62f), new Vector2(330f, 5f), -38f);
            CreateCrack(panel, "CrackBranchA", new Vector2(-83f, 180f), new Vector2(154f, 4f), 26f);
            CreateCrack(panel, "CrackBranchB", new Vector2(72f, -50f), new Vector2(184f, 4f), 48f);
            CreateCrack(panel, "CrackBranchC", new Vector2(28f, 92f), new Vector2(132f, 4f), -8f);
        }

        return panel;
    }

    //creates notice dot
    private RectTransform CreateNotificationDot(RectTransform parent)
    {
        RectTransform dot = HandyUIFactory.CreateCirclePanel(
            "NotificationDot",
            parent,
            new Color(0.94f, 0.08f, 0.08f, 1f),
            new Vector2(58f, 56f),
            new Vector2(28f, 28f));

        CreateText(
            "NotificationDotText",
            dot,
            "!",
            20f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            Color.white,
            Vector2.zero,
            new Vector2(28f, 28f));

        return dot;
    }

    //creates app button
    private RectTransform CreateAppButton(
        string objectName,
        RectTransform parent,
        string iconText,
        string labelText,
        Color accentColor,
        Vector2 anchoredPosition,
        Sprite iconSprite,
        UnityEngine.Events.UnityAction onClick)
    {
        RectTransform buttonRect = HandyUIFactory.CreateTransparentButton(
            objectName,
            parent,
            anchoredPosition,
            new Vector2(168f, 168f),
            onClick);

        RectTransform icon = HandyUIFactory.CreateRoundedPanel(
            "Icon",
            buttonRect,
            iconSprite != null ? Color.white : accentColor,
            new Vector2(0f, 22f),
            new Vector2(104f, 104f),
            HandyTextureProvider.RoundedAppIconMask);

        if (iconSprite != null)
        {
            HandyTextureProvider.ApplyMaskSprite(icon.GetComponent<Image>(), HandyTextureProvider.RoundedAppIconMask);
            Mask iconMask = icon.gameObject.AddComponent<Mask>();
            iconMask.showMaskGraphic = false;

            RectTransform artwork = CreatePanel("IconArtwork", icon, Color.white, Vector2.zero, Vector2.zero);
            HandyUIFactory.StretchToParent(artwork);

            Image artworkImage = artwork.GetComponent<Image>();
            HandyTextureProvider.ApplySprite(artworkImage, iconSprite, accentColor, true);
            artworkImage.raycastTarget = false;
        }
        else
        {
            CreateText(
                "IconText",
                icon,
                iconText,
                29f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                Color.white,
                Vector2.zero,
                new Vector2(86f, 70f));
        }

        CreateText(
            "Label",
            buttonRect,
            labelText,
            18f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            new Color(0.92f, 0.96f, 0.98f, 1f),
            new Vector2(0f, -58f),
            new Vector2(150f, 32f));

        return buttonRect;
    }

    //creates UI button
    private RectTransform CreateButton(
        string objectName,
        RectTransform parent,
        string text,
        float fontSize,
        Color normalColor,
        Color textColor,
        Vector2 anchoredPosition,
        Vector2 size,
        UnityEngine.Events.UnityAction onClick)
    {
        RectTransform rect = CreatePanel(objectName, parent, normalColor, anchoredPosition, size);

        HandyUIFactory.AddButton(rect, CreateButtonColors(normalColor), onClick);

        if (!string.IsNullOrEmpty(text))
        {
            CreateText(
                "Text",
                rect,
                text,
                fontSize,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                textColor,
                Vector2.zero,
                new Vector2(size.x - 34f, size.y - 14f));
        }

        return rect;
    }

    //creates button colors
    private ColorBlock CreateButtonColors(Color normalColor)
    {
        return HandyUIFactory.CreateButtonColors(
            normalColor,
            Color.Lerp(normalColor, Color.white, 0.12f),
            Color.Lerp(normalColor, Color.black, 0.18f),
            Color.Lerp(normalColor, Color.white, 0.08f),
            new Color(0.18f, 0.18f, 0.18f, 0.55f));
    }

    //creates crack mark
    private void CreateCrack(
        RectTransform parent,
        string objectName,
        Vector2 anchoredPosition,
        Vector2 size,
        float rotation)
    {
        RectTransform crack = CreatePanel(
            objectName,
            parent,
            new Color(0.92f, 0.92f, 0.95f, 0.7f),
            anchoredPosition,
            size);
        crack.localEulerAngles = new Vector3(0f, 0f, rotation);
    }

    //creates UI panel
    private RectTransform CreatePanel(
        string objectName,
        RectTransform parent,
        Color color,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        RectTransform rect = HandyUIFactory.CreatePanel(objectName, parent, color, anchoredPosition, size);
        if (size == Vector2.zero)
        {
            HandyUIFactory.StretchToParent(rect);
        }

        return rect;
    }

    //creates UI text
    private TextMeshProUGUI CreateText(
        string objectName,
        RectTransform parent,
        string text,
        float fontSize,
        FontStyles fontStyle,
        TextAlignmentOptions alignment,
        Color color,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        return HandyUIFactory.CreateText(objectName, parent, text, anchoredPosition, size, fontSize, fontStyle, alignment, color, true, Mathf.Max(10f, fontSize * 0.55f));
    }

    //sets saved phone state
    private void ApplySaveState()
    {
        bool phoneDestroyed = phoneSaveState.PhoneDestroyed;
        phoneHomePanel.SetActive(!phoneDestroyed);
        destroyedPhonePanel.SetActive(phoneDestroyed);

        IReadOnlyList<string> configuredMailIds = mailboxCatalog?.MailIds;
        bool hasUnreadMail = phoneSaveState.HasUnreadMailFor(configuredMailIds);

        notificationDot.SetActive(hasUnreadMail && !phoneDestroyed);
    }

}

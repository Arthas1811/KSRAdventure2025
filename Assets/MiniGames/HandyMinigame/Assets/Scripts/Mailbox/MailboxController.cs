using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//runs mailbox app
[DisallowMultipleComponent]
public class MailboxController : MonoBehaviour
{
    private static readonly Color BackdropColor = new Color(0.075f, 0.085f, 0.1f, 1f);
    private static readonly Color FrameColor = new Color(0.022f, 0.026f, 0.034f, 1f);
    private static readonly Color ShellColor = new Color(0.965f, 0.975f, 0.988f, 1f);
    private static readonly Color HeaderColor = new Color(0.035f, 0.24f, 0.46f, 1f);
    private static readonly Color RailColor = new Color(0.91f, 0.935f, 0.965f, 1f);
    private static readonly Color PaneColor = new Color(1f, 1f, 1f, 1f);
    private static readonly Color DividerColor = new Color(0.79f, 0.83f, 0.88f, 1f);
    private static readonly Color AccentColor = new Color(0.0f, 0.36f, 0.72f, 1f);
    private static readonly Color AccentHoverColor = new Color(0.02f, 0.44f, 0.84f, 1f);
    private static readonly Color UnreadCardColor = new Color(0.92f, 0.965f, 1f, 1f);
    private static readonly Color ReadCardColor = new Color(1f, 1f, 1f, 1f);
    private static readonly Color HoverCardColor = new Color(0.9f, 0.955f, 1f, 1f);
    private static readonly Color SelectedCardColor = new Color(0.84f, 0.92f, 1f, 1f);
    private static readonly Color PrimaryTextColor = new Color(0.09f, 0.12f, 0.17f, 1f);
    private static readonly Color MutedTextColor = new Color(0.36f, 0.42f, 0.5f, 1f);
    private static readonly Vector2 LandscapeFrameSize = new Vector2(1540f, 770f);
    private static readonly Vector2 LandscapeFrameSpriteSize = new Vector2(770f, 1540f);
    private static readonly Vector2 LandscapeScreenSize = new Vector2(1500f, 720f);
    private const float MailCardWidth = 430f;
    private const float MailCardHeight = 112f;
    private const float ReadingContentWidth = 660f;
    private const float ReadingImageMaxHeight = 300f;
    private const float SearchBoxCenterX = 20f;
    private const float SearchDropdownTopY = 278f;
    private const int MaxSearchDropdownResults = 5;
    private const float SearchResultRowHeight = 58f;
    private const string FolderInbox = "Inbox";
    private const string FolderUnread = "Unread";
    private const string FolderArchive = "Archive";
    private const string FolderSent = "Sent";
    private const string FolderPaperBin = "Paper Bin";

    private readonly Dictionary<string, MailCardView> mailCardViews = new Dictionary<string, MailCardView>();
    private readonly Dictionary<string, FolderRowView> folderRowViews = new Dictionary<string, FolderRowView>();

    private PhoneSaveState phoneSaveState;
    private PhoneSceneNavigation navigation;
    private MailboxMessageCatalog mailboxCatalog;
    private RectTransform mailListContent;
    private GameObject mailListScrollObject;
    private GameObject emptyInboxPanel;
    private TextMeshProUGUI inboxTitleText;
    private TextMeshProUGUI inboxSubtitleText;
    private TextMeshProUGUI emptyInboxTitleText;
    private TextMeshProUGUI emptyInboxMessageText;
    private TMP_InputField searchInputField;
    private GameObject searchDropdownObject;
    private RectTransform searchDropdownLayer;
    private RectTransform searchDropdownPanel;
    private TextMeshProUGUI searchIndicatorText;
    private GameObject readingEmptyPanel;
    private GameObject readingContentPanel;
    private ScrollRect readingScrollRect;
    private RectTransform readingScrollContent;
    private TextMeshProUGUI readingSubjectText;
    private TextMeshProUGUI readingSenderText;
    private TextMeshProUGUI readingAddressText;
    private TextMeshProUGUI readingTimeText;
    private TextMeshProUGUI readingBodyText;
    private LayoutElement readingBodyLayoutElement;
    private GameObject attachmentButtonObject;
    private TextMeshProUGUI attachmentButtonText;
    private Button attachmentButton;
    private GameObject readingImageFrame;
    private LayoutElement readingImageFrameLayoutElement;
    private Image readingImage;
    private RectTransform mailboxCanvasRect;
    private GameObject contextMenuLayerObject;
    private RectTransform contextMenuPanel;
    private string selectedMailId;
    private string selectedFolder = FolderInbox;
    private string searchQuery = string.Empty;

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
        RenderMailbox();
    }

    //builds UI
    private void BuildInterface()
    {
        RectTransform canvasRect = HandyUIFactory.CreateOverlayCanvas(transform, "GeneratedMailboxApp", 0);
        mailboxCanvasRect = canvasRect;

        RectTransform backdrop = CreatePanel("Backdrop", canvasRect, BackdropColor, Vector2.zero, Vector2.zero);
        HandyUIFactory.StretchToParent(backdrop);
        HandyUIFactory.ConfigureBackdrop(backdrop.GetComponent<Image>(), BackdropColor);
        if (HandyTextureProvider.Background == null && !PhoneSceneNavigation.IsPhoneOverlayOpen)
        {
            CreatePanel("BackdropBandLeft", canvasRect, new Color(0.0f, 0.22f, 0.29f, 0.55f), new Vector2(-680f, 0f), new Vector2(520f, 1180f));
            CreatePanel("BackdropBandRight", canvasRect, new Color(0.33f, 0.12f, 0.08f, 0.45f), new Vector2(700f, -20f), new Vector2(520f, 1180f));
        }

        RectTransform frame = CreatePanel("PhoneFrame", canvasRect, FrameColor, Vector2.zero, LandscapeFrameSize);
        Image frameImage = frame.GetComponent<Image>();
        if (HandyTextureProvider.PhoneFrame != null)
        {
            frameImage.color = Color.clear;
            frameImage.raycastTarget = false;
        }

        RectTransform phoneScreen = CreatePanel("PhoneScreen", frame, Color.white, Vector2.zero, LandscapeScreenSize);
        HandyUIFactory.ConfigureRoundedScreen(phoneScreen, ShellColor);
        RectTransform shell = CreatePanel("MailboxShell", phoneScreen, ShellColor, Vector2.zero, LandscapeScreenSize);

        CreateMailboxColumns(shell);
        CreateHeader(shell);
        CreateContextMenuLayer(canvasRect);
        CreateSearchDropdownLayer(canvasRect);
        CreateLandscapePhoneFrameOverlay(canvasRect);
    }

    //creates landscape frame art
    private void CreateLandscapePhoneFrameOverlay(RectTransform canvasRect)
    {
        if (HandyTextureProvider.PhoneFrame == null)
        {
            return;
        }

        RectTransform overlay = HandyUIFactory.CreatePanel("PhoneFrameOverlay", canvasRect, Color.white, Vector2.zero, LandscapeFrameSpriteSize);
        overlay.localEulerAngles = new Vector3(0f, 0f, -90f);

        Image overlayImage = overlay.GetComponent<Image>();
        HandyTextureProvider.ApplySprite(overlayImage, HandyTextureProvider.PhoneFrame, FrameColor, true);
        overlayImage.raycastTarget = false;
        overlay.SetAsLastSibling();
    }

    //creates header UI
    private void CreateHeader(RectTransform shell)
    {
        RectTransform header = CreatePanel("AppHeader", shell, HeaderColor, new Vector2(0f, 305f), new Vector2(1500f, 110f));

        CreateButton(
            "HomeButton",
            header,
            string.Empty,
            18f,
            new Vector2(-670f, -8f),
            new Vector2(46f, 46f),
            new Color(0.98f, 0.99f, 1f, 0.18f),
            navigation.LoadPhoneHome,
            HandyTextureProvider.HomeIcon);

        CreateText(
            "AppTitle",
            header,
            "KSR Mail",
            34f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            Color.white,
            new Vector2(-490f, -8f),
            new Vector2(250f, 54f));

        CreateSearchBox(header);

        CreateText(
            "AccountText",
            header,
            "student@ksr-adventure.local",
            16f,
            FontStyles.Normal,
            TextAlignmentOptions.Right,
            new Color(0.83f, 0.91f, 1f, 1f),
            new Vector2(475f, -8f),
            new Vector2(290f, 40f));

    }

    //creates search box
    private void CreateSearchBox(RectTransform header)
    {
        RectTransform search = CreatePanel("SearchBox", header, new Color(1f, 1f, 1f, 0.18f), new Vector2(20f, -8f), new Vector2(520f, 44f));
        Image searchBackground = search.GetComponent<Image>();
        searchBackground.raycastTarget = true;

        searchInputField = search.gameObject.AddComponent<TMP_InputField>();
        searchInputField.targetGraphic = searchBackground;
        searchInputField.lineType = TMP_InputField.LineType.SingleLine;
        searchInputField.characterLimit = 80;
        searchInputField.interactable = true;
        searchInputField.colors = CreateButtonColors(searchBackground.color);
        Navigation searchNavigation = searchInputField.navigation;
        searchNavigation.mode = Navigation.Mode.None;
        searchInputField.navigation = searchNavigation;

        RectTransform textArea = HandyUIFactory.CreateUIObject("TextArea", search);
        HandyUIFactory.StretchToParent(textArea);
        textArea.offsetMin = new Vector2(18f, 6f);
        textArea.offsetMax = new Vector2(-116f, -6f);
        textArea.gameObject.AddComponent<RectMask2D>();

        TextMeshProUGUI placeholder = CreateText(
            "Placeholder",
            textArea,
            "Search mail",
            18f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            new Color(0.86f, 0.92f, 1f, 0.82f),
            Vector2.zero,
            Vector2.zero);
        HandyUIFactory.StretchToParent(placeholder.rectTransform);

        TextMeshProUGUI inputText = CreateText(
            "Text",
            textArea,
            string.Empty,
            18f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            Color.white,
            Vector2.zero,
            Vector2.zero);
        HandyUIFactory.StretchToParent(inputText.rectTransform);

        searchInputField.textViewport = textArea;
        searchInputField.placeholder = placeholder;
        searchInputField.textComponent = inputText;
        searchInputField.onValueChanged.AddListener(OnSearchChanged);
        searchInputField.onSelect.AddListener(_ => UpdateSearchDropdown());
        searchInputField.onDeselect.AddListener(_ =>
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                SetSearchIndicator(string.Empty);
            }
        });

        searchIndicatorText = CreateText(
            "SearchIndicator",
            search,
            string.Empty,
            13f,
            FontStyles.Bold,
            TextAlignmentOptions.Right,
            new Color(0.8f, 0.9f, 1f, 0.86f),
            new Vector2(222f, 0f),
            new Vector2(90f, 28f));

        AddPointerClickHandler(search.gameObject, _ =>
        {
            searchInputField.Select();
            searchInputField.ActivateInputField();
            UpdateSearchDropdown();
        });
    }

    //creates search results
    private void CreateSearchDropdownLayer(RectTransform canvasRect)
    {
        searchDropdownLayer = HandyUIFactory.CreateUIObject("SearchDropdownLayer", canvasRect);
        HandyUIFactory.StretchToParent(searchDropdownLayer);
        searchDropdownLayer.SetAsLastSibling();

        searchDropdownPanel = CreatePanel(
            "SearchDropdown",
            searchDropdownLayer,
            new Color(1f, 1f, 1f, 0.98f),
            Vector2.zero,
            new Vector2(560f, 320f));
        searchDropdownObject = searchDropdownPanel.gameObject;
        searchDropdownPanel.SetAsLastSibling();
        searchDropdownObject.SetActive(false);
    }

    //creates mailbox columns
    private void CreateMailboxColumns(RectTransform shell)
    {
        RectTransform contentArea = CreatePanel("ContentArea", shell, new Color(0f, 0f, 0f, 0f), new Vector2(0f, -50f), new Vector2(1460f, 600f));

        RectTransform rail = CreatePanel("FolderRail", contentArea, RailColor, new Vector2(-625f, 0f), new Vector2(210f, 600f));
        CreateFolderRail(rail);

        RectTransform inboxPane = CreatePanel("InboxPane", contentArea, PaneColor, new Vector2(-300f, 0f), new Vector2(430f, 600f));
        CreateInboxPane(inboxPane);

        RectTransform divider = CreatePanel("ColumnDivider", contentArea, DividerColor, new Vector2(-70f, 0f), new Vector2(2f, 578f));
        divider.GetComponent<Image>().raycastTarget = false;

        RectTransform readingPane = CreatePanel("ReadingPane", contentArea, PaneColor, new Vector2(330f, 0f), new Vector2(780f, 600f));
        CreateReadingPane(readingPane);
    }

    //creates context menu layer
    private void CreateContextMenuLayer(RectTransform canvasRect)
    {
        RectTransform layer = HandyUIFactory.CreateUIObject("ContextMenuLayer", canvasRect);
        HandyUIFactory.StretchToParent(layer);
        contextMenuLayerObject = layer.gameObject;

        RectTransform dismissArea = HandyUIFactory.CreateUIObject("ContextMenuDismissArea", layer);
        HandyUIFactory.StretchToParent(dismissArea);
        Image dismissImage = dismissArea.gameObject.AddComponent<Image>();
        dismissImage.color = new Color(0f, 0f, 0f, 0f);
        dismissImage.raycastTarget = true;

        Button dismissButton = dismissArea.gameObject.AddComponent<Button>();
        dismissButton.targetGraphic = dismissImage;
        dismissButton.transition = Selectable.Transition.None;
        dismissButton.onClick.AddListener(HideContextMenu);

        contextMenuPanel = CreatePanel(
            "MailContextMenu",
            layer,
            new Color(1f, 1f, 1f, 0.98f),
            Vector2.zero,
            new Vector2(230f, 180f));

        Image panelImage = contextMenuPanel.GetComponent<Image>();
        panelImage.raycastTarget = true;
        contextMenuLayerObject.SetActive(false);
    }

    //creates folder rail
    private void CreateFolderRail(RectTransform rail)
    {
        CreateText(
            "FoldersTitle",
            rail,
            "Mailbox",
            22f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            PrimaryTextColor,
            new Vector2(0f, 245f),
            new Vector2(160f, 40f));

        CreateFolderRow(rail, "Inbox", FolderInbox, true, new Vector2(0f, 185f));
        CreateFolderRow(rail, "Unread", FolderUnread, false, new Vector2(0f, 132f));
        CreateFolderRow(rail, "Archive", FolderArchive, false, new Vector2(0f, 79f));
        CreateFolderRow(rail, "Sent", FolderSent, false, new Vector2(0f, 26f));
        CreateFolderRow(rail, "PaperBin", FolderPaperBin, false, new Vector2(0f, -27f));

        CreatePanel("RailSeparator", rail, DividerColor, new Vector2(0f, -92f), new Vector2(160f, 2f));
        CreateText(
            "RailHint",
            rail,
            "Messages are loaded from MailboxMessages.json.",
            15f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            MutedTextColor,
            new Vector2(0f, -180f),
            new Vector2(160f, 120f));
    }

    //creates folder row
    private void CreateFolderRow(RectTransform parent, string objectName, string label, bool selected, Vector2 position)
    {
        RectTransform row = CreatePanel(
            objectName,
            parent,
            selected ? new Color(0.8f, 0.9f, 1f, 1f) : new Color(0f, 0f, 0f, 0f),
            position,
            new Vector2(168f, 44f));
        Image rowImage = row.GetComponent<Image>();
        rowImage.raycastTarget = true;

        Button button = row.gameObject.AddComponent<Button>();
        button.targetGraphic = rowImage;
        button.colors = CreateButtonColors(rowImage.color);
        button.onClick.AddListener(() => SelectFolder(label));

        TextMeshProUGUI labelText = CreateText(
            "Label",
            row,
            label,
            18f,
            selected ? FontStyles.Bold : FontStyles.Normal,
            TextAlignmentOptions.Left,
            selected ? AccentColor : PrimaryTextColor,
            new Vector2(14f, 0f),
            new Vector2(132f, 32f));

        folderRowViews[label] = new FolderRowView
        {
            Background = rowImage,
            Button = button,
            LabelText = labelText
        };
        UpdateFolderRow(label);
    }

    //creates inbox pane
    private void CreateInboxPane(RectTransform inboxPane)
    {
        inboxTitleText = CreateText(
            "InboxTitle",
            inboxPane,
            "Inbox",
            30f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            PrimaryTextColor,
            new Vector2(-115f, 248f),
            new Vector2(190f, 44f));

        inboxSubtitleText = CreateText(
            "InboxSubtitle",
            inboxPane,
            "Configured messages",
            16f,
            FontStyles.Normal,
            TextAlignmentOptions.Right,
            MutedTextColor,
            new Vector2(110f, 248f),
            new Vector2(188f, 32f));

        mailListScrollObject = CreatePanel("MailListScroll", inboxPane, new Color(0f, 0f, 0f, 0f), new Vector2(0f, -46f), new Vector2(430f, 500f)).gameObject;
        mailListContent = CreateVerticalScrollContent(mailListScrollObject.GetComponent<RectTransform>(), 8f, new RectOffset(0, 0, 4, 4));

        emptyInboxPanel = CreatePanel("EmptyInboxPanel", inboxPane, new Color(0.955f, 0.97f, 0.985f, 1f), new Vector2(0f, -40f), new Vector2(380f, 360f)).gameObject;
        CreateText(
            "EmptyIcon",
            emptyInboxPanel.GetComponent<RectTransform>(),
            "@",
            54f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            AccentColor,
            new Vector2(0f, 110f),
            new Vector2(120f, 80f));
        emptyInboxTitleText = CreateText(
            "EmptyTitle",
            emptyInboxPanel.GetComponent<RectTransform>(),
            string.Empty,
            24f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            PrimaryTextColor,
            new Vector2(0f, 30f),
            new Vector2(320f, 50f));
        emptyInboxMessageText = CreateText(
            "EmptyMessage",
            emptyInboxPanel.GetComponent<RectTransform>(),
            string.Empty,
            18f,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            MutedTextColor,
            new Vector2(0f, -55f),
            new Vector2(300f, 120f));
    }

    //creates reading pane
    private void CreateReadingPane(RectTransform readingPane)
    {
        readingEmptyPanel = CreatePanel("ReadingEmptyPanel", readingPane, new Color(0.965f, 0.975f, 0.988f, 1f), Vector2.zero, new Vector2(700f, 500f)).gameObject;
        CreateText(
            "ReadingEmptyIcon",
            readingEmptyPanel.GetComponent<RectTransform>(),
            "Mail",
            44f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            AccentColor,
            new Vector2(0f, 85f),
            new Vector2(240f, 70f));
        CreateText(
            "ReadingEmptyTitle",
            readingEmptyPanel.GetComponent<RectTransform>(),
            "Select a message",
            30f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            PrimaryTextColor,
            new Vector2(0f, 10f),
            new Vector2(420f, 52f));
        CreateText(
            "ReadingEmptyMessage",
            readingEmptyPanel.GetComponent<RectTransform>(),
            "Open a mail from the inbox list to read the full message.",
            18f,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            MutedTextColor,
            new Vector2(0f, -60f),
            new Vector2(470f, 68f));

        readingContentPanel = CreatePanel("ReadingContentPanel", readingPane, PaneColor, Vector2.zero, new Vector2(700f, 560f)).gameObject;
        RectTransform content = readingContentPanel.GetComponent<RectTransform>();

        readingSubjectText = CreateText(
            "MailSubject",
            content,
            string.Empty,
            32f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            PrimaryTextColor,
            new Vector2(0f, 228f),
            new Vector2(700f, 58f));
        readingSenderText = CreateText(
            "MailSender",
            content,
            string.Empty,
            20f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            PrimaryTextColor,
            new Vector2(-180f, 178f),
            new Vector2(340f, 34f));
        readingAddressText = CreateText(
            "MailAddress",
            content,
            string.Empty,
            16f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            MutedTextColor,
            new Vector2(-172f, 150f),
            new Vector2(356f, 30f));
        readingTimeText = CreateText(
            "MailTime",
            content,
            string.Empty,
            17f,
            FontStyles.Normal,
            TextAlignmentOptions.Right,
            MutedTextColor,
            new Vector2(250f, 172f),
            new Vector2(180f, 34f));

        CreatePanel("ReadingDivider", content, DividerColor, new Vector2(0f, 126f), new Vector2(660f, 2f));

        RectTransform readingScrollRoot = CreatePanel("ReadingScroll", content, new Color(0f, 0f, 0f, 0f), new Vector2(0f, -80f), new Vector2(660f, 390f));
        readingScrollContent = CreateVerticalScrollContent(
            readingScrollRoot,
            14f,
            new RectOffset(20, 20, 12, 12),
            TextAnchor.UpperLeft,
            true,
            true);
        readingScrollRect = readingScrollRoot.GetComponent<ScrollRect>();

        readingBodyText = CreateText(
            "MailBody",
            readingScrollContent,
            string.Empty,
            21f,
            FontStyles.Normal,
            TextAlignmentOptions.TopLeft,
            PrimaryTextColor,
            Vector2.zero,
            new Vector2(ReadingContentWidth, 120f));
        readingBodyText.overflowMode = TextOverflowModes.Overflow;
        readingBodyLayoutElement = readingBodyText.gameObject.AddComponent<LayoutElement>();
        readingBodyLayoutElement.preferredWidth = ReadingContentWidth;
        readingBodyLayoutElement.minHeight = 90f;

        RectTransform attachmentButtonRect = CreateButton(
            "AttachmentButton",
            readingScrollContent,
            "Attachment",
            17f,
            Vector2.zero,
            new Vector2(340f, 44f),
            new Color(0.91f, 0.95f, 1f, 1f),
            null);
        LayoutElement attachmentButtonLayoutElement = attachmentButtonRect.gameObject.AddComponent<LayoutElement>();
        attachmentButtonLayoutElement.preferredWidth = 340f;
        attachmentButtonLayoutElement.preferredHeight = 44f;
        attachmentButtonObject = attachmentButtonRect.gameObject;
        attachmentButton = attachmentButtonObject.GetComponent<Button>();
        attachmentButtonText = attachmentButtonObject.GetComponentInChildren<TextMeshProUGUI>();
        attachmentButtonText.color = AccentColor;
        attachmentButtonObject.SetActive(false);

        readingImageFrame = CreatePanel("MailImageFrame", readingScrollContent, new Color(0.92f, 0.945f, 0.97f, 1f), Vector2.zero, new Vector2(ReadingContentWidth, 180f)).gameObject;
        readingImageFrameLayoutElement = readingImageFrame.AddComponent<LayoutElement>();
        readingImageFrameLayoutElement.preferredWidth = ReadingContentWidth;
        readingImageFrameLayoutElement.preferredHeight = 180f;
        readingImage = CreatePanel("MailImage", readingImageFrame.GetComponent<RectTransform>(), Color.white, Vector2.zero, Vector2.zero).GetComponent<Image>();
        HandyUIFactory.StretchToParent(readingImage.rectTransform);
        readingImage.rectTransform.offsetMin = new Vector2(10f, 10f);
        readingImage.rectTransform.offsetMax = new Vector2(-10f, -10f);
        readingImage.type = Image.Type.Simple;
        readingImage.preserveAspect = true;
        readingImageFrame.SetActive(false);
        readingContentPanel.SetActive(false);
    }

    //shows mailbox
    private void RenderMailbox()
    {
        ClearMailCards();
        UpdateFolderRows();

        List<MailboxMessage> visibleMails = GetVisibleMails();
        bool selectedMailVisible = ContainsMail(visibleMails, selectedMailId);
        if (!selectedMailVisible)
        {
            selectedMailId = null;
            ShowReadingPlaceholder();
        }

        UpdateInboxHeading(visibleMails.Count);
        bool canShowConfiguredMails = visibleMails.Count > 0;
        mailListScrollObject.SetActive(canShowConfiguredMails);
        emptyInboxPanel.SetActive(!canShowConfiguredMails);

        if (!mailboxCatalog.HasMails)
        {
            SetEmptyInboxText("No configured messages", "MailboxMessages.json does not contain any valid mail entries.");
            return;
        }

        if (visibleMails.Count == 0)
        {
            SetEmptyInboxText(GetEmptyTitleForSelectedFolder(), GetEmptyMessageForSelectedFolder());
            return;
        }

        foreach (MailboxMessage mail in visibleMails)
        {
            CreateMailCard(mail);
        }

        RefreshMailListLayout();
    }

    //selects mail folder
    private void SelectFolder(string folderName)
    {
        HideContextMenu();
        HideSearchDropdown();
        selectedFolder = string.IsNullOrWhiteSpace(folderName) ? FolderInbox : folderName;
        RenderMailbox();
    }

    //updates mail search
    private void OnSearchChanged(string value)
    {
        HideContextMenu();
        searchQuery = value ?? string.Empty;
        UpdateSearchDropdown();
        RenderMailbox();
    }

    //updates search results
    private void UpdateSearchDropdown()
    {
        if (searchDropdownObject == null || searchDropdownLayer == null || searchDropdownPanel == null)
        {
            return;
        }

        ClearSearchDropdownItems();

        string normalizedQuery = searchQuery?.Trim();
        if (string.IsNullOrEmpty(normalizedQuery))
        {
            searchDropdownObject.SetActive(false);
            SetSearchIndicator(searchInputField != null && searchInputField.isFocused ? "Typing" : string.Empty);
            return;
        }

        List<MailboxMessage> matchingMails = GetSearchMatches(normalizedQuery);
        SetSearchIndicator(matchingMails.Count == 1 ? "1 result" : $"{matchingMails.Count} results");

        int shownResultCount = Mathf.Min(matchingMails.Count, MaxSearchDropdownResults);
        int rowCount = shownResultCount > 0 ? shownResultCount : 1;
        if (matchingMails.Count > MaxSearchDropdownResults)
        {
            rowCount++;
        }

        float padding = 8f;
        float menuWidth = 560f;
        float menuHeight = padding * 2f + rowCount * SearchResultRowHeight;
        searchDropdownPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, menuWidth);
        searchDropdownPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, menuHeight);
        searchDropdownPanel.anchoredPosition = new Vector2(SearchBoxCenterX, SearchDropdownTopY - (menuHeight * 0.5f));

        if (shownResultCount == 0)
        {
            CreateSearchInfoRow("No matching mail", "Search includes inbox, unread, archive, and paper bin.", 0, menuWidth, menuHeight, padding);
        }
        else
        {
            for (int i = 0; i < shownResultCount; i++)
            {
                CreateSearchResultRow(matchingMails[i], i, menuWidth, menuHeight, padding);
            }

            if (matchingMails.Count > MaxSearchDropdownResults)
            {
                CreateSearchInfoRow(
                    $"{matchingMails.Count - MaxSearchDropdownResults} more matches",
                    "Keep typing to narrow the results.",
                    shownResultCount,
                    menuWidth,
                    menuHeight,
                    padding);
            }
        }

        searchDropdownObject.SetActive(true);
        searchDropdownLayer.SetAsLastSibling();
        searchDropdownPanel.SetAsLastSibling();
    }

    //gets search matches
    private List<MailboxMessage> GetSearchMatches(string normalizedQuery)
    {
        List<MailboxMessage> matchingMails = new List<MailboxMessage>();
        if (!mailboxCatalog.HasMails)
        {
            return matchingMails;
        }

        foreach (MailboxMessage mail in mailboxCatalog.Mails)
        {
            if (!phoneSaveState.IsMailRecieved(mail.Id))
            {
                continue;
            }

            if (MailMatchesSearch(mail, normalizedQuery))
            {
                matchingMails.Add(mail);
            }
        }

        return matchingMails;
    }

    //creates search result row
    private void CreateSearchResultRow(MailboxMessage mail, int index, float menuWidth, float menuHeight, float padding)
    {
        string folderName = GetFolderForMail(mail);
        RectTransform row = CreateSearchDropdownRow(index, menuWidth, menuHeight, padding);
        Image rowImage = row.GetComponent<Image>();

        Button button = row.gameObject.AddComponent<Button>();
        button.targetGraphic = rowImage;
        button.colors = CreateButtonColors(rowImage.color);
        button.onClick.AddListener(() => OpenSearchResult(mail));

        CreateText(
            "Title",
            row,
            $"{Fallback(mail.SenderName, "Unknown sender")} - {Fallback(mail.Subject, "(No subject)")}",
            16f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            PrimaryTextColor,
            new Vector2(-52f, 13f),
            new Vector2(410f, 24f));

        CreateText(
            "Folder",
            row,
            folderName,
            13f,
            FontStyles.Bold,
            TextAlignmentOptions.Right,
            AccentColor,
            new Vector2(204f, 13f),
            new Vector2(110f, 22f));

        CreateText(
            "Preview",
            row,
            Fallback(mail.Preview, mail.Body),
            13f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            MutedTextColor,
            new Vector2(-8f, -14f),
            new Vector2(500f, 24f));
    }

    //creates search info row
    private void CreateSearchInfoRow(string title, string message, int index, float menuWidth, float menuHeight, float padding)
    {
        RectTransform row = CreateSearchDropdownRow(index, menuWidth, menuHeight, padding);
        row.GetComponent<Image>().raycastTarget = false;

        CreateText(
            "Title",
            row,
            title,
            16f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            PrimaryTextColor,
            new Vector2(0f, 13f),
            new Vector2(500f, 24f));

        CreateText(
            "Message",
            row,
            message,
            13f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            MutedTextColor,
            new Vector2(0f, -14f),
            new Vector2(500f, 24f));
    }

    //creates search row panel
    private RectTransform CreateSearchDropdownRow(int index, float menuWidth, float menuHeight, float padding)
    {
        float y = (menuHeight * 0.5f) - padding - (SearchResultRowHeight * 0.5f) - index * SearchResultRowHeight;
        RectTransform row = CreatePanel(
            "SearchResultRow",
            searchDropdownPanel,
            new Color(1f, 1f, 1f, 0f),
            new Vector2(0f, y),
            new Vector2(menuWidth - 14f, SearchResultRowHeight));
        row.GetComponent<Image>().raycastTarget = true;
        return row;
    }

    //opens search result
    private void OpenSearchResult(MailboxMessage mail)
    {
        if (mail == null)
        {
            return;
        }

        HideSearchDropdown();
        selectedFolder = GetFolderForMail(mail);
        searchQuery = string.Empty;
        if (searchInputField != null)
        {
            searchInputField.SetTextWithoutNotify(string.Empty);
            searchInputField.DeactivateInputField();
        }

        SetSearchIndicator(string.Empty);
        RenderMailbox();
        OpenMail(mail);
    }

    //gets mail folder
    private string GetFolderForMail(MailboxMessage mail)
    {
        if (mail == null)
        {
            return FolderInbox;
        }

        if (phoneSaveState.IsMailDeleted(mail.Id))
        {
            return FolderPaperBin;
        }

        return phoneSaveState.IsMailArchived(mail.Id) ? FolderArchive : FolderInbox;
    }

    //hides search results
    private void HideSearchDropdown()
    {
        if (searchDropdownObject != null)
        {
            searchDropdownObject.SetActive(false);
        }
    }

    //clears search result rows
    private void ClearSearchDropdownItems()
    {
        if (searchDropdownPanel == null)
        {
            return;
        }

        for (int i = searchDropdownPanel.childCount - 1; i >= 0; i--)
        {
            GameObject child = searchDropdownPanel.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    //sets search indicator
    private void SetSearchIndicator(string text)
    {
        if (searchIndicatorText != null)
        {
            searchIndicatorText.text = text;
        }
    }

    //gets shown mails
    private List<MailboxMessage> GetVisibleMails()
    {
        List<MailboxMessage> visibleMails = new List<MailboxMessage>();
        if (!mailboxCatalog.HasMails)
        {
            return visibleMails;
        }

        foreach (MailboxMessage mail in mailboxCatalog.Mails)
        {
            if (!phoneSaveState.IsMailRecieved(mail.Id))
            {
                continue;
            }

            bool isDeleted = phoneSaveState.IsMailDeleted(mail.Id);
            if (isDeleted)
            {
                if (selectedFolder != FolderPaperBin)
                {
                    continue;
                }
            }
            else if (selectedFolder == FolderPaperBin)
            {
                continue;
            }

            if (selectedFolder == FolderUnread && phoneSaveState.IsMailRead(mail.Id))
            {
                continue;
            }

            bool isArchived = phoneSaveState.IsMailArchived(mail.Id);
            if (selectedFolder == FolderArchive)
            {
                if (!isArchived)
                {
                    continue;
                }
            }
            else if (selectedFolder == FolderSent)
            {
                continue;
            }
            else if (selectedFolder == FolderInbox && isArchived)
            {
                continue;
            }

            visibleMails.Add(mail);
        }

        return visibleMails;
    }

    //checks mail list
    private static bool ContainsMail(List<MailboxMessage> mails, string mailId)
    {
        if (mails == null || string.IsNullOrWhiteSpace(mailId))
        {
            return false;
        }

        foreach (MailboxMessage mail in mails)
        {
            if (mail != null && mail.Id == mailId)
            {
                return true;
            }
        }

        return false;
    }

    //checks mail search
    private static bool MailMatchesSearch(MailboxMessage mail, string query)
    {
        if (mail == null)
        {
            return false;
        }

        string normalizedQuery = query?.Trim();
        if (string.IsNullOrEmpty(normalizedQuery))
        {
            return true;
        }

        return ContainsIgnoreCase(mail.SenderName, normalizedQuery)
            || ContainsIgnoreCase(mail.SenderAddress, normalizedQuery)
            || ContainsIgnoreCase(mail.Subject, normalizedQuery)
            || ContainsIgnoreCase(mail.Preview, normalizedQuery)
            || ContainsIgnoreCase(mail.Body, normalizedQuery)
            || ContainsIgnoreCase(mail.AttachmentName, normalizedQuery);
    }

    //checks text match
    private static bool ContainsIgnoreCase(string value, string query)
    {
        return !string.IsNullOrEmpty(value)
            && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    //updates inbox heading
    private void UpdateInboxHeading(int visibleMailCount)
    {
        if (inboxTitleText != null)
        {
            inboxTitleText.text = selectedFolder;
        }

        if (inboxSubtitleText != null)
        {
            string messageLabel = visibleMailCount == 1 ? "1 message" : $"{visibleMailCount} messages";
            inboxSubtitleText.text = messageLabel;
        }
    }

    //gets empty title
    private string GetEmptyTitleForSelectedFolder()
    {
        if (selectedFolder == FolderUnread)
        {
            return "No unread mail";
        }

        if (selectedFolder == FolderArchive)
        {
            return "Archive is empty";
        }

        if (selectedFolder == FolderSent)
        {
            return "No sent mail";
        }

        if (selectedFolder == FolderPaperBin)
        {
            return "Paper bin is empty";
        }

        return "No mail yet";
    }

    //gets empty message
    private string GetEmptyMessageForSelectedFolder()
    {
        if (selectedFolder == FolderUnread)
        {
            return "Every received message has already been opened.";
        }

        if (selectedFolder == FolderArchive)
        {
            return "Right-click a message and choose Archive to move it here.";
        }

        if (selectedFolder == FolderSent)
        {
            return "The phone cannot send messages in this minigame yet.";
        }

        if (selectedFolder == FolderPaperBin)
        {
            return "Right-click a message and choose Delete to move it here.";
        }

        return "The inbox is empty. A message will appear here once the story sends one to the phone.";
    }

    //updates folder rows
    private void UpdateFolderRows()
    {
        foreach (string folderName in folderRowViews.Keys)
        {
            UpdateFolderRow(folderName);
        }
    }

    //updates folder row
    private void UpdateFolderRow(string folderName)
    {
        if (!folderRowViews.TryGetValue(folderName, out FolderRowView view))
        {
            return;
        }

        bool isSelected = folderName == selectedFolder;
        Color backgroundColor = isSelected ? new Color(0.8f, 0.9f, 1f, 1f) : new Color(0f, 0f, 0f, 0f);
        view.Background.color = backgroundColor;
        view.Background.raycastTarget = true;
        view.Button.colors = CreateButtonColors(backgroundColor);
        view.LabelText.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
        view.LabelText.color = isSelected ? AccentColor : PrimaryTextColor;
    }

    //creates mail card
    private void CreateMailCard(MailboxMessage mail)
    {
        RectTransform card = CreatePanel("MailCard", mailListContent, ReadCardColor, Vector2.zero, new Vector2(MailCardWidth, MailCardHeight));
        LayoutElement layoutElement = card.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = MailCardWidth;
        layoutElement.minWidth = MailCardWidth;
        layoutElement.preferredHeight = MailCardHeight;
        layoutElement.minHeight = MailCardHeight;

        Button button = card.gameObject.AddComponent<Button>();
        button.targetGraphic = card.GetComponent<Image>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => OpenMail(mail));

        RectTransform unreadDot = HandyUIFactory.CreateCirclePanel("UnreadDot", card, AccentColor, new Vector2(-191f, 29f), new Vector2(9f, 9f));
        TextMeshProUGUI senderText = CreateText(
            "Sender",
            card,
            mail.SenderName,
            18f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            PrimaryTextColor,
            new Vector2(-28f, 29f),
            new Vector2(308f, 28f));
        TextMeshProUGUI timeText = CreateText(
            "Time",
            card,
            mail.Time,
            14f,
            FontStyles.Normal,
            TextAlignmentOptions.Right,
            MutedTextColor,
            new Vector2(170f, 29f),
            new Vector2(74f, 24f));
        TextMeshProUGUI subjectText = CreateText(
            "Subject",
            card,
            mail.Subject,
            17f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            PrimaryTextColor,
            new Vector2(0f, 3f),
            new Vector2(392f, 28f));
        TextMeshProUGUI previewText = CreateText(
            "Preview",
            card,
            mail.Preview,
            15f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            MutedTextColor,
            new Vector2(0f, -29f),
            new Vector2(392f, 44f));

        MailCardView view = new MailCardView
        {
            Background = card.GetComponent<Image>(),
            Button = button,
            UnreadDot = unreadDot.gameObject,
            SenderText = senderText,
            TimeText = timeText,
            SubjectText = subjectText,
            PreviewText = previewText
        };

        AddHoverHandlers(card.gameObject, () =>
        {
            view.IsHovered = true;
            UpdateMailCardView(mail);
        }, () =>
        {
            view.IsHovered = false;
            UpdateMailCardView(mail);
        });
        AddPointerClickHandler(card.gameObject, eventData =>
        {
            if (eventData != null && eventData.button == PointerEventData.InputButton.Right)
            {
                ShowMailContextMenu(mail, eventData.position);
            }
        });

        mailCardViews[mail.Id] = view;
        UpdateMailCardView(mail);
    }

    //shows mail menu
    private void ShowMailContextMenu(MailboxMessage mail, Vector2 screenPosition)
    {
        if (mail == null || contextMenuLayerObject == null || contextMenuPanel == null)
        {
            return;
        }

        ClearContextMenuItems();

        bool isArchived = phoneSaveState.IsMailArchived(mail.Id);
        bool isRead = phoneSaveState.IsMailRead(mail.Id);
        bool isDeleted = phoneSaveState.IsMailDeleted(mail.Id);
        bool canOpenPdf = CanOpenPdfAttachment(mail);
        int optionCount = 3 + (isDeleted ? 0 : 1) + (canOpenPdf ? 1 : 0);
        float menuWidth = 230f;
        float itemHeight = 40f;
        float padding = 8f;
        float menuHeight = padding * 2f + optionCount * itemHeight;

        contextMenuPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, menuWidth);
        contextMenuPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, menuHeight);

        int optionIndex = 0;
        CreateContextMenuItem("Open", optionIndex++, menuWidth, menuHeight, itemHeight, padding, () => OpenMail(mail));
        if (!isDeleted)
        {
            CreateContextMenuItem(isArchived ? "Move to Inbox" : "Archive", optionIndex++, menuWidth, menuHeight, itemHeight, padding, () =>
            {
                HideContextMenu();
                phoneSaveState.SetMailArchived(mail.Id, mailboxCatalog.MailIds, !isArchived);
                RenderMailbox();
            });
        }

        CreateContextMenuItem(isDeleted ? "Restore" : "Delete", optionIndex++, menuWidth, menuHeight, itemHeight, padding, () =>
        {
            HideContextMenu();
            phoneSaveState.SetMailDeleted(mail.Id, mailboxCatalog.MailIds, !isDeleted);
            RenderMailbox();
        });

        CreateContextMenuItem(isRead ? "Mark as unread" : "Mark as read", optionIndex++, menuWidth, menuHeight, itemHeight, padding, () =>
        {
            HideContextMenu();
            phoneSaveState.SetMailRead(mail.Id, mailboxCatalog.MailIds, !isRead);
            RenderMailbox();
        });

        if (canOpenPdf)
        {
            CreateContextMenuItem("Open PDF", optionIndex, menuWidth, menuHeight, itemHeight, padding, () =>
            {
                HideContextMenu();
                OpenPdfAttachment(mail.AttachmentPath, Fallback(mail.AttachmentName, "PDF attachment"));
            });
        }

        PositionContextMenu(screenPosition, menuWidth, menuHeight);
        contextMenuLayerObject.SetActive(true);
        contextMenuLayerObject.transform.SetAsLastSibling();
    }

    //creates menu item
    private void CreateContextMenuItem(
        string label,
        int index,
        float menuWidth,
        float menuHeight,
        float itemHeight,
        float padding,
        UnityEngine.Events.UnityAction action)
    {
        float y = (menuHeight * 0.5f) - padding - (itemHeight * 0.5f) - index * itemHeight;
        RectTransform item = CreatePanel(
            "ContextMenuItem",
            contextMenuPanel,
            new Color(1f, 1f, 1f, 0f),
            new Vector2(0f, y),
            new Vector2(menuWidth - 12f, itemHeight));

        Image itemImage = item.GetComponent<Image>();
        itemImage.raycastTarget = true;

        Button button = item.gameObject.AddComponent<Button>();
        button.targetGraphic = itemImage;
        button.colors = CreateButtonColors(itemImage.color);
        button.onClick.AddListener(action);

        CreateText(
            "Label",
            item,
            label,
            17f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            PrimaryTextColor,
            new Vector2(8f, 0f),
            new Vector2(menuWidth - 34f, itemHeight - 8f));
    }

    //places mail menu
    private void PositionContextMenu(Vector2 screenPosition, float menuWidth, float menuHeight)
    {
        if (mailboxCanvasRect == null || contextMenuPanel == null)
        {
            return;
        }

        Vector2 localPosition = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mailboxCanvasRect, screenPosition, null, out localPosition);

        contextMenuPanel.anchorMin = new Vector2(0.5f, 0.5f);
        contextMenuPanel.anchorMax = new Vector2(0.5f, 0.5f);
        contextMenuPanel.pivot = new Vector2(0f, 1f);

        Rect screenRect = new Rect(
            LandscapeScreenSize.x * -0.5f,
            LandscapeScreenSize.y * -0.5f,
            LandscapeScreenSize.x,
            LandscapeScreenSize.y);
        float margin = 8f;
        float x = Mathf.Clamp(localPosition.x, screenRect.xMin + margin, screenRect.xMax - menuWidth - margin);
        float y = Mathf.Clamp(localPosition.y, screenRect.yMin + menuHeight + margin, screenRect.yMax - margin);
        contextMenuPanel.anchoredPosition = new Vector2(x, y);
    }

    //clears menu items
    private void ClearContextMenuItems()
    {
        if (contextMenuPanel == null)
        {
            return;
        }

        for (int i = contextMenuPanel.childCount - 1; i >= 0; i--)
        {
            GameObject child = contextMenuPanel.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    //hides mail menu
    private void HideContextMenu()
    {
        if (contextMenuLayerObject != null)
        {
            contextMenuLayerObject.SetActive(false);
        }
    }

    //checks PDF attachment
    private static bool CanOpenPdfAttachment(MailboxMessage mail)
    {
        return mail != null
            && IsAttachmentType(mail.AttachmentType?.Trim(), "Pdf")
            && !string.IsNullOrWhiteSpace(mail.AttachmentPath);
    }

    //opens mail
    private void OpenMail(MailboxMessage mail)
    {
        HideContextMenu();
        selectedMailId = mail.Id;
        phoneSaveState.MarkMailRead(mail.Id, mailboxCatalog.MailIds);
        UpdateMailCards();

        readingEmptyPanel.SetActive(false);
        readingContentPanel.SetActive(true);

        readingSubjectText.text = Fallback(mail.Subject, "(No subject)");
        readingSenderText.text = Fallback(mail.SenderName, "Unknown sender");
        readingAddressText.text = Fallback(mail.SenderAddress, "No sender address");
        readingTimeText.text = mail.Time;
        readingBodyText.text = Fallback(mail.Body, "(No message body)");
        ApplyMailAttachment(mail);
        RefreshReadingContentLayout();
    }

    //shows empty reading pane
    private void ShowReadingPlaceholder()
    {
        readingEmptyPanel.SetActive(true);
        readingContentPanel.SetActive(false);
    }

    //sets mail attachment
    private void ApplyMailAttachment(MailboxMessage mail)
    {
        readingImageFrame.SetActive(false);
        readingImage.sprite = null;
        attachmentButtonObject.SetActive(false);
        attachmentButton.onClick.RemoveAllListeners();

        string attachmentType = mail.AttachmentType?.Trim();
        string imagePath = string.IsNullOrWhiteSpace(mail.ImagePath) && IsAttachmentType(attachmentType, "Image")
            ? mail.AttachmentPath
            : mail.ImagePath;

        if (!string.IsNullOrWhiteSpace(imagePath))
        {
            ApplyMailImage(imagePath);
        }

        if (string.IsNullOrWhiteSpace(mail.AttachmentPath))
        {
            return;
        }

        if (IsAttachmentType(attachmentType, "Pdf"))
        {
            string attachmentName = Fallback(mail.AttachmentName, "PDF attachment");
            attachmentButtonText.text = $"Open PDF: {attachmentName}";
            attachmentButton.onClick.AddListener(() => OpenPdfAttachment(mail.AttachmentPath, attachmentName));
            attachmentButtonObject.SetActive(true);
            return;
        }

        if (IsAttachmentType(attachmentType, "Image"))
        {
            return;
        }

        attachmentButtonText.text = Fallback(mail.AttachmentName, "Attachment");
        attachmentButtonObject.SetActive(true);
    }

    //sets mail image
    private void ApplyMailImage(string imagePath)
    {
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite == null)
        {
            Debug.LogWarning($"MailboxController: Could not load mail image at Resources/{imagePath}.");
            return;
        }

        readingImageFrame.SetActive(true);
        readingImage.sprite = sprite;
        readingImage.type = Image.Type.Simple;
        readingImage.preserveAspect = true;

        float spriteWidth = Mathf.Max(1f, sprite.rect.width);
        float spriteHeight = Mathf.Max(1f, sprite.rect.height);
        float imageHeight = (ReadingContentWidth - 20f) * (spriteHeight / spriteWidth) + 20f;
        readingImageFrameLayoutElement.preferredHeight = Mathf.Clamp(imageHeight, 140f, ReadingImageMaxHeight);
    }

    //opens PDF attachment
    private void OpenPdfAttachment(string pdfResourcePath, string attachmentName)
    {
        TextAsset pdfAsset = Resources.Load<TextAsset>(pdfResourcePath);
        if (pdfAsset == null)
        {
            Debug.LogWarning($"MailboxController: Could not load PDF attachment at Resources/{pdfResourcePath}. Use a .bytes file in Resources for PDFs.");
            return;
        }

        string fileName = SanitizeFileName(attachmentName);
        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".pdf";
        }

        string outputPath = Path.Combine(Application.temporaryCachePath, fileName);
        File.WriteAllBytes(outputPath, pdfAsset.bytes);
        Application.OpenURL(new Uri(outputPath).AbsoluteUri);
    }

    //updates mail cards
    private void UpdateMailCards()
    {
        foreach (MailboxMessage mail in mailboxCatalog.Mails)
        {
            UpdateMailCardView(mail);
        }
    }

    //updates mail card view
    private void UpdateMailCardView(MailboxMessage mail)
    {
        if (!mailCardViews.TryGetValue(mail.Id, out MailCardView view))
        {
            return;
        }

        bool isRead = phoneSaveState.IsMailRead(mail.Id);
        bool isSelected = selectedMailId == mail.Id;
        Color baseColor = isRead ? ReadCardColor : UnreadCardColor;
        Color backgroundColor = isSelected ? SelectedCardColor : view.IsHovered ? HoverCardColor : baseColor;

        view.Background.color = backgroundColor;
        view.UnreadDot.SetActive(!isRead);
        view.SenderText.rectTransform.anchoredPosition = isRead ? new Vector2(-36f, 29f) : new Vector2(-28f, 29f);
        view.SenderText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, isRead ? 326f : 308f);
        view.SenderText.fontStyle = isRead ? FontStyles.Normal : FontStyles.Bold;
        view.SubjectText.fontStyle = isRead ? FontStyles.Normal : FontStyles.Bold;
        view.TimeText.color = isRead ? MutedTextColor : AccentColor;
        view.PreviewText.color = isRead ? MutedTextColor : PrimaryTextColor;
    }

    //clears mail cards
    private void ClearMailCards()
    {
        mailCardViews.Clear();

        if (mailListContent == null)
        {
            return;
        }

        for (int i = mailListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(mailListContent.GetChild(i).gameObject);
        }
    }

    //adds hover handlers
    private static void AddHoverHandlers(GameObject target, UnityEngine.Events.UnityAction onEnter, UnityEngine.Events.UnityAction onExit)
    {
        EventTrigger trigger = GetOrCreateEventTrigger(target);

        EventTrigger.Entry enterEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        enterEntry.callback.AddListener(_ => onEnter?.Invoke());
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        exitEntry.callback.AddListener(_ => onExit?.Invoke());
        trigger.triggers.Add(exitEntry);
    }

    //adds click handler
    private static void AddPointerClickHandler(GameObject target, Action<PointerEventData> onClick)
    {
        EventTrigger trigger = GetOrCreateEventTrigger(target);
        EventTrigger.Entry clickEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        clickEntry.callback.AddListener(eventData => onClick?.Invoke(eventData as PointerEventData));
        trigger.triggers.Add(clickEntry);
    }

    //gets event trigger
    private static EventTrigger GetOrCreateEventTrigger(GameObject target)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        return trigger != null ? trigger : target.AddComponent<EventTrigger>();
    }

    //updates mail list layout
    private void RefreshMailListLayout()
    {
        if (mailListContent == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(mailListContent);

        ScrollRect scrollRect = mailListScrollObject != null
            ? mailListScrollObject.GetComponent<ScrollRect>()
            : null;
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    //updates reading layout
    private void RefreshReadingContentLayout()
    {
        if (readingScrollContent == null)
        {
            return;
        }

        if (readingBodyText != null && readingBodyLayoutElement != null)
        {
            float preferredHeight = readingBodyText.GetPreferredValues(readingBodyText.text, ReadingContentWidth, 0f).y;
            readingBodyLayoutElement.preferredHeight = Mathf.Max(90f, preferredHeight + 8f);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(readingScrollContent);

        if (readingScrollRect != null)
        {
            readingScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    //sets empty inbox text
    private void SetEmptyInboxText(string title, string message)
    {
        emptyInboxTitleText.text = title;
        emptyInboxMessageText.text = message;
    }

    //creates scroll content
    private RectTransform CreateVerticalScrollContent(
        RectTransform scrollRoot,
        float spacing,
        RectOffset padding = null,
        TextAnchor childAlignment = TextAnchor.UpperCenter,
        bool childControlWidth = false,
        bool childControlHeight = false)
    {
        ScrollRect scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;

        Image scrollRootImage = scrollRoot.GetComponent<Image>();
        if (scrollRootImage != null)
        {
            scrollRootImage.raycastTarget = true;
        }

        RectTransform viewport = CreatePanel("Viewport", scrollRoot, new Color(0f, 0f, 0f, 0f), Vector2.zero, Vector2.zero);
        HandyUIFactory.StretchToParent(viewport);

        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform content = HandyUIFactory.CreateUIObject("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup layoutGroup = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = padding ?? new RectOffset(6, 6, 4, 4);
        layoutGroup.spacing = spacing;
        layoutGroup.childAlignment = childAlignment;
        layoutGroup.childControlWidth = childControlWidth;
        layoutGroup.childControlHeight = childControlHeight;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport;
        scrollRect.content = content;
        return content;
    }

    //creates UI button
    private RectTransform CreateButton(
        string objectName,
        RectTransform parent,
        string text,
        float fontSize,
        Vector2 anchoredPosition,
        Vector2 size,
        Color normalColor,
        UnityEngine.Events.UnityAction onClick,
        Sprite iconSprite = null)
    {
        return HandyUIFactory.CreateButton(
            objectName,
            parent,
            text,
            anchoredPosition,
            size,
            normalColor,
            onClick,
            CreateButtonColors(normalColor),
            iconSprite,
            fontSize,
            Color.white,
            new Vector2(size.x - 16f, size.y - 10f),
            iconSprite != null ? 0f : 10f);
    }

    //creates button colors
    private ColorBlock CreateButtonColors(Color normalColor)
    {
        return HandyUIFactory.CreateButtonColors(
            normalColor,
            Color.Lerp(normalColor, AccentHoverColor, 0.18f),
            Color.Lerp(normalColor, Color.black, 0.12f),
            normalColor,
            Color.Lerp(normalColor, Color.gray, 0.4f));
    }

    //creates UI panel
    private RectTransform CreatePanel(string objectName, RectTransform parent, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        return HandyUIFactory.CreateSlicedPanel(objectName, parent, color, anchoredPosition, size, HandyTextureProvider.RoundedPanelMask);
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
        return HandyUIFactory.CreateText(
            objectName,
            parent,
            text,
            anchoredPosition,
            size,
            fontSize,
            fontStyle,
            alignment,
            color,
            false,
            -1f,
            TextWrappingModes.Normal,
            TextOverflowModes.Ellipsis);
    }

    //gets fallback text
    private static string Fallback(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    //checks attachment type
    private static bool IsAttachmentType(string value, string expectedType)
    {
        return string.Equals(value, expectedType, StringComparison.OrdinalIgnoreCase);
    }

    //cleans file name
    private static string SanitizeFileName(string value)
    {
        string fileName = Fallback(value, "MailboxAttachment");
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidChar, '_');
        }

        return fileName;
    }

    //stores mail card UI
    private class MailCardView
    {
        public Image Background;
        public Button Button;
        public GameObject UnreadDot;
        public TextMeshProUGUI SenderText;
        public TextMeshProUGUI TimeText;
        public TextMeshProUGUI SubjectText;
        public TextMeshProUGUI PreviewText;
        public bool IsHovered;
    }

    //stores folder row UI
    private class FolderRowView
    {
        public Image Background;
        public Button Button;
        public TextMeshProUGUI LabelText;
    }
}


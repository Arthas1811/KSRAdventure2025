using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Hosts the Unity-side PDF preview modal.
public sealed class MailboxPdfPreviewController : IDisposable
{
    private static readonly Color ModalBackdropColor = new Color(0.012f, 0.018f, 0.026f, 0.9f);
    private static readonly Color ModalColor = new Color(0.965f, 0.975f, 0.988f, 1f);
    private static readonly Color HeaderColor = new Color(0.035f, 0.24f, 0.46f, 1f);
    private static readonly Color PreviewBackColor = new Color(0.84f, 0.87f, 0.91f, 1f);
    private static readonly Color PreviewPageBackColor = new Color(1f, 1f, 1f, 1f);
    private static readonly Color PrimaryTextColor = new Color(0.09f, 0.12f, 0.17f, 1f);
    private static readonly Color MutedTextColor = new Color(0.36f, 0.42f, 0.5f, 1f);
    private static readonly Color AccentColor = new Color(0.0f, 0.36f, 0.72f, 1f);
    private static readonly Color AccentHoverColor = new Color(0.02f, 0.44f, 0.84f, 1f);

    private const float ScrollViewportWidth = 980f;
    private const float ScrollViewportHeight = 420f;
    private const float ZoomStep = 0.25f;

    private GameObject layerObject;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI pageText;
    private TextMeshProUGUI zoomText;
    private TextMeshProUGUI statusText;
    private Button previousButton;
    private Button nextButton;
    private Button zoomOutButton;
    private Button zoomInButton;
    private ScrollRect previewScrollRect;
    private RectTransform previewContent;
    private RectTransform previewPageRect;
    private RawImage previewImage;
    private MailboxPdfPreviewDocument document;
    private Texture2D pageTexture;
    private int pageIndex;
    private float zoom = 1f;

    public MailboxPdfPreviewController(RectTransform canvasRect)
    {
        CreateLayer(canvasRect);
    }

    public void Open(string pdfResourcePath, string attachmentName)
    {
        string title = string.IsNullOrWhiteSpace(attachmentName) ? "PDF attachment" : attachmentName;
        TextAsset pdfAsset = Resources.Load<TextAsset>(pdfResourcePath);
        if (pdfAsset == null)
        {
            ShowLayer(title);
            ShowError($"Could not load PDF attachment at Resources/{pdfResourcePath}. Use a .bytes file in Resources for PDFs.");
            return;
        }

        if (MailboxPdfWebGLPreview.Open(title, pdfAsset.bytes))
        {
            return;
        }

        ShowLayer(title);
        if (!MailboxPdfPreviewDocument.IsSupported)
        {
            ShowError("PDF preview is only available in Windows x64 and WebGL builds.");
            return;
        }

        if (!MailboxPdfPreviewDocument.TryLoad(pdfAsset.bytes, out document, out string error))
        {
            ShowError(error);
            return;
        }

        pageIndex = 0;
        zoom = 1f;
        RenderCurrentPage();
    }

    public void Dispose()
    {
        CloseDocument();
        if (layerObject != null)
        {
            UnityEngine.Object.Destroy(layerObject);
            layerObject = null;
        }
    }

    private void CreateLayer(RectTransform canvasRect)
    {
        RectTransform layer = HandyUIFactory.CreateUIObject("PdfPreviewLayer", canvasRect);
        HandyUIFactory.StretchToParent(layer);
        layerObject = layer.gameObject;

        RectTransform dimmer = HandyUIFactory.CreateStretchPanel("PdfPreviewBackdrop", layer, ModalBackdropColor);
        dimmer.GetComponent<Image>().raycastTarget = true;

        RectTransform modal = CreatePanel("PdfPreviewModal", layer, ModalColor, Vector2.zero, new Vector2(1120f, 650f));
        Mask modalMask = modal.gameObject.AddComponent<Mask>();
        modalMask.showMaskGraphic = true;

        RectTransform header = CreateSolidPanel("PdfPreviewHeader", modal, HeaderColor, new Vector2(0f, 293f), new Vector2(1124f, 66f));
        titleText = CreateText(
            "PdfPreviewTitle",
            header,
            "PDF preview",
            24f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            Color.white,
            Vector2.zero,
            new Vector2(900f, 38f));
        ConfigurePreviewTitle(titleText.rectTransform);
        titleText.textWrappingMode = TextWrappingModes.NoWrap;
        titleText.overflowMode = TextOverflowModes.Ellipsis;

        RectTransform closeButton = CreateButton(
            "PdfPreviewCloseButton",
            header,
            "X",
            18f,
            Vector2.zero,
            new Vector2(42f, 42f),
            new Color(1f, 1f, 1f, 0.18f),
            ClosePreview);
        AnchorToHeaderRight(closeButton);

        RectTransform scrollRoot = CreatePanel("PdfPreviewScroll", modal, PreviewBackColor, new Vector2(0f, 6f), new Vector2(1010f, 450f));
        previewScrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
        previewScrollRect.horizontal = true;
        previewScrollRect.vertical = true;
        previewScrollRect.movementType = ScrollRect.MovementType.Clamped;
        previewScrollRect.scrollSensitivity = 30f;

        RectTransform viewport = CreatePanel("Viewport", scrollRoot, new Color(0f, 0f, 0f, 0f), Vector2.zero, Vector2.zero);
        HandyUIFactory.StretchToParent(viewport);
        viewport.offsetMin = new Vector2(15f, 15f);
        viewport.offsetMax = new Vector2(-15f, -15f);
        viewport.gameObject.AddComponent<RectMask2D>();

        previewContent = HandyUIFactory.CreateUIObject("Content", viewport);
        previewContent.anchorMin = new Vector2(0.5f, 1f);
        previewContent.anchorMax = new Vector2(0.5f, 1f);
        previewContent.pivot = new Vector2(0.5f, 1f);
        previewContent.anchoredPosition = Vector2.zero;
        previewContent.sizeDelta = new Vector2(ScrollViewportWidth, ScrollViewportHeight);

        previewPageRect = CreatePanel("PdfPageBack", previewContent, PreviewPageBackColor, new Vector2(0f, -20f), new Vector2(360f, 500f));
        previewPageRect.anchorMin = new Vector2(0.5f, 1f);
        previewPageRect.anchorMax = new Vector2(0.5f, 1f);
        previewPageRect.pivot = new Vector2(0.5f, 1f);
        Image pageBackImage = previewPageRect.GetComponent<Image>();
        pageBackImage.raycastTarget = false;

        RectTransform imageRect = HandyUIFactory.CreateUIObject("PdfPageImage", previewPageRect);
        HandyUIFactory.StretchToParent(imageRect);
        previewImage = imageRect.gameObject.AddComponent<RawImage>();
        previewImage.color = Color.white;
        previewImage.raycastTarget = false;
        previewImage.enabled = false;

        previewScrollRect.viewport = viewport;
        previewScrollRect.content = previewContent;

        statusText = CreateText(
            "PdfPreviewStatus",
            scrollRoot,
            string.Empty,
            22f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            MutedTextColor,
            Vector2.zero,
            new Vector2(800f, 120f));

        CreateFooterControls(modal);
        layerObject.SetActive(false);
    }

    private void CreateFooterControls(RectTransform modal)
    {
        previousButton = CreateButton(
            "PdfPreviousButton",
            modal,
            "Previous",
            17f,
            new Vector2(-384f, -292f),
            new Vector2(112f, 42f),
            AccentColor,
            PreviousPage).GetComponent<Button>();

        nextButton = CreateButton(
            "PdfNextButton",
            modal,
            "Next",
            17f,
            new Vector2(-258f, -292f),
            new Vector2(96f, 42f),
            AccentColor,
            NextPage).GetComponent<Button>();

        pageText = CreateText(
            "PdfPageText",
            modal,
            "Page - / -",
            18f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            PrimaryTextColor,
            new Vector2(0f, -292f),
            new Vector2(220f, 36f));

        zoomOutButton = CreateButton(
            "PdfZoomOutButton",
            modal,
            "-",
            20f,
            new Vector2(282f, -292f),
            new Vector2(44f, 42f),
            AccentColor,
            ZoomOut).GetComponent<Button>();

        zoomText = CreateText(
            "PdfZoomText",
            modal,
            "100%",
            18f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            PrimaryTextColor,
            new Vector2(350f, -292f),
            new Vector2(80f, 36f));

        zoomInButton = CreateButton(
            "PdfZoomInButton",
            modal,
            "+",
            20f,
            new Vector2(418f, -292f),
            new Vector2(44f, 42f),
            AccentColor,
            ZoomIn).GetComponent<Button>();
    }

    private static void ConfigurePreviewTitle(RectTransform titleRect)
    {
        titleRect.anchorMin = new Vector2(0f, 0.5f);
        titleRect.anchorMax = new Vector2(1f, 0.5f);
        titleRect.pivot = new Vector2(0f, 0.5f);
        titleRect.offsetMin = new Vector2(40f, -19f);
        titleRect.offsetMax = new Vector2(-96f, 19f);
    }

    private static void AnchorToHeaderRight(RectTransform rect)
    {
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-24f, 0f);
    }

    private void ShowLayer(string title)
    {
        CloseDocument();
        titleText.text = title;
        pageIndex = 0;
        zoom = 1f;
        ClearPageTexture();
        SetStatus("Loading PDF...");
        UpdateControls(false);
        layerObject.SetActive(true);
        layerObject.transform.SetAsLastSibling();
    }

    private void ClosePreview()
    {
        CloseDocument();
        if (layerObject != null)
        {
            layerObject.SetActive(false);
        }
    }

    private void CloseDocument()
    {
        ClearPageTexture();
        if (document != null)
        {
            document.Dispose();
            document = null;
        }
    }

    private void PreviousPage()
    {
        if (document == null || pageIndex <= 0)
        {
            return;
        }

        pageIndex--;
        RenderCurrentPage();
    }

    private void NextPage()
    {
        if (document == null || pageIndex >= document.PageCount - 1)
        {
            return;
        }

        pageIndex++;
        RenderCurrentPage();
    }

    private void ZoomOut()
    {
        if (document == null)
        {
            return;
        }

        zoom = Mathf.Max(MailboxPdfPreviewDocument.MinZoom, zoom - ZoomStep);
        RenderCurrentPage();
    }

    private void ZoomIn()
    {
        if (document == null)
        {
            return;
        }

        zoom = Mathf.Min(MailboxPdfPreviewDocument.MaxZoom, zoom + ZoomStep);
        RenderCurrentPage();
    }

    private void RenderCurrentPage()
    {
        if (document == null)
        {
            UpdateControls(false);
            return;
        }

        ClearPageTexture();
        SetStatus("Rendering page...");
        UpdateControls(false);

        if (!document.TryRenderPage(pageIndex, zoom, out pageTexture, out string error))
        {
            ShowError(error);
            return;
        }

        previewImage.texture = pageTexture;
        previewImage.enabled = true;
        ResizePreviewImage(pageTexture.width, pageTexture.height);
        SetStatus(string.Empty);
        UpdateControls(true);

        Canvas.ForceUpdateCanvases();
        previewScrollRect.verticalNormalizedPosition = 1f;
        previewScrollRect.horizontalNormalizedPosition = 0.5f;
    }

    private void ResizePreviewImage(float width, float height)
    {
        float contentWidth = Mathf.Max(ScrollViewportWidth, width + 40f);
        float contentHeight = Mathf.Max(ScrollViewportHeight, height + 40f);
        previewContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
        previewContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);

        previewPageRect.anchoredPosition = new Vector2(0f, -20f);
        previewPageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        previewPageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    private void ShowError(string message)
    {
        ClearPageTexture();
        SetStatus(string.IsNullOrWhiteSpace(message) ? "Could not preview the PDF." : message);
        UpdateControls(false);
    }

    private void SetStatus(string message)
    {
        statusText.text = message ?? string.Empty;
        statusText.gameObject.SetActive(!string.IsNullOrWhiteSpace(statusText.text));
    }

    private void ClearPageTexture()
    {
        if (pageTexture != null)
        {
            UnityEngine.Object.Destroy(pageTexture);
            pageTexture = null;
        }

        if (previewImage != null)
        {
            previewImage.texture = null;
            previewImage.enabled = false;
        }
    }

    private void UpdateControls(bool hasPage)
    {
        int pageCount = document != null ? document.PageCount : 0;
        pageText.text = hasPage ? $"Page {pageIndex + 1} / {pageCount}" : "Page - / -";
        zoomText.text = $"{Mathf.RoundToInt(zoom * 100f)}%";

        previousButton.interactable = hasPage && pageIndex > 0;
        nextButton.interactable = hasPage && pageIndex < pageCount - 1;
        zoomOutButton.interactable = hasPage && zoom > MailboxPdfPreviewDocument.MinZoom + 0.01f;
        zoomInButton.interactable = hasPage && zoom < MailboxPdfPreviewDocument.MaxZoom - 0.01f;
    }

    private RectTransform CreateButton(
        string objectName,
        RectTransform parent,
        string text,
        float fontSize,
        Vector2 anchoredPosition,
        Vector2 size,
        Color normalColor,
        UnityEngine.Events.UnityAction onClick)
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
            null,
            fontSize,
            Color.white,
            new Vector2(size.x - 16f, size.y - 10f),
            10f);
    }

    private ColorBlock CreateButtonColors(Color normalColor)
    {
        return HandyUIFactory.CreateButtonColors(
            normalColor,
            Color.Lerp(normalColor, AccentHoverColor, 0.18f),
            Color.Lerp(normalColor, Color.black, 0.12f),
            normalColor,
            Color.Lerp(normalColor, Color.gray, 0.4f));
    }

    private RectTransform CreatePanel(string objectName, RectTransform parent, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        return HandyUIFactory.CreateSlicedPanel(objectName, parent, color, anchoredPosition, size, HandyTextureProvider.RoundedPanelMask);
    }

    private RectTransform CreateSolidPanel(string objectName, RectTransform parent, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        return HandyUIFactory.CreatePanel(objectName, parent, color, anchoredPosition, size);
    }

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
}

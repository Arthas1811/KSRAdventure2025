using TMPro;
using UnityEngine;
using UnityEngine.UI;

//runs block blast game
[DisallowMultipleComponent]
public class BlockBlastGame : MonoBehaviour
{
    private const float BoardCellSize = 54f;
    private const float BoardCellSpacing = 5f;
    private const float ShapeBlockSize = BoardCellSize;
    private const float ShapeBlockSpacing = BoardCellSpacing;
    private static readonly Color EmptyCellColor = new Color(0.18f, 0.22f, 0.31f, 1f);
    private static readonly Color BoardBackColor = new Color(0.09f, 0.12f, 0.18f, 1f);
    private static readonly Color ValidPlacementMarkColor = new Color(0.82f, 1f, 0.76f, 0.76f);
    private static readonly Color InvalidPlacementMarkColor = new Color(0.62f, 0.04f, 0.04f, 0.96f);
    private static readonly Color LineClearMarkColor = new Color(1f, 0.18f, 0.75f, 1f);

    private readonly BlockBlastCell[,] visualCells = new BlockBlastCell[BlockBlastBoard.Size, BlockBlastBoard.Size];

    private BlockBlastBoard board;
    private BlockBlastTray tray;
    private ScoreSaveState scoreSaveState;
    private PhoneSceneNavigation navigation;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI highScoreText;
    private TextMeshProUGUI finalScoreText;
    private TextMeshProUGUI finalHighScoreText;
    private GameObject gameOverPanel;
    private RectTransform boardRect;
    private RectTransform[] traySlotRects;
    private int score;
    private int highScore;

    public bool IsGameOver { get; private set; }

    public RectTransform DragLayer { get; private set; }

    private float BoardGridSize => BlockBlastBoard.Size * BoardCellSize + (BlockBlastBoard.Size - 1) * BoardCellSpacing;
    private float BoardCellStride => BoardCellSize + BoardCellSpacing;
    private Sprite MarkingPreviewSprite => HandyTextureProvider.GetBlockBlastSprite(HandyBlockTextureKey.Red);

    //starts game screen
    private void Start()
    {
        navigation = HandySceneServices.EnsureNavigation(this);

        HandySceneServices.EnsureEventSystem();

        board = new BlockBlastBoard();
        scoreSaveState = new ScoreSaveState(HandySceneServices.EnsureSaveDataManager());
        highScore = scoreSaveState.LoadBlockBlastHighScore();

        BuildInterface();
        StartNewGame();
    }

    //creates dragged shape
    public DraggableBlockShape CreateDraggableShape(BlockBlastShape shape, RectTransform parentSlot)
    {
        float width = shape.Width * ShapeBlockSize + (shape.Width - 1) * ShapeBlockSpacing;
        float height = shape.Height * ShapeBlockSize + (shape.Height - 1) * ShapeBlockSpacing;

        RectTransform shapeRoot = HandyUIFactory.CreateUIObject(shape.ShapeName, parentSlot);
        HandyUIFactory.SetCentered(shapeRoot, Vector2.zero, new Vector2(width, height));

        Image hitArea = shapeRoot.gameObject.AddComponent<Image>();
        hitArea.color = new Color(1f, 1f, 1f, 0.001f);
        hitArea.raycastTarget = true;

        shapeRoot.gameObject.AddComponent<CanvasGroup>();
        Sprite blockSprite = HandyTextureProvider.GetBlockBlastSprite(shape.TextureKey);

        foreach (Vector2Int cell in shape.Cells)
        {
            Color blockColor = blockSprite != null ? Color.white : shape.Color;
            RectTransform block = HandyUIFactory.CreatePanel($"{shape.ShapeName}Block", shapeRoot, blockColor, Vector2.zero, new Vector2(ShapeBlockSize, ShapeBlockSize));
            block.anchorMin = new Vector2(0f, 1f);
            block.anchorMax = new Vector2(0f, 1f);
            block.pivot = new Vector2(0f, 1f);
            block.anchoredPosition = new Vector2(cell.x * (ShapeBlockSize + ShapeBlockSpacing), -cell.y * (ShapeBlockSize + ShapeBlockSpacing));

            Image blockImage = block.GetComponent<Image>();
            HandyTextureProvider.ApplyTintedSprite(blockImage, blockSprite, blockColor, true);
            blockImage.raycastTarget = false;
        }

        DraggableBlockShape draggableShape = shapeRoot.gameObject.AddComponent<DraggableBlockShape>();
        draggableShape.Initialize(this, shape, parentSlot);
        return draggableShape;
    }

    //updates place preview
    public void UpdatePlacementPreview(DraggableBlockShape draggableShape)
    {
        ClearPlacementPreview();

        if (!TryGetBoardOrigin(draggableShape, out Vector2Int origin))
        {
            return;
        }

        bool canPlace = board.CanPlace(draggableShape.Shape, origin);
        Color previewColor = canPlace ? ValidPlacementMarkColor : InvalidPlacementMarkColor;
        Sprite previewSprite = canPlace
            ? HandyTextureProvider.GetBlockBlastSprite(draggableShape.Shape.TextureKey)
            : MarkingPreviewSprite;

        foreach (Vector2Int shapeCell in draggableShape.Shape.Cells)
        {
            int boardX = origin.x + shapeCell.x;
            int boardY = origin.y + shapeCell.y;

            if (board.IsInside(boardX, boardY))
            {
                visualCells[boardX, boardY].SetPreview(previewColor, previewSprite);
            }
        }

        if (canPlace)
        {
            HighlightLinesClearedByPlacement(draggableShape.Shape, origin);
        }
    }

    //clears place preview
    public void ClearPlacementPreview()
    {
        for (int y = 0; y < BlockBlastBoard.Size; y++)
        {
            for (int x = 0; x < BlockBlastBoard.Size; x++)
            {
                visualCells[x, y]?.ClearPreview();
            }
        }
    }

    //marks cleared lines
    private void HighlightLinesClearedByPlacement(BlockBlastShape shape, Vector2Int origin)
    {
        for (int y = 0; y < BlockBlastBoard.Size; y++)
        {
            if (WouldRowBeFullAfterPlacement(y, shape, origin))
            {
                for (int x = 0; x < BlockBlastBoard.Size; x++)
                {
                    visualCells[x, y]?.SetPreview(LineClearMarkColor, MarkingPreviewSprite);
                }
            }
        }

        for (int x = 0; x < BlockBlastBoard.Size; x++)
        {
            if (WouldColumnBeFullAfterPlacement(x, shape, origin))
            {
                for (int y = 0; y < BlockBlastBoard.Size; y++)
                {
                    visualCells[x, y]?.SetPreview(LineClearMarkColor, MarkingPreviewSprite);
                }
            }
        }
    }

    //checks row clear preview
    private bool WouldRowBeFullAfterPlacement(int row, BlockBlastShape shape, Vector2Int origin)
    {
        for (int x = 0; x < BlockBlastBoard.Size; x++)
        {
            if (!WouldCellBeOccupiedAfterPlacement(x, row, shape, origin))
            {
                return false;
            }
        }

        return true;
    }

    //checks column clear preview
    private bool WouldColumnBeFullAfterPlacement(int column, BlockBlastShape shape, Vector2Int origin)
    {
        for (int y = 0; y < BlockBlastBoard.Size; y++)
        {
            if (!WouldCellBeOccupiedAfterPlacement(column, y, shape, origin))
            {
                return false;
            }
        }

        return true;
    }

    //checks cell place preview
    private bool WouldCellBeOccupiedAfterPlacement(int boardX, int boardY, BlockBlastShape shape, Vector2Int origin)
    {
        if (board.IsOccupied(boardX, boardY))
        {
            return true;
        }

        foreach (Vector2Int shapeCell in shape.Cells)
        {
            if (origin.x + shapeCell.x == boardX && origin.y + shapeCell.y == boardY)
            {
                return true;
            }
        }

        return false;
    }

    //tries dragged shape place
    public bool TryPlaceDraggedShape(DraggableBlockShape draggableShape)
    {
        if (IsGameOver || !TryGetBoardOrigin(draggableShape, out Vector2Int origin) || !board.CanPlace(draggableShape.Shape, origin))
        {
            return false;
        }

        BlockBlastPlacementResult placementResult = board.PlaceShape(draggableShape.Shape, origin);
        score += CalculateScoreForClear(placementResult.ClearedBlocks, placementResult.ClearedLines, placementResult.PerfectClear);
        UpdateBoardVisuals();
        UpdateScoreText();

        tray.MarkShapeUsed(draggableShape);
        CheckForGameOver();
        return true;
    }

    //keeps drag inside bounds
    public Vector2 ClampDragPosition(RectTransform draggedRect, Vector2 desiredPosition)
    {
        if (draggedRect == null || DragLayer == null)
        {
            return desiredPosition;
        }

        Vector2 travelPosition = ClampPointInside(desiredPosition, GetContinuousDragBounds());
        return ClampRectInside(travelPosition, draggedRect, DragLayer.rect);
    }

    //counts clear score
    public static int CalculateScoreForClear(int clearedBlocks, int clearedLines, bool perfectClear)
    {
        int scoreForMove = clearedBlocks * 10 + GetLineClearBonus(clearedLines);

        if (perfectClear && clearedBlocks > 0)
        {
            scoreForMove += 75;
        }

        return scoreForMove;
    }

    //gets line clear bonus
    public static int GetLineClearBonus(int clearedLines)
    {
        switch (clearedLines)
        {
            case 0:
            case 1:
                return 0;
            case 2:
                return 10;
            case 3:
                return 30;
            case 4:
                return 50;
            case 5:
                return 70;
            default:
                return 90;
        }
    }

    //builds UI
    private void BuildInterface()
    {
        RectTransform canvasRect = HandyUIFactory.CreateOverlayCanvas(transform, "GeneratedBlockBlastApp", 25);

        Color backdropFallbackColor = new Color(0.055f, 0.075f, 0.105f, 1f);
        RectTransform backdrop = HandyUIFactory.CreateStretchPanel("Backdrop", canvasRect, backdropFallbackColor);
        HandyUIFactory.ConfigureBackdrop(backdrop.GetComponent<Image>(), backdropFallbackColor);
        if (HandyTextureProvider.Background == null && !PhoneSceneNavigation.IsPhoneOverlayOpen)
        {
            HandyUIFactory.CreateAccentBand("LeftAccentBand", canvasRect, new Color(0.02f, 0.17f, 0.2f, 0.58f), new Vector2(-650f, 0f), new Vector2(390f, 1100f), -7f);
            HandyUIFactory.CreateAccentBand("RightAccentBand", canvasRect, new Color(0.24f, 0.09f, 0.05f, 0.52f), new Vector2(640f, 0f), new Vector2(350f, 1100f), 8f);
        }

        Color phoneFrameFallbackColor = new Color(0.014f, 0.017f, 0.024f, 1f);
        RectTransform phoneFrame = HandyUIFactory.CreatePanel("PhoneFrame", canvasRect, phoneFrameFallbackColor, Vector2.zero, new Vector2(560f, 1040f));
        Image phoneFrameImage = phoneFrame.GetComponent<Image>();
        if (HandyTextureProvider.PhoneFrame != null)
        {
            phoneFrameImage.color = Color.clear;
            phoneFrameImage.raycastTarget = false;
        }

        Color phoneScreenFallbackColor = new Color(0.085f, 0.105f, 0.145f, 1f);
        RectTransform phoneScreen = HandyUIFactory.CreatePanel("PhoneScreen", phoneFrame, Color.white, new Vector2(0f, -4f), new Vector2(512f, 1000f));
        HandyUIFactory.ConfigureRoundedScreen(phoneScreen, phoneScreenFallbackColor);
        RectTransform contentRoot = HandyUIFactory.CreatePanel("PhoneContent", phoneScreen, Color.clear, Vector2.zero, new Vector2(470f, 920f));
        contentRoot.localScale = new Vector3(0.94f, 0.94f, 1f);
        contentRoot.GetComponent<Image>().raycastTarget = false;

        CreateHeader(contentRoot);
        CreateBoard(contentRoot);
        CreateTray(contentRoot);
        CreateBottomButtons(contentRoot);

        DragLayer = HandyUIFactory.CreateUIObject("DragLayer", contentRoot);
        HandyUIFactory.StretchToParent(DragLayer);
        DragLayer.SetAsLastSibling();

        CreateGameOverPanel(contentRoot);

        if (HandyTextureProvider.PhoneFrame != null)
        {
            HandyUIFactory.CreatePhoneFrameOverlay(canvasRect, phoneFrameFallbackColor);
        }
    }

    //creates header UI
    private void CreateHeader(RectTransform screen)
    {
        CreateText("TitleText", screen, "Block Blast", new Vector2(0f, 410f), new Vector2(280f, 48f), 38f, FontStyles.Bold, TextAlignmentOptions.Center);
        scoreText = CreateText("ScoreText", screen, "Score: 0", new Vector2(-120f, 356f), new Vector2(200f, 38f), 25f, FontStyles.Bold, TextAlignmentOptions.Left);
        highScoreText = CreateText("HighScoreText", screen, "Best: 0", new Vector2(128f, 356f), new Vector2(200f, 38f), 25f, FontStyles.Bold, TextAlignmentOptions.Right);
    }

    //creates game board
    private void CreateBoard(RectTransform screen)
    {
        RectTransform boardBack = HandyUIFactory.CreatePanel("BlockBlastBoard", screen, BoardBackColor, new Vector2(0f, 76f), new Vector2(BoardGridSize + 28f, BoardGridSize + 28f));
        boardRect = HandyUIFactory.CreateUIObject("BoardGrid", boardBack);
        HandyUIFactory.SetCentered(boardRect, Vector2.zero, new Vector2(BoardGridSize, BoardGridSize));

        for (int y = 0; y < BlockBlastBoard.Size; y++)
        {
            for (int x = 0; x < BlockBlastBoard.Size; x++)
            {
                RectTransform cellRect = HandyUIFactory.CreatePanel($"Cell_{x}_{y}", boardRect, EmptyCellColor, GetBoardCellPosition(x, y), new Vector2(BoardCellSize, BoardCellSize));
                BlockBlastCell cell = cellRect.gameObject.AddComponent<BlockBlastCell>();
                cell.Initialize(cellRect.GetComponent<Image>(), EmptyCellColor);
                visualCells[x, y] = cell;
            }
        }
    }

    //creates shape tray
    private void CreateTray(RectTransform screen)
    {
        RectTransform trayBack = HandyUIFactory.CreatePanel("ShapeTray", screen, new Color(0.07f, 0.09f, 0.13f, 1f), new Vector2(0f, -310f), new Vector2(486f, 150f));
        RectTransform[] traySlots = new RectTransform[3];

        for (int i = 0; i < traySlots.Length; i++)
        {
            traySlots[i] = HandyUIFactory.CreatePanel($"ShapeSlot_{i + 1}", trayBack, new Color(0.13f, 0.16f, 0.22f, 1f), new Vector2(-154f + i * 154f, 0f), new Vector2(132f, 118f));
        }

        traySlotRects = traySlots;
        tray = trayBack.gameObject.AddComponent<BlockBlastTray>();
        tray.Initialize(this, traySlots);
    }

    //creates bottom buttons
    private void CreateBottomButtons(RectTransform screen)
    {
        CreateButton("HomeButton", screen, string.Empty, new Vector2(-124f, -426f), new Vector2(48f, 48f), new Color(0.19f, 0.45f, 0.58f, 1f), navigation.LoadPhoneHome, HandyTextureProvider.HomeIcon);
        CreateButton("RetryButton", screen, "Retry", new Vector2(124f, -426f), new Vector2(170f, 48f), new Color(0.54f, 0.31f, 0.16f, 1f), StartNewGame);
    }

    //creates game over panel
    private void CreateGameOverPanel(RectTransform screen)
    {
        RectTransform panel = HandyUIFactory.CreatePanel("GameOverPanel", screen, new Color(0.035f, 0.045f, 0.065f, 0.96f), Vector2.zero, new Vector2(470f, 390f));
        gameOverPanel = panel.gameObject;

        CreateText("GameOverTitle", panel, "Game Over", new Vector2(0f, 128f), new Vector2(330f, 56f), 42f, FontStyles.Bold, TextAlignmentOptions.Center);
        finalScoreText = CreateText("FinalScoreText", panel, "Score: 0", new Vector2(0f, 58f), new Vector2(320f, 40f), 28f, FontStyles.Bold, TextAlignmentOptions.Center);
        finalHighScoreText = CreateText("FinalHighScoreText", panel, "Best: 0", new Vector2(0f, 12f), new Vector2(320f, 38f), 25f, FontStyles.Bold, TextAlignmentOptions.Center);
        CreateButton("GameOverRetryButton", panel, "Retry", new Vector2(-105f, -98f), new Vector2(160f, 54f), new Color(0.54f, 0.31f, 0.16f, 1f), StartNewGame);
        CreateButton("GameOverHomeButton", panel, string.Empty, new Vector2(105f, -98f), new Vector2(54f, 54f), new Color(0.19f, 0.45f, 0.58f, 1f), navigation.LoadPhoneHome, HandyTextureProvider.HomeIcon);
        gameOverPanel.SetActive(false);
    }

    //starts new game
    private void StartNewGame()
    {
        IsGameOver = false;
        score = 0;
        board.Clear();
        UpdateBoardVisuals();
        UpdateScoreText();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        tray.GenerateNewTray();
        CheckForGameOver();
    }

    //checks game over
    private void CheckForGameOver()
    {
        tray.RefreshShapeAvailability(board);

        if (!tray.HasAnyShapeThatFits(board))
        {
            EndGame();
        }
    }

    //ends game
    private void EndGame()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;

        if (score > highScore)
        {
            highScore = score;
            scoreSaveState.SaveBlockBlastHighScore(highScore);
        }

        UpdateScoreText();
        finalScoreText.text = $"Score: {score}";
        finalHighScoreText.text = $"Best: {highScore}";
        gameOverPanel.SetActive(true);
        gameOverPanel.transform.SetAsLastSibling();
    }

    //updates board look
    private void UpdateBoardVisuals()
    {
        for (int y = 0; y < BlockBlastBoard.Size; y++)
        {
            for (int x = 0; x < BlockBlastBoard.Size; x++)
            {
                Sprite blockSprite = HandyTextureProvider.GetBlockBlastSprite(board.GetCellTextureKey(x, y));
                visualCells[x, y].SetState(board.IsOccupied(x, y), board.GetCellColor(x, y), blockSprite);
            }
        }
    }

    //updates score text
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }

        if (highScoreText != null)
        {
            highScoreText.text = $"Best: {highScore}";
        }
    }

    //gets board place origin
    private bool TryGetBoardOrigin(DraggableBlockShape draggableShape, out Vector2Int origin)
    {
        origin = Vector2Int.zero;

        if (draggableShape == null || draggableShape.ShapeRect == null || boardRect == null)
        {
            return false;
        }

        Vector3[] corners = new Vector3[4];
        draggableShape.ShapeRect.GetWorldCorners(corners);
        Vector2 topLeftScreenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[1]);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(boardRect, topLeftScreenPoint, null, out Vector2 boardLocalPoint))
        {
            return false;
        }

        float left = -BoardGridSize * 0.5f;
        float top = BoardGridSize * 0.5f;
        int x = Mathf.RoundToInt((boardLocalPoint.x - left) / BoardCellStride);
        int y = Mathf.RoundToInt((top - boardLocalPoint.y) / BoardCellStride);
        origin = new Vector2Int(x, y);
        return true;
    }

    //gets board cell position
    private Vector2 GetBoardCellPosition(int x, int y)
    {
        float left = -BoardGridSize * 0.5f + BoardCellSize * 0.5f;
        float top = BoardGridSize * 0.5f - BoardCellSize * 0.5f;
        return new Vector2(left + x * BoardCellStride, top - y * BoardCellStride);
    }

    //gets drag bounds
    private Rect GetContinuousDragBounds()
    {
        Rect combinedBounds = DragLayer.rect;
        bool hasBounds = false;

        IncludeDragBounds(boardRect, ref combinedBounds, ref hasBounds);

        if (traySlotRects != null)
        {
            foreach (RectTransform traySlotRect in traySlotRects)
            {
                IncludeDragBounds(traySlotRect, ref combinedBounds, ref hasBounds);
            }
        }

        return hasBounds ? combinedBounds : DragLayer.rect;
    }

    //adds drag bounds
    private void IncludeDragBounds(RectTransform dragZone, ref Rect combinedBounds, ref bool hasBounds)
    {
        if (dragZone == null || !dragZone.gameObject.activeInHierarchy)
        {
            return;
        }

        Rect zoneRect = GetRectInDragLayer(dragZone);
        if (!hasBounds)
        {
            combinedBounds = zoneRect;
            hasBounds = true;
            return;
        }

        combinedBounds = Rect.MinMaxRect(
            Mathf.Min(combinedBounds.xMin, zoneRect.xMin),
            Mathf.Min(combinedBounds.yMin, zoneRect.yMin),
            Mathf.Max(combinedBounds.xMax, zoneRect.xMax),
            Mathf.Max(combinedBounds.yMax, zoneRect.yMax));
    }

    //gets drag layer rect
    private Rect GetRectInDragLayer(RectTransform sourceRect)
    {
        Vector3[] worldCorners = new Vector3[4];
        sourceRect.GetWorldCorners(worldCorners);

        Vector2 firstPoint = DragLayer.InverseTransformPoint(worldCorners[0]);
        float minX = firstPoint.x;
        float maxX = firstPoint.x;
        float minY = firstPoint.y;
        float maxY = firstPoint.y;

        for (int i = 1; i < worldCorners.Length; i++)
        {
            Vector2 localPoint = DragLayer.InverseTransformPoint(worldCorners[i]);
            minX = Mathf.Min(minX, localPoint.x);
            maxX = Mathf.Max(maxX, localPoint.x);
            minY = Mathf.Min(minY, localPoint.y);
            maxY = Mathf.Max(maxY, localPoint.y);
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    //keeps rect inside bounds
    private static Vector2 ClampRectInside(Vector2 desiredPosition, RectTransform draggedRect, Rect bounds)
    {
        Vector2 shapeSize = Vector2.Scale(draggedRect.rect.size, draggedRect.localScale);
        Vector2 pivot = draggedRect.pivot;

        float minX = bounds.xMin + shapeSize.x * pivot.x;
        float maxX = bounds.xMax - shapeSize.x * (1f - pivot.x);
        float minY = bounds.yMin + shapeSize.y * pivot.y;
        float maxY = bounds.yMax - shapeSize.y * (1f - pivot.y);

        if (minX > maxX)
        {
            minX = bounds.center.x;
            maxX = bounds.center.x;
        }

        if (minY > maxY)
        {
            minY = bounds.center.y;
            maxY = bounds.center.y;
        }

        return new Vector2(
            Mathf.Clamp(desiredPosition.x, minX, maxX),
            Mathf.Clamp(desiredPosition.y, minY, maxY));
    }

    //keeps point inside bounds
    private static Vector2 ClampPointInside(Vector2 desiredPosition, Rect bounds)
    {
        return new Vector2(
            Mathf.Clamp(desiredPosition.x, bounds.xMin, bounds.xMax),
            Mathf.Clamp(desiredPosition.y, bounds.yMin, bounds.yMax));
    }

    //creates UI button
    private RectTransform CreateButton(string objectName, Transform parent, string label, Vector2 anchoredPosition, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick, Sprite iconSprite = null)
    {
        return HandyUIFactory.CreateButton(
            objectName,
            parent,
            label,
            anchoredPosition,
            size,
            color,
            onClick,
            CreateButtonColors(color),
            iconSprite,
            23f,
            Color.white,
            size,
            iconSprite != null ? 0f : 12f);
    }

    //creates button colors
    private ColorBlock CreateButtonColors(Color normalColor)
    {
        Color highlightedColor = Color.Lerp(normalColor, Color.white, 0.18f);
        return HandyUIFactory.CreateButtonColors(
            normalColor,
            highlightedColor,
            Color.Lerp(normalColor, Color.black, 0.18f),
            highlightedColor,
            new Color(normalColor.r, normalColor.g, normalColor.b, 0.4f),
            0.08f);
    }

    //creates UI text
    private TextMeshProUGUI CreateText(string objectName, Transform parent, string text, Vector2 anchoredPosition, Vector2 size, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
    {
        return HandyUIFactory.CreateText(objectName, parent, text, anchoredPosition, size, fontSize, fontStyle, alignment, Color.white, true, Mathf.Max(12f, fontSize * 0.55f));
    }

}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//runs tetris game
[DisallowMultipleComponent]
public class TetrisGame : MonoBehaviour
{
    private const float BoardCellSize = 24f;
    private const float BoardCellSpacing = 2f;
    private const float PreviewCellSize = 24f;
    private const float PreviewCellSpacing = 2f;
    private const float LineClearFlashSeconds = 0.08f;
    private const float LineCollapseSeconds = 0.18f;

    private static readonly Color EmptyCellColor = new Color(0.16f, 0.2f, 0.27f, 1f);
    private static readonly Color BoardBackColor = new Color(0.06f, 0.08f, 0.12f, 1f);
    private static readonly Color GhostColor = new Color(1f, 1f, 1f, 0.22f);
    private static readonly Color ClearFlashColor = new Color(1f, 0.96f, 0.62f, 1f);

    private readonly Image[,] visualCells = new Image[TetrisBoard.Width, TetrisBoard.VisibleHeight];
    private readonly Image[,] nextPreviewCells = new Image[4, 4];

    private TetrisBoard board;
    private TetrisBagRandomizer bag;
    private TetrisPiece activePiece;
    private TetrisTetromino nextType;
    private TetrisInput input;
    private ScoreSaveState scoreSaveState;
    private PhoneSceneNavigation navigation;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI highScoreText;
    private TextMeshProUGUI levelText;
    private TextMeshProUGUI linesText;
    private TextMeshProUGUI finalScoreText;
    private TextMeshProUGUI finalHighScoreText;
    private GameObject gameOverPanel;
    private RectTransform boardGridRect;
    private float gravityTimer;
    private int score;
    private int highScore;
    private int level;
    private int totalLines;
    private bool isLineClearAnimating;

    public bool IsGameOver { get; private set; }

    private float BoardGridWidth => TetrisBoard.Width * BoardCellSize + (TetrisBoard.Width - 1) * BoardCellSpacing;
    private float BoardGridHeight => TetrisBoard.VisibleHeight * BoardCellSize + (TetrisBoard.VisibleHeight - 1) * BoardCellSpacing;
    private float BoardCellStride => BoardCellSize + BoardCellSpacing;

    //starts game screen
    private void Start()
    {
        navigation = HandySceneServices.EnsureNavigation(this);

        input = GetComponent<TetrisInput>();
        if (input == null)
        {
            input = gameObject.AddComponent<TetrisInput>();
        }

        input.Initialize(this);
        HandySceneServices.EnsureEventSystem();

        board = new TetrisBoard();
        bag = new TetrisBagRandomizer();
        scoreSaveState = new ScoreSaveState(HandySceneServices.EnsureSaveDataManager());
        highScore = scoreSaveState.LoadTetrisHighScore();

        BuildInterface();
        StartNewGame();
    }

    //updates game loop
    private void Update()
    {
        if (IsGameOver || isLineClearAnimating || activePiece == null)
        {
            return;
        }

        gravityTimer += Time.deltaTime;
        float gravitySeconds = GetGravitySecondsPerCell(level);

        while (!IsGameOver && gravityTimer >= gravitySeconds)
        {
            gravityTimer -= gravitySeconds;
            StepGravity();
        }
    }

    //moves piece left
    public void MoveLeft()
    {
        TryMoveActivePiece(-1, 0);
    }

    //moves piece right
    public void MoveRight()
    {
        TryMoveActivePiece(1, 0);
    }

    //moves piece down
    public void SoftDropOneRow()
    {
        if (IsGameOver || isLineClearAnimating || activePiece == null)
        {
            return;
        }

        if (TryMoveActivePiece(0, 1))
        {
            score += 1;
            gravityTimer = 0f;
            UpdateScoreText();
        }
        else
        {
            LockActivePiece();
        }
    }

    //drops piece fast
    public void HardDrop()
    {
        if (IsGameOver || isLineClearAnimating || activePiece == null)
        {
            return;
        }

        Vector2Int landingPosition = TetrisGhostPiece.GetLandingPosition(board, activePiece);
        int droppedRows = Mathf.Max(0, landingPosition.y - activePiece.Position.y);
        activePiece = activePiece.WithPosition(landingPosition);
        score += CalculateHardDropScore(droppedRows);
        LockActivePiece();
    }

    //turns piece right
    public void RotateClockwise()
    {
        TryRotateActivePiece(1);
    }

    //turns piece left
    public void RotateCounterclockwise()
    {
        TryRotateActivePiece(-1);
    }

    //starts new game
    public void StartNewGame()
    {
        IsGameOver = false;
        score = 0;
        level = 0;
        totalLines = 0;
        gravityTimer = 0f;
        isLineClearAnimating = false;
        StopAllCoroutines();
        board.Clear();
        bag = new TetrisBagRandomizer();
        nextType = bag.Draw();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        SpawnNextPiece();
        UpdateAllVisuals();
    }

    //counts line clear score
    public static int CalculateLineClearScore(int clearedLines, int level, bool perfectClear)
    {
        if (clearedLines <= 0)
        {
            return 0;
        }

        int multiplier = level + 1;
        int clearScore;

        switch (clearedLines)
        {
            case 1:
                clearScore = 100;
                break;
            case 2:
                clearScore = 300;
                break;
            case 3:
                clearScore = 500;
                break;
            default:
                clearScore = 800;
                break;
        }

        if (perfectClear)
        {
            clearScore += GetPerfectClearBonus(clearedLines);
        }

        return clearScore * multiplier;
    }

    //counts soft drop score
    public static int CalculateSoftDropScore(int rows)
    {
        return Mathf.Max(0, rows);
    }

    //counts hard drop score
    public static int CalculateHardDropScore(int rows)
    {
        return Mathf.Max(0, rows) * 2;
    }

    //gets level line need
    public static int GetLinesNeededForNextLevel(int currentLevel)
    {
        return currentLevel * 10 + 10;
    }

    //gets level from lines
    public static int GetLevelForTotalLines(int lines)
    {
        int calculatedLevel = 0;

        while (lines >= GetLinesNeededForNextLevel(calculatedLevel))
        {
            calculatedLevel++;
        }

        return calculatedLevel;
    }

    //gets fall speed
    public static float GetGravitySecondsPerCell(int level)
    {
        if (level <= 0)
        {
            return 0.8f;
        }

        switch (level)
        {
            case 1:
                return 0.717f;
            case 2:
                return 0.633f;
            case 3:
                return 0.55f;
            case 4:
                return 0.467f;
            case 5:
                return 0.383f;
            case 6:
                return 0.3f;
            case 7:
                return 0.217f;
            case 8:
                return 0.133f;
            case 9:
                return 0.1f;
            default:
                if (level <= 12)
                {
                    return 0.083f;
                }

                if (level <= 15)
                {
                    return 0.067f;
                }

                if (level <= 18)
                {
                    return 0.05f;
                }

                if (level <= 28)
                {
                    return 0.033f;
                }

                return 0.017f;
        }
    }

    //steps fall timer
    private void StepGravity()
    {
        if (!TryMoveActivePiece(0, 1))
        {
            LockActivePiece();
        }
    }

    //tries move active piece
    private bool TryMoveActivePiece(int xOffset, int yOffset)
    {
        if (IsGameOver || activePiece == null)
        {
            return false;
        }

        TetrisPiece movedPiece = activePiece.Move(xOffset, yOffset);
        if (!board.CanPlace(movedPiece))
        {
            return false;
        }

        activePiece = movedPiece;
        UpdateBoardVisuals();
        return true;
    }

    //tries rotate active piece
    private void TryRotateActivePiece(int direction)
    {
        if (IsGameOver || activePiece == null)
        {
            return;
        }

        if (board.TryRotateWithSrs(activePiece, direction, out TetrisPiece rotatedPiece))
        {
            activePiece = rotatedPiece;
            UpdateBoardVisuals();
        }
    }

    //locks active piece
    private void LockActivePiece()
    {
        if (activePiece == null)
        {
            return;
        }

        TetrisPiece pieceToLock = activePiece;
        activePiece = null;

        if (!board.PlacePiece(pieceToLock))
        {
            EndGame();
            return;
        }

        int[] fullRows = board.GetFullRows();
        UpdateBoardVisuals();

        if (fullRows.Length > 0)
        {
            StartCoroutine(AnimateLineClearAndContinue(fullRows));
            return;
        }

        SpawnNextPiece();
        UpdateAllVisuals();
    }

    //plays line clear move
    private IEnumerator AnimateLineClearAndContinue(int[] clearedRows)
    {
        isLineClearAnimating = true;
        gravityTimer = 0f;

        HashSet<int> clearedRowSet = new HashSet<int>(clearedRows);
        List<RectTransform> movingBlocks = CreateMovingBlockOverlays(clearedRowSet, out List<Vector2> startPositions, out List<Vector2> targetPositions);

        for (int pulse = 0; pulse < 2; pulse++)
        {
            SetClearedRowsColor(clearedRows, ClearFlashColor);
            yield return WaitRealtime(LineClearFlashSeconds);
            SetClearedRowsFromBoard(clearedRows);
            yield return WaitRealtime(LineClearFlashSeconds * 0.5f);
        }

        HideAnimatedSourceCells(clearedRowSet, movingBlocks.Count > 0);

        float elapsed = 0f;
        while (elapsed < LineCollapseSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / LineCollapseSeconds);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            for (int i = 0; i < movingBlocks.Count; i++)
            {
                movingBlocks[i].anchoredPosition = Vector2.Lerp(startPositions[i], targetPositions[i], easedProgress);
            }

            yield return null;
        }

        foreach (RectTransform movingBlock in movingBlocks)
        {
            Destroy(movingBlock.gameObject);
        }

        TetrisLockResult clearResult = board.ClearCompletedRows();

        if (clearResult.ClearedLines > 0)
        {
            score += CalculateLineClearScore(clearResult.ClearedLines, level, clearResult.PerfectClear);
            totalLines += clearResult.ClearedLines;
            level = GetLevelForTotalLines(totalLines);
        }

        isLineClearAnimating = false;
        SpawnNextPiece();
        UpdateAllVisuals();
    }

    //spawns next piece
    private void SpawnNextPiece()
    {
        activePiece = TetrisPiece.CreateSpawn(nextType);
        nextType = bag.Draw();
        gravityTimer = 0f;

        if (!board.CanPlace(activePiece))
        {
            EndGame();
            return;
        }

        input?.ResetDropInputsForNewPiece();
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
            scoreSaveState.SaveTetrisHighScore(highScore);
        }

        UpdateScoreText();
        finalScoreText.text = $"Score: {score}";
        finalHighScoreText.text = $"Best: {highScore}";
        gameOverPanel.SetActive(true);
        gameOverPanel.transform.SetAsLastSibling();
    }

    //updates all looks
    private void UpdateAllVisuals()
    {
        UpdateBoardVisuals();
        UpdateNextPreview();
        UpdateScoreText();
    }

    //updates board look
    private void UpdateBoardVisuals()
    {
        for (int y = 0; y < TetrisBoard.VisibleHeight; y++)
        {
            int boardY = y + TetrisBoard.HiddenRows;

            for (int x = 0; x < TetrisBoard.Width; x++)
            {
                SetBoardCellVisual(x, y, boardY);
            }
        }

        if (activePiece == null)
        {
            return;
        }

        TetrisPiece ghostPiece = activePiece.WithPosition(TetrisGhostPiece.GetLandingPosition(board, activePiece));
        foreach (Vector2Int cell in ghostPiece.BoardCells())
        {
            if (board.IsVisibleRow(cell.y) && !board.IsOccupied(cell.x, cell.y))
            {
                SetTetrisCell(visualCells[cell.x, cell.y - TetrisBoard.HiddenRows], GhostColor, HandyTextureProvider.GetTetrisSprite(activePiece.Type));
            }
        }

        foreach (Vector2Int cell in activePiece.BoardCells())
        {
            if (board.IsVisibleRow(cell.y))
            {
                SetTetrisCell(visualCells[cell.x, cell.y - TetrisBoard.HiddenRows], Color.white, HandyTextureProvider.GetTetrisSprite(activePiece.Type));
            }
        }
    }

    //updates next preview
    private void UpdateNextPreview()
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                SetTetrisCell(nextPreviewCells[x, y], EmptyCellColor, null);
            }
        }

        Sprite nextSprite = HandyTextureProvider.GetTetrisSprite(nextType);
        foreach (Vector2Int cell in TetrisPiece.GetCells(nextType, TetrisRotationState.Spawn))
        {
            if (cell.x >= 0 && cell.x < 4 && cell.y >= 0 && cell.y < 4)
            {
                SetTetrisCell(nextPreviewCells[cell.x, cell.y], nextSprite != null ? Color.white : TetrisPiece.GetColor(nextType), nextSprite);
            }
        }
    }

    //sets board cell look
    private void SetBoardCellVisual(int x, int visibleY, int boardY)
    {
        if (board.TryGetCellTetromino(x, boardY, out TetrisTetromino tetromino))
        {
            Sprite sprite = HandyTextureProvider.GetTetrisSprite(tetromino);
            SetTetrisCell(visualCells[x, visibleY], sprite != null ? Color.white : board.GetCellColor(x, boardY), sprite);
            return;
        }

        SetTetrisCell(visualCells[x, visibleY], EmptyCellColor, null);
    }

    //sets tetris cell look
    private static void SetTetrisCell(Image image, Color color, Sprite sprite)
    {
        HandyTextureProvider.ApplyTintedSprite(image, sprite, color, true);
        if (image != null)
        {
            image.raycastTarget = false;
        }
    }

    //updates score text
    private void UpdateScoreText()
    {
        scoreText.text = $"Score: {score}";
        highScoreText.text = $"Best: {highScore}";
        levelText.text = $"Level: {level}";
        linesText.text = $"Lines: {totalLines}";
    }

    //gets perfect clear bonus
    private static int GetPerfectClearBonus(int clearedLines)
    {
        switch (clearedLines)
        {
            case 1:
                return 800;
            case 2:
                return 1200;
            case 3:
                return 1800;
            default:
                return 2000;
        }
    }

    //builds UI
    private void BuildInterface()
    {
        RectTransform canvasRect = HandyUIFactory.CreateOverlayCanvas(transform, "GeneratedTetrisApp", 25);

        Color backdropFallbackColor = new Color(0.045f, 0.06f, 0.085f, 1f);
        RectTransform backdrop = HandyUIFactory.CreateStretchPanel("Backdrop", canvasRect, backdropFallbackColor);
        HandyUIFactory.ConfigureBackdrop(backdrop.GetComponent<Image>(), backdropFallbackColor);
        if (HandyTextureProvider.Background == null && !PhoneSceneNavigation.IsPhoneOverlayOpen)
        {
            HandyUIFactory.CreateAccentBand("LeftAccentBand", canvasRect, new Color(0.02f, 0.24f, 0.25f, 0.48f), new Vector2(-650f, 0f), new Vector2(360f, 1100f), -6f);
            HandyUIFactory.CreateAccentBand("RightAccentBand", canvasRect, new Color(0.36f, 0.12f, 0.16f, 0.45f), new Vector2(650f, 0f), new Vector2(360f, 1100f), 7f);
        }

        Color phoneFrameFallbackColor = new Color(0.014f, 0.017f, 0.024f, 1f);
        RectTransform phoneFrame = HandyUIFactory.CreatePanel("PhoneFrame", canvasRect, phoneFrameFallbackColor, Vector2.zero, new Vector2(560f, 1040f));
        Image phoneFrameImage = phoneFrame.GetComponent<Image>();
        if (HandyTextureProvider.PhoneFrame != null)
        {
            phoneFrameImage.color = Color.clear;
            phoneFrameImage.raycastTarget = false;
        }

        Color phoneScreenFallbackColor = new Color(0.08f, 0.1f, 0.14f, 1f);
        RectTransform phoneScreen = HandyUIFactory.CreatePanel("PhoneScreen", phoneFrame, Color.white, new Vector2(0f, -2f), new Vector2(512f, 1000f));
        HandyUIFactory.ConfigureRoundedScreen(phoneScreen, phoneScreenFallbackColor);
        RectTransform contentRoot = HandyUIFactory.CreatePanel("PhoneContent", phoneScreen, Color.clear, Vector2.zero, new Vector2(470f, 920f));
        contentRoot.localScale = new Vector3(0.94f, 0.94f, 1f);
        contentRoot.GetComponent<Image>().raycastTarget = false;

        CreateHeader(contentRoot);
        CreateBoard(contentRoot);
        CreateSidePanel(contentRoot);
        CreateControls(contentRoot);
        CreateGameOverPanel(contentRoot);

        if (HandyTextureProvider.PhoneFrame != null)
        {
            HandyUIFactory.CreatePhoneFrameOverlay(canvasRect, phoneFrameFallbackColor);
        }
    }

    //creates header UI
    private void CreateHeader(RectTransform screen)
    {
        CreateText("TitleText", screen, "Tetris", new Vector2(0f, 410f), new Vector2(260f, 48f), 40f, FontStyles.Bold, TextAlignmentOptions.Center);
        scoreText = CreateText("ScoreText", screen, "Score: 0", new Vector2(-130f, 360f), new Vector2(210f, 34f), 23f, FontStyles.Bold, TextAlignmentOptions.Left);
        highScoreText = CreateText("HighScoreText", screen, "Best: 0", new Vector2(130f, 360f), new Vector2(210f, 34f), 23f, FontStyles.Bold, TextAlignmentOptions.Right);
    }

    //creates game board
    private void CreateBoard(RectTransform screen)
    {
        RectTransform boardBack = HandyUIFactory.CreatePanel("TetrisBoard", screen, BoardBackColor, new Vector2(-96f, 54f), new Vector2(BoardGridWidth + 26f, BoardGridHeight + 26f));
        boardGridRect = HandyUIFactory.CreateUIObject("BoardGrid", boardBack);
        HandyUIFactory.SetCentered(boardGridRect, Vector2.zero, new Vector2(BoardGridWidth, BoardGridHeight));

        for (int y = 0; y < TetrisBoard.VisibleHeight; y++)
        {
            for (int x = 0; x < TetrisBoard.Width; x++)
            {
                RectTransform cellRect = HandyUIFactory.CreatePanel($"Cell_{x}_{y}", boardGridRect, EmptyCellColor, GetBoardCellPosition(x, y), new Vector2(BoardCellSize, BoardCellSize));
                visualCells[x, y] = cellRect.GetComponent<Image>();
            }
        }
    }

    //creates side panel
    private void CreateSidePanel(RectTransform screen)
    {
        RectTransform nextPanel = HandyUIFactory.CreatePanel("NextPiecePreview", screen, new Color(0.065f, 0.085f, 0.12f, 1f), new Vector2(166f, 220f), new Vector2(150f, 160f));
        CreateText("NextLabel", nextPanel, "Next", new Vector2(0f, 58f), new Vector2(110f, 28f), 22f, FontStyles.Bold, TextAlignmentOptions.Center);

        RectTransform nextGrid = HandyUIFactory.CreateUIObject("NextPieceGrid", nextPanel);
        HandyUIFactory.SetCentered(nextGrid, new Vector2(0f, -18f), new Vector2(4 * PreviewCellSize + 3 * PreviewCellSpacing, 4 * PreviewCellSize + 3 * PreviewCellSpacing));

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                RectTransform cellRect = HandyUIFactory.CreatePanel($"NextCell_{x}_{y}", nextGrid, EmptyCellColor, GetPreviewCellPosition(x, y), new Vector2(PreviewCellSize, PreviewCellSize));
                nextPreviewCells[x, y] = cellRect.GetComponent<Image>();
            }
        }

        RectTransform statsPanel = HandyUIFactory.CreatePanel("StatsPanel", screen, new Color(0.065f, 0.085f, 0.12f, 1f), new Vector2(166f, 42f), new Vector2(150f, 122f));
        levelText = CreateText("LevelText", statsPanel, "Level: 0", new Vector2(0f, 28f), new Vector2(132f, 32f), 23f, FontStyles.Bold, TextAlignmentOptions.Center);
        linesText = CreateText("LinesText", statsPanel, "Lines: 0", new Vector2(0f, -22f), new Vector2(132f, 32f), 23f, FontStyles.Bold, TextAlignmentOptions.Center);
    }

    //creates game controls
    private void CreateControls(RectTransform screen)
    {
        RectTransform controlsPanel = HandyUIFactory.CreatePanel("ControlPanel", screen, new Color(0.06f, 0.08f, 0.115f, 1f), new Vector2(0f, -356f), new Vector2(470f, 170f));
        CreateText("KeybindDescription", controlsPanel, "A/D move | S soft | Space hard | Z/X rotate", new Vector2(0f, 64f), new Vector2(430f, 24f), 15f, FontStyles.Bold, TextAlignmentOptions.Center);

        CreateButton("LeftButton", controlsPanel, "A\nLeft", new Vector2(-112f, 28f), new Vector2(98f, 40f), new Color(0.17f, 0.35f, 0.55f, 1f), MoveLeft);
        CreateButton("SoftDropButton", controlsPanel, "S\nSoft", new Vector2(0f, 28f), new Vector2(98f, 40f), new Color(0.16f, 0.45f, 0.35f, 1f), SoftDropOneRow);
        CreateButton("RightButton", controlsPanel, "D\nRight", new Vector2(112f, 28f), new Vector2(98f, 40f), new Color(0.17f, 0.35f, 0.55f, 1f), MoveRight);

        CreateButton("RotateCounterclockwiseButton", controlsPanel, "Z\nCCW", new Vector2(-78f, -16f), new Vector2(104f, 40f), new Color(0.42f, 0.23f, 0.55f, 1f), RotateCounterclockwise);
        CreateButton("RotateClockwiseButton", controlsPanel, "X / Up\nCW", new Vector2(42f, -16f), new Vector2(120f, 40f), new Color(0.42f, 0.23f, 0.55f, 1f), RotateClockwise);

        CreateButton("HardDropButton", controlsPanel, "Space - Hard Drop", new Vector2(-40f, -58f), new Vector2(230f, 40f), new Color(0.48f, 0.27f, 0.14f, 1f), HardDrop);
        CreateButton("HomeButton", controlsPanel, string.Empty, new Vector2(150f, -58f), new Vector2(40f, 40f), new Color(0.18f, 0.45f, 0.58f, 1f), navigation.LoadPhoneHome, HandyTextureProvider.HomeIcon);
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

    //gets board cell position
    private Vector2 GetBoardCellPosition(int x, int visibleY)
    {
        float left = -BoardGridWidth * 0.5f + BoardCellSize * 0.5f;
        float top = BoardGridHeight * 0.5f - BoardCellSize * 0.5f;
        return new Vector2(left + x * BoardCellStride, top - visibleY * BoardCellStride);
    }

    //creates moving block overlays
    private List<RectTransform> CreateMovingBlockOverlays(HashSet<int> clearedRows, out List<Vector2> startPositions, out List<Vector2> targetPositions)
    {
        List<RectTransform> movingBlocks = new List<RectTransform>();
        startPositions = new List<Vector2>();
        targetPositions = new List<Vector2>();

        for (int boardY = TetrisBoard.HiddenRows; boardY < TetrisBoard.Height; boardY++)
        {
            if (clearedRows.Contains(boardY))
            {
                continue;
            }

            int rowsClearedBelow = CountClearedRowsBelow(boardY, clearedRows);
            if (rowsClearedBelow == 0)
            {
                continue;
            }

            int targetBoardY = boardY + rowsClearedBelow;
            if (!board.IsVisibleRow(targetBoardY))
            {
                continue;
            }

            int visibleY = boardY - TetrisBoard.HiddenRows;
            int targetVisibleY = targetBoardY - TetrisBoard.HiddenRows;

            for (int x = 0; x < TetrisBoard.Width; x++)
            {
                if (!board.IsOccupied(x, boardY))
                {
                    continue;
                }

                RectTransform movingBlock = HandyUIFactory.CreatePanel("LineCollapseBlock", boardGridRect, board.GetCellColor(x, boardY), GetBoardCellPosition(x, visibleY), new Vector2(BoardCellSize, BoardCellSize));
                Image movingImage = movingBlock.GetComponent<Image>();
                if (board.TryGetCellTetromino(x, boardY, out TetrisTetromino tetromino))
                {
                    Sprite sprite = HandyTextureProvider.GetTetrisSprite(tetromino);
                    SetTetrisCell(movingImage, sprite != null ? Color.white : board.GetCellColor(x, boardY), sprite);
                }

                movingImage.raycastTarget = false;
                movingBlock.SetAsLastSibling();
                movingBlocks.Add(movingBlock);
                startPositions.Add(GetBoardCellPosition(x, visibleY));
                targetPositions.Add(GetBoardCellPosition(x, targetVisibleY));
            }
        }

        return movingBlocks;
    }

    //hides moved source cells
    private void HideAnimatedSourceCells(HashSet<int> clearedRows, bool hasMovingBlocks)
    {
        for (int boardY = TetrisBoard.HiddenRows; boardY < TetrisBoard.Height; boardY++)
        {
            int visibleY = boardY - TetrisBoard.HiddenRows;

            for (int x = 0; x < TetrisBoard.Width; x++)
            {
                if (clearedRows.Contains(boardY) || (hasMovingBlocks && CountClearedRowsBelow(boardY, clearedRows) > 0))
                {
                    SetTetrisCell(visualCells[x, visibleY], EmptyCellColor, null);
                }
            }
        }
    }

    //sets cleared row color
    private void SetClearedRowsColor(int[] clearedRows, Color color)
    {
        foreach (int boardY in clearedRows)
        {
            if (!board.IsVisibleRow(boardY))
            {
                continue;
            }

            int visibleY = boardY - TetrisBoard.HiddenRows;
            for (int x = 0; x < TetrisBoard.Width; x++)
            {
                SetTetrisCell(visualCells[x, visibleY], color, null);
            }
        }
    }

    //sets cleared rows from board
    private void SetClearedRowsFromBoard(int[] clearedRows)
    {
        foreach (int boardY in clearedRows)
        {
            if (!board.IsVisibleRow(boardY))
            {
                continue;
            }

            int visibleY = boardY - TetrisBoard.HiddenRows;
            for (int x = 0; x < TetrisBoard.Width; x++)
            {
                SetBoardCellVisual(x, visibleY, boardY);
            }
        }
    }

    //counts cleared rows below
    private static int CountClearedRowsBelow(int boardY, HashSet<int> clearedRows)
    {
        int rowsBelow = 0;

        foreach (int clearedRow in clearedRows)
        {
            if (clearedRow > boardY)
            {
                rowsBelow++;
            }
        }

        return rowsBelow;
    }

    //waits real time
    private static IEnumerator WaitRealtime(float seconds)
    {
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    //gets preview cell position
    private static Vector2 GetPreviewCellPosition(int x, int y)
    {
        float stride = PreviewCellSize + PreviewCellSpacing;
        float width = 4 * PreviewCellSize + 3 * PreviewCellSpacing;
        float left = -width * 0.5f + PreviewCellSize * 0.5f;
        float top = width * 0.5f - PreviewCellSize * 0.5f;
        return new Vector2(left + x * stride, top - y * stride);
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
            21f,
            Color.white,
            size,
            iconSprite != null ? 0f : 10f);
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
        return HandyUIFactory.CreateText(objectName, parent, text, anchoredPosition, size, fontSize, fontStyle, alignment, Color.white, true, Mathf.Max(11f, fontSize * 0.55f));
    }

}

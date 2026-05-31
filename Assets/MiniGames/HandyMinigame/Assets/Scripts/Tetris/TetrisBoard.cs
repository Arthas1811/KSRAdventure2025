using System.Collections.Generic;
using UnityEngine;

//stores tetris lock result
public struct TetrisLockResult
{
    //stores tetris lock result
    public TetrisLockResult(bool wasLocked, int clearedLines, bool perfectClear, int[] clearedRows)
    {
        WasLocked = wasLocked;
        ClearedLines = clearedLines;
        PerfectClear = perfectClear;
        ClearedRows = clearedRows;
    }

    public bool WasLocked { get; }

    public int ClearedLines { get; }

    public bool PerfectClear { get; }

    public int[] ClearedRows { get; }

    public static TetrisLockResult Invalid => new TetrisLockResult(false, 0, false, new int[0]);
}

//stores tetris board
public class TetrisBoard
{
    public const int Width = 10;

    public const int Height = 22;

    public const int HiddenRows = 2;

    public const int VisibleHeight = Height - HiddenRows;

    private static readonly Vector2Int[] NoKick = { Vector2Int.zero };

    private static readonly Dictionary<string, Vector2Int[]> JlstzKickData = new Dictionary<string, Vector2Int[]>
    {
        { "0>1", CreateKicks((0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2)) },
        { "1>0", CreateKicks((0, 0), (1, 0), (1, -1), (0, 2), (1, 2)) },
        { "1>2", CreateKicks((0, 0), (1, 0), (1, -1), (0, 2), (1, 2)) },
        { "2>1", CreateKicks((0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2)) },
        { "2>3", CreateKicks((0, 0), (1, 0), (1, 1), (0, -2), (1, -2)) },
        { "3>2", CreateKicks((0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2)) },
        { "3>0", CreateKicks((0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2)) },
        { "0>3", CreateKicks((0, 0), (1, 0), (1, 1), (0, -2), (1, -2)) },
    };

    private static readonly Dictionary<string, Vector2Int[]> IKickData = new Dictionary<string, Vector2Int[]>
    {
        { "0>1", CreateKicks((0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2)) },
        { "1>0", CreateKicks((0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2)) },
        { "1>2", CreateKicks((0, 0), (-1, 0), (2, 0), (-1, 2), (2, -1)) },
        { "2>1", CreateKicks((0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1)) },
        { "2>3", CreateKicks((0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2)) },
        { "3>2", CreateKicks((0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2)) },
        { "3>0", CreateKicks((0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1)) },
        { "0>3", CreateKicks((0, 0), (-1, 0), (2, 0), (-1, 2), (2, -1)) },
    };

    private readonly bool[,] occupied = new bool[Width, Height];
    private readonly Color[,] colors = new Color[Width, Height];
    private readonly TetrisTetromino[,] tetrominoTypes = new TetrisTetromino[Width, Height];

    //checks inside board
    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    //checks visible row
    public bool IsVisibleRow(int y)
    {
        return y >= HiddenRows && y < Height;
    }

    //checks occupied cell
    public bool IsOccupied(int x, int y)
    {
        return IsInside(x, y) && occupied[x, y];
    }

    //gets cell color
    public Color GetCellColor(int x, int y)
    {
        return IsInside(x, y) ? colors[x, y] : Color.clear;
    }

    //gets cell tetris type
    public bool TryGetCellTetromino(int x, int y, out TetrisTetromino tetromino)
    {
        tetromino = default;
        if (!IsOccupied(x, y))
        {
            return false;
        }

        tetromino = tetrominoTypes[x, y];
        return true;
    }

    //clears board
    public void Clear()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                occupied[x, y] = false;
                colors[x, y] = Color.clear;
                tetrominoTypes[x, y] = default;
            }
        }
    }

    //checks valid place
    public bool CanPlace(TetrisPiece piece)
    {
        if (piece == null)
        {
            return false;
        }

        foreach (Vector2Int cell in piece.BoardCells())
        {
            if (!IsInside(cell.x, cell.y) || occupied[cell.x, cell.y])
            {
                return false;
            }
        }

        return true;
    }

    //locks tetris piece
    public TetrisLockResult LockPiece(TetrisPiece piece)
    {
        if (!PlacePiece(piece))
        {
            return TetrisLockResult.Invalid;
        }

        return ClearCompletedRows();
    }

    //places tetris piece
    public bool PlacePiece(TetrisPiece piece)
    {
        if (!CanPlace(piece))
        {
            return false;
        }

        foreach (Vector2Int cell in piece.BoardCells())
        {
            occupied[cell.x, cell.y] = true;
            colors[cell.x, cell.y] = piece.Color;
            tetrominoTypes[cell.x, cell.y] = piece.Type;
        }

        return true;
    }

    //gets full rows
    public int[] GetFullRows()
    {
        List<int> fullRows = new List<int>();

        for (int y = 0; y < Height; y++)
        {
            if (IsRowFull(y))
            {
                fullRows.Add(y);
            }
        }

        return fullRows.ToArray();
    }

    //clears full rows
    public TetrisLockResult ClearCompletedRows()
    {
        int[] clearedRows = ClearFullRows();
        return new TetrisLockResult(true, clearedRows.Length, IsBoardEmpty(), clearedRows);
    }

    //tries wall kick rotate
    public bool TryRotateWithSrs(TetrisPiece piece, int direction, out TetrisPiece rotatedPiece)
    {
        rotatedPiece = piece;

        if (piece == null || direction == 0)
        {
            return false;
        }

        int normalizedDirection = direction > 0 ? 1 : -1;
        TetrisPiece baseRotatedPiece = piece.Rotate(normalizedDirection);
        Vector2Int[] kicks = GetSrsKicks(piece.Type, piece.Rotation, baseRotatedPiece.Rotation);

        foreach (Vector2Int kick in kicks)
        {
            TetrisPiece kickedPiece = baseRotatedPiece.WithPosition(piece.Position + kick);
            if (CanPlace(kickedPiece))
            {
                rotatedPiece = kickedPiece;
                return true;
            }
        }

        return false;
    }

    //checks empty board
    public bool IsBoardEmpty()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (occupied[x, y])
                {
                    return false;
                }
            }
        }

        return true;
    }

    //checks full row
    public bool IsRowFull(int y)
    {
        if (y < 0 || y >= Height)
        {
            return false;
        }

        for (int x = 0; x < Width; x++)
        {
            if (!occupied[x, y])
            {
                return false;
            }
        }

        return true;
    }

    //clears full rows
    private int[] ClearFullRows()
    {
        List<int> clearedRows = new List<int>(GetFullRows());
        int clearedBelow = 0;

        for (int y = Height - 1; y >= 0; y--)
        {
            if (clearedRows.Contains(y))
            {
                clearedBelow++;
                continue;
            }

            if (clearedBelow > 0)
            {
                CopyRow(y, y + clearedBelow);
            }
        }

        for (int y = 0; y < clearedBelow; y++)
        {
            ClearRow(y);
        }

        return clearedRows.ToArray();
    }

    //copies row
    private void CopyRow(int sourceY, int targetY)
    {
        for (int x = 0; x < Width; x++)
        {
            occupied[x, targetY] = occupied[x, sourceY];
            colors[x, targetY] = colors[x, sourceY];
            tetrominoTypes[x, targetY] = tetrominoTypes[x, sourceY];
        }
    }

    //clears row
    private void ClearRow(int y)
    {
        for (int x = 0; x < Width; x++)
        {
            occupied[x, y] = false;
            colors[x, y] = Color.clear;
            tetrominoTypes[x, y] = default;
        }
    }

    //gets wall kicks
    private static Vector2Int[] GetSrsKicks(TetrisTetromino type, TetrisRotationState from, TetrisRotationState to)
    {
        if (type == TetrisTetromino.O)
        {
            return NoKick;
        }

        string key = $"{(int)from}>{(int)to}";
        Dictionary<string, Vector2Int[]> source = type == TetrisTetromino.I ? IKickData : JlstzKickData;
        return source.TryGetValue(key, out Vector2Int[] kicks) ? kicks : NoKick;
    }

    //creates wall kicks
    private static Vector2Int[] CreateKicks(params (int x, int y)[] srsKicks)
    {
        Vector2Int[] converted = new Vector2Int[srsKicks.Length];

        for (int i = 0; i < srsKicks.Length; i++)
        {
            converted[i] = new Vector2Int(srsKicks[i].x, -srsKicks[i].y);
        }

        return converted;
    }
}

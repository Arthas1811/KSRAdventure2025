using System.Collections.Generic;
using UnityEngine;

//stores block blast place result
public struct BlockBlastPlacementResult
{
    //stores block blast place result
    public BlockBlastPlacementResult(bool wasPlaced, int clearedBlocks, int clearedLines, bool perfectClear, int[] clearedRows, int[] clearedColumns)
    {
        WasPlaced = wasPlaced;
        ClearedBlocks = clearedBlocks;
        ClearedLines = clearedLines;
        PerfectClear = perfectClear;
        ClearedRows = clearedRows;
        ClearedColumns = clearedColumns;
    }

    public bool WasPlaced { get; }

    public int ClearedBlocks { get; }

    public int ClearedLines { get; }

    public bool PerfectClear { get; }

    public int[] ClearedRows { get; }

    public int[] ClearedColumns { get; }

    public static BlockBlastPlacementResult Invalid => new BlockBlastPlacementResult(false, 0, 0, false, new int[0], new int[0]);
}

//stores block blast board
public class BlockBlastBoard
{
    public const int Size = 8;

    private readonly bool[,] occupied = new bool[Size, Size];
    private readonly Color[,] colors = new Color[Size, Size];
    private readonly HandyBlockTextureKey[,] textureKeys = new HandyBlockTextureKey[Size, Size];

    //checks inside board
    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < Size && y >= 0 && y < Size;
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

    //gets cell texture key
    public HandyBlockTextureKey GetCellTextureKey(int x, int y)
    {
        return IsInside(x, y) ? textureKeys[x, y] : HandyBlockTextureKey.None;
    }

    //clears board
    public void Clear()
    {
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                occupied[x, y] = false;
                colors[x, y] = Color.clear;
                textureKeys[x, y] = HandyBlockTextureKey.None;
            }
        }
    }

    //checks valid place
    public bool CanPlace(BlockBlastShape shape, Vector2Int origin)
    {
        foreach (Vector2Int shapeCell in shape.Cells)
        {
            int boardX = origin.x + shapeCell.x;
            int boardY = origin.y + shapeCell.y;

            if (!IsInside(boardX, boardY) || occupied[boardX, boardY])
            {
                return false;
            }
        }

        return true;
    }

    //places block shape
    public BlockBlastPlacementResult PlaceShape(BlockBlastShape shape, Vector2Int origin)
    {
        if (!CanPlace(shape, origin))
        {
            return BlockBlastPlacementResult.Invalid;
        }

        foreach (Vector2Int shapeCell in shape.Cells)
        {
            int boardX = origin.x + shapeCell.x;
            int boardY = origin.y + shapeCell.y;
            occupied[boardX, boardY] = true;
            colors[boardX, boardY] = shape.Color;
            textureKeys[boardX, boardY] = shape.TextureKey;
        }

        List<int> fullRows = FindFullRows();
        List<int> fullColumns = FindFullColumns();
        bool[,] cellsToClear = new bool[Size, Size];

        foreach (int row in fullRows)
        {
            for (int x = 0; x < Size; x++)
            {
                cellsToClear[x, row] = true;
            }
        }

        foreach (int column in fullColumns)
        {
            for (int y = 0; y < Size; y++)
            {
                cellsToClear[column, y] = true;
            }
        }

        int clearedBlocks = 0;
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                if (cellsToClear[x, y] && occupied[x, y])
                {
                    clearedBlocks++;
                    occupied[x, y] = false;
                    colors[x, y] = Color.clear;
                    textureKeys[x, y] = HandyBlockTextureKey.None;
                }
            }
        }

        int clearedLines = fullRows.Count + fullColumns.Count;
        return new BlockBlastPlacementResult(true, clearedBlocks, clearedLines, IsBoardEmpty(), fullRows.ToArray(), fullColumns.ToArray());
    }

    //checks shape fit
    public bool CanShapeFitAnywhere(BlockBlastShape shape)
    {
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                if (CanPlace(shape, new Vector2Int(x, y)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    //checks any shape fit
    public bool CanAnyShapeFit(IEnumerable<BlockBlastShape> shapes)
    {
        foreach (BlockBlastShape shape in shapes)
        {
            if (shape != null && CanShapeFitAnywhere(shape))
            {
                return true;
            }
        }

        return false;
    }

    //checks empty board
    public bool IsBoardEmpty()
    {
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                if (occupied[x, y])
                {
                    return false;
                }
            }
        }

        return true;
    }

    //finds full rows
    private List<int> FindFullRows()
    {
        List<int> fullRows = new List<int>();

        for (int y = 0; y < Size; y++)
        {
            bool rowFull = true;
            for (int x = 0; x < Size; x++)
            {
                if (!occupied[x, y])
                {
                    rowFull = false;
                    break;
                }
            }

            if (rowFull)
            {
                fullRows.Add(y);
            }
        }

        return fullRows;
    }

    //finds full columns
    private List<int> FindFullColumns()
    {
        List<int> fullColumns = new List<int>();

        for (int x = 0; x < Size; x++)
        {
            bool columnFull = true;
            for (int y = 0; y < Size; y++)
            {
                if (!occupied[x, y])
                {
                    columnFull = false;
                    break;
                }
            }

            if (columnFull)
            {
                fullColumns.Add(x);
            }
        }

        return fullColumns;
    }
}

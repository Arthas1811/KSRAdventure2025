using System;
using System.Collections.Generic;
using UnityEngine;

//stores block blast shape
public class BlockBlastShape
{
    private readonly Vector2Int[] cells;

    //sets block blast shape
    public BlockBlastShape(string shapeName, IEnumerable<Vector2Int> cells, Color color)
        : this(shapeName, cells, color, HandyBlockTextureKey.None)
    {
    }

    //sets block blast shape
    public BlockBlastShape(string shapeName, IEnumerable<Vector2Int> cells, Color color, HandyBlockTextureKey textureKey)
    {
        ShapeName = shapeName;
        Color = color;
        TextureKey = textureKey;

        List<Vector2Int> copiedCells = new List<Vector2Int>();
        int maxX = 0;
        int maxY = 0;

        foreach (Vector2Int cell in cells)
        {
            if (cell.x < 0 || cell.y < 0)
            {
                throw new ArgumentException("Block Blast shape cells must use non-negative coordinates.");
            }

            copiedCells.Add(cell);
            maxX = Mathf.Max(maxX, cell.x);
            maxY = Mathf.Max(maxY, cell.y);
        }

        if (copiedCells.Count == 0)
        {
            throw new ArgumentException("Block Blast shapes need at least one occupied cell.");
        }

        this.cells = copiedCells.ToArray();
        Width = maxX + 1;
        Height = maxY + 1;
    }

    public string ShapeName { get; }

    public IReadOnlyList<Vector2Int> Cells => cells;

    public Color Color { get; }

    public HandyBlockTextureKey TextureKey { get; }

    public int Width { get; }

    public int Height { get; }
}

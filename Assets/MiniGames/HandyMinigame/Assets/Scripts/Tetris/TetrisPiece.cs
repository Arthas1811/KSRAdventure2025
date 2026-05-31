using System;
using System.Collections.Generic;
using UnityEngine;

//lists tetris block types
public enum TetrisTetromino
{
    I,
    O,
    T,
    S,
    Z,
    J,
    L,
}

public enum TetrisRotationState
{
    Spawn = 0,
    Right = 1,
    Reverse = 2,
    Left = 3,
}

//stores tetris piece
public class TetrisPiece
{
    private static readonly Dictionary<TetrisTetromino, Vector2Int[][]> CellsByType = new Dictionary<TetrisTetromino, Vector2Int[][]>
    {
        {
            TetrisTetromino.I,
            new[]
            {
                new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(3, 1) },
                new[] { new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2), new Vector2Int(2, 3) },
                new[] { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2), new Vector2Int(3, 2) },
                new[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(1, 3) },
            }
        },
        {
            TetrisTetromino.O,
            new[]
            {
                new[] { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) },
                new[] { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) },
                new[] { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) },
                new[] { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            }
        },
        {
            TetrisTetromino.T,
            new[]
            {
                new[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
                new[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) },
                new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) },
                new[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2) },
            }
        },
        {
            TetrisTetromino.S,
            new[]
            {
                new[] { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
                new[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 2) },
                new[] { new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(0, 2), new Vector2Int(1, 2) },
                new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2) },
            }
        },
        {
            TetrisTetromino.Z,
            new[]
            {
                new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1) },
                new[] { new Vector2Int(2, 0), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) },
                new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 2) },
                new[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 2) },
            }
        },
        {
            TetrisTetromino.J,
            new[]
            {
                new[] { new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
                new[] { new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) },
                new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(2, 2) },
                new[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 2), new Vector2Int(1, 2) },
            }
        },
        {
            TetrisTetromino.L,
            new[]
            {
                new[] { new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
                new[] { new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 2) },
                new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(0, 2) },
                new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2) },
            }
        },
    };

    //sets tetris piece data
    public TetrisPiece(TetrisTetromino type, Vector2Int position, TetrisRotationState rotation = TetrisRotationState.Spawn)
    {
        Type = type;
        Position = position;
        Rotation = rotation;
        Color = GetColor(type);
    }

    public TetrisTetromino Type { get; }

    public Vector2Int Position { get; }

    public TetrisRotationState Rotation { get; }

    public Color Color { get; }

    public IReadOnlyList<Vector2Int> Cells => GetCells(Type, Rotation);

    //creates spawn piece
    public static TetrisPiece CreateSpawn(TetrisTetromino type)
    {
        return new TetrisPiece(type, new Vector2Int(3, 0), TetrisRotationState.Spawn);
    }

    //gets tetris cells
    public static IReadOnlyList<Vector2Int> GetCells(TetrisTetromino type, TetrisRotationState rotation)
    {
        return CellsByType[type][(int)rotation];
    }

    //gets board cells
    public IEnumerable<Vector2Int> BoardCells()
    {
        foreach (Vector2Int cell in Cells)
        {
            yield return Position + cell;
        }
    }

    //sets piece position
    public TetrisPiece WithPosition(Vector2Int position)
    {
        return new TetrisPiece(Type, position, Rotation);
    }

    //moves piece
    public TetrisPiece Move(int xOffset, int yOffset)
    {
        return WithPosition(Position + new Vector2Int(xOffset, yOffset));
    }

    //sets piece rotation
    public TetrisPiece WithRotation(TetrisRotationState rotation)
    {
        return new TetrisPiece(Type, Position, rotation);
    }

    //rotates piece
    public TetrisPiece Rotate(int direction)
    {
        int nextRotation = ((int)Rotation + direction) % 4;
        if (nextRotation < 0)
        {
            nextRotation += 4;
        }

        return WithRotation((TetrisRotationState)nextRotation);
    }

    //gets piece color
    public static Color GetColor(TetrisTetromino type)
    {
        switch (type)
        {
            case TetrisTetromino.I:
                return new Color(0.12f, 0.84f, 0.95f, 1f);
            case TetrisTetromino.O:
                return new Color(0.96f, 0.84f, 0.12f, 1f);
            case TetrisTetromino.T:
                return new Color(0.63f, 0.31f, 0.88f, 1f);
            case TetrisTetromino.S:
                return new Color(0.25f, 0.78f, 0.29f, 1f);
            case TetrisTetromino.Z:
                return new Color(0.9f, 0.16f, 0.18f, 1f);
            case TetrisTetromino.J:
                return new Color(0.18f, 0.34f, 0.92f, 1f);
            case TetrisTetromino.L:
                return new Color(0.96f, 0.52f, 0.12f, 1f);
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}

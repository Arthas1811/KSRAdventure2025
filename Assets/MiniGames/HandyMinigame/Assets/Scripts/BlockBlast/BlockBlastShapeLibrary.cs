using UnityEngine;
using Random = UnityEngine.Random;

//stores block blast shapes
public static class BlockBlastShapeLibrary
{
    private static readonly Color SingleColor = CreateColor(183, 92, 224);
    private static readonly Color Line2Color = CreateColor(225, 72, 62);
    private static readonly Color Line3Color = CreateColor(89, 117, 235);
    private static readonly Color Line4Color = CreateColor(73, 203, 231);
    private static readonly Color Line5Color = CreateColor(255, 205, 71);
    private static readonly Color Square2Color = CreateColor(96, 210, 88);
    private static readonly Color Square3Color = CreateColor(245, 139, 45);
    private static readonly Color RectangleColor = CreateColor(45, 198, 155);
    private static readonly Color L3Color = CreateColor(255, 112, 177);
    private static readonly Color L4Color = CreateColor(142, 109, 255);
    private static readonly Color L4MirrorColor = CreateColor(34, 170, 255);
    private static readonly Color L5Color = CreateColor(174, 214, 43);
    private static readonly Color Z4Color = CreateColor(0, 180, 120);
    private static readonly Color Z4MirrorColor = CreateColor(255, 92, 89);

    private static readonly BlockBlastShape[] shapes =
    {
        Create("Single", SingleColor, HandyBlockTextureKey.Magenta, new Vector2Int(0, 0)),
        Create("Vertical2", Line2Color, HandyBlockTextureKey.Red, new Vector2Int(0, 0), new Vector2Int(0, 1)),
        Create("Horizontal2", Line2Color, HandyBlockTextureKey.Red, new Vector2Int(0, 0), new Vector2Int(1, 0)),
        Create("Horizontal3", Line3Color, HandyBlockTextureKey.Blue, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)),
        Create("Vertical3", Line3Color, HandyBlockTextureKey.Blue, new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2)),
        Create("Vertical4", Line4Color, HandyBlockTextureKey.LightBlue, new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3)),
        Create("Horizontal4", Line4Color, HandyBlockTextureKey.LightBlue, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0)),
        Create("Vertical5", Line5Color, HandyBlockTextureKey.Yellow, new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(0, 3), new Vector2Int(0, 4)),
        Create("Horizontal5", Line5Color, HandyBlockTextureKey.Yellow, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0), new Vector2Int(4, 0)),
        Create("Square2", Square2Color, HandyBlockTextureKey.Green, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1)),
        Create("Square3", Square3Color, HandyBlockTextureKey.Orange, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2)),
        Create("Rectangle2x3", RectangleColor, HandyBlockTextureKey.Mint, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 2), new Vector2Int(1, 2)),
        Create("Rectangle3x2", RectangleColor, HandyBlockTextureKey.Mint, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)),
        Create("L3", L3Color, HandyBlockTextureKey.Pink, new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1)),
        Create("L3Mirror", L3Color, HandyBlockTextureKey.Pink, new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1)),
        Create("L3TopLeft", L3Color, HandyBlockTextureKey.Pink, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1)),
        Create("L3TopRight", L3Color, HandyBlockTextureKey.Pink, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1)),
        Create("L4RightFoot", L4Color, HandyBlockTextureKey.Violett, new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 2)),
        Create("L4LeftFoot", L4MirrorColor, HandyBlockTextureKey.DarkBlue, new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(0, 2)),
        Create("L4RightHead", L4MirrorColor, HandyBlockTextureKey.DarkBlue, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, 2)),
        Create("L4LeftHead", L4Color, HandyBlockTextureKey.Violett, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2)),
        Create("L4WideFoot", L4MirrorColor, HandyBlockTextureKey.DarkBlue, new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)),
        Create("L4WideFootMirror", L4Color, HandyBlockTextureKey.Violett, new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1)),
        Create("L4WideHead", L4Color, HandyBlockTextureKey.Violett, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1)),
        Create("L4WideHeadMirror", L4MirrorColor, HandyBlockTextureKey.DarkBlue, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(2, 1)),
        Create("L5Bottom", L5Color, HandyBlockTextureKey.DarkGreen, new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2)),
        Create("L5TopLeft", L5Color, HandyBlockTextureKey.DarkGreen, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(0, 2)),
        Create("L5TopRight", L5Color, HandyBlockTextureKey.DarkGreen, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2)),
        Create("L5BottomRight", L5Color, HandyBlockTextureKey.DarkGreen, new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2)),
        Create("Z4Horizontal", Z4Color, HandyBlockTextureKey.Turquoise, new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(1, 1)),
        Create("Z4Vertical", Z4Color, HandyBlockTextureKey.Turquoise, new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2)),
        Create("Z4HorizontalMirror", Z4MirrorColor, HandyBlockTextureKey.DarkRed, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1)),
        Create("Z4VerticalMirror", Z4MirrorColor, HandyBlockTextureKey.DarkRed, new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 2)),
    };

    //gets random shape
    public static BlockBlastShape GetRandomShape()
    {
        return shapes[Random.Range(0, shapes.Length)];
    }

    //creates shape
    private static BlockBlastShape Create(string shapeName, Color color, HandyBlockTextureKey textureKey, params Vector2Int[] cells)
    {
        return new BlockBlastShape(shapeName, cells, color, textureKey);
    }

    //creates color
    private static Color32 CreateColor(byte r, byte g, byte b)
    {
        return new Color32(r, g, b, 255);
    }
}

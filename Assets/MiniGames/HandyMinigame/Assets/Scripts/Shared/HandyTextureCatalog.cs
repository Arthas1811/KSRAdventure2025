using UnityEngine;

//stores handy textures
[CreateAssetMenu(fileName = "HandyTextureCatalog", menuName = "MiniGames/Handy/Texture Catalog")]
public class HandyTextureCatalog : ScriptableObject
{
    [Header("Phone Home")]
    public Sprite background;
    public Sprite phoneFrame;
    public Sprite phoneWallpaper;
    public Sprite crackedDisplay;
    public Sprite homeIcon;
    public Sprite mailboxIcon;
    public Sprite blockBlastIcon;
    public Sprite tetrosIcon;

    [Header("Block Blast Blocks")]
    public Sprite blockBlastBlueBlock;
    public Sprite blockBlastDarkBlueBlock;
    public Sprite blockBlastDarkGreenBlock;
    public Sprite blockBlastDarkRedBlock;
    public Sprite blockBlastGreenBlock;
    public Sprite blockBlastLightBlueBlock;
    public Sprite blockBlastMagentaBlock;
    public Sprite blockBlastMintBlock;
    public Sprite blockBlastOrangeBlock;
    public Sprite blockBlastPinkBlock;
    public Sprite blockBlastRedBlock;
    public Sprite blockBlastTurquoiseBlock;
    public Sprite blockBlastViolettBlock;
    public Sprite blockBlastYellowBlock;

    [Header("Tetris Blocks")]
    public Sprite tetrisDarkBlueBlock;
    public Sprite tetrisGreenBlock;
    public Sprite tetrisLightBlueBlock;
    public Sprite tetrisMagentaBlock;
    public Sprite tetrisOrangeBlock;
    public Sprite tetrisRedBlock;
    public Sprite tetrisYellowBlock;

    //gets block blast sprite
    public Sprite GetBlockBlastSprite(HandyBlockTextureKey textureKey)
    {
        switch (textureKey)
        {
            case HandyBlockTextureKey.Blue:
                return blockBlastBlueBlock;
            case HandyBlockTextureKey.DarkBlue:
                return blockBlastDarkBlueBlock;
            case HandyBlockTextureKey.DarkGreen:
                return blockBlastDarkGreenBlock;
            case HandyBlockTextureKey.DarkRed:
                return blockBlastDarkRedBlock;
            case HandyBlockTextureKey.Green:
                return blockBlastGreenBlock;
            case HandyBlockTextureKey.LightBlue:
                return blockBlastLightBlueBlock;
            case HandyBlockTextureKey.Magenta:
                return blockBlastMagentaBlock;
            case HandyBlockTextureKey.Mint:
                return blockBlastMintBlock;
            case HandyBlockTextureKey.Orange:
                return blockBlastOrangeBlock;
            case HandyBlockTextureKey.Pink:
                return blockBlastPinkBlock;
            case HandyBlockTextureKey.Red:
                return blockBlastRedBlock;
            case HandyBlockTextureKey.Turquoise:
                return blockBlastTurquoiseBlock;
            case HandyBlockTextureKey.Violett:
                return blockBlastViolettBlock;
            case HandyBlockTextureKey.Yellow:
                return blockBlastYellowBlock;
            default:
                return null;
        }
    }

    //gets tetris sprite
    public Sprite GetTetrisSprite(TetrisTetromino tetromino)
    {
        switch (tetromino)
        {
            case TetrisTetromino.I:
                return tetrisLightBlueBlock;
            case TetrisTetromino.O:
                return tetrisYellowBlock;
            case TetrisTetromino.T:
                return tetrisMagentaBlock;
            case TetrisTetromino.S:
                return tetrisGreenBlock;
            case TetrisTetromino.Z:
                return tetrisRedBlock;
            case TetrisTetromino.J:
                return tetrisDarkBlueBlock;
            case TetrisTetromino.L:
                return tetrisOrangeBlock;
            default:
                return null;
        }
    }
}

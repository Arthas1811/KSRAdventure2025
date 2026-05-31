using UnityEngine;

//finds tetris ghost piece
public static class TetrisGhostPiece
{
    //gets ghost landing spot
    public static Vector2Int GetLandingPosition(TetrisBoard board, TetrisPiece activePiece)
    {
        if (board == null || activePiece == null)
        {
            return Vector2Int.zero;
        }

        TetrisPiece ghostPiece = activePiece;
        while (board.CanPlace(ghostPiece.Move(0, 1)))
        {
            ghostPiece = ghostPiece.Move(0, 1);
        }

        return ghostPiece.Position;
    }
}

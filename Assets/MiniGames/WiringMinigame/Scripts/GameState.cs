using UnityEngine;

public static class GameState
{
    public static int puzzlesSolved = 0;
    public static char selectedCableChar;
    public static int timeLeft;
    public static bool blueSolved = false;
    public static bool greenSolved = false;
    public static bool orangeSolved = false;
    public static bool purpleSolved = false;
    public static bool redSolved = false;
    public static bool yellowSolved = false;
    public static char[] cableOrder = new char[6];
    public static char[] colours = new char[6] {'B','G','O','P','R','Y'};
}

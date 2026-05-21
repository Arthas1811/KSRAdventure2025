using UnityEngine;
using TMPro;

public class DisplayAmountSolved : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public TextMeshProUGUI puzzlesSolvedText;
    void Update()
    {
        puzzlesSolvedText.text = GameState.puzzlesSolved.ToString() + "/3";
    }
}
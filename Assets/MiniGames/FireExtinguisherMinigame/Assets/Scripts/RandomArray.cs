using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomArray : MonoBehaviour
{
    [Header("Sequence Settings")]
    public int sequenceLength = 6;
    public int totalSequences = 4;

    [Header("UI")]
    public TMP_Text displayText;

    public static List<List<string>> AllSequences { get; private set; }
    public static int CurrentSequenceIndex { get; set; } = 0;

    private readonly string[] keys = { "W", "A", "S", "D" };

    void Start()
    {
        GenerateAllSequences();
        ShowCurrentSequence();
    }

    void GenerateAllSequences()
    {
        AllSequences = new List<List<string>>();
        CurrentSequenceIndex = 0;

        for (int s = 0; s < totalSequences; s++)
        {
            List<string> seq = new List<string>();
            for (int i = 0; i < sequenceLength; i++)
                seq.Add(keys[Random.Range(0, keys.Length)]);
            AllSequences.Add(seq);
        }
    }

    public void ShowCurrentSequence()
    {
        if (CurrentSequenceIndex < AllSequences.Count && displayText != null)
            displayText.text = string.Join(" ", AllSequences[CurrentSequenceIndex]);
    }
}
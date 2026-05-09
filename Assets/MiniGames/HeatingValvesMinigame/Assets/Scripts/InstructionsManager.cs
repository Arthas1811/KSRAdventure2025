using UnityEngine;
using System;
using System.Collections.Generic;

public class InstructionsManager : MonoBehaviour
{
    public static InstructionsManager Instance;
    public event Action<string> OnTextChanged;

    private HashSet<string> completedParts = new HashSet<string>();

    private bool message1Sent = false;
    private bool message2Sent = false;
    private bool message3Sent = false;

    private void Awake()
    {
        Instance = this;
    }
    public void PartCompleted(string partID)
    {
        completedParts.Add(partID);
        CheckProgress();
    }
    private void CheckProgress()
    {
        if (!message1Sent && completedParts.Contains("eins") && completedParts.Contains("zwei"))
        {
            message1Sent = true;
            OnTextChanged?.Invoke("Click on the yellow square!");
        }

        if (!message2Sent &&
            completedParts.Contains("1") &&
            completedParts.Contains("2") &&
            completedParts.Contains("3") &&
            completedParts.Contains("4") &&
            completedParts.Contains("5"))
        {
            message2Sent = true;
            OnTextChanged?.Invoke("The valve is complete!");
        }


        if (!message3Sent &&
            completedParts.Contains("w1") &&
            completedParts.Contains("w2") &&
            completedParts.Contains("w3"))
        {
            message3Sent = true;
            OnTextChanged?.Invoke("Victory!");
        }
    }
}
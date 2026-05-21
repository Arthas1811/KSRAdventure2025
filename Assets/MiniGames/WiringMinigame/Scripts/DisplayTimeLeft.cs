using UnityEngine;
using TMPro;

public class DisplayTimeLeft : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public TextMeshProUGUI timeLeftText;
    void Update()
    {
        timeLeftText.text = GameState.timeLeft.ToString();
    }
}


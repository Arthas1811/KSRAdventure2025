using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;

public class SequenceChecker : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text feedbackText;
    public TMP_Text winText;
    public Image timerBar; // Assign a UI Image with Image Type = Filled, Fill Method = Horizontal

    [Header("Flames - in order Fire_1 to Fire_4")]
    public GameObject[] flames = new GameObject[4];

    [Header("Reference")]
    public RandomArray randomArray;

    [Header("Timer")]
    public float sequenceTime = 15f;

    private float timeLeft;
    private int currentIndex = 0;
    private bool gameOver = false;

    [Header("KSR Adventure Assignments")]
    JObject saveData;
    public SaveDataManager saveDataManager;

    void Start()
    {
        saveData = SaveDataManager.Instance.readData();
        timeLeft = sequenceTime;
    }

    void Update()
    {
        if (gameOver) return;
        if (RandomArray.AllSequences == null || RandomArray.AllSequences.Count == 0) return;

        // Timer countdown
        timeLeft -= Time.deltaTime;
        if (timerBar != null)
            timerBar.fillAmount = timeLeft / sequenceTime;

        if (timeLeft <= 0f)
        {
            gameOver = true;
            if (feedbackText != null) feedbackText.text = "Zeit abgelaufen! Game Over.";
            if (timerBar != null) timerBar.fillAmount = 0f;
            Invoke(nameof(Quit), 2f);
            return;
        }

        var kb = Keyboard.current;
        if (kb.wKey.wasPressedThisFrame) CheckInput("W");
        else if (kb.aKey.wasPressedThisFrame) CheckInput("A");
        else if (kb.sKey.wasPressedThisFrame) CheckInput("S");
        else if (kb.dKey.wasPressedThisFrame) CheckInput("D");
    }

    void CheckInput(string key)
    {
        int seqIdx = RandomArray.CurrentSequenceIndex;
        var sequence = RandomArray.AllSequences[seqIdx];
        string expected = sequence[currentIndex];

        if (key == expected)
        {
            currentIndex++;

            if (currentIndex >= sequence.Count)
            {
                if (seqIdx < flames.Length && flames[seqIdx] != null)
                    flames[seqIdx].SetActive(false);

                RandomArray.CurrentSequenceIndex++;
                currentIndex = 0;
                timeLeft = sequenceTime; // Reset timer for next sequence

                if (RandomArray.CurrentSequenceIndex >= RandomArray.AllSequences.Count)
                {
                    gameOver = true;
                    if (feedbackText != null) feedbackText.text = "";
                    if (winText != null) winText.text = "Du hast gewonnen!";
                    Invoke(nameof(Quit), 2f);
                    return;
                }

                randomArray.ShowCurrentSequence();
                if (feedbackText != null)
                    feedbackText.text = $"Feuer {seqIdx + 1} geloescht!";
            }
            else
            {
                if (feedbackText != null)
                    feedbackText.text = $"{currentIndex}/{sequence.Count}";
            }
        }
        else
        {
            gameOver = true;
            if (feedbackText != null) feedbackText.text = "Falsch! Game Over.";
            Invoke(nameof(Quit), 2f);
        }
    }

    void Quit()
    {
        saveData["states"]["minigames"]["fireExtinguished"] = true;
        saveData["states"]["basement"]["h025DoorOpen"] = true;
        SaveDataManager.Instance.saveData(saveData);
        SceneManager.LoadScene("main");
    }
}
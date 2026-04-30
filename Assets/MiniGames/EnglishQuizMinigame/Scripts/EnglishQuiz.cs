using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

[System.Serializable]
public class EnglishQuestion
{
    public string question;
    public string[] answers;
    public int correctIndex;
}

[System.Serializable]
public class EnglishQuestionList
{
    public EnglishQuestion[] questions;
}

public class EnglishQuiz : MonoBehaviour
{
    [Header("JSON File (in Assets/Resources)")]
    public string JsonFileName = "questions";

    [Header("UI")]
    public TextMeshProUGUI QuestionText;
    public TextMeshProUGUI FeedbackText;
    public TextMeshProUGUI ScoreText;
    public Button[] AnswerButtons;

    [Header("Results Panel")]
    public GameObject ResultsPanel;
    public TextMeshProUGUI ResultsTitleText;
    public TextMeshProUGUI ResultsSummaryText;

    [Header("Behavior")]
    public float FeedbackDelay = 2f;
    public float WinThreshold = 0.6f;

    private EnglishQuestionList QuizData;
    private int CurrentQuestion = 0;
    private int Points = 0;

    void Start()
    {
        if (AnswerButtons == null || AnswerButtons.Length != 4)
        {
            Debug.LogError("EnglishQuiz: Need exactly 4 answer buttons.");
            return;
        }
        if (QuestionText == null || FeedbackText == null || ScoreText == null)
        {
            Debug.LogError("EnglishQuiz: QuestionText, FeedbackText or ScoreText missing.");
            return;
        }

        LoadQuestions();

        if (QuizData == null || QuizData.questions == null || QuizData.questions.Length == 0)
        {
            Debug.LogError("EnglishQuiz: No questions loaded from JSON.");
            return;
        }

        ResultsPanel.SetActive(false);
        FeedbackText.text = "";
        UpdateScore();
        ShowQuestion();
    }

    void LoadQuestions()
    {
        TextAsset Ta = Resources.Load<TextAsset>(JsonFileName);
        if (Ta == null)
        {
            Debug.LogError($"EnglishQuiz: {JsonFileName}.json not in Assets/Resources/");
            return;
        }
        try
        {
            QuizData = JsonUtility.FromJson<EnglishQuestionList>(Ta.text);
        }
        catch (System.Exception E)
        {
            Debug.LogError("EnglishQuiz: JSON error: " + E.Message);
        }
    }

    void ShowQuestion()
    {
        if (CurrentQuestion >= QuizData.questions.Length)
        {
            EndQuiz();
            return;
        }

        FeedbackText.text = "";

        var Q = QuizData.questions[CurrentQuestion];
        QuestionText.text = Q.question;

        for (int I = 0; I < AnswerButtons.Length; I++)
        {
            var Btn = AnswerButtons[I];
            Btn.onClick.RemoveAllListeners();

            if (Q.answers != null && I < Q.answers.Length)
            {
                Btn.gameObject.SetActive(true);
                Btn.interactable = true;

                var Label = Btn.GetComponentInChildren<TextMeshProUGUI>();
                if (Label != null) Label.text = Q.answers[I];

                var Idx = I;
                Btn.onClick.AddListener(() => OnAnswer(Idx));
            }
            else
            {
                Btn.gameObject.SetActive(false);
            }
        }
    }

    void OnAnswer(int Index)
    {
        foreach (var B in AnswerButtons) B.interactable = false;

        var Correct = QuizData.questions[CurrentQuestion].correctIndex;

        if (Index == Correct)
        {
            FeedbackText.text = "Correct!";
            FeedbackText.color = new Color(0.1f, 0.7f, 0.1f);
            Points++;
        }
        else
        {
            FeedbackText.text = "Wrong! Answer: " + QuizData.questions[CurrentQuestion].answers[Correct];
            FeedbackText.color = new Color(0.85f, 0.1f, 0.1f);
        }

        UpdateScore();
        StartCoroutine(AdvanceAfterDelay(FeedbackDelay));
    }

    IEnumerator AdvanceAfterDelay(float Delay)
    {
        yield return new WaitForSeconds(Delay);
        FeedbackText.text = "";
        CurrentQuestion++;
        ShowQuestion();
    }

    void EndQuiz()
    {
        QuestionText.text = "";
        FeedbackText.text = "";
        foreach (var B in AnswerButtons) B.gameObject.SetActive(false);

        float Percentage = (float)Points / QuizData.questions.Length;

        if (Percentage >= WinThreshold)
        {
            Win();
        }
        else
        {
            Lose();
        }
    }

    void Win()
    {
        ResultsPanel.SetActive(true);
        ResultsTitleText.text = "You passed!";
        ResultsSummaryText.text = "Score: " + Points + " / " + QuizData.questions.Length
                                + "\nWell done!";
    }

    void Lose()
    {
        ResultsPanel.SetActive(true);
        ResultsTitleText.text = "You failed!";
        ResultsSummaryText.text = "Score: " + Points + " / " + QuizData.questions.Length
                                + "\nYou needed " + Mathf.CeilToInt(QuizData.questions.Length * WinThreshold)
                                + " correct answers to pass.";
    }

    public void BackToMain()
    {
        SceneManager.LoadScene("EnglishQuizMinigame");
    }

    void UpdateScore()
    {
        ScoreText.text = "Score: " + Points + " / " + QuizData.questions.Length;
    }
}
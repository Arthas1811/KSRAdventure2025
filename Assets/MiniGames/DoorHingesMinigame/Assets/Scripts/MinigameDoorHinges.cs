using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MinigameDoorHinges : MonoBehaviour
{
    private int RemovedScrews = 0;

    //SCREW BUTTONS
    [Header("Screw Buttons")]
    public GameObject ScrewTopButton;
    public GameObject ScrewBottomButton;

    //HOLE IMAGES
    [Header("Hole Images")]
    public GameObject HoleTopImage;
    public GameObject HoleBottomImage;

    //UI
    [Header("Progress UI")]
    public GameObject ScrewIconOne;
    public GameObject ScrewIconTwo;
    public TextMeshProUGUI ScrewCounterText;

    //START
    void Start()
    {
        RemovedScrews = 0;
        ScrewCounterText.text = "0 / 2 screws";

        ScrewIconOne.SetActive(false);
        ScrewIconTwo.SetActive(false);

        HoleTopImage.SetActive(false);
        HoleBottomImage.SetActive(false);
    }

    //SCREW

    public void OnTopScrewRemoved()
    {
        ScrewTopButton.SetActive(false);
        HoleTopImage.SetActive(true);
        ScrewIconOne.SetActive(true);
        RegisterScrew();
    }

    public void OnBottomScrewRemoved()
    {
        ScrewBottomButton.SetActive(false);
        HoleBottomImage.SetActive(true);
        ScrewIconTwo.SetActive(true);
        RegisterScrew();
    }

    //LOGIC
    private void RegisterScrew()
    {
        RemovedScrews++;
        ScrewCounterText.text = RemovedScrews + " / 2 screws";

        if (RemovedScrews >= 2)
        {
            Win();
        }
    }

    //WIN & LOSE

    private void Win()
    {
        Debug.Log("Minigame won");
        SceneManager.LoadScene("main");
    }

    public void Lose()
    {
        SceneManager.LoadScene("main");
    }

    //BACK BUTTON
    public void OnBackButtonPressed()
    {
        Lose();
    }
}
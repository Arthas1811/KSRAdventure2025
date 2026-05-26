using System.ComponentModel;
using System.Net.Security;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Verkabeln : MonoBehaviour
{
    float timer = 0f;
    public GameObject blueEnd;
    public GameObject greenEnd;
    public GameObject orangeEnd;
    public GameObject purpleEnd;
    public GameObject redEnd;
    public GameObject yellowEnd;
    public GameObject blueCable;
    public GameObject greenCable;
    public GameObject orangeCable;
    public GameObject purpleCable;
    public GameObject redCable;
    public GameObject yellowCable;

    [Header("KSR Adventure Assignments")]
    JObject saveData;
    public SaveDataManager saveDataManager;
    void Start()
    {
        saveData = SaveDataManager.Instance.readData();
        RandomizeCables();
        SetEndPosition();
        //CheckOrder();
    }

    void Update()
    {
        NextPuzzle();
        ManageTime();
    }

    void RandomizeCables()
    {
        for (int i = 0; i < 6; i++)
        {
            GameState.cableOrder[i] = '\0';
        }
        foreach (char colour in GameState.colours)
        {
            int position = UnityEngine.Random.Range(0, 6);
            while (GameState.cableOrder[position] != '\0')
            {
                position = UnityEngine.Random.Range(0, 6);
            }
            GameState.cableOrder[position] = colour;
        }
    }

    void CheckOrder()
    {
        foreach (char colour in GameState.cableOrder)
        {
            Debug.Log(colour);
        }
    }

    void NextPuzzle()
    {
        if (GameState.blueSolved && GameState.greenSolved && GameState.orangeSolved && GameState.purpleSolved && GameState.redSolved && GameState.yellowSolved)
        {
            GameState.puzzlesSolved += 1;
            if (GameState.puzzlesSolved == 3)
            {
                win();
            }
            GameState.blueSolved = false;
            GameState.greenSolved = false;
            GameState.orangeSolved = false;
            GameState.purpleSolved = false;
            GameState.redSolved = false;
            GameState.yellowSolved = false;
            RandomizeCables();
            SetEndPosition();
            ResetCables();
        }
    }

    void SetEndPosition()
    {
        for (int position = 0; position < 6; position++)
        {
            if (GameState.cableOrder[position] == 'B') { blueEnd.transform.position = new Vector3(5.5f, 4f - position * 1.5f, 0f); }
            if (GameState.cableOrder[position] == 'G') { greenEnd.transform.position = new Vector3(5.4f, 4f - position * 1.5f, 0f); }
            if (GameState.cableOrder[position] == 'O') { orangeEnd.transform.position = new Vector3(5.5f, 4f - position * 1.5f, 0f); }
            if (GameState.cableOrder[position] == 'P') { purpleEnd.transform.position = new Vector3(5.5f, 4f - position * 1.5f, 0f); }
            if (GameState.cableOrder[position] == 'R') { redEnd.transform.position = new Vector3(5.4f, 4f - position * 1.5f, 0f); }
            if (GameState.cableOrder[position] == 'Y') { yellowEnd.transform.position = new Vector3(5.5f, 4f - position * 1.5f, 0f); }
        }
    }

    void ManageTime()
    {
        timer += Time.deltaTime;
        GameState.timeLeft = 45 - Mathf.CeilToInt(timer);
        if (timer > 45)
        {
            lose();
        }
    }

    void ResetCables()
    {
        blueCable.transform.position = new Vector3(-11.5f, 4f, 0f);
        blueCable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        greenCable.transform.position = new Vector3(-11.5f, 2.5f, 0f);
        greenCable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        orangeCable.transform.position = new Vector3(-11.5f, 1f, 0f);
        orangeCable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        purpleCable.transform.position = new Vector3(-11.5f, -0.5f, 0f);
        purpleCable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        redCable.transform.position = new Vector3(-11.5f, -2f, 0f);
        redCable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        yellowCable.transform.position = new Vector3(-11.5f, -3.5f, 0f);
        yellowCable.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    void win()
    {
        Debug.Log("Win");
        saveData["states"]["minigames"]["wiringMinigameCompleted"] = true;
        saveData["states"]["basement"]["h013DoorOpen"] = true;
        SaveDataManager.Instance.saveData(saveData);
        SceneManager.LoadScene("main");
    }
    void lose()
    {
        Debug.Log("Lose");
        SceneManager.LoadScene("main");
    }
}

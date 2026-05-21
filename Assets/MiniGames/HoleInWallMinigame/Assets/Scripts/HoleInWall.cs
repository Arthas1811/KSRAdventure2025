using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class HoleInWall : MonoBehaviour
{

    [Header("Background Images")]
    public RawImage noHoleBackground;
    public RawImage smallHoleBackground;
    public RawImage middleHoleBackground;
    public RawImage bigHoleBackground;
    public RawImage holeBackground;
    public Image hammerImage;
    public Image drillImage;
    private GameObject obj;

    [Header("Drills and Hammers")]
    public List<DrillButtons> firstStageDrills;
    public List<HammerButtons> secondStageHammers;
    public List<HammerButtons> thirdStageHammers;
    public List<DrillButtons> fourthStageDrills;
    public List<HammerButtons> fifthStageHammers;
    public List<DrillButtons> sixthStageDrills;
    public List<HammerButtons> seventhStageHammers;

    [Header("Other Gameobjects")]
    public Button quitButton;
    public GameObject explanationPanel;
    public Button understandBtn;

    [Header("Settings")]
    public float drillTime = 1.5f; // in seconds
    public int currentStageId = 0; // Stage number -1
    public bool stageCompleted = false;
    public bool mouseOutOfBounds = false;
    public bool isDrilling = false;
    private string saveString;
    [Header("Scripts")]
    public DrillSFX drillSFX;

    [Header("KSR Adventure Assignments")]
    JObject saveData;
    public SaveDataManager saveDataManager;

    void Start()
    {
        saveData = SaveDataManager.Instance.readData();
        obj = GameObject.Find("NoHoleBackground");
        noHoleBackground = obj.GetComponent<RawImage>();
        obj = GameObject.Find("SmallHoleBackground");
        smallHoleBackground = obj.GetComponent<RawImage>();
        obj = GameObject.Find("MiddleHoleBackground");
        middleHoleBackground = obj.GetComponent<RawImage>();
        obj = GameObject.Find("BigHoleBackground");
        bigHoleBackground = obj.GetComponent<RawImage>();
        obj = GameObject.Find("HoleBackground");
        holeBackground = obj.GetComponent<RawImage>();
        obj = GameObject.Find("HammerImage");
        hammerImage = obj.GetComponent<Image>();
        obj = GameObject.Find("DrillImage");
        drillImage = obj.GetComponent<Image>();

        hammerImage.gameObject.SetActive(false);
        drillImage.gameObject.SetActive(false);
        Cursor.visible = true;


        // Hide all hammers
        foreach (var hammer in secondStageHammers)
        {
            hammer.button.interactable = false;
            hammer.button.gameObject.SetActive(false);
        }
        foreach (var hammer in thirdStageHammers)
        {
            hammer.button.interactable = false;
            hammer.button.gameObject.SetActive(false);
        }
        foreach (var hammer in fifthStageHammers)
        {
            hammer.button.interactable = false;
            hammer.button.gameObject.SetActive(false);
        }
        foreach (var hammer in seventhStageHammers)
        {
            hammer.button.interactable = false;
            hammer.button.gameObject.SetActive(false);
        }

        saveString = (string)saveData["states"]["minigames"]["holeInWallSaveString"];
        LoadSaveString(saveString);
    }

    void Update()
    {
        if (currentStageId == 0)
        {
            FirstStageDrilling();
        }
        else if (currentStageId == 1)
        {
            SecondStageHammering();
        }
        else if (currentStageId == 2)
        {
            ThirdStageHammering();
        }
        else if (currentStageId == 3)
        {
            FourthStageDrilling();
        }
        else if (currentStageId == 4)
        {
            FifthStageHammering();
        }
        else if (currentStageId == 5)
        {
            SixthStageDrilling();
        }
        else if (currentStageId == 6)
        {
            SeventhStageHammering();
        }
    }

    // Stagefunctions
    void ChangeStage()
    {
        currentStageId++;
        stageCompleted = false;
        UpdateToolImage();
        if (currentStageId == 1)
        {
            foreach (var hammer in secondStageHammers)
            {
                hammer.button.interactable = true;
                hammer.button.gameObject.SetActive(true);
            }
        }

        else if (currentStageId == 2)
        {
            noHoleBackground.gameObject.SetActive(false);
            foreach (var hammer in thirdStageHammers)
            {
                hammer.button.interactable = true;
                hammer.button.gameObject.SetActive(true);
                hammer.hits = 3;
            }
        }

        else if (currentStageId == 3)
        {
            smallHoleBackground.gameObject.SetActive(false);
            drillTime = drillTime / 2;
        }

        else if (currentStageId == 4)
        {
            foreach (var hammer in fifthStageHammers)
            {
                hammer.button.interactable = true;
                hammer.button.gameObject.SetActive(true);
                hammer.hits = 2;
            }
        }

        else if (currentStageId == 5)
        {
            middleHoleBackground.gameObject.SetActive(false);
        }

        else if (currentStageId == 6)
        {
            foreach (var hammer in seventhStageHammers)
            {
                hammer.button.interactable = true;
                hammer.button.gameObject.SetActive(true);
                hammer.hits = 2;
            }
        }

        else if (currentStageId == 7)
        {
            bigHoleBackground.gameObject.SetActive(false);
            Win();
        }

    }

    void UpdateToolImage()
    {
        bool hammerStage = (currentStageId == 1 || currentStageId == 2 || currentStageId == 4 || currentStageId == 6);
        drillImage.gameObject.SetActive(!hammerStage);
        hammerImage.gameObject.SetActive(hammerStage);
    }

    void FirstStageDrilling()
    {
        stageCompleted = true;
        foreach (var drill in firstStageDrills)
        {
            if (drill.isHolding)
            {
                drill.holdingTime += Time.deltaTime;

                // complete Hole
                if (drill.holdingTime >= drillTime)
                {
                    drill.button.interactable = false;
                }
            }


            if (drill.button.interactable)
            {
                stageCompleted = false;
            }

        }
        if (stageCompleted)
        {
            ChangeStage();
        }
    }

    void SecondStageHammering()
    {
        stageCompleted = true;
        foreach (var hammer in secondStageHammers)
        {
            if (hammer.button.interactable)
            {
                stageCompleted = false;
            }
        }
        if (stageCompleted)
        {
            ChangeStage();
        }
    }

    void ThirdStageHammering()
    {
        stageCompleted = true;
        foreach (var hammer in thirdStageHammers)
        {
            if (hammer.button.interactable)
            {
                stageCompleted = false;
            }
        }
        if (stageCompleted)
        {
            ChangeStage();
        }
    }

    void FourthStageDrilling()
    {
        stageCompleted = true;
        foreach (var drill in fourthStageDrills)
        {
            if (drill.isHolding)
            {
                drill.holdingTime += Time.deltaTime;

                // complete Hole
                if (drill.holdingTime >= drillTime)
                {
                    drill.button.interactable = false;
                }
            }


            if (drill.button.interactable)
            {
                stageCompleted = false;
            }

        }
        if (stageCompleted)
        {
            ChangeStage();
        }
    }

    void FifthStageHammering()
    {
        stageCompleted = true;
        foreach (var hammer in fifthStageHammers)
        {
            if (hammer.button.interactable)
            {
                stageCompleted = false;
            }
        }
        if (stageCompleted)
        {
            ChangeStage();
        }
    }

    void SixthStageDrilling()
    {
        stageCompleted = true;
        foreach (var drill in sixthStageDrills)
        {
            if (drill.isHolding)
            {
                drill.holdingTime += Time.deltaTime;

                // complete Hole
                if (drill.holdingTime >= drillTime)
                {
                    drill.button.interactable = false;
                }
            }


            if (drill.button.interactable)
            {
                stageCompleted = false;
            }

        }
        if (stageCompleted)
        {
            ChangeStage();
        }
    }

    void SeventhStageHammering()
    {
        stageCompleted = true;
        foreach (var hammer in seventhStageHammers)
        {
            if (hammer.button.interactable)
            {
                stageCompleted = false;
            }
        }
        if (stageCompleted)
        {
            ChangeStage();
        }
    }

    // Button Interactions
    public void StartDrilling(Button drillBtn)
    {
        if (currentStageId == 0)
        {
            foreach (var drill in firstStageDrills)
            {
                if (drill.button == drillBtn && drill.button.interactable)
                {
                    drill.isHolding = true;
                    isDrilling = true;
                }
            }
        }
        else if (currentStageId == 3)
        {
            foreach (var drill in fourthStageDrills)
            {
                if (drill.button == drillBtn && drill.button.interactable)
                {
                    drill.isHolding = true;
                    isDrilling = true;
                }
            }
        }
        else if (currentStageId == 5)
        {
            foreach (var drill in sixthStageDrills)
            {
                if (drill.button == drillBtn && drill.button.interactable)
                {
                    drill.isHolding = true;
                    isDrilling = true;
                }
            }
        }

        if (isDrilling)
        {
            drillSFX.SFXStart();
        }
    }

    public void StopDrilling(Button drillBtn)
    {
        if (currentStageId == 0)
        {
            foreach (var drill in firstStageDrills)
            {
                if (drill.button == drillBtn)
                {
                    drill.isHolding = false;
                }
            }
        }
        else if (currentStageId == 3)
        {
            foreach (var drill in fourthStageDrills)
            {
                if (drill.button == drillBtn)
                {
                    drill.isHolding = false;
                }
            }
        }
        else if (currentStageId == 5)
        {
            foreach (var drill in sixthStageDrills)
            {
                if (drill.button == drillBtn)
                {
                    drill.isHolding = false;
                }
            }
        }
        drillSFX.SFXEnd();
    }

    public void HitHammer(Button hammerBtn)
    {
        if (currentStageId == 1)
        {
            foreach (var hammer in secondStageHammers)
            {
                if (hammer.button == hammerBtn && hammer.button.interactable)
                {
                    hammer.hitCount++;
                    if (hammer.hitCount >= hammer.hits)
                    {
                        hammer.button.interactable = false;
                    }
                }
            }
        }
        else if (currentStageId == 2)
        {
            foreach (var hammer in thirdStageHammers)
            {
                if (hammer.button == hammerBtn && hammer.button.interactable)
                {
                    hammer.hitCount++;
                    if (hammer.hitCount >= hammer.hits)
                    {
                        hammer.button.interactable = false;
                    }
                }
            }
        }
        else if (currentStageId == 4)
        {
            foreach (var hammer in fifthStageHammers)
            {
                if (hammer.button == hammerBtn && hammer.button.interactable)
                {
                    hammer.hitCount++;
                    if (hammer.hitCount >= hammer.hits)
                    {
                        hammer.button.interactable = false;
                    }
                }
            }
        }
        else if (currentStageId == 6)
        {
            foreach (var hammer in seventhStageHammers)
            {
                if (hammer.button == hammerBtn && hammer.button.interactable)
                {
                    hammer.hitCount++;
                    if (hammer.hitCount >= hammer.hits)
                    {
                        hammer.button.interactable = false;
                    }
                }
            }
        }
    }

    // Explanatory tutorial window, only open during first round
    public void Understand()
    {
        explanationPanel.SetActive(false);
        hammerImage.gameObject.SetActive(false);
        drillImage.gameObject.SetActive(true);
        Cursor.visible = false;
    }

    // Hides custom cursor if the leavebutton is hovered
    public void LeaveButtonHovered(bool hovered)
    {
        if (hovered)
        {
            hammerImage.gameObject.SetActive(false);
            drillImage.gameObject.SetActive(false);
            Cursor.visible = true;
        }
        else
        {
            if (currentStageId == 0 || currentStageId == 3 || currentStageId == 5)
            {
                drillImage.gameObject.SetActive(true);
            }
            else
            {
                hammerImage.gameObject.SetActive(false);
            }
            Cursor.visible = false;
        }
    }

    // Minigame conditions
    void Win()
    {
        saveData["states"]["basement"]["h025HoleOpen"] = true;
        SaveDataManager.Instance.saveData(saveData);

        Quit();
    }

    // Default leaving + Saving data + Switching scene
    public void Quit()
    {
        string saveString = SaveString();
        saveData["states"]["minigames"]["holeInWallSaveString"] = saveString;
        SaveDataManager.Instance.saveData(saveData);
        Cursor.visible = true;
        hammerImage.gameObject.SetActive(false);
        drillImage.gameObject.SetActive(false);
        SceneManager.LoadScene("main");
    }

    // savestring format: Stage ID|info for each button in scene
    // for hammer buttons: information is the # of clicks; separated by ,
    // for drill buttons: 0 for not completed 1 for completed
    string SaveString()
    {
        string saveString = currentStageId.ToString();
        saveString += "|";
        if (currentStageId == 0)
        {
            saveString += DrillSaveString(firstStageDrills);

        }
        else if (currentStageId == 1)
        {
            saveString += HammerSaveString(secondStageHammers);

        }
        else if (currentStageId == 2)
        {
            saveString += HammerSaveString(thirdStageHammers);
        }
        else if (currentStageId == 3)
        {
            saveString += DrillSaveString(fourthStageDrills);

        }
        else if (currentStageId == 4)
        {
            saveString += HammerSaveString(fifthStageHammers);
        }
        else if (currentStageId == 5)
        {
            saveString += DrillSaveString(sixthStageDrills);
        }
        else if (currentStageId == 6)
        {
            saveString += HammerSaveString(seventhStageHammers);
        }


        return saveString;
    }

    string HammerSaveString(IEnumerable<HammerButtons> hammers)
    {
        string hammerString = "";
        int hammerAmount = hammers.Count();
        foreach (var hammer in hammers)
        {
            hammerAmount--;
            hammerString += hammer.hitCount.ToString();
            if (hammerAmount > 0)
            {
                hammerString += ",";
            }
        }
        return hammerString;
    }

    string DrillSaveString(IEnumerable<DrillButtons> drills)
    {
        string drillString = "";
        foreach (var drill in drills)
        {
            if (drill.button.interactable)
            {
                drillString += "0";
            }
            else
            {
                drillString += "1";
            }
        }
        return drillString;
    }

    void LoadSaveString(string saveString = null)
    {
        if (string.IsNullOrEmpty(saveString) || saveString == "0|0000000000")
        {
            return;
        }
        Understand();
        string[] saveStringInformation = saveString.Split("|");
        int requiredStageID = int.Parse(saveStringInformation[0]);
        string buttonInformation = saveStringInformation[1];
        while (requiredStageID > currentStageId)
        {
            ChangeStage();
        }
        if (currentStageId == 0)
        {
            LoadDrillSaveString(firstStageDrills, buttonInformation);

        }
        else if (currentStageId == 1)
        {
            LoadHammerSaveString(secondStageHammers, buttonInformation);

        }
        else if (currentStageId == 2)
        {
            LoadHammerSaveString(thirdStageHammers, buttonInformation);
        }
        else if (currentStageId == 3)
        {
            LoadDrillSaveString(fourthStageDrills, buttonInformation);

        }
        else if (currentStageId == 4)
        {
            LoadHammerSaveString(fifthStageHammers, buttonInformation);
        }
        else if (currentStageId == 5)
        {
            LoadDrillSaveString(sixthStageDrills, buttonInformation);
        }
        else if (currentStageId == 6)
        {
            LoadHammerSaveString(seventhStageHammers, buttonInformation);
        }
    }

    void LoadHammerSaveString(IEnumerable<HammerButtons> hammers, string buttonInformation)
    {
        int hammerIndex = 0;
        string[] buttonSettings = buttonInformation.Split(",");
        foreach (var hammer in hammers)
        {
            int buttonSetting = int.Parse(buttonSettings[hammerIndex]);
            hammer.hitCount = buttonSetting;
            if (hammer.hitCount >= hammer.hits)
            {
                hammer.button.interactable = false;
            }
            hammerIndex++;
        }
    }
    void LoadDrillSaveString(IEnumerable<DrillButtons> drills, string buttonInformation)
    {
        int drillIndex = 0;
        foreach (var drill in drills)
        {
            int buttonSetting = (int)buttonInformation[drillIndex] - '0';
            drill.button.interactable = (buttonSetting != 1);
            drillIndex++;
        }
    }

}
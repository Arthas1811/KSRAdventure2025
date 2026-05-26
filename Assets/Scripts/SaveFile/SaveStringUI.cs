using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveStringUI : MonoBehaviour
{
    public TMP_InputField exportStringField;
    public TMP_InputField importStringField;

    public void GenerateAndCopySaveString()
    {
        string saveString = SaveDataManager.Instance.GetExportString();
        exportStringField.text = saveString;
        GUIUtility.systemCopyBuffer = saveString; 
    }

    public void LoadImportedSaveString()
    {
        string saveString = importStringField.text;
        bool success = SaveDataManager.Instance.ImportSaveString(saveString);

        if (success)
        {
            SceneManager.LoadScene("main");
        }
        else
        {
            Debug.LogError("Invalid save string");
        }
    }
}
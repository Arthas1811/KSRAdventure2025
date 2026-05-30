using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveStringUI : MonoBehaviour
{
    public TMP_InputField exportStringField;
    public TMP_InputField importStringField;

    public GameObject enterButton;
    public GameObject copyButton;
    string saveString;

    private void OnEnable()
    {
        if (SaveDataManager.Instance != null && exportStringField != null)
        {
            GenerateSaveString();
        }
    }

    public void GenerateSaveString()
    {
        saveString = SaveDataManager.Instance.GetExportString();
        exportStringField.text = saveString;
    }

    public void LoadImportedSaveString()
    {
        saveString = (importStringField != null ? importStringField.text : string.Empty).Trim();

        if (string.IsNullOrEmpty(saveString))
        {
            Debug.LogError("Import save string is empty");
            return;
        }

        bool success = SaveDataManager.Instance.ImportSaveString(saveString);

        if (success)
        {
            SceneManager.sceneLoaded += OnSceneLoadedAfterImport;
            SceneManager.LoadScene("main");
        }
        else
        {
            Debug.LogError("Invalid save string");
        }
    }

    private static void OnSceneLoadedAfterImport(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedAfterImport;

        // Persistent (DontDestroyOnLoad) singletons survive the scene reload and
        // therefore never re-run Start(); force them to re-sync from the
        // freshly-imported save data here.
        if (InventoryState.Instance != null)
            InventoryState.Instance.Clear();

        if (Inventory.Instance != null)
            Inventory.Instance.LoadFromSave();
    }

    public void CopySaveString()
    {
        if (saveString != null)
        {
            GUIUtility.systemCopyBuffer = saveString;
        }
    }
}
using UnityEngine;
using TMPro;

public class UITextUpdater : MonoBehaviour
{
    public TextMeshProUGUI textField;

    void Start()
    {
        if (InstructionsManager.Instance != null)
        {
            InstructionsManager.Instance.OnTextChanged += UpdateText;
        }
    }
    void OnDestroy()
    {
        if (InstructionsManager.Instance != null)
        {
            InstructionsManager.Instance.OnTextChanged -= UpdateText;
        }
    }
    void UpdateText(string newText)
    {
        if (textField != null)
        {
            textField.text = newText;
        }
    }
}
using UnityEngine;

public class ButtonTest : MonoBehaviour
{
    public string buttonName;

    public void OnButtonClicked()
    {
        Debug.Log("Button " + buttonName + " wurde gedrückt");
    }
}
using UnityEngine;
using System.Collections;

public class PanelSwitcher : MonoBehaviour
{
    public string targetMessage;
    public float delay = 3f;

    public GameObject oldPanel;
    public GameObject oldPanel2;
    public GameObject newPanel;

    private bool alreadySwitched = false;

    void Start()
    {
        if (InstructionsManager.Instance != null)
        {
            InstructionsManager.Instance.OnTextChanged += CheckMessage;
        }
    }

    void OnDestroy()
    {
        if (InstructionsManager.Instance != null)
        {
            InstructionsManager.Instance.OnTextChanged -= CheckMessage;
        }
    }

    void CheckMessage(string message)
    {
        Debug.Log("PanelSwitcher empfangen: " + message);

        if (!alreadySwitched && message.Trim() == targetMessage.Trim())
        {
            alreadySwitched = true;
            StartCoroutine(SwitchAfterDelay());
        }
    }

    IEnumerator SwitchAfterDelay()
    {
        Debug.Log("Panelwechsel startet in " + delay + " Sekunden.");

        yield return new WaitForSeconds(delay);

        if (oldPanel != null)
        {
            oldPanel.SetActive(false);
            oldPanel2.SetActive(false);
        }

        if (newPanel != null)
        {
            newPanel.SetActive(true);
        }


        Debug.Log("Panel gewechselt.");
    }
}
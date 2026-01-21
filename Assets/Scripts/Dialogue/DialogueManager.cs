using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public GameObject dialogueCanvas;
    public GameObject inventoryPanel;
    public GameObject inventoryBackground;
    public GameObject navigationUI;

    public DialogueUI dialogueUI;

    private DialogueData currentDialogue;
    private DialogueNode currentNode;

    public Click main;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartDialogue(string fileName)
    {
        main.dialogueOpen = true;

        TextAsset jsonFile = Resources.Load<TextAsset>("Dialogue/library_dialogue/" + fileName);
        if (jsonFile == null)
        {
            Debug.LogError("Dialogue file not found: " + fileName);
            return;
        }

        currentDialogue = Newtonsoft.Json.JsonConvert.DeserializeObject<DialogueData>(jsonFile.text);
        if (currentDialogue == null || currentDialogue.nodes == null || currentDialogue.nodes.Count == 0)
        {
            Debug.LogError("Dialogue invalid or empty");
            return;
        }

        dialogueCanvas.SetActive(true);

        dialogueUI.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();

        inventoryPanel.SetActive(false);
        inventoryBackground.SetActive(false);
        navigationUI.SetActive(false);

        GoToNode(currentDialogue.nodes[0].id);
    }

    public void GoToNode(int id)
    {
        if (id == -1)
        {
            EndDialogue();
            return;
        }

        currentNode = currentDialogue.GetNode(id);

        if (currentNode == null)
        {
            Debug.LogError("Dialogue node not found: " + id);
            EndDialogue();
            return;
        }

        dialogueUI.ShowNode(currentNode);
    }

    public void ChooseOption(int index)
    {
        if (currentNode == null)
            return;

        if (currentNode.choices == null || index < 0 || index >= currentNode.choices.Count)
        {
            Debug.LogError("Invalid dialogue choice index");
            return;
        }

        GoToNode(currentNode.choices[index].next);
    }

    public void EndDialogue()
    {
        main.dialogueOpen = false;
        currentDialogue = null;
        currentNode = null;

        dialogueUI.Hide();
        dialogueCanvas.SetActive(false);
        inventoryPanel.SetActive(true);
        inventoryBackground.SetActive(true);
        navigationUI.SetActive(true);
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text speakerText;
    public TMP_Text dialogueText;
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;
    public Button skipButton;
    public GameObject continueIndicator;

    [Header("Typewriter Settings")]
    public float typeSpeed = 0.03f;

    private Coroutine typingCoroutine;
    private string fullLine;
    private bool isTyping = false;
    private bool waitingForClick = false;

    private DialogueNode currentNode;

    public Image portraitImage;

    void Awake()
    {
        skipButton.onClick.AddListener(OnSkipPressed);
        continueIndicator.SetActive(false);
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameObject.activeSelf)
            return;

        // Skip typewriter
        if (isTyping && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SkipTypewriter();
            return;
        }

        if (!isTyping && waitingForClick && Mouse.current.leftButton.wasPressedThisFrame)
        {
            waitingForClick = false;
            DialogueManager.Instance.GoToNode(currentNode.next);
        }
    }

    public void ShowNode(DialogueNode node)
    {
        currentNode = node;

        gameObject.SetActive(true);
        skipButton.gameObject.SetActive(true);
        continueIndicator.SetActive(false);

        speakerText.text = node.speaker;

        // image handling
        if (!string.IsNullOrEmpty(node.img_path))
        {
            Sprite loadedSprite = Resources.Load<Sprite>(node.img_path);

            if (loadedSprite != null)
            {
                portraitImage.sprite = loadedSprite;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Could not load sprite at path: " + node.img_path);
            }
        }


        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        fullLine = node.text;
        typingCoroutine = StartCoroutine(TypeLine(fullLine));

        foreach (Transform t in choicesContainer)
            Destroy(t.gameObject);

        if (node.choices != null && node.choices.Count > 0)
        {
            waitingForClick = false;

            foreach (var choice in node.choices)
            {
                var button = Instantiate(choiceButtonPrefab, choicesContainer);
                button.GetComponentInChildren<TMP_Text>().text = choice.text;

                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    DialogueManager.Instance.GoToNode(choice.next);
                });
            }
        }
        else
        {
            waitingForClick = true;
        }
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        continueIndicator.SetActive(false);

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        continueIndicator.SetActive(true);
    }

    void SkipTypewriter()
    {
        StopCoroutine(typingCoroutine);
        dialogueText.text = fullLine;
        isTyping = false;
        continueIndicator.SetActive(true);
    }

    void OnSkipPressed()
    {
        DialogueManager.Instance.EndDialogue();
    }

    public void Hide()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = "";
        isTyping = false;
        waitingForClick = false;

        continueIndicator.SetActive(false);
        skipButton.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}

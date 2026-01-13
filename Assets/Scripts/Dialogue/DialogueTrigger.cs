using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public string dialogue;
    private void OnMouseDown()
    {
        DialogueManager.Instance.StartDialogue(dialogue);
    }
}



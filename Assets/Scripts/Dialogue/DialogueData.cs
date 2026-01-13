using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueData
{
    public string id;
    public List<DialogueNode> nodes;

    public DialogueNode GetNode(int id)
    {
        return nodes.Find(node => node.id == id);
    }
}

[System.Serializable]
public class DialogueNode
{
    public int id;
    public string speaker;
    public string text;
    public List<DialogueChoice> choices;
    public int next;
}

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public int next;
}

using UnityEngine;

public class ToggleObject : MonoBehaviour
{
    public GameObject targetObject;

    public void ToggleVisibility()
    {
        targetObject.SetActive(!targetObject.activeSelf);
    }

}
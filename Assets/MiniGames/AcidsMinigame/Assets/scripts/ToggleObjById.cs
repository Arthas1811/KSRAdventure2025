using UnityEngine;

public class ObjectToggler : MonoBehaviour
{
    public void ToggleByTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objects)
        {
            obj.SetActive(!obj.activeInHierarchy);
        }
    }
}
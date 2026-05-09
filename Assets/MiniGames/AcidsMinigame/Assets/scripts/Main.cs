using UnityEngine;

public class Main : MonoBehaviour
{

    
    [Header("Objekt das sichtbar werden soll")]
    public GameObject resultObject;

    // Kann später aus beliebigem Code aufgerufen werden
    public void ShowObject()
    {
        if (resultObject != null)
        {
            resultObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Kein Objekt zugewiesen!");
        }
    }

    // Optional: wieder ausblenden
    public void HideObject()
    {
        if (resultObject != null)
        {
            resultObject.SetActive(false);
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ToggleObjects[] toggles;
    void Start()
    {
        //ShowObject();
    }

    // Update is called once per frame
    void Update()
    {

    }


    public class ToggleObjects : MonoBehaviour
    {
        public GameObject targetObject;
        private void Toggle(string ButtonType)
        {
            if (targetObject != null)
            {
                targetObject.SetActive(!targetObject.activeSelf);

            }
        }
    }


}

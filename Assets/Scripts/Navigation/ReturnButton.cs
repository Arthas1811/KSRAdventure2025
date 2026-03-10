using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void ChangeScene(string sceneName)
    {
        Debug.Log("BUTTON CLICKED, loading: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}   
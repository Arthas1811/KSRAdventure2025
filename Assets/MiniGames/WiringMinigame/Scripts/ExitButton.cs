using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public void Exit()
    {
        SceneManager.LoadScene("main");
        //Debug.Log("Exit");
    }
}

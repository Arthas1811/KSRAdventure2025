using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;
    public GameObject panel;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Exit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("main");
    }
}

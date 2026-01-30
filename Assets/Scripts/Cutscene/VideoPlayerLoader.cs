using UnityEngine;
using System.Collections;

public class VideoPlayerLoader : MonoBehaviour
{
    public UnityEngine.Video.VideoPlayer videoPlayer;

    public UnityEngine.UI.Button skipButton;
    public UnityEngine.UI.Button continueButton;

    void Start()
    {
        skipButton.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(false);

        skipButton.onClick.AddListener(Skip);
        continueButton.onClick.AddListener(Continue);

        if (!string.IsNullOrEmpty(SceneData.VideoPath))
        {
            videoPlayer.url = SceneData.VideoPath;
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("Video path is not correct or not set in SceneData.");
        }

        videoPlayer.loopPointReached += OnVideoEnd;
    }

    public void Skip()
    {
        videoPlayer.Pause();
        videoPlayer.frame = (long)videoPlayer.frameCount - 1;

        skipButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(true);
    }

    public void Continue()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("main");
    }

    void OnVideoEnd(UnityEngine.Video.VideoPlayer vp)
    {
        skipButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(true);
    }

}

// Script taken from KSRAdventure2025

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// Manages full-screen video playback and transitions for cutscenes
public class VideoPlayerLoader : MonoBehaviour
{
    public static string VideoPath;
    public UnityEngine.Video.VideoPlayer VideoPlayer;
    public UnityEngine.UI.Button SkipButton;
    public UnityEngine.UI.Button ContinueButton;

    private void Awake()
    {
        // Hide UI buttons initially so they don't interrupt the video
        if (SkipButton != null)
            SkipButton.gameObject.SetActive(false);

        if (ContinueButton != null)
            ContinueButton.gameObject.SetActive(false);

        // Ensure the camera background is black to prevent flashing weird colors while the video buffers
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = Color.black;
        }
    }

    void Start()
    {
        if (ContinueButton != null)
        {
            ContinueButton.gameObject.SetActive(false);
            ContinueButton.onClick.AddListener(Continue);
        }

        // Setup the video player with the path passed from the previous scene
        if (!string.IsNullOrEmpty(VideoPlayerLoader.VideoPath) && VideoPlayer != null)
        {
            VideoPlayer.playOnAwake = false;
            VideoPlayer.waitForFirstFrame = true;
            VideoPlayer.url = VideoPlayerLoader.VideoPath;
            VideoPlayer.Prepare();
        }

        if (VideoPlayer != null)
        {
            VideoPlayer.prepareCompleted += OnVideoPrepared;
            VideoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    // Wait until the video is fully prepared to avoid stuttering
    private void OnVideoPrepared(UnityEngine.Video.VideoPlayer vp)
    {
        vp.Play();
    }

    public void Continue()
    {
        SceneManager.LoadScene("main");
    }

    // Show the continue button once the video finishes
    void OnVideoEnd(UnityEngine.Video.VideoPlayer vp)
    {
        if (SkipButton != null)
            SkipButton.gameObject.SetActive(false);

        if (ContinueButton != null)
            ContinueButton.gameObject.SetActive(true);
    }
}
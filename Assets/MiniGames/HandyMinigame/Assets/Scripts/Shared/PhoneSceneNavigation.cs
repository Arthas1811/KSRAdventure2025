using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//loads phone scenes
public class PhoneSceneNavigation : MonoBehaviour
{
    public const string PhoneHomeScene = "HandyMinigame";

    public const string MailboxScene = "HandyMinigameMailbox";

    public const string BlockBlastScene = "HandyMinigameBlockBlast";

    public const string TetrisScene = "HandyMinigameTetris";

    public const string MainScene = "main";

    public const Key PhoneToggleKey = Key.H;

    private static readonly string[] PhoneScenes =
    {
        PhoneHomeScene,
        MailboxScene,
        BlockBlastScene,
        TetrisScene
    };

    private static bool eventsRegistered;
    private static string currentOverlaySceneName;

    public static bool IsPhoneOverlayOpen { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        IsPhoneOverlayOpen = false;
        eventsRegistered = false;
        currentOverlaySceneName = null;
    }

    public static void OpenPhoneOverlay()
    {
        OpenPhoneOverlay(PhoneHomeScene);
    }

    public static void TogglePhoneOverlay()
    {
        if (IsPhoneOverlayOpen)
        {
            ClosePhoneOverlay();
            return;
        }

        OpenPhoneOverlay();
    }

    public static void OpenPhoneOverlay(string sceneName)
    {
        if (!IsPhoneScene(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        EnsureSceneEventsRegistered();
        IsPhoneOverlayOpen = true;

        if (currentOverlaySceneName == sceneName)
        {
            return;
        }

        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            currentOverlaySceneName = sceneName;
            return;
        }

        if (!string.IsNullOrEmpty(currentOverlaySceneName)
            && SceneManager.GetSceneByName(currentOverlaySceneName).isLoaded)
        {
            SwitchOverlayScene(sceneName);
            return;
        }

        currentOverlaySceneName = sceneName;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public static bool IsPhoneScene(string sceneName)
    {
        for (int i = 0; i < PhoneScenes.Length; i++)
        {
            if (PhoneScenes[i] == sceneName)
            {
                return true;
            }
        }

        return false;
    }

    //loads phone home
    public void LoadPhoneHome()
    {
        LoadScene(PhoneHomeScene);
    }

    //loads mailbox
    public void LoadMailbox()
    {
        LoadScene(MailboxScene);
    }

    //loads block blast
    public void LoadBlockBlast()
    {
        LoadScene(BlockBlastScene);
    }

    //loads tetris
    public void LoadTetris()
    {
        LoadScene(TetrisScene);
    }

    //loads main scene
    public void LoadMain()
    {
        LoadScene(MainScene);
    }

    //loads scene
    public void LoadScene(string sceneName)
    {
        if (IsPhoneOverlayOpen)
        {
            if (sceneName == MainScene)
            {
                ClosePhoneOverlay();
                return;
            }

            if (IsPhoneScene(sceneName))
            {
                SwitchOverlayScene(sceneName);
                return;
            }
        }

        SceneManager.LoadScene(sceneName);
    }

    private static void SwitchOverlayScene(string sceneName)
    {
        EnsureSceneEventsRegistered();

        if (!IsPhoneScene(sceneName))
        {
            return;
        }

        IsPhoneOverlayOpen = true;

        if (currentOverlaySceneName == sceneName)
        {
            return;
        }

        Scene targetScene = SceneManager.GetSceneByName(sceneName);
        if (!targetScene.isLoaded)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOperation != null)
            {
                loadOperation.completed += _ => UnloadOtherPhoneScenes(sceneName);
            }
        }
        else
        {
            UnloadOtherPhoneScenes(sceneName);
        }

        currentOverlaySceneName = sceneName;
    }

    private static void UnloadOtherPhoneScenes(string sceneName)
    {
        for (int i = 0; i < PhoneScenes.Length; i++)
        {
            string phoneSceneName = PhoneScenes[i];
            if (phoneSceneName == sceneName)
            {
                continue;
            }

            Scene loadedScene = SceneManager.GetSceneByName(phoneSceneName);
            if (loadedScene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(loadedScene);
            }
        }
    }

    public static void ClosePhoneOverlay()
    {
        Scene mainScene = SceneManager.GetSceneByName(MainScene);
        if (!mainScene.isLoaded)
        {
            IsPhoneOverlayOpen = false;
            currentOverlaySceneName = null;
            SceneManager.LoadScene(MainScene);
            return;
        }

        bool unloadStarted = false;
        for (int i = 0; i < PhoneScenes.Length; i++)
        {
            Scene loadedScene = SceneManager.GetSceneByName(PhoneScenes[i]);
            if (loadedScene.isLoaded)
            {
                unloadStarted = true;
                SceneManager.UnloadSceneAsync(loadedScene);
            }
        }

        SceneManager.SetActiveScene(mainScene);
        currentOverlaySceneName = null;
        if (!unloadStarted)
        {
            IsPhoneOverlayOpen = false;
        }
    }

    private static void EnsureSceneEventsRegistered()
    {
        if (eventsRegistered)
        {
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        eventsRegistered = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single && !IsPhoneScene(scene.name))
        {
            IsPhoneOverlayOpen = false;
            currentOverlaySceneName = null;
            return;
        }

        if (!IsPhoneOverlayOpen || !IsPhoneScene(scene.name))
        {
            return;
        }

        currentOverlaySceneName = scene.name;
        ConfigureSceneForOverlay(scene);
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        if (!IsPhoneScene(scene.name))
        {
            return;
        }

        if (scene.name == currentOverlaySceneName)
        {
            currentOverlaySceneName = null;
        }

        bool anyPhoneSceneLoaded = false;
        for (int i = 0; i < PhoneScenes.Length; i++)
        {
            if (SceneManager.GetSceneByName(PhoneScenes[i]).isLoaded)
            {
                anyPhoneSceneLoaded = true;
                break;
            }
        }

        if (!anyPhoneSceneLoaded)
        {
            IsPhoneOverlayOpen = false;
        }
    }

    private static void ConfigureSceneForOverlay(Scene scene)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            Camera[] cameras = rootObjects[i].GetComponentsInChildren<Camera>(true);
            for (int cameraIndex = 0; cameraIndex < cameras.Length; cameraIndex++)
            {
                Camera phoneCamera = cameras[cameraIndex];
                phoneCamera.enabled = false;
                if (phoneCamera.CompareTag("MainCamera"))
                {
                    phoneCamera.tag = "Untagged";
                }
            }

            AudioListener[] listeners = rootObjects[i].GetComponentsInChildren<AudioListener>(true);
            for (int listenerIndex = 0; listenerIndex < listeners.Length; listenerIndex++)
            {
                listeners[listenerIndex].enabled = false;
            }

            Light[] lights = rootObjects[i].GetComponentsInChildren<Light>(true);
            for (int lightIndex = 0; lightIndex < lights.Length; lightIndex++)
            {
                lights[lightIndex].enabled = false;
            }
        }
    }
}

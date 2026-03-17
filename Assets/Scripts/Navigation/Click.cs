using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using UnityEngine.UI;

public class Click : MonoBehaviour
{
    public Camera cam;
    public Renderer sphere;
    private Dictionary<string, string> materialPaths = new Dictionary<string, string>();
    private Dictionary<string, Material> materialCache = new Dictionary<string, Material>();
    private string currentImage = "";
    public GameObject hotspotPrefab;
    public GameObject polygonPrefab;
    private JObject data;
    private JObject saveData;
    private Dictionary<GameObject, string[]> hotspotActions = new Dictionary<GameObject, string[]>();
    private Dictionary<GameObject, string[]> hotspotRequirements = new Dictionary<GameObject, string[]>();
    private List<GameObject> polygons = new List<GameObject>();
    private Vector2 mouseOne;
    private Vector2 mouseTwo;
    private Vector2 deltaMouse;
    public float zoomDuration = 0.2f;
    public float FOV = 60f;
    public Inventory inventory;
    public bool inventoryOpen = false;
    public bool dialogueOpen = false;
    public Texture2D hoverCursorTexture;
    public Vector2 hoverCursorHotspot = Vector2.zero;
    public bool forceSoftwareCursor = true;
    public Key polygonToggleKey = Key.H;
    public bool showPolygons = false;
    public Key MoveForwardKey = Key.UpArrow;
    [Range(0f, 1f)] public float visiblePolygonAlpha = 0.35f;
    private bool isHoveringPolygon = false;
    public CameraMovement cameraMovement;
    public Image uiImage;
    public bool imageOpen = false;
    public GameObject closeImageButton;

    private string GetSaveDataPath()
    {
        return Path.Combine(Application.persistentDataPath, "saveData.json");
    }

    void openImage(string imagePath)
    {
        imageOpen = true;
        uiImage.gameObject.SetActive(true);
        closeImageButton.SetActive(true);
        uiImage.preserveAspect = true;

        Texture2D texture = LoadTextureFromPath(imagePath);
        if (texture == null)
        {
            Debug.LogError($"openImage: Texture konnte nicht geladen werden: {imagePath}");
            return;
        }
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        uiImage.sprite = sprite;
        Debug.Log("Opened image: " + imagePath);
    }

    public void closeImage()
    {
        imageOpen = false;
        uiImage.gameObject.SetActive(false);
        closeImageButton.SetActive(false);
        uiImage.sprite = null;
    }

    void hotspotInstantiation(string currentImage)
    {
        if (data == null || data[currentImage] == null) return;
        JArray customHotspots = data[currentImage]["customHotspots"] as JArray;
        if (customHotspots == null) return;

        foreach (var customHotspot in customHotspots)
        {
            float x = customHotspot["x"]?.ToObject<float>() ?? 0f;
            float y = customHotspot["y"]?.ToObject<float>() ?? 0f;
            float z = customHotspot["z"]?.ToObject<float>() ?? 0f;

            Vector3 pos = new Vector3(x, y, z);
            GameObject hsObj = Instantiate(hotspotPrefab, pos, Quaternion.identity);
            hsObj.transform.SetParent(this.transform, false);

            string[] actions = customHotspot["action"]?.ToString().Split(',').Select(s => s.Trim()).Where(s => s != "").ToArray()
                ?? new string[0];
            string[] requirements = customHotspot["requirements"]?.ToString().Split(',').Select(s => s.Trim()).Where(s => s != "").ToArray()
                ?? new string[0];
            hotspotActions[hsObj] = actions;
            hotspotRequirements[hsObj] = requirements;

            if (customHotspot["polygon"] != null)
            {
                var poly = Instantiate(polygonPrefab, this.transform);
                polygons.Add(poly);
            }
        }
    }

    IEnumerator updateImage(float duration, float startFOV, string currentImage, Dictionary<GameObject, string[]> hotspotActions, Dictionary<GameObject, string[]> hotspotRequirements)
    {
        float startTime = Time.time;
        float subtraction = 40;
        if (startFOV - subtraction < 1) subtraction = startFOV - 1;

        while ((Time.time - startTime) < duration)
        {
            float t = (Time.time - startTime) / duration;
            cam.fieldOfView = Mathf.Lerp(startFOV, startFOV - subtraction, t);
            yield return null;
        }
        cam.fieldOfView = startFOV;

        hotspotDestroy(hotspotActions.Keys.ToList());
        hotspotActions.Clear();
        hotspotRequirements.Clear();
        hotspotInstantiation(currentImage);
        setMaterial(currentImage);
    }

    void setMaterial(string currentImage)
    {
        Material mat = GetMaterialForImage(currentImage);
        if (mat != null)
            sphere.material = mat;
        else
            Debug.LogWarning($"setMaterial: Material für Bild {currentImage} konnte nicht gesetzt werden.");
    }

    Material GetMaterialForImage(string imageKey)
    {
        if (string.IsNullOrEmpty(imageKey))
            return null;

        if (!materialPaths.TryGetValue(imageKey, out string path) || string.IsNullOrEmpty(path))
        {
            if (data != null && data[imageKey] != null)
            {
                path = data[imageKey]?["states"]?["main"]?["path"]?.ToString();
                if (!string.IsNullOrEmpty(path))
                {
                    materialPaths[imageKey] = path;
                }
            }
        }

        if (string.IsNullOrEmpty(path))
            return null;

        if (materialCache.TryGetValue(path, out Material mat))
            return mat;

        mat = LoadMaterialFromPath(path);
        if (mat != null)
            materialCache[path] = mat;
        return mat;
    }

    void hotspotDestroy(IEnumerable<GameObject> hotspots)
    {
        foreach (var hotspot in hotspots)
        {
            if (hotspot != null)
            {
                Destroy(hotspot);
            }
        }
        foreach (var polygon in polygons)
        {
            if (polygon != null)
            {
                Destroy(polygon);
            }
        }
        polygons.Clear();
    }

    void updateState(string action, string currentImage)
    {
        action = action.Replace("data:", "");
        string[] parts = action.Split(':');
        if (parts.Length < 2) return;
        string newValueStr = parts[parts.Length - 1];
        string targetKey = parts[parts.Length - 2];

        JToken current = saveData;
        for (int i = 0; i < parts.Length - 2; i++)
        {
            string key = parts[i];
            if (current[key] != null)
            {
                current = current[key];
            }
            else
            {
                JObject next = new JObject();
                if (current is JObject obj)
                {
                    obj[key] = next;
                    current = next;
                }
            }
        }

        if (bool.TryParse(newValueStr, out bool boolValue))
        {
            ((JObject)current)[targetKey] = boolValue;
        }
        else if (int.TryParse(newValueStr, out int intValue))
        {
            ((JObject)current)[targetKey] = intValue;
        }
        else
        {
            ((JObject)current)[targetKey] = newValueStr;
        }

        saveDataInFile(saveData);
        Debug.Log($"Updated {targetKey} to {newValueStr}");
    }

    void saveDataInFile(JObject saveData)
    {
        string savePath = GetSaveDataPath();
        string saveJson = saveData.ToString();
        File.WriteAllText(savePath, saveJson);
    }

    private bool checkRequirements(string[] requirements)
    {
        if (requirements == null || requirements.Length == 0)
            return true;

        foreach (var requirement in requirements)
        {
            if (string.IsNullOrWhiteSpace(requirement))
                continue;

            if (requirement.StartsWith("item:"))
            {
                string itemData = requirement.Substring("item:".Length);
                if (inventory != null)
                {
                    bool hasItem = inventory.HasItem(itemData);
                    if (!hasItem) return false;
                }
                else
                {
                    if (!saveData.ContainsKey(itemData) || !saveData[itemData].ToObject<bool>())
                        return false;
                }
            }
            else
            {
                string[] parts = requirement.Split('=');
                if (parts.Length != 2) continue;
                string key = parts[0];
                string requiredValue = parts[1];

                JToken actual = saveData.SelectToken(key);
                if (actual == null || actual.ToString() != requiredValue)
                    return false;
            }
        }
        return true;
    }

    void completeAction(string action, string[] requirements, string currentImage, float zoomDuration, float FOV, Dictionary<GameObject, string[]> hotspotActions, Dictionary<GameObject, string[]> hotspotRequirements)
    {
        if (!checkRequirements(requirements))
            return;

        if (action.StartsWith("scene:"))
        {
            string sceneName = action.Split(':')[1];
            Debug.Log("Loading scene: " + sceneName);
            SceneManager.LoadScene(sceneName);
            return;
        }
        else if (action.StartsWith("item:"))
        {
            string[] parts = action.Split(':');
            if (parts.Length >= 3)
            {
                if (parts[1] == "add")
                    inventory?.add(parts[2]);
                else if (parts[1] == "remove")
                    inventory?.remove(parts[2]);
            }
        }
        else if (action.StartsWith("data:"))
        {
            updateState(action, currentImage);
        }
        else if (action.StartsWith("dialogue:"))
        {
            string id = action.Split(':')[1];
            DialogueManager.Instance.StartDialogue(id);
            return;
        }
        else if (action.StartsWith("location:"))
        {
            string[] parts = action.Split(':');
            string locationName = parts[1];
            string entryImage = parts.Length >= 3 ? parts[2] : null;
            LoadLocation(locationName, entryImage);
            return;
        }
        else if (action.StartsWith("sfx:"))
        {
            string clipName = action.Split(':')[1];
            AudioClip clip = LoadAudioClip($"Audio/SFX/{clipName}");
            AudioManager.Instance.PlaySFX(clip);
            return;
        }
        else if (action.StartsWith("music:"))
        {
            string clipName = action.Split(':')[1];
            AudioClip clip = LoadAudioClip($"Audio/Music/{clipName}");
            AudioManager.Instance.PlayMusic(clip);
            return;
        }
        else if (action.StartsWith("cutscene:"))
        {
            string videoName = action.Split(':')[1];
            openCutscene($"Videos/Cutscenes/{videoName}");
            return;
        }
        else if (action.StartsWith("image:"))
        {
            string imageName = action.Split(':')[1];
            openImage($"Images/Images/{imageName}");
            return;
        }
        else
        {
            currentImage = action;
            saveData["currentImage"] = currentImage;
            saveDataInFile(saveData);

            var states = data[currentImage]["states"] as JObject;
            JToken activeState = states["main"];
            foreach (var state in states)
            {
                if (state.Key == "main") continue;
                var req = state.Value["requirements"]?.ToObject<string[]>();
                if (checkRequirements(req))
                {
                    activeState = state.Value;
                }
            }

            float finalY = activeState["x"] != null ? activeState["x"].ToObject<float>() : cameraMovement.x;
            float finalX = activeState["y"] != null ? activeState["y"].ToObject<float>() : cameraMovement.y;
            if (activeState["x"] != null || activeState["y"] != null)
            {
                cameraMovement.setNewRotation(Mathf.Clamp(finalX, -90f, 90f), finalY);
            }

            if (activeState["fov"] != null)
            {
                this.FOV = activeState["fov"].ToObject<float>();
                cameraMovement.setNewFOV(this.FOV);
            }

            string bestPath = activeState["path"]?.ToString();
            if (!string.IsNullOrEmpty(bestPath))
            {
                Material newMat = LoadMaterialFromPath(bestPath);
                if (newMat != null)
                    sphere.material = newMat;
                else
                    setMaterial(currentImage);
            }
            else
            {
                setMaterial(currentImage);
            }

            hotspotDestroy(hotspotActions.Keys.ToList());
            hotspotActions.Clear();
            hotspotRequirements.Clear();
            hotspotInstantiation(currentImage);
        }
    }

    void Start()
    {
        uiImage.gameObject.SetActive(false);
        closeImageButton.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        cam.fieldOfView = FOV;

        string savePath = GetSaveDataPath();
        if (!File.Exists(savePath))
        {
            string defaultSavePath = Path.Combine(Application.dataPath, "Scripts/SaveFile/saveData.json");
            if (File.Exists(defaultSavePath))
                File.Copy(defaultSavePath, savePath);
        }

        if (!File.Exists(savePath))
        {
            Debug.LogError("Start: saveData.json nicht gefunden.");
            saveData = new JObject();
        }
        else
        {
            string saveJson = File.ReadAllText(savePath);
            saveData = JObject.Parse(saveJson);
        }

        string locationName = saveData.ContainsKey("currentLocation") ? saveData["currentLocation"].ToString() : "start";
        string entryImage = saveData.ContainsKey("currentImage") ? saveData["currentImage"].ToString() : null;
        LoadLocation(locationName, entryImage);
    }

    void LoadLocation(string locationName, string entryImage = null)
    {
        TextAsset json = Resources.Load<TextAsset>($"Locations/{locationName}");
        if (json == null)
        {
            Debug.LogError($"Location JSON nicht gefunden: {locationName}");
            return;
        }

        data = JObject.Parse(json.text);

        if (data["meta"]?["music"] != null)
        {
            string musicName = data["meta"]["music"].ToString();
            AudioClip clip = LoadAudioClip($"Audio/{musicName}");
            AudioManager.Instance.PlayMusic(clip);
        }
        else
        {
            AudioManager.Instance.PlayMusic(null);
        }

        ClearMaterialCache();
        materialPaths.Clear();

        foreach (var image in data)
        {
            if (image.Key == "meta") continue;
            string path = image.Value["states"]?["main"]?["path"]?.ToString();
            if (!string.IsNullOrEmpty(path))
            {
                materialPaths[image.Key] = path;
            }
        }

        if (!string.IsNullOrEmpty(entryImage) && data[entryImage] != null)
            currentImage = entryImage;
        else
            currentImage = data.Properties().First().Name;

        hotspotDestroy(hotspotActions.Keys.ToList());
        hotspotActions.Clear();
        hotspotRequirements.Clear();
        hotspotInstantiation(currentImage);
        setMaterial(currentImage);

        saveData["currentLocation"] = locationName;
        saveData["currentImage"] = currentImage;
        saveDataInFile(saveData);
    }

    void Update()
    {
        if (!inventoryOpen && !imageOpen && !dialogueOpen && Keyboard.current != null && Keyboard.current[MoveForwardKey].wasPressedThisFrame)
        {
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenter);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            foreach (var hit in hits)
            {
                if (hotspotActions.TryGetValue(hit.collider.gameObject, out string[] actions) && hotspotRequirements.TryGetValue(hit.collider.gameObject, out string[] requirements))
                {
                    foreach (var action in actions)
                        completeAction(action, requirements, currentImage, zoomDuration, FOV, hotspotActions, hotspotRequirements);
                }
            }
        }

        if (Keyboard.current != null && Keyboard.current[polygonToggleKey].wasPressedThisFrame)
        {
            showPolygons = !showPolygons;
            UpdateAllPolygonVisuals();
        }

        UpdateHoverCursor();

        if (!inventoryOpen && !dialogueOpen && !imageOpen)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                mouseOne = Mouse.current.position.ReadValue();
            }
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                mouseTwo = Mouse.current.position.ReadValue();
                deltaMouse = mouseOne - mouseTwo;
                if (deltaMouse.magnitude < 5f)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        if (hotspotActions.TryGetValue(hit.collider.gameObject, out string[] actions) && hotspotRequirements.TryGetValue(hit.collider.gameObject, out string[] requirements))
                        {
                            foreach (var action in actions)
                                completeAction(action, requirements, currentImage, zoomDuration, FOV, hotspotActions, hotspotRequirements);
                        }
                    }
                }
            }

            Vector2 scroll = Mouse.current.scroll.ReadValue();
            if (scroll.y < 0)
            {
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView + 2f, 5f, 60f);
                FOV = cam.fieldOfView;
            }
            else if (scroll.y > 0)
            {
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - 2f, 5f, 60f);
                FOV = cam.fieldOfView;
            }
        }
    }

    private void UpdateHoverCursor()
    {
        if (inventoryOpen || dialogueOpen || imageOpen)
        {
            SetHoverCursor(false);
            return;
        }
    }

    private void SetHoverCursor(bool hoveringPolygon)
    {
        if (hoveringPolygon == isHoveringPolygon) return;
        isHoveringPolygon = hoveringPolygon;
        if (isHoveringPolygon && hoverCursorTexture != null)
            Cursor.SetCursor(hoverCursorTexture, hoverCursorHotspot, CursorMode.Auto);
        else
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void UpdateAllPolygonVisuals()
    {
        foreach (var poly in polygons)
        {
            if (poly == null) continue;
            var rend = poly.GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = showPolygons ? visiblePolygonAlpha : 0f;
                rend.material.color = c;
            }
        }
    }

    private void ClearMaterialCache()
    {
        foreach (var mat in materialCache.Values)
        {
            if (mat != null)
                Destroy(mat);
        }
        materialCache.Clear();
    }

    private string NormalizePathForResources(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        string normalized = path.Replace("\\", "/").Trim();
        if (normalized.StartsWith("Assets/")) normalized = normalized.Substring("Assets/".Length);
        if (normalized.StartsWith("Resources/")) normalized = normalized.Substring("Resources/".Length);
        normalized = Path.ChangeExtension(normalized, null);
        if (normalized.StartsWith("/")) normalized = normalized.Substring(1);
        return normalized;
    }

    private Texture2D LoadTextureFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        string resourcesPath = NormalizePathForResources(path);
        if (!string.IsNullOrEmpty(resourcesPath))
        {
            Texture2D tex = Resources.Load<Texture2D>(resourcesPath);
            if (tex != null) return tex;
        }

        List<string> candidates = new List<string>();
        string normalized = path.Replace("\\", "/").Trim();
        if (Path.IsPathRooted(normalized))
        {
            candidates.Add(normalized);
        }
        else
        {
            candidates.Add(Path.Combine(Application.dataPath, normalized));
            candidates.Add(Path.Combine(Application.streamingAssetsPath, normalized));
            candidates.Add(Path.Combine(Application.dataPath, "Resources", normalized));
            if (normalized.StartsWith("Assets/"))
            {
                string trimmed = normalized.Substring("Assets/".Length);
                candidates.Add(Path.Combine(Application.dataPath, trimmed));
                candidates.Add(Path.Combine(Application.streamingAssetsPath, trimmed));
                candidates.Add(Path.Combine(Application.dataPath, "Resources", trimmed));
            }
        }

        foreach (var candidate in candidates)
        {
            if (!File.Exists(candidate)) continue;
            try
            {
                byte[] bytes = File.ReadAllBytes(candidate);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(bytes))
                {
                    tex.wrapMode = TextureWrapMode.Clamp;
                    return tex;
                }
                Destroy(tex);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"LoadTextureFromPath: Fehler beim Laden {candidate}: {e.Message}");
            }
        }

        Debug.LogWarning($"LoadTextureFromPath: keine gültige Textur gefunden für Pfad '{path}'");
        return null;
    }

    private Material LoadMaterialFromPath(string path)
    {
        Texture2D tex = LoadTextureFromPath(path);
        if (tex == null) return null;
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.mainTexture = tex;
        return mat;
    }

    private AudioClip LoadAudioClip(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        string normalized = path.Replace("\\", "/").Trim();
        string resourcesPath = NormalizePathForResources(normalized);
        AudioClip clip = null;
        if (!string.IsNullOrEmpty(resourcesPath))
            clip = Resources.Load<AudioClip>(resourcesPath);
        if (clip != null) return clip;

        string candidate = Path.Combine(Application.streamingAssetsPath, normalized);
        if (File.Exists(candidate))
        {
            // StreamingAssets audio loading not implemented here
            Debug.LogWarning($"LoadAudioClip: Datei existiert in StreamingAssets ({candidate}), aber nicht geladen.");
        }

        return null;
    }

    public void openCutscene(string videoPath)
    {
        Debug.Log($"openCutscene: path={videoPath}. Stelle sicher, dass dein Cutscene-Player den Pfad unterstützt.");
    }
}
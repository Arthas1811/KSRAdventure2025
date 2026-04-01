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

    // Returns the path where saveData.json is written at runtime (writable in builds)
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "saveData.json");

    void openImage(string resourceName)
    {
        if (resourceName.StartsWith("Assets/Resources/"))
            resourceName = resourceName.Substring("Assets/Resources/".Length);
        
        int dotIndex = resourceName.LastIndexOf('.');
        if (dotIndex >= 0)
            resourceName = resourceName.Substring(0, dotIndex);

        // If no folder specified, assume Images/Images/
        if (!resourceName.Contains("/"))
            resourceName = "Images/Images/" + resourceName;

        Texture2D texture = Resources.Load<Texture2D>(resourceName);
        if (texture == null)
        {
            Debug.LogError($"Image not found in Resources: {resourceName}");
            return;
        }

        imageOpen = true;
        uiImage.gameObject.SetActive(true);
        closeImageButton.SetActive(true);
        uiImage.preserveAspect = true;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        uiImage.sprite = sprite;
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
        JArray customHotspots = (JArray)data[currentImage]["customHotspots"];

        foreach (var customHotspot in customHotspots)
        {
            string[] actions = customHotspot["actions"].ToObject<string[]>();
            string[] requirements = customHotspot["requirements"].ToObject<string[]>();

            var polygonCoordiantes = customHotspot["polygonString"].ToString().Split(";").Select(p => p.Split(",")).Select(a => new Vector2(float.Parse(a[0]), float.Parse(a[1]))).ToList();

            var vectors = new List<Vector3>();
            foreach (var coordinates in polygonCoordiantes)
            {
                float u = coordinates.x;
                float v = coordinates.y;
                float longitude = u * 2f * Mathf.PI;
                float latitude = (1f - v) * Mathf.PI;
                float x = 70f * Mathf.Sin(latitude) * Mathf.Cos(longitude) * -1f;
                float y = 70f * Mathf.Cos(latitude);
                float z = 70f * Mathf.Sin(latitude) * Mathf.Sin(longitude);
                vectors.Add(new Vector3(x, y, z));
            }

            int[] triangles = new int[(vectors.Count - 2) * 3];
            for (int i = 0; i < vectors.Count - 2; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            Mesh mesh = new Mesh { name = "mesh", vertices = vectors.ToArray(), triangles = triangles, uv = polygonCoordiantes.ToArray() };
            mesh.RecalculateNormals();

            GameObject polygonObject = Instantiate(polygonPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            polygonObject.GetComponent<MeshFilter>().sharedMesh = mesh;

            MeshRenderer meshRenderer = polygonObject.GetComponent<MeshRenderer>();
            Material basePolygonMat = Resources.Load<Material>("Materials/PolygonTemplate");
            Material material = basePolygonMat != null ? new Material(basePolygonMat) : new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            
            material.SetColor("_BaseColor", new Color(1f, 1f, 1f, showPolygons ? visiblePolygonAlpha : 0f));
            material.SetFloat("_Surface", 1);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.SetOverrideTag("RenderType", "Transparent");
            meshRenderer.sharedMaterial = material;

            MeshCollider meshCollider = polygonObject.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = true;

            polygons.Add(polygonObject);
            hotspotActions[polygonObject] = actions;
            hotspotRequirements[polygonObject] = requirements;
        }
    }

    IEnumerator updateImage(float duration, float startFOV, string currentImage, Dictionary<GameObject, string[]> hotspotActions, Dictionary<GameObject, string[]> hotspotRequirements)
    {
        float startTime = Time.time;
        float subtraction = 40;
        if (startFOV - subtraction < 1)
            subtraction = startFOV - 1;

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
                    materialPaths[imageKey] = path;
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
            Destroy(hotspot);

        foreach (var polygon in polygons)
            Destroy(polygon);

        polygons.Clear();
    }

    void updateState(string action, string currentImage)
    {
        action = action.Replace("data:", "");
        string[] parts = action.Split(':');
        string newValueStr = parts[parts.Length - 1];
        string targetKey = parts[parts.Length - 2];

        JToken current = saveData;
        for (int i = 0; i < parts.Length - 2; i++)
        {
            if (current[parts[i]] != null)
                current = (JObject)current[parts[i]];
            else
            {
                Debug.LogError("path not found");
                return;
            }
        }

        if (newValueStr == "++")
        {
            if (current[targetKey] != null && int.TryParse(current[targetKey].ToString(), out int currentValue))
                current[targetKey] = currentValue + 1;
            else
                current[targetKey] = 1;
        }
        else if (newValueStr == "--")
        {
            if (current[targetKey] != null && int.TryParse(current[targetKey].ToString(), out int currentValue))
                current[targetKey] = currentValue - 1;
            else
                current[targetKey] = -1;
        }
        else if (bool.TryParse(newValueStr, out bool boolValue))
            current[targetKey] = boolValue;
        else if (int.TryParse(newValueStr, out int intValue))
            current[targetKey] = intValue;
        else
            current[targetKey] = newValueStr;

        saveDataInFile(saveData);
        Debug.Log($"Updated {targetKey} to {newValueStr}");
    }

    void saveDataInFile(JObject saveData)
    {
        // Application.persistentDataPath is writable in all builds (unlike dataPath)
        string saveJson = saveData.ToString();
        File.WriteAllText(SaveFilePath, saveJson);
    }

    private bool checkRequirements(string[] requirements)
    {
        if (requirements == null || requirements.Length == 0)
            return true;

        foreach (var requirement in requirements)
        {
            if (requirement.StartsWith("item:"))
            {
                string[] parts = requirement.Split(':');
                string itemName = parts[1];
                bool itemValue = parts[2].ToLower() == "true";

                JArray itemsOwned = saveData["itemsOwned"] as JArray;
                bool hasItem = itemsOwned != null && itemsOwned.Any(t => t.ToString() == itemName);

                if (hasItem != itemValue)
                    return false;
            }
            else
            {
                string[] parts = requirement.Split(':');
                JToken current = saveData;
                bool pathFound = true;

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (current != null && current[parts[i]] != null)
                        current = current[parts[i]];
                    else
                    {
                        pathFound = false;
                        break;
                    }
                }

                if (!pathFound)
                    return false;

                string requiredValue = parts[parts.Length - 1].ToLower();
                string actualValue = current.ToString().ToLower();

                if (actualValue != requiredValue)
                    return false;
            }
        }
        return true;
    }

    void completeAction(string action, string[] requirements, string currentImage, float zoomDuration, float FOV, Dictionary<GameObject, string[]> hotspotActions, Dictionary<GameObject, string[]> hotspotRequirements)
    {
        if (!checkRequirements(requirements))
            return;

        if (action.Split(":")[0] == "scene")
        {
            Debug.Log("Loading scene: " + action.Split(":")[1]);
            SceneManager.LoadScene(action.Split(":")[1]);
            return;
        }
        else if (action.Split(":")[0] == "item")
        {
            if (action.Split(":")[1] == "add")
                inventory.add(action.Split(":")[2]);
            else if (action.Split(":")[1] == "remove")
                inventory.remove(action.Split(":")[2]);
            
            if (File.Exists(SaveFilePath))
                saveData = JObject.Parse(File.ReadAllText(SaveFilePath));
        }
        else if (action.Split(":")[0] == "data")
        {
            updateState(action, currentImage);
        }
        else if (action.Split(":")[0] == "dialogue")
        {
            DialogueManager.Instance.StartDialogue(action.Split(":")[1]);
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
            // Loads from Resources/Audio/SFX/<clipName>
            AudioClip clip = LoadAudioClip($"Audio/SFX/{clipName}");
            AudioManager.Instance.PlaySFX(clip);
            return;
        }
        else if (action.StartsWith("music:"))
        {
            string clipName = action.Split(':')[1];
            // Loads from Resources/Audio/Music/<clipName>
            AudioClip clip = LoadAudioClip($"Audio/Music/{clipName}");
            AudioManager.Instance.PlayMusic(clip);
            return;
        }
        else if (action.StartsWith("cutscene:"))
        {
            string videoName = action.Split(':')[1];
            // Pass a Resources-relative path; the cutscene scene loads it via Resources.Load<VideoClip>
            openCutscene($"Videos/Cutscenes/{videoName}");
            return;
        }
        else if (action.StartsWith("image:"))
        {
            string imageName = action.Split(':')[1];
            openImage(imageName);
            return;
        }
        else
        {
            if (data[action] == null)
            {
                Debug.LogError($"Action '{action}' is either malformed (missing a valid prefix like 'data:') or refers to a missing image.");
                return;
            }

            currentImage = action;
            saveData["currentImage"] = currentImage;
            saveDataInFile(saveData);

            var states = data[currentImage]["states"] as JObject;
            JToken activeState = states["main"];

            foreach (var state in states)
            {
                if (state.Key == "main") continue;
                if (checkRequirements(state.Value["requirements"]?.ToObject<string[]>()))
                    activeState = state.Value;
            }

            float finalY = activeState["x"] != null ? activeState["x"].ToObject<float>() : cameraMovement.x;
            float finalX = activeState["y"] != null ? activeState["y"].ToObject<float>() : cameraMovement.y;
            if (activeState["x"] != null || activeState["y"] != null)
                cameraMovement.setNewRotation(Mathf.Clamp(finalX, -90f, 90f), finalY);

            if (activeState["fov"] != null)
            {
                this.FOV = activeState["fov"].ToObject<float>();
                cameraMovement.setNewFOV(this.FOV);
            }

            string bestPath = activeState["path"].ToString();
            sphere.material = LoadMaterialFromPath(bestPath);

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

        // Try to load existing save from persistentDataPath first,
        // then fall back to the default bundled in Resources
        string saveJson;
        if (File.Exists(SaveFilePath))
        {
            saveJson = File.ReadAllText(SaveFilePath);
        }
        else
        {
            TextAsset defaultSave = Resources.Load<TextAsset>("SaveFile/saveData");
            if (defaultSave == null)
            {
                Debug.LogError("Default saveData.json not found in Resources/SaveFile/");
                return;
            }
            saveJson = defaultSave.text;
        }

        saveData = JObject.Parse(saveJson);

        string locationName = saveData.ContainsKey("currentLocation")
            ? saveData["currentLocation"].ToString()
            : "start";

        string entryImage = saveData.ContainsKey("currentImage")
            ? saveData["currentImage"].ToString()
            : null;

        LoadLocation(locationName, entryImage);
    }

    void LoadLocation(string locationName, string entryImage = null)
    {
        // Loads from Resources/Locations/<locationName>.json
        TextAsset json = Resources.Load<TextAsset>($"Locations/{locationName}");
        if (json == null)
        {
            Debug.LogError($"Location JSON not found: Locations/{locationName}");
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
            string key = image.Key;
            string path = image.Value["states"]?["main"]?["path"]?.ToString();
            if (!string.IsNullOrEmpty(path))
                materialPaths[key] = path;
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
                    break;
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
                mouseOne = Mouse.current.position.ReadValue();

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

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray);
        bool hoveringPolygon = false;

        foreach (var hit in hits)
        {
            if (hotspotActions.ContainsKey(hit.collider.gameObject))
            {
                hoveringPolygon = true;
                break;
            }
        }

        SetHoverCursor(hoveringPolygon);
    }

    private void SetHoverCursor(bool hoveringPolygon)
    {
        if (hoveringPolygon == isHoveringPolygon)
            return;

        isHoveringPolygon = hoveringPolygon;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (isHoveringPolygon && hoverCursorTexture != null)
            Cursor.SetCursor(hoverCursorTexture, hoverCursorHotspot, forceSoftwareCursor ? CursorMode.ForceSoftware : CursorMode.Auto);
        else
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnDisable()
    {
        // reset curosr on scenen change
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        isHoveringPolygon = false;
    }

    private void UpdateAllPolygonVisuals()
    {
        foreach (var polygon in polygons)
        {
            if (polygon == null) continue;
            MeshRenderer meshRenderer = polygon.GetComponent<MeshRenderer>();
            if (meshRenderer == null || meshRenderer.sharedMaterial == null) continue;

            float alpha = showPolygons ? visiblePolygonAlpha : 0f;
            meshRenderer.sharedMaterial.SetColor("_BaseColor", new Color(1f, 1f, 1f, alpha));
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

    private Material LoadMaterialFromPath(string resourcePath)
    {
        // Strip "Assets/Resources/" prefix if present
        if (resourcePath.StartsWith("Assets/Resources/"))
            resourcePath = resourcePath.Substring("Assets/Resources/".Length);

        // Strip file extension (.jpg, .png, etc.)
        int dotIndex = resourcePath.LastIndexOf('.');
        if (dotIndex >= 0)
            resourcePath = resourcePath.Substring(0, dotIndex);

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            Debug.LogError($"Texture not found in Resources: {resourcePath}");
            return null;
        }

        Material baseSphereMat = Resources.Load<Material>("Materials/SphereTemplate");
        Material material = baseSphereMat != null ? new Material(baseSphereMat) : new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        material.mainTexture = texture;
        return material;
    }
    private AudioClip LoadAudioClip(string resourcePath)
    {
        // resourcePath is Resources-relative, e.g. "Audio/SFX/door"
        return Resources.Load<AudioClip>(resourcePath);
    }

    public void openCutscene(string videoName)
    {
        // Strip any full path prefixes, keep only the filename
        if (videoName.Contains("/"))
            videoName = videoName.Substring(videoName.LastIndexOf('/') + 1);
        if (videoName.Contains("\\"))
            videoName = videoName.Substring(videoName.LastIndexOf('\\') + 1);

        // Strip extension if present
        int dotIndex = videoName.LastIndexOf('.');
        string nameOnly = dotIndex >= 0 ? videoName.Substring(0, dotIndex) : videoName;

        SceneData.VideoPath = Path.Combine(Application.streamingAssetsPath, "Videos", "Cutscenes", nameOnly + ".mp4");
        SceneManager.LoadScene("cutscenes");
    }
}
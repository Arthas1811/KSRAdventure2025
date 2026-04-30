using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using Newtonsoft.Json.Linq;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Click : MonoBehaviour
{
    // Object references and prefabs
    public Camera Cam;
    public Renderer Sphere;
    public GameObject HotspotPrefab;
    public GameObject PolygonPrefab;
    
    // Spawnable items for the physics minigame
    public GameObject Floor;
    public GameObject Carton;
    public GameObject Trash;
    public GameObject Chair;
    public GameObject Fan;
    public GameObject GreyBox1;
    public GameObject GreyBox2;
    public GameObject Soldier;
    public GameObject Colours;
    public GameObject Cube;
    public GameObject Grid;
    public GameObject Tank;
    public GameObject Unknown;
    public GameObject Mirror;
    public GameObject Paper;
    public GameObject PaperBox;
    public GameObject ShoeBox1;
    public GameObject ShoeBox2;
    
    // UI Elements
    public GameObject[] InstructionText;
    public GameObject InstructionTitle;
    public GameObject InstructionBackground;
    public Image UiImage;
    public TMP_Text StabilityTimerText;
    public Button RepeatButton;
    public Button NextInstructionButton;

    // Settings
    public float ZoomDuration = 0.2f;
    public float FOV = 60f;
    [Range(0f, 1f)] public float VisiblePolygonAlpha = 0.35f;
    public Texture2D HoverCursorTexture;
    public Vector2 HoverCursorHotspot = Vector2.zero;
    private bool ForceSoftwareCursor = true;

    // State Flags
    public bool InventoryOpen = false;
    public bool DialogueOpen = false;
    public bool ImageOpen = false;
    public bool ShowInstructions = false;

    // Keybindings
    public Key RotateLeftKey = Key.LeftArrow;
    public Key RotateRightKey = Key.RightArrow;
    public Key RotateUpKey = Key.UpArrow;
    public Key RotateDownKey = Key.DownArrow;
    public Key FallKey = Key.Enter;
    public Key SelectKey = Key.Space;
    private Key MoveForwardKey = Key.UpArrow;
    
    public CameraMovement CameraMovement;
    
    public Material PolygonTemplateMaterial;
    public Material SphereTemplateMaterial;
    public AudioClip RefuseSoundShort;
    public AudioClip FallCollisionSound;
    public TextAsset[] LocationFiles;
    public Texture2D[] SceneTextures;

    // Internal trackers
    private Dictionary<string, string> MaterialPaths = new Dictionary<string, string>();
    private Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();
    private string CurrentImage = "";
    
    // Spawn anchors
    private float X0Position = -0.16f;
    private float Y0Position = 1.6f;
    private float Z0Position = 1.4f;
    private float X0PositionFloor = -0.16f;
    private float Y0PositionFloor = -2.4f;
    private float Z0PositionFloor = 1.4f;
    
    private JObject Data;
    private JObject SaveData;
    private Dictionary<GameObject, string[]> HotspotActions = new Dictionary<GameObject, string[]>();
    private Dictionary<GameObject, string[]> HotspotRequirements = new Dictionary<GameObject, string[]>();
    private List<GameObject> Polygons = new List<GameObject>();
    
    private Vector2 MouseOne;
    private Vector2 MouseTwo;
    private Vector2 DeltaMouse;
    private bool ShowPolygons = false;
    private bool IsHoveringPolygon = false;
    private bool SpawnActive = false;
    private const string SkipInstructionsOnReloadKey = "SkipInstructionsOnReload";
    private readonly Dictionary<string, int> SpawnCounts = new Dictionary<string, int>();
    private readonly HashSet<string> SpawnedHotspotActions = new HashSet<string>();
    private GameObject CurrentSpawnedObject;
    private int CurrentIndexInstructions = 0;
    
    // Win condition thresholds
    private float MinYPosition = 1f;
    private float AdditionalSpawnHeight = 2.0f;
    private bool StabilityCheckFinished = false;
    private bool StabilityCoroutineRunning = false;
    private float StabilityCountdown = 0f;
    private Coroutine StabilityCoroutine = null;
    private const float StabilityDuration = 4.6f;
    
    private float PreviewForwardDistance = 0f;
    private float PreviewRightDistance = 0f;
    private bool AutoSpawnFloor = true;
    private List<GameObject> SpawnedGameObjects = new List<GameObject>();

    // -------------------------------------------------------------------------
    // Win / Lose 
    // -------------------------------------------------------------------------
    
    void Win()
    {
        Inventory.Instance.add("plan"); 
        SaveDataManager.SaveData(SaveData);
        OpenCutscene("FinishedGame");
    }

    public void Lose()
    {
        SceneManager.LoadScene("main");
    }

    // -------------------------------------------------------------------------
    // Hotspot system
    // -------------------------------------------------------------------------
    
    // Parses JSON data to create clickable 3D polygons overlaid on the 360-degree sphere
    void HotspotInstantiation(string currentImage)
    {   
        // Function taken from KSRAdventure2025

        JArray customHotspots = (JArray)Data[currentImage]["customHotspots"];

        foreach (var customHotspot in customHotspots)
        {
            string[] actions = customHotspot["actions"].ToObject<string[]>();
            string[] requirements = customHotspot["requirements"].ToObject<string[]>();

            // Extract 2D UV coordinates from JSON
            var polygonCoordiantes = customHotspot["polygonString"].ToString().Split(";").Select(p => p.Split(",")).Select(a => new Vector2(float.Parse(a[0]), float.Parse(a[1]))).ToList();

            var vectors = new List<Vector3>();
            
            // Map 2D UV coordinates to 3D spherical coordinates
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

            // Generate triangles for the mesh
            int[] triangles = new int[(vectors.Count - 2) * 3];
            for (int i = 0; i < vectors.Count - 2; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            // Create and assign the mesh
            Mesh mesh = new Mesh { name = "mesh", vertices = vectors.ToArray(), triangles = triangles, uv = polygonCoordiantes.ToArray() };
            mesh.RecalculateNormals();

            GameObject polygonObject = Instantiate(PolygonPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            polygonObject.GetComponent<MeshFilter>().sharedMesh = mesh;

            // Setup a transparent material for the polygon so it can act as an invisible/faint button
            MeshRenderer meshRenderer = polygonObject.GetComponent<MeshRenderer>();
            Material basePolygonMat = PolygonTemplateMaterial;
            Material material = basePolygonMat != null ? new Material(basePolygonMat) : new Material(Shader.Find("Universal Render Pipeline/Unlit"));

            material.SetColor("_BaseColor", new Color(1f, 1f, 1f, ShowPolygons ? VisiblePolygonAlpha : 0f));
            material.SetFloat("_Surface", 1);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.SetOverrideTag("RenderType", "Transparent");
            meshRenderer.sharedMaterial = material;

            // Add collider so it can be hit by raycasts
            MeshCollider meshCollider = polygonObject.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = true;

            Polygons.Add(polygonObject);
            HotspotActions[polygonObject] = actions;
            HotspotRequirements[polygonObject] = requirements;
        }
    }

    void SetMaterial(string currentImage)
    {
        // Function taken from KSRAdventure2025

        Material mat = GetMaterialForImage(currentImage);
        if (mat != null)
            Sphere.material = mat;
    }

    Material GetMaterialForImage(string imageKey)
    {
        // Function taken from KSRAdventure2025

        if (string.IsNullOrEmpty(imageKey))
            return null;

        // Retrieve the path from JSON if we don't have it mapped yet
        if (!MaterialPaths.TryGetValue(imageKey, out string path) || string.IsNullOrEmpty(path))
        {
            if (Data != null && Data[imageKey] != null)
            {
                path = Data[imageKey]?["states"]?["main"]?["path"]?.ToString();
                if (!string.IsNullOrEmpty(path))
                    MaterialPaths[imageKey] = path;
            }
        }

        if (string.IsNullOrEmpty(path))
            return null;

        // Check cache before doing a heavy resource load
        if (MaterialCache.TryGetValue(path, out Material mat))
            return mat;

        mat = LoadMaterialFromPath(path);
        if (mat != null)
            MaterialCache[path] = mat;
        return mat;
    }

    // Evaluates game state conditions to determine if a hotspot is currently active
    private bool CheckRequirements(string[] requirements)
    {
        // Function taken from KSRAdventure2025

        if (requirements == null || requirements.Length == 0)
            return true;

        foreach (var requirement in requirements)
        {
            string[] parts = requirement.Split(':');
            JToken current = SaveData;
            bool pathFound = true;

            // Navigate the JSON structure to find the required variable
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

            // Compare actual state with the required state
            string requiredValue = parts[parts.Length - 1].ToLower();
            string actualValue = current.ToString().ToLower();

            if (actualValue != requiredValue)
                return false;
        }
        return true;
    }

    // Handles the outcome of clicking a valid hotspot
    void CompleteAction(string action, string[] requirements, string currentImage, float zoomDuration, float FOV, Dictionary<GameObject, string[]> hotspotActions, Dictionary<GameObject, string[]> hotspotRequirements, GameObject hotspot = null)
    {
        // Function taken from KSRAdventure2025

        // Attempt to spawn an object if the action maps to one
        Spawn(action, hotspot);
        
        if (!CheckRequirements(requirements))
            return;

        action = action?.Trim();
        if (string.IsNullOrEmpty(action))
        {
            return;
        }
        else
        {
            // Transition to the new 360 image
            currentImage = action;
            SaveData["currentImage"] = currentImage;
            SaveDataManager.SaveData(SaveData);

            var states = Data[currentImage]?["states"] as JObject;
            if (states == null)
            {
                return;
            }

            JToken activeState = states["main"];
            if (activeState == null)
            {
                return;
            }

            // Check if there's an alternative state that meets current requirements
            foreach (var state in states)
            {
                if (state.Key == "main") continue;
                if (CheckRequirements(state.Value["requirements"]?.ToObject<string[]>()))
                    activeState = state.Value;
            }

            // Apply camera adjustments for the new scene
            float finalY = activeState["x"] != null ? activeState["x"].ToObject<float>() : CameraMovement.X;
            float finalX = activeState["y"] != null ? activeState["y"].ToObject<float>() : CameraMovement.Y;
            if (activeState["x"] != null || activeState["y"] != null)
                CameraMovement.SetNewRotation(Mathf.Clamp(finalX, -90f, 90f), finalY);

            if (activeState["fov"] != null)
            {
                this.FOV = activeState["fov"].ToObject<float>();
                CameraMovement.SetNewFOV(this.FOV);
            }

            // Load and apply the new 360 background texture
            string bestPath = activeState["path"].ToString();
            Sphere.material = LoadMaterialFromPath(bestPath);

            // Rebuild hotspots for the new image
            hotspotActions.Clear();
            hotspotRequirements.Clear();
            HotspotInstantiation(currentImage);
        }
    }

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        if (SceneTextures.Length > 0)
        {
            Sphere.material.mainTexture = SceneTextures[0];
        }
        
        UiImage.gameObject.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Cam.fieldOfView = FOV;

        SaveData = SaveDataManager.ReadData();

        string locationName = SaveData.ContainsKey("currentLocation")
            ? SaveData["currentLocation"].ToString()
            : "Polygons";

        string entryImage = SaveData.ContainsKey("currentImage")
            ? SaveData["currentImage"].ToString()
            : null;

        LoadLocation(locationName, entryImage);

        if (RepeatButton != null)
        {
            RepeatButton.onClick.AddListener(RepeatGame);
        }

        if (AutoSpawnFloor)
            SpawnFloor();

        // Handle tutorial display logic based on whether we just reloaded the scene
        if (PlayerPrefs.GetInt(SkipInstructionsOnReloadKey, 0) == 1)
        {
            PlayerPrefs.DeleteKey(SkipInstructionsOnReloadKey);
            HideInstructions();
        }
        else
        {
            Instructions(InstructionText, CurrentIndexInstructions);
        }
    }

    void Update()
    {
        if (ShowInstructions == false)
        {
            // Keyboard interaction with the center of the screen
            if (!InventoryOpen && !ImageOpen && !DialogueOpen && Keyboard.current != null && Keyboard.current[MoveForwardKey].wasPressedThisFrame)
            {
                Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                Ray ray = Camera.main.ScreenPointToRay(screenCenter);
                RaycastHit[] hits = Physics.RaycastAll(ray);

                foreach (var hit in hits)
                {
                    if (HotspotActions.TryGetValue(hit.collider.gameObject, out string[] actions) && HotspotRequirements.TryGetValue(hit.collider.gameObject, out string[] requirements))
                    {
                        foreach (var action in actions)
                            CompleteAction(action, requirements, CurrentImage, ZoomDuration, FOV, HotspotActions, HotspotRequirements);
                        break;
                    }
                }
            }

            // Keyboard controls for manipulating the currently held object before dropping it
            if (Keyboard.current != null && Keyboard.current[RotateLeftKey].wasPressedThisFrame && CurrentSpawnedObject != null && CurrentSpawnedObject.GetComponent<Rigidbody>() is Rigidbody currentRbLeft && currentRbLeft.isKinematic)
            {
                RotateObjectLeft();
            }
            if (Keyboard.current != null && Keyboard.current[RotateRightKey].wasPressedThisFrame && CurrentSpawnedObject != null && CurrentSpawnedObject.GetComponent<Rigidbody>() is Rigidbody currentRbRight && currentRbRight.isKinematic)
            {
                RotateObjectRight();
            }
            if (Keyboard.current != null && Keyboard.current[RotateUpKey].wasPressedThisFrame && CurrentSpawnedObject != null && CurrentSpawnedObject.GetComponent<Rigidbody>() is Rigidbody currentRbUp && currentRbUp.isKinematic)
            {
                RotateObjectForward();
            }
            if (Keyboard.current != null && Keyboard.current[RotateDownKey].wasPressedThisFrame && CurrentSpawnedObject != null && CurrentSpawnedObject.GetComponent<Rigidbody>() is Rigidbody currentRbDown && currentRbDown.isKinematic)
            {
                RotateObjectBackward();
            }
            
            // Drop the object into the scene
            if (Keyboard.current != null && Keyboard.current[FallKey].wasPressedThisFrame)
            {
                Fall();
            }
            
            PreventInfiniteFall();
            AlignPreviewObjectWithCamera();
            UpdateHoverCursor();

            // Win Condition Logic: Check if the stacked objects have reached the target height
            bool anyObjectAboveMinimumHeight = false;
            foreach (GameObject element in SpawnedGameObjects)
            {
                Vector3 position = element.transform.position;
                float y_position = position.y;

                if (y_position >= MinYPosition)
                {
                    anyObjectAboveMinimumHeight = true;
                }
            }

            // Start or maintain the stability timer if the height requirement is met
            if (anyObjectAboveMinimumHeight)
            {
                if (!StabilityCoroutineRunning && !StabilityCheckFinished)
                {
                    StabilityCoroutineRunning = true;
                    StabilityCountdown = StabilityDuration;
                    StabilityCoroutine = StartCoroutine(StabilityCheck(StabilityDuration));
                }
            }
            else
            {
                // Reset the timer if the tower collapses below the threshold
                StabilityCheckFinished = false;
                if (StabilityCoroutineRunning)
                {
                    StabilityCoroutineRunning = false;
                    if (StabilityCoroutine != null)
                    {
                        StopCoroutine(StabilityCoroutine);
                        StabilityCoroutine = null;
                    }
                }
                StabilityCountdown = 0f;
            }

            if (StabilityCoroutineRunning)
            {
                StabilityCountdown = Mathf.Max(0f, StabilityCountdown - Time.deltaTime);
            }

            UpdateStabilityTimerText();

            // If the tower stayed standing for the full duration, trigger the win
            if (StabilityCheckFinished && anyObjectAboveMinimumHeight)
            {
                Win();
            }

            // Mouse interaction logic (Clicking on hotspots)
            if (!InventoryOpen && !DialogueOpen && !ImageOpen)
            {
                if (Keyboard.current[SelectKey].wasPressedThisFrame)
                {
                    MouseOne = Mouse.current.position.ReadValue();
                }
                if (Keyboard.current[SelectKey].wasReleasedThisFrame)
                {
                    MouseTwo = Mouse.current.position.ReadValue();
                    ProcessPointerSelection(MouseOne, MouseTwo, "SelectKey");
                }

                if (Mouse.current.leftButton.wasPressedThisFrame)
                    MouseOne = Mouse.current.position.ReadValue();

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    MouseTwo = Mouse.current.position.ReadValue();
                    ProcessPointerSelection(MouseOne, MouseTwo, "Mouse");
                }

                // Handle camera zooming via scroll wheel
                Vector2 scroll = Mouse.current.scroll.ReadValue();
                if (scroll.y < 0)
                {
                    Cam.fieldOfView = Mathf.Clamp(Cam.fieldOfView + 2f, 5f, 60f);
                    FOV = Cam.fieldOfView;
                }
                else if (scroll.y > 0)
                {
                    Cam.fieldOfView = Mathf.Clamp(Cam.fieldOfView - 2f, 5f, 60f);
                    FOV = Cam.fieldOfView;
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // UI & instructions
    // -------------------------------------------------------------------------

    private void UpdateStabilityTimerText()
    {
        if (StabilityCoroutineRunning)
        {
            // Only show the timer to the player when it's getting close to finishing
            if (StabilityCountdown < (StabilityDuration - 0.6))
            {
                StabilityTimerText.text = $"{StabilityCountdown:0.0}s";
                return;
            }
        }

        StabilityTimerText.text = string.Empty;
    }

    // Restarts minigame
    public void RepeatGame()
    {
        PlayerPrefs.SetInt(SkipInstructionsOnReloadKey, 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void HideInstructions()
    {
        InstructionBackground.SetActive(false);
        InstructionTitle.SetActive(false);
        NextInstructionButton.gameObject.SetActive(false);

        for (int i = 0; i < InstructionText.Length; i++)
        {
            if (InstructionText[i] != null)
                InstructionText[i].SetActive(false);
        }

        ShowInstructions = false;
        if (RepeatButton != null)
            RepeatButton.interactable = true;
    }

    private void Instructions(GameObject[] instructionText, int currentIndexInstructions)
    {
        ShowInstructions = true;
        InstructionBackground.SetActive(true);
        InstructionTitle.SetActive(true);
        if (currentIndexInstructions < InstructionText.Length)
        {
            InstructionText[currentIndexInstructions].SetActive(true);
        }
        else
        {
            // Close instructions once we've reached the end of the array
            InstructionBackground.SetActive(false);
            InstructionTitle.SetActive(false);
            NextInstructionButton.gameObject.SetActive(false);
            ShowInstructions = false;
        }

        if (RepeatButton != null)
            RepeatButton.interactable = !ShowInstructions;

        for (int i = 0; i < InstructionText.Length; i += 1)
        {
            if (currentIndexInstructions != i)
            {
                InstructionText[i].SetActive(false);
            }
        }
    }

    public void NextInstruction()
    {
        CurrentIndexInstructions++;
        Instructions(InstructionText, CurrentIndexInstructions);
    }

    // -------------------------------------------------------------------------
    // camera & navigation
    // -------------------------------------------------------------------------

    public void OpenCutscene(string videoName)
    {
        // Function taken from KSRAdventure2025

        // Sanitize the file path
        if (videoName.Contains("/"))
            videoName = videoName.Substring(videoName.LastIndexOf('/') + 1);
        if (videoName.Contains("\\"))
            videoName = videoName.Substring(videoName.LastIndexOf('\\') + 1);

        int dotIndex = videoName.LastIndexOf('.');
        string nameOnly = dotIndex >= 0 ? videoName.Substring(0, dotIndex) : videoName;

        VideoPlayerLoader.VideoPath = Path.Combine(Application.streamingAssetsPath, nameOnly + ".mp4");
        SceneManager.LoadScene("Cutscenes");
    }

    // Loads the JSON data for a specific location and initializes the first scene
    void LoadLocation(string locationName, string entryImage = null)
    {
        // Function taken from KSRAdventure2025

        TextAsset json = System.Array.Find(LocationFiles, f => f != null && f.name == locationName);
        if (json == null)
        {
            return;
        }

        Data = JObject.Parse(json.text);
        MaterialPaths.Clear();

        // Extract texture paths from JSON
        foreach (var image in Data)
        {
            if (image.Key == "meta") continue;
            string key = image.Key;
            string path = image.Value["states"]?["main"]?["path"]?.ToString();
            if (!string.IsNullOrEmpty(path))
                MaterialPaths[key] = path;
        }

        if (!string.IsNullOrEmpty(entryImage) && Data[entryImage] != null)
            CurrentImage = entryImage;
        else
            CurrentImage = Data.Properties().First().Name;

        HotspotActions.Clear();
        HotspotRequirements.Clear();

        HotspotInstantiation(CurrentImage);
        SetMaterial(CurrentImage);

        SaveData["currentLocation"] = locationName;
        SaveData["currentImage"] = CurrentImage;
        SaveDataManager.SaveData(SaveData);
    }

    // -------------------------------------------------------------------------
    // Spawning & movement of objects
    // -------------------------------------------------------------------------

    // Prepares an object to be dropped into the scene
    public void Spawn(string action, GameObject hotspot = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            return;
        if (SpawnActive == true)
            return; // Don't allow multiple objects to be held at once
            
        string actionKey = action.Split(' ')[0];
        if (string.IsNullOrEmpty(actionKey))
            return;

        // Prevent clicking the same hotspot twice to spawn an object
        if (hotspot != null)
        {
            string hotspotKey = $"{hotspot.GetInstanceID()}_{actionKey}";
            if (SpawnedHotspotActions.Contains(hotspotKey))
            {
                AudioClip clip = RefuseSoundShort;
                AudioManager.Instance.PlaySFX(clip);
                return;
            }

            SpawnedHotspotActions.Add(hotspotKey);
        }

        // Object usage limits
        int maxSpawns;
        if (actionKey == "Chair" || actionKey == "ShoeBox2")
        {
            maxSpawns = 2;
        }
        else
        {
            maxSpawns = 1;
        }

        int currentCount;
        if (SpawnCounts.TryGetValue(actionKey, out int count))
        {
            currentCount = count;
        }
        else
        {
            currentCount = 0;
        }
        
        if (currentCount >= maxSpawns)
        {
            AudioClip clip = RefuseSoundShort;
            AudioManager.Instance.PlaySFX(clip);
            return; // Reached max spawn limit for this item
        }

        GameObject Target = GetSpawnable(actionKey);
        if (Target == null)
        {
            return;
        }

        GameObject instance = Instantiate(Target);
        
        // Calculate a safe spawn height so the new object doesn't clip into existing ones
        float spawnY = Y0Position;
        foreach (GameObject element in SpawnedGameObjects)
        {
            Vector3 ExistingPosition = element.transform.position;
            float y_position = ExistingPosition.y;
            if (y_position >= (Y0Position - 1f))
            {
                if (spawnY > (y_position + AdditionalSpawnHeight))
                {
                    spawnY = spawnY;
                }
                else
                {
                    spawnY = y_position + AdditionalSpawnHeight;
                }
            }
        }
        
        Vector3 position = new Vector3(X0Position, spawnY, Z0Position);
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.identity;
        
        // Keep the object suspended in the air until the player presses the drop key
        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        instance.SetActive(true);
        CameraMovement.SetNewRotation(40f, 0f);
        CurrentSpawnedObject = instance;

        // Calculate preview alignment offsets
        Vector3 cameraPosition = CameraMovement.transform.position;
        Vector3 flatForward = Vector3.ProjectOnPlane(CameraMovement.transform.forward, Vector3.up).normalized;
        Vector3 flatRight = Vector3.Cross(Vector3.up, flatForward).normalized;
        Vector3 offset = instance.transform.position - cameraPosition;
        PreviewForwardDistance = Vector3.Dot(offset, flatForward);
        PreviewRightDistance = Vector3.Dot(offset, flatRight);
        SpawnCounts[actionKey] = currentCount + 1;
        SpawnActive = true;
    }

    private void SpawnFloor()
    {
        if (Floor == null)
            return;

        Vector3 position = new Vector3(X0PositionFloor, Y0PositionFloor, Z0PositionFloor);
        Floor.transform.position = position;
        Floor.transform.rotation = Quaternion.identity;
        Floor.SetActive(true);
    }

    // Keeps the object locked to the camera's view before it is dropped
    private void AlignPreviewObjectWithCamera()
    {
        // Math of this function was explained by AI

        if (CurrentSpawnedObject == null || CameraMovement == null)
            return;

        // Stop tracking if the object has already been dropped
        if (CurrentSpawnedObject.GetComponent<Rigidbody>() is Rigidbody rb && !rb.isKinematic)
            return;
            
        Transform cam = CameraMovement.transform;

        Vector3 FlatForward = new Vector3(cam.forward.x, 0f, cam.forward.z).normalized;
        Vector3 FlatRight = new Vector3(cam.right.x, 0f, cam.right.z).normalized;

        Vector3 NewPosition = cam.position + (FlatForward * PreviewForwardDistance) + (FlatRight * PreviewRightDistance);
        NewPosition.y = CurrentSpawnedObject.transform.position.y;
        CurrentSpawnedObject.transform.position = NewPosition;
    }

    // Drops the currently held object
    private void Fall()
    {
        if (CurrentSpawnedObject == null)
            return;

        // Enable physics to let it fall
        if (CurrentSpawnedObject.GetComponent<Rigidbody>() is Rigidbody rb)
            rb.isKinematic = false;
            
        // Attach the collision sound effect component dynamically
        AudioClip collisionClip = FallCollisionSound;
        if (collisionClip != null)
        {
            CollisionSound cs = CurrentSpawnedObject.AddComponent<CollisionSound>();
            cs.Clip = collisionClip;
        }
        
        SpawnActive = false;
        SpawnedGameObjects.Add(CurrentSpawnedObject);
    }

    // Safety net in case physics go wild and objects clip through the floor
    public void PreventInfiniteFall()
    {
        foreach (GameObject element in SpawnedGameObjects)
        {
            Vector3 currentPosition = element.transform.position;
            float y_position = currentPosition.y;
            if (y_position < Y0PositionFloor)
            {
                currentPosition.y = Y0PositionFloor;
                element.transform.position = currentPosition;

                if (element.GetComponent<Rigidbody>() is Rigidbody rb)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }

    private void RotateObjectLeft()
    {
        if (CurrentSpawnedObject == null)
            return;

        CurrentSpawnedObject.transform.Rotate(0f, -90f, 0f, Space.World);
    }

    private void RotateObjectRight()
    {
        if (CurrentSpawnedObject == null)
            return;

        CurrentSpawnedObject.transform.Rotate(0f, 90f, 0f, Space.World);
    }

    private void RotateObjectForward()
    {
        if (CurrentSpawnedObject == null)
            return;

        CurrentSpawnedObject.transform.Rotate(90f, 0f, 0f, Space.World);
    }

    private void RotateObjectBackward()
    {
        if (CurrentSpawnedObject == null)
            return;

        CurrentSpawnedObject.transform.Rotate(-90f, 0f, 0f, Space.World);
    }

    // Timer that verifies the tower of objects doesn't immediately tip over
    public IEnumerator StabilityCheck(float waitingTime)
    {
        yield return new WaitForSeconds(waitingTime);
        StabilityCheckFinished = true;
        StabilityCoroutineRunning = false;
    }

    // Simple factory method to map string actions to game objects
    private GameObject GetSpawnable(string action)
    {
        string key = action.Split(' ')[0];
        switch (key)
        {
            case "Floor": return Floor;
            case "Carton": return Carton;
            case "Trash": return Trash;
            case "Chair": return Chair;
            case "Fan": return Fan;
            case "GreyBox1": return GreyBox1;
            case "GreyBox2": return GreyBox2;
            case "Soldier": return Soldier;
            case "Colours": return Colours;
            case "Cube": return Cube;
            case "Grid": return Grid;
            case "Tank": return Tank;
            case "Unknown": return Unknown;
            case "Mirror": return Mirror;
            case "Paper": return Paper;
            case "PaperBox": return PaperBox;
            case "ShoeBox1": return ShoeBox1;
            case "ShoeBox2": return ShoeBox2;
            default: return null;
        }
    }

    // -------------------------------------------------------------------------
    // Cursor 
    // -------------------------------------------------------------------------

    // Checks if the user is looking at an interactable object and updates the cursor graphic
    private void UpdateHoverCursor()
    {
        // Function taken from KSRAdventure2025

        if (InventoryOpen || DialogueOpen || ImageOpen)
        {
            SetHoverCursor(false);
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray);
        bool hoveringPolygon = false;

        foreach (var hit in hits)
        {
            if (HotspotActions.ContainsKey(hit.collider.gameObject))
            {
                hoveringPolygon = true;
                break;
            }
        }

        SetHoverCursor(hoveringPolygon);
    }

    private void SetHoverCursor(bool hoveringPolygon)
    {
        // Function taken from KSRAdventure2025

        if (hoveringPolygon == IsHoveringPolygon)
            return;

        IsHoveringPolygon = hoveringPolygon;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (IsHoveringPolygon && HoverCursorTexture != null)
            Cursor.SetCursor(HoverCursorTexture, HoverCursorHotspot, ForceSoftwareCursor ? CursorMode.ForceSoftware : CursorMode.Auto);
        else
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    // -------------------------------------------------------------------------
    // Material
    // -------------------------------------------------------------------------

    // Cleans up unity resource paths and maps them to the loaded scene textures
    private Material LoadMaterialFromPath(string resourcePath)
    {
        // Function taken from KSRAdventure2025

        if (string.IsNullOrEmpty(resourcePath))
            return null;

        // Path formatting cleanup
        if (resourcePath.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase))
        {
            resourcePath = resourcePath.Substring("Assets/Resources/".Length);
        }
        else if (resourcePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            resourcePath = resourcePath.Substring("Assets/".Length);
            int resourcesIndex = resourcePath.IndexOf("Resources/", StringComparison.OrdinalIgnoreCase);
            if (resourcesIndex == 0)
                resourcePath = resourcePath.Substring("Resources/".Length);
        }
        else if (resourcePath.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase))
        {
            resourcePath = resourcePath.Substring("Resources/".Length);
        }

        resourcePath = resourcePath.TrimStart('/', '\\');

        int dotIndex = resourcePath.LastIndexOf('.');
        if (dotIndex >= 0)
            resourcePath = resourcePath.Substring(0, dotIndex);

        string textureName = System.IO.Path.GetFileNameWithoutExtension(resourcePath);
        Texture2D texture = SceneTextures.FirstOrDefault(t => t != null && t.name == textureName);
        if (texture == null)
        {
            return null;
        }

        // Apply texture to the sphere material
        Material baseSphereMat = SphereTemplateMaterial;
        Material material = baseSphereMat != null ? new Material(baseSphereMat) : new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        material.mainTexture = texture;
        return material;
    }

    // Determines if a click counts as a valid hotspot interaction
    private void ProcessPointerSelection(Vector2 startPosition, Vector2 endPosition, string source)
    {
        // Function taken from KSRAdventure2025

        // Cancel the interaction if the user dragged the mouse rather than clicking
        DeltaMouse = startPosition - endPosition;
        if (DeltaMouse.magnitude >= 5f)
            return;

        Ray ray = Camera.main.ScreenPointToRay(endPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (HotspotActions.TryGetValue(hit.collider.gameObject, out string[] actions) && HotspotRequirements.TryGetValue(hit.collider.gameObject, out string[] requirements))
        {
            if (!CheckRequirements(requirements))
            {
                return;
            }

            foreach (var action in actions)
            {
                CompleteAction(action, requirements, CurrentImage, ZoomDuration, FOV, HotspotActions, HotspotRequirements, hit.collider.gameObject);
            }
        }
    }
}
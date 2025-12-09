using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class Click : MonoBehaviour
{
    public Camera cam;
    public Renderer sphere;
    private Dictionary<string, Material> materials = new Dictionary<string, Material>();
    private string currentImage = "start";
    public GameObject hotspotPrefab;
    public GameObject polygonPrefab;
    private JObject data;
    private Dictionary<GameObject, string> hotspotActions = new Dictionary<GameObject, string>();
    private List<GameObject> polygons = new List<GameObject>();
    private Vector2 mouseOne;
    private Vector2 mouseTwo;
    private Vector2 deltaMouse;
    public float zoomDuration = 0.2f;

    void hotspotInstantiation(string currentImage)
    {
        // JArray hotspots = (JArray)data[currentImage.ToString()]["hotspots"];

        // foreach (var hotspot in hotspots)
        // {
        //     string action = hotspot["action"].ToString();
        //     float xyDegree = float.Parse(hotspot["xyDegree"].ToString());
        //     float yzDegree = float.Parse(hotspot["yzDegree"].ToString());
        //     float bodyDegree = float.Parse(hotspot["bodyDegree"].ToString());

        //     Vector3 position = Quaternion.Euler(yzDegree, xyDegree, 0) * Vector3.forward * 25f;

        //     GameObject hotspotObject = Instantiate(hotspotPrefab, position, Quaternion.identity);
        //     hotspotObject.transform.LookAt(Vector3.zero);

        //     Vector3 euler = hotspotObject.transform.eulerAngles;
        //     euler.x = bodyDegree;
        //     hotspotObject.transform.eulerAngles = euler;

        //     // hotspotObject.transform.position += new Vector3(0f, -10f, 0f);
        //     hotspotActions[hotspotObject] = action;
        // }

        JArray customHotspots = (JArray)data[currentImage]["customHotspots"];

        foreach (var customHotspot in customHotspots)
        {
            string action = customHotspot["action"].ToString();
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
            //System.Array.Reverse(triangles);

            Mesh mesh = new Mesh { name = "mesh", vertices = vectors.ToArray(), triangles = triangles, uv = polygonCoordiantes.ToArray() };
            mesh.RecalculateNormals();

            GameObject polygonObject = Instantiate(polygonPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            polygonObject.GetComponent<MeshFilter>().sharedMesh = mesh;

            MeshRenderer meshRenderer = polygonObject.GetComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", new Color(1, 0, 0, 0.4f));
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
            hotspotActions[polygonObject] = action;
        }
    }


    IEnumerator updateImage(float duration, string currentImage, Dictionary<GameObject, string> hotspotActions)
    {
        cam.fieldOfView = 60f;
        float startTime = Time.time;
        while((Time.time - startTime) < duration)
        {
            float t = (Time.time - startTime) / duration;
            cam.fieldOfView = Mathf.Lerp(60f, 20f, t);
            yield return null;
        }
        cam.fieldOfView = 20f;
        cam.fieldOfView = 60f;

        hotspotDestroy(hotspotActions.Keys.ToList());
        hotspotActions.Clear();
        hotspotInstantiation(currentImage);
        setMaterial(currentImage);
    }

    void setMaterial(string currentImage)
    {
        sphere.material = materials[currentImage];
    }
    void hotspotDestroy(IEnumerable<GameObject> hotspots)
    {
        foreach (var hotspot in hotspots)
        {
            Destroy(hotspot);
        }

        foreach (var polygon in polygons)
        {
            Destroy(polygon);
        }

        polygons.Clear();
    }
    void Start()
    {
        cam.fieldOfView = 60f;

        string jsonPath = "Assets/Scripts/Navigation/locations.json";
        string json = File.ReadAllText(jsonPath);

        data = JObject.Parse(json);

        foreach (var image in data)
        {
            string key = image.Key;
            string path = image.Value["path"].ToString();

            Material material = LoadMaterialFromPath(path);
            materials.Add(key, material);

        }
    
        hotspotInstantiation(currentImage);
        setMaterial(currentImage);
    }

    void Update()
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
                if (hotspotActions.TryGetValue(hit.collider.gameObject, out string action))
                {
                    if (action.Split(":")[0] == "scene")
                    {
                        Debug.Log("Loading scene: " + action.Split(":")[1]);
                        SceneManager.LoadScene(action.Split(":")[1]);
                        return;
                    }
                    // else if (action.Split(":")[0] = "item")
                    // {
                    //     inventory.add(action.Split(":")[1]);
                    // }
                    else
                    {
                        currentImage = action;
                    }
                    
                    StartCoroutine(updateImage(zoomDuration, currentImage, hotspotActions));
                }
            }
        }
        }
        // if (using1) {
        //     sphere.material = material2;
        //     button.transform.position = new Vector3(0f, -10f, 25f);
        //     button.transform.rotation = Quaternion.Euler(0, 0, 0);
        // }

        // else if (!using1) {
        //     sphere.material = material1;
        //     button.transform.position = new Vector3(0f, -10f, -25f);
        //     button.transform.rotation = Quaternion.Euler(0, 0, 0);
        // }
    }

    private Material LoadMaterialFromPath(string path) 
    {
        byte[] data = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(data);

        Material material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        material.mainTexture = texture;

        return material;
    }
}

## Main Code
You can find the entire code to view and edit [here](/Assets/Scripts/Click.cs).  
#### Split up and explained code:
Imports:
```cs
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
```
class start:
```cs
public class Click : MonoBehaviour {

    //define variables
    public Renderer sphere;
    private Dictionary<string, Material> materials = new Dictionary<string, Material>();
    private int currentImage = 1;
    public GameObject hotspotPrefab;
    public GameObject polygonPrefab;
    private JObject data;
    private Dictionary<GameObject, string> hotspotActions = new Dictionary<GameObject, string>();
    private List<GameObject> polygons = new List<GameObject>();
```
Function to instantiate the hotspots:
```cs
    void hotspotInstantiation(int currentImage) {

        //read json
        JArray costumHotspots = (JArray)data[currentImage.ToString()]["costumHotspots"];

        //iterate through the hotspots (areas to click)
        foreach (var costumHotspot in costumHotspots)
        {
            //assign actions and coordinates of polygons
            string action = costumHotspot["action"].ToString();
            var polygonCoordiantes = costumHotspot["polygonString"].ToString().Split(";").Select(p => p.Split(",")).Select(a => new Vector2(float.Parse(a[0]), float.Parse(a[1]))).ToList();

            //generate x y and z coordiantes on the sphere using latitude and longitude
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

            //create meshtriangles for costum polygons
            int[] triangles = new int[(vectors.Count - 2) * 3];
            for (int i = 0; i < vectors.Count - 2; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            Mesh mesh = new Mesh { name = "mesh", vertices = vectors.ToArray(), triangles = triangles, uv = polygonCoordiantes.ToArray() };
            mesh.RecalculateNormals();

            //create polygon
            GameObject polygonObject = Instantiate(polygonPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
            polygonObject.GetComponent<MeshFilter>().sharedMesh = mesh;

            //render polygon
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

            if (int.TryParse(action, out _))
            {
                hotspotActions[polygonObject] = action;
            }
        }
    }
```
function to set material (image) on the sphere
```cs
    void setMaterial(int currentImage)
    {
        sphere.material = materials[currentImage.ToString()];
    }
```
function to destroy polygons on switching images
```cs
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
```
function which runs on scene load
```cs
    void Start()
    {
        //load json
        string jsonPath = "Assets/Scripts/locations.json";
        string json = File.ReadAllText(jsonPath);

        data = JObject.Parse(json);

        //create materials from images 
        foreach (var image in data)
        {
            string key = image.Key;
            string path = image.Value["path"].ToString();

            Material material = LoadMaterialFromPath(path);
            materials.Add(key, material);

        }

        //first instantiation (loading hotspots and material)
        hotspotInstantiation(currentImage);
        setMaterial(currentImage);
    }
```
function which runs every frame (handling almost everything and calling functions)
```cs
    void Update()
    {
        //check for mouse press
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            //create a ray
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            //check if ray collides
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hotspotActions.TryGetValue(hit.collider.gameObject, out string action))
                {
                    //check if action is scene switch (for minigames, cutscenes or other scene switches) or not
                    if (action.split(":")[0] != "scene")
                    {
                        //switch to image if the given action is not a scene switch
                        currentImage = int.Parse(action);
                    }
                    else
                    {
                        //load new scene if the action is a scene switch
                        SceneManager.LoadScene(action);
                        return;
                    }
                    //instantiate hotspots and materials
                    hotspotDestroy(hotspotActions.Keys.ToList());
                    hotspotActions.Clear();
                    hotspotInstantiation(currentImage);
                    setMaterial(currentImage);
                }
            }
        }

    }
```
function to load material
```cs
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

```


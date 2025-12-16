# Documentation
## Json Structure
The [locations.json](/Assets/Scripts/locations.json) file in this case is used to build up the whole navigation system and link images to one another. Jsons are mostly used to store data as it is in this case.

The Json Structure is built as following. **"image_name"** at the beginning should be replaced wkth the name of the image file. The **"path"** should contain the path to the image. In this structure, **"costumHotspots"** was used as the definition of an area one clicks to complete an action (like switching to the next image or starting a minigame).
```json
    "image_name": {
        "path": "Assets/Images/Navigation/image_name.jpg",
        "costumHotspots": [
            {
                "action": "2",
                "polygonString": "0.47452445652173914,0.6022418478260869;0.5322690217391305,0.6059782608695652;0.5407608695652174,0.37024456521739135;0.47927989130434784,0.3627717391304348"
            }
        ]
    }
```
The to be completed action is defined as **"action"** and can either contain the image name (to switch to the next image):
```json
"action": "image_name",
```
or a unity scene name (to switch to the named scene like a minigame or a cutscene): 
```json
"action": "scene:scene_name",
```
This action has to be defined as **scene:scene_name** or else the code will think it's and image name. 

The polygon string contains coordinates of specific points on the 360° image:
```json
"polygonString": "0.47452445652173914,0.6022418478260869;0.5322690217391305,0.6059782608695652;0.5407608695652174,0.37024456521739135;0.47927989130434784,0.3627717391304348"
```
### Json generator:
Using the [Json generator](https://lesieber.github.io/hotspots-website) you can effortlessly create a json code with the above mentioned structure for a given image.

To use it, either drag and drop or upload an image. The input field at the top is the image name and will be autofilled on uploading an image but can be edited if needed. After uploading and image, click the **"new area"** button to create an area which can be clicked later ingame (to complete an action which was mentioned above). Click on the image to add points/dots (the corners of the desired area). The corners/dots/points have to be added/clicked **CLOCKWISE** to ensure that the area generates correctly when playing the game. When you're finished with the area, click on the input field named **"action"**. Enter either the image name to which the area should lead to or a unity scene name (in the above mentioned syntax with: **"scene:scene_name"**). After completing the area, click **"finish area"**, to finish the area. If you want to add more areas, press **"new area"** again and repeat the process. As soon as all desired areas have been created, click **"generate code"** to get the output. Either select and copy the code or just press **"copy code"** to simply copy the code
### Adding the code to the file:
To add the generated code to the [final file](/Assets/Scripts/locations.json), open the file and paste the code into the document. To add more code snippets you have to put a comma at the end (to seperate them) as following:
```json
{
    "image_name": {
        "path": "Assets/Images/Navigation/image_name.jpg",
        "costumHotspots": [
            {
                "action": "2",
                "polygonString": "0.47452445652173914,0.6022418478260869;0.5322690217391305,0.6059782608695652;0.5407608695652174,0.37024456521739135;0.47927989130434784,0.3627717391304348"
            }
        ]
    }, <-------
        "image_name": {
        "path": "Assets/Images/image_name.jpg",
        "costumHotspots": [
            {
                "action": "2",
                "polygonString": "0.47452445652173914,0.6022418478260869;0.5322690217391305,0.6059782608695652;0.5407608695652174,0.37024456521739135;0.47927989130434784,0.3627717391304348"
            }
        ]
    }
}
```
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
The main class start:
```cs
public class Click : MonoBehaviour {
```
Object/variables/list/dictionarys creation:
```cs
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
```
read json file for the hotspots:
```cs
        JArray costumHotspots = (JArray)data[currentImage.ToString()]["costumHotspots"];
```
loop through the hotspots
```cs
        foreach (var costumHotspot in costumHotspots)
        {
            string action = costumHotspot["action"].ToString();
            var polygonCoordiantes = costumHotspot["polygonString"].ToString().Split(";").Select(p => p.Split(",")).Select(a => new Vector2(float.Parse(a[0]), float.Parse(a[1]))).ToList();

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

            if (int.TryParse(action, out _))
            {
                hotspotActions[polygonObject] = action;
            }
        }
    }

    void setMaterial(int currentImage)
    {
        sphere.material = materials[currentImage.ToString()];
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
        string jsonPath = "Assets/Scripts/locations.json";
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
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hotspotActions.TryGetValue(hit.collider.gameObject, out string action))
                {
                    if (action.split(":")[0] != "scene")
                    {
                        currentImage = int.Parse(action);
                    }
                    else
                    {
                        SceneManager.LoadScene(action);
                        return;
                    }
                    hotspotDestroy(hotspotActions.Keys.ToList());
                    hotspotActions.Clear();
                    hotspotInstantiation(currentImage);
                    setMaterial(currentImage);
                }
            }
        }

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

```


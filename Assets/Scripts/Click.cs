using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

public class Click : MonoBehaviour
{
    public Renderer sphere;
    private Dictionary<string, Material> materials = new Dictionary<string, Material>();
    private int currentImage = 1;
    public GameObject hotspotPrefab;
    private JObject data;
    private Dictionary<GameObject, string> hotspotTargets = new Dictionary<GameObject, string>();


    void hotspotInstantiation(int currentImage)
    {
        JArray hotspots = (JArray)data[currentImage.ToString()]["hotspots"];

        foreach (var hotspot in hotspots)
        {
            string targetImage = hotspot["image"].ToString();
            float xyDegree = float.Parse(hotspot["xyDegree"].ToString());
            float yzDegree = float.Parse(hotspot["yzDegree"].ToString());
            float bodyDegree = float.Parse(hotspot["bodyDegree"].ToString());

            Vector3 position = Quaternion.Euler(yzDegree, xyDegree, 0) * Vector3.forward * 25f;

            GameObject hotspotObject = Instantiate(hotspotPrefab, position, Quaternion.identity);
            hotspotObject.transform.LookAt(Vector3.zero);

            Vector3 euler = hotspotObject.transform.eulerAngles;
            euler.x = bodyDegree;
            hotspotObject.transform.eulerAngles = euler;

            // hotspotObject.transform.position += new Vector3(0f, -10f, 0f);
            hotspotTargets[hotspotObject] = targetImage;
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
                if (hotspotTargets.TryGetValue(hit.collider.gameObject, out string targetImage))
                {
                    currentImage = int.Parse(targetImage);

                    hotspotDestroy(hotspotTargets.Keys.ToList());
                    hotspotTargets.Clear();
                    hotspotInstantiation(currentImage);
                    setMaterial(currentImage);
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

        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = texture;

        return material;
    }
}

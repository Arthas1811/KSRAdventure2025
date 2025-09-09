using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using Newtonsoft.Json.Linq;

public class Click : MonoBehaviour
{
    public string image1;
    public string image2;

    private Material material1;
    private Material material2;

    public Renderer sphere;
    public GameObject button;

    public bool using1 = true;

    void Start() 
    {
        string path = "/locations.json";
        string json = File.ReadAllText(path);

        JObject data = JObject.Parse(json);

        Di

        material1 = LoadMaterialFromPath(image1);
        material2 = LoadMaterialFromPath(image2);
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit)) {
                using1 = !using1;
            }
        }

        if (using1) {
            sphere.material = material2;
            button.transform.position = new Vector3(0f, -10f, 25f);
            button.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        else if (!using1) {
            sphere.material = material1;
            button.transform.position = new Vector3(0f, -10f, -25f);
            button.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
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

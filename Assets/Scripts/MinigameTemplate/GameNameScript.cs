using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

public class GameNameScript : MonoBehaviour
{
    public RawImage background;
    JObject saveData;
    public SaveDataManager saveDataManager;
    void Start()
    {
        saveData = saveDataManager.readData();
        GameObject obj = GameObject.Find("Background");
        background = obj.GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        if (true){// replace with actual if statement
            Win();
        }
        else
        {
            Lose();
        }
    }
    void Win()
    {
        Inventory.Instance.add("redbull"); // evt. add item to inventory
 
        // Savedata changes
        saveData["states"]["start"]["openDoor"] = true; // start -> place,
        saveDataManager.Instance.saveData(saveData);
 
        // switch scene 
        SceneManager.LoadScene("main");
    }
 
    void Lose()
    {
        SceneManager.LoadScene("main");
    }
}
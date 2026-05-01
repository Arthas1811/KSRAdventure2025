using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Net.Security;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

public class Server2 : MonoBehaviour
{
    //SAVING
    //S JObject saveData;
    //S public SaveDataManager saveDataManager;

    public GameObject startGO;
    public GameObject endGO;
    public List<Node> endNodesList;
    public List<Node> startNodesList;
    public TextMeshProUGUI textTMP;
    public AudioSource ambSound;
    public Sprite bgImg;
    public GameObject bgImgObj;
    public GameObject auxObj;
    public GameObject particlesObj;

    private int totalV;
    private int beatCount = 0;

    void Start()
    {
        startGO.SetActive(false); // hides the circuit till toor is opened
        endGO.SetActive(false);
        particlesObj.SetActive(false);

        Transform startListTransform = startGO.transform;
        Transform endListTransform = endGO.transform;

        for (int i = 0; i < endListTransform.childCount; i++) // get all start nodes
        {
            Node node = startListTransform.GetChild(i).GetComponent<Node>();
            if (node != null)
                startNodesList.Add(node);
        }

        for (int i = 0; i < endListTransform.childCount; i++) // get all end nodes
        {
            Node node = endListTransform.GetChild(i).GetComponent<Node>();
            if (node != null)
                endNodesList.Add(node);
        }
    }

    void Update()
    {
        totalV = 0;
        int C = 0;

        foreach (Node node in startNodesList) // calculate total V
        {
            int p1 = node.V;
            Connection c = node.GetComponent<Connection>();
            int p2 = 0;

            if (c && c.end)
            {
                C += 1;
                Node e = c.end;
                p2 = e.V;
            }

            totalV += p1 * p2;
        }

        textTMP.text = totalV.ToString() + " V"; // display total V

        ambSound.volume = totalV / 42f / 4f; // adjust volume basen on current

        if (totalV == 22 && C == 5) // win condition
        {
            particlesObj.SetActive(true);
            //S Inventory.Instance.add("redbull");
            //S saveData["states"]["start"]["openDoor"] = true;
            //S saveDataManager.Instance.saveData(saveData);
            SceneManager.LoadScene("main");
        }
    }

    public void beat() // called when door is hit
    {
        beatCount += 1;
        auxObj.GetComponent<Audio>().Hit();

        if (beatCount >= 1)
        {
            open();
        }
    }

    void open() // called when door is hit enough times
    {
        bgImgObj.GetComponent<Image>().sprite = bgImg;
        startGO.SetActive(true);
        endGO.SetActive(true);
    }
}
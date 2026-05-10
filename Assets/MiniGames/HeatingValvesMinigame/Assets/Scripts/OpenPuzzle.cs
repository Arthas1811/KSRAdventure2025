using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject panel;
    public GameObject Ventil1;
    public GameObject Ventil2;
    public void OpenPanel()
    {
        panel.SetActive(true);
        Ventil1.SetActive(false);
        Ventil2.SetActive(false);
    }
}
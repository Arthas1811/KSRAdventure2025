using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ServerDestruction : MonoBehaviour
{
    [Header("UI Referenzen")]
    public GameObject winScreen;
    public GameObject tabellenPopup;
    public Button legendeButton;
    public Button schliessenButton;

    [Header("Alle Sicherungs-Buttons")]
    public List<Button> alleSicherungen = new List<Button>();

    // Speichert den Zustand jeder Sicherung (false = aus, true = an)
    private Dictionary<string, bool> sicherungZustand = new Dictionary<string, bool>();

    // Die zwei richtigen Sicherungen
    private string loesung1 = "Sicherung_156F2";
    private string loesung2 = "Sicherung_160F6";

    void Start()
    {
        winScreen.SetActive(false);
        tabellenPopup.SetActive(false);

        // Tabellen-Buttons
        legendeButton.onClick.AddListener(ZeigeTabelle);
        schliessenButton.onClick.AddListener(SchliesseTabelle);

        // Sicherungs-Buttons
        foreach (Button sicherung in alleSicherungen)
        {
            string sicherungsName = sicherung.gameObject.name;
            sicherungZustand[sicherungsName] = false;

            Button aktuellerButton = sicherung;
            sicherung.onClick.AddListener(() => SicherungKlick(aktuellerButton));
        }
    }

    void ZeigeTabelle()
    {
        tabellenPopup.SetActive(true);
    }

    void SchliesseTabelle()
    {
        tabellenPopup.SetActive(false);
    }

    void SicherungKlick(Button sicherung)
    {
        string name = sicherung.gameObject.name;

        // Zustand umschalten
        sicherungZustand[name] = !sicherungZustand[name];
        bool istAn = sicherungZustand[name];

        // Overlay ein-/ausblenden
        Transform overlay = sicherung.transform.Find("Overlay");
        if (overlay != null)
        {
            overlay.gameObject.SetActive(istAn);
        }

        // Prüfen ob gewonnen
        PruefeGewonnen();
    }

    void PruefeGewonnen()
    {
        // Beide richtigen müssen an sein
        bool loesung1An = sicherungZustand.ContainsKey(loesung1) && sicherungZustand[loesung1];
        bool loesung2An = sicherungZustand.ContainsKey(loesung2) && sicherungZustand[loesung2];

        if (!loesung1An || !loesung2An)
        {
            return;
        }

        // Alle anderen müssen aus sein
        foreach (var kvp in sicherungZustand)
        {
            if (kvp.Key == loesung1 || kvp.Key == loesung2)
            {
                continue;
            }

            if (kvp.Value == true)
            {
                return;
            }
        }

        // Gewonnen
        StartCoroutine(ZeigeGewonnen());
    }

    IEnumerator ZeigeGewonnen()
    {
        yield return new WaitForSeconds(0.5f);
        winScreen.SetActive(true);
    }
}

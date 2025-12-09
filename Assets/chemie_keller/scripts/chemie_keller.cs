using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.IO;

public class chemie_keller : MonoBehaviour
{
    [Header("Stretch Settings")]
    public float minStretch = 0.9f;   // kleiner als 1 = leicht schrumpfen
    public float maxStretch = 1f;   // größer als 1 = leicht wachsen
    public float animationSpeed = 2f;

    [Header("Mapped Value")]
    public float c;
   public float heightOffset;

    [Header("UI")]
    public TextMeshProUGUI cText;
    public TextMeshProUGUI winText;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool isFrozen = false;

    // Wird im Editor ausgeführt, damit bei falschen Inspector-Werten automatisch korrigiert wird
    private void OnValidate()
    {
        // Wenn min > max, tauschen wir sie automatisch
        if (minStretch > maxStretch)
        {
            float tmp = minStretch;
            minStretch = maxStretch;
            maxStretch = tmp;
        }
        // Verhindere negative oder 0 Werte, die spiegeln oder absinken würden
        minStretch = Mathf.Max(minStretch, 0.01f);
        maxStretch = Mathf.Max(maxStretch, 0.01f);
        animationSpeed = Mathf.Max(0.01f, animationSpeed);
    }

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;

        if (cText != null) cText.gameObject.SetActive(false);
        if (winText != null) winText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isFrozen)
        {
            float t = Mathf.Sin(Time.time * animationSpeed) * 0.5f + 0.5f; // 0..1
            float dynamicStretch = Mathf.Lerp(minStretch, maxStretch, t);

            // Safety clamp: verhindert 0 oder negative Skalierung
            dynamicStretch = Mathf.Clamp(dynamicStretch, 0.01f, 10f);

            // --- Nur Y-Achse strecken (empfohlen) ---
            transform.localScale = new Vector3(originalScale.x, originalScale.y * dynamicStretch, originalScale.z);

            // Position anpassen, damit Boden/oben "konstant" wirkt
            heightOffset = (originalScale.y * (dynamicStretch - 1f)) / 2f;

            // Normalisieren und auf den c-Wert mappen
            float normalized = Mathf.InverseLerp(-0.11f, 0.11f, heightOffset);
            normalized = Mathf.Clamp01(normalized);
            c = Mathf.Lerp(8f, 21f, normalized);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isFrozen = true;
            if (cText != null)
            {
                cText.gameObject.SetActive(true);
                c = Mathf.Round(c * 10f) / 10f;
                cText.text = c.ToString("F2") + " ml";
                //cText.text = heightOffset.ToString("F2");
            }

            if (winText != null)
            {
                winText.gameObject.SetActive(true);
                if (11 <= c && c <= 13)
                    winText.text = "WOW! You (almost) hit the perfect amount";
                else
                    winText.text = "You screwed up too much. Unacceptable.";
            }
        }
    }
}

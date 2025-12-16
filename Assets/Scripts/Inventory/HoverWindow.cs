using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class HoverWindow : MonoBehaviour
{
    public static HoverWindow Instance;

    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public TMP_Text hoverWindowTitle;
    public TMP_Text hoverWindowText;

    [Header("Settings")]
    public Vector2 offset = new Vector2(10f, -10f);

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    private void Update()
    {
        if (canvasGroup == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            mousePos,
            null,
            out localPos
        );
        transform.localPosition = localPos + offset;
    }

    // show hover window
    public void Show(string name, string description)
    {
        if (hoverWindowTitle != null)
            hoverWindowTitle.text = name;
        if (hoverWindowText != null)
            hoverWindowText.text = description;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
     }

    // hide hover window
    public void Hide()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
}

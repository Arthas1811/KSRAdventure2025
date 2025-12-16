using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class InventoryAnimatedToggle : MonoBehaviour
{
    [Header("Assign Inventory Canvas Root")]
    public GameObject inventoryCanvas;

    private CanvasGroup canvasGroup;
    private bool isOpen = false;
    private float animationDuration = 0.25f; // seconds

    void Start()
    {
        if (inventoryCanvas == null)
        {
            Debug.LogError("Assign the Inventory Canvas in the inspector!");
            return;
        }

        canvasGroup = inventoryCanvas.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = inventoryCanvas.AddComponent<CanvasGroup>();

        inventoryCanvas.SetActive(false);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
            ToggleInventory();
    }

    private void ToggleInventory()
    {
        isOpen = !isOpen;

        StopAllCoroutines();
        StartCoroutine(AnimateInventory(isOpen));
    }

    private IEnumerator AnimateInventory(bool open)
    {
        if (open)
            inventoryCanvas.SetActive(true);

        float startAlpha = canvasGroup.alpha;
        float endAlpha = open ? 1f : 0f;

        Vector3 startScale = inventoryCanvas.transform.localScale;
        Vector3 endScale = open ? Vector3.one : new Vector3(0.9f, 0.9f, 0.9f);

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            t = Mathf.SmoothStep(0, 1, t); // smooth curve

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            inventoryCanvas.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        inventoryCanvas.transform.localScale = endScale;

        canvasGroup.interactable = open;
        canvasGroup.blocksRaycasts = open;

        if (!open)
            inventoryCanvas.SetActive(false);
    }
}

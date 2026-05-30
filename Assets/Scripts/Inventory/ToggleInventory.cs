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
    public Click main;

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

        // IMPORTANT: keep the canvas GameObject ACTIVE at all times and hide it
        // purely through the CanvasGroup. If we SetActive(false), the panel's
        // GridLayoutGroup stops laying out children, so any item picked up while
        // the inventory is closed never gets positioned (it would only appear
        // after a scene reload). Alpha 0 + blocksRaycasts false is fully invisible
        // and non-interactive, but layout keeps working so items show up instantly.
        inventoryCanvas.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        inventoryCanvas.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
    }

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && !main.dialogueOpen)
            ToggleInventory();
    }

    private void ToggleInventory()
    {
        isOpen = !isOpen;
        main.inventoryOpen = isOpen;
        StopAllCoroutines();
        StartCoroutine(AnimateInventory(isOpen));
    }

    private IEnumerator AnimateInventory(bool open)
    {
        // The canvas stays active the whole time (see Start), so the layout group
        // is always live. On open we just refresh from the save in case items were
        // picked up while it was hidden.
        if (open && Inventory.Instance != null)
            Inventory.Instance.LoadFromSave();

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
        // NOTE: intentionally NOT calling inventoryCanvas.SetActive(false) on close.
        // Keeping it active (alpha 0) leaves the GridLayoutGroup live so future
        // pickups lay out immediately instead of needing a scene reload.
    }
}

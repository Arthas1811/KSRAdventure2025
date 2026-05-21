using UnityEngine;
using UnityEngine.InputSystem;

// Script for custom cursor, help from: https://www.youtube.com/watch?v=BCYJ4UNClhk
public class CursorUI : MonoBehaviour
{
    [SerializeField]
    private InputActionReference pointerPositionAction;

    private RectTransform _cursorTransform;
    private Canvas _parentCanvas;
    private RectTransform _canvasRectTransform;
    private Camera _canvasCamera;

    private void Awake()
    {
        _cursorTransform = GetComponent<RectTransform>();
        _parentCanvas = GetComponentInParent<Canvas>();
        if (_parentCanvas != null)
        {
            _canvasRectTransform = _parentCanvas.GetComponent<RectTransform>();
            _canvasCamera = _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera;
        }
    }

    private void OnEnable()
    {
        Cursor.visible = false;
        pointerPositionAction.action.performed += OnPointerPositionChanged;
    }

    private void OnDisable()
    {
        Cursor.visible = true;
        pointerPositionAction.action.performed -= OnPointerPositionChanged;
    }

    private void OnPointerPositionChanged(InputAction.CallbackContext ctx)
    {
        if(_cursorTransform == null || _canvasRectTransform == null)
        {
            return;
        }
        var mousePosition = ctx.ReadValue<Vector2>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform, mousePosition, _canvasCamera, out var localPoint
        ))
        {
            _cursorTransform.anchoredPosition = localPoint;
        }
    }
}

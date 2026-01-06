using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public float sensitivity = 0.2f;
    private float x = 0f;
    private float y = 0f;
    public Click main;

    void Update()
    {
        if (!main.inventoryOpen) {
            if (Mouse.current != null) {
                if (Mouse.current.leftButton.isPressed) {
                    Vector2 mouse = Mouse.current.delta.ReadValue();

                    x -= mouse.y * sensitivity;
                    y += mouse.x * sensitivity;

                    x = Mathf.Clamp(x, -90f, 90f);

                    transform.localRotation = Quaternion.Euler(-x, -y, 0f);
                }
            }
        }
    }
}

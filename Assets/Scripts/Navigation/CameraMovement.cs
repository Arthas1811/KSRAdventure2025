using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public float sensitivity = 0.2f;
    public float x = 0f;
    public float y = 0f;
    public Click main;

    void Update()
    {
        if (!main.inventoryOpen && !main.dialogueOpen) {
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

    public void setNewRotation(float newX, float newY) {
        x = newX;
        y = newY;
        transform.localRotation = Quaternion.Euler(-x, -y, 0f);
    }

    public void setNewFOV(float newFOV) {
        Camera.main.fieldOfView = newFOV;
    }
}

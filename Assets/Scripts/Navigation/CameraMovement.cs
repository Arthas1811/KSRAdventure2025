using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public float sensitivity = 0.2f;
    public float x = 0f;
    public float y = 0f;
    public Click main;

    public Key MoveUpwardsKey = Key.W;
    public Key MoveDownwardsKey = Key.S;
    public Key MoveRightKey = Key.D;
    public Key MoveLeftKey = Key.A;

    void Update()
    {
        if (!main.inventoryOpen && !main.dialogueOpen && !main.imageOpen)
        {
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    Vector2 mouse = Mouse.current.delta.ReadValue();

                    x -= mouse.y * sensitivity;
                    y += mouse.x * sensitivity;

                    x = Mathf.Clamp(x, -90f, 90f);

                    transform.localRotation = Quaternion.Euler(-x, -y, 0f);
                }
            }
        }
        if (Keyboard.current != null && Keyboard.current[MoveUpwardsKey].isPressed)
        {
            x += 0.5f;
            x = Mathf.Clamp(x, -90f, 90f);
            transform.localRotation = Quaternion.Euler(-x, -y, 0f);
        }
        if (Keyboard.current != null && Keyboard.current[MoveDownwardsKey].isPressed)
        {
            x -= 0.5f;
            x = Mathf.Clamp(x, -90f, 90f);
            transform.localRotation = Quaternion.Euler(-x, -y, 0f);
        }
        if (Keyboard.current != null && Keyboard.current[MoveRightKey].isPressed)
        {
            y -= 0.5f;
            x = Mathf.Clamp(x, -90f, 90f);
            transform.localRotation = Quaternion.Euler(-x, -y, 0f);
        }
        if (Keyboard.current != null && Keyboard.current[MoveLeftKey].isPressed)
        {
            y += 0.5f;
            x = Mathf.Clamp(x, -90f, 90f);
            transform.localRotation = Quaternion.Euler(-x, -y, 0f);
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

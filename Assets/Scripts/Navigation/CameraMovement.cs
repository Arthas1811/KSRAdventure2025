using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    public float mouseSensitivity = 0.2f;
    public float arrowSensitivity = 1f;
    public Slider mouseSlider;
    public Slider arrowSlider;
    public float x = 0f;
    public float y = 0f;
    public Click main;

    public Key MoveUpwardsKey = Key.W;
    public Key MoveDownwardsKey = Key.S;
    public Key MoveRightKey = Key.D;
    public Key MoveLeftKey = Key.A;

    void Update()
    {
        mouseSensitivity = mouseSlider.value;
        arrowSensitivity = arrowSlider.value;
        if (PhoneSceneNavigation.IsPhoneOverlayOpen)
        {
            return;
        }

        if (!main.inventoryOpen && !main.dialogueOpen && !main.imageOpen && !main.saveStringUIOpen)
        {
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    Vector2 mouse = Mouse.current.delta.ReadValue();

                    x -= mouse.y * mouseSensitivity;
                    y += mouse.x * mouseSensitivity;

                    x = Mathf.Clamp(x, -90f, 90f);

                    transform.localRotation = Quaternion.Euler(-x, -y, 0f);
                }
            }
            if (Keyboard.current != null && Keyboard.current[MoveUpwardsKey].isPressed)
            {
                x += arrowSensitivity;
                x = Mathf.Clamp(x, -90f, 90f);
                transform.localRotation = Quaternion.Euler(-x, -y, 0f);
            }
            if (Keyboard.current != null && Keyboard.current[MoveDownwardsKey].isPressed)
            {
                x -= arrowSensitivity;
                x = Mathf.Clamp(x, -90f, 90f);
                transform.localRotation = Quaternion.Euler(-x, -y, 0f);
            }
            if (Keyboard.current != null && Keyboard.current[MoveRightKey].isPressed)
            {
                y -= arrowSensitivity;
                x = Mathf.Clamp(x, -90f, 90f);
                transform.localRotation = Quaternion.Euler(-x, -y, 0f);
            }
            if (Keyboard.current != null && Keyboard.current[MoveLeftKey].isPressed)
            {
                y += arrowSensitivity;
                x = Mathf.Clamp(x, -90f, 90f);
                transform.localRotation = Quaternion.Euler(-x, -y, 0f);
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

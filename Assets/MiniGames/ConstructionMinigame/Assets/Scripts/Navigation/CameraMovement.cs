// Script taken from KSRAdventure2025

using UnityEngine;
using UnityEngine.InputSystem;

// Handles first-person camera rotation using either the mouse or keyboard.
public class CameraMovement : MonoBehaviour
{
    public float Sensitivity = 0.2f;
    public float X = 0f;
    public float Y = 0f;
    public Click Main;

    public Key MoveUpwardsKey = Key.W;
    public Key MoveDownwardsKey = Key.S;
    public Key MoveRightKey = Key.D;
    public Key MoveLeftKey = Key.A;

    void Update()
    {
        // Only allow camera movement if no UI panels are active
        if (!Main.InventoryOpen && !Main.DialogueOpen && !Main.ImageOpen && !Main.ShowInstructions)
        {
            // Mouse Look
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    Vector2 mouse = Mouse.current.delta.ReadValue();

                    X -= mouse.y * Sensitivity;
                    Y += mouse.x * Sensitivity;

                    // Clamp the vertical rotation to prevent the camera from flipping upside down
                    X = Mathf.Clamp(X, -90f, 90f);

                    transform.localRotation = Quaternion.Euler(-X, -Y, 0f);
                }
            }
        }

        // Keyboard Look 
        if (Keyboard.current != null && !Main.ShowInstructions && Keyboard.current[MoveUpwardsKey].isPressed)
        {
            X += 0.5f;
            X = Mathf.Clamp(X, -90f, 90f);
            transform.localRotation = Quaternion.Euler(-X, -Y, 0f);
        }
        if (Keyboard.current != null && !Main.ShowInstructions && Keyboard.current[MoveDownwardsKey].isPressed)
        {
            X -= 0.5f;
            X = Mathf.Clamp(X, -90f, 90f);
            transform.localRotation = Quaternion.Euler(-X, -Y, 0f);
        }
        if (Keyboard.current != null && !Main.ShowInstructions && Keyboard.current[MoveRightKey].isPressed)
        {
            Y -= 0.5f;
            X = Mathf.Clamp(X, -90f, 90f);
            transform.localRotation = Quaternion.Euler(-X, -Y, 0f);
        }
        if (Keyboard.current != null && !Main.ShowInstructions && Keyboard.current[MoveLeftKey].isPressed)
        {
            Y += 0.5f;
            X = Mathf.Clamp(X, -90f, 90f);
            transform.localRotation = Quaternion.Euler(-X, -Y, 0f);
        }
    }

    // Force the camera to look at a specific angle 
    public void SetNewRotation(float newX, float newY) {
        X = newX;
        Y = newY;
        transform.localRotation = Quaternion.Euler(-X, -Y, 0f);
    }

    public void SetNewFOV(float newFOV) {
        Camera.main.fieldOfView = newFOV;
    }
}
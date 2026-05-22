// Script taken from KSRAdventure2025

using UnityEngine;
using UnityEngine.InputSystem;

// Handles first-person camera rotation using either the mouse or keyboard.
public class CameraMovementConstruction : MonoBehaviour
{
    public float Sensitivity = 0.2f;
    public float X = 0f;
    public float Y = 0f;
    public Construction Main;

    public Key MoveUpwardsKey = Key.W;
    public Key MoveDownwardsKey = Key.S;
    public Key MoveRightKey = Key.D;
    public Key MoveLeftKey = Key.A;

    public float KeyboardRotationStep = 0.5f;
    public float MaxVerticalAngle = 90f;
    public float EulerZRotation = 0f;

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
                    X = Mathf.Clamp(X, -MaxVerticalAngle, MaxVerticalAngle);

                    transform.localRotation = Quaternion.Euler(-X, -Y, EulerZRotation);
                }
            }
        }

        // Keyboard Look 
        if (Keyboard.current != null && !Main.ShowInstructions && Keyboard.current[MoveUpwardsKey].isPressed)
        {
            X += KeyboardRotationStep;
            X = Mathf.Clamp(X, -MaxVerticalAngle, MaxVerticalAngle);
            transform.localRotation = Quaternion.Euler(-X, -Y, EulerZRotation);
        }
        if (Keyboard.current != null && !Main.ShowInstructions && Keyboard.current[MoveDownwardsKey].isPressed)
        {
            X -= KeyboardRotationStep;
            X = Mathf.Clamp(X, -MaxVerticalAngle, MaxVerticalAngle);
            transform.localRotation = Quaternion.Euler(-X, -Y, EulerZRotation);
        }
        if (Keyboard.current != null && !Main.ShowInstructions && Keyboard.current[MoveRightKey].isPressed)
        {
            Y -= KeyboardRotationStep;
            X = Mathf.Clamp(X, -MaxVerticalAngle, MaxVerticalAngle);
            transform.localRotation = Quaternion.Euler(-X, -Y, EulerZRotation);
        }
        if (Keyboard.current != null && !Main.ShowInstructions && Keyboard.current[MoveLeftKey].isPressed)
        {
            Y += KeyboardRotationStep;
            X = Mathf.Clamp(X, -MaxVerticalAngle, MaxVerticalAngle);
            transform.localRotation = Quaternion.Euler(-X, -Y, EulerZRotation);
        }
    }

    // Force the camera to look at a specific angle 
    public void SetNewRotation(float newX, float newY) {
        X = newX;
        Y = newY;
        transform.localRotation = Quaternion.Euler(-X, -Y, EulerZRotation);
    }

    public void SetNewFOV(float newFOV) {
        Camera.main.fieldOfView = newFOV;
    }
}
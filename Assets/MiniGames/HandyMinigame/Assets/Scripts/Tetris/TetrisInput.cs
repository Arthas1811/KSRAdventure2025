using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

//handles tetris input
public class TetrisInput : MonoBehaviour
{
    private const float HorizontalInitialDelay = 0.18f;
    private const float HorizontalRepeatDelay = 0.06f;
    private const float SoftDropRepeatDelay = 0.04f;

    private TetrisGame game;
    private bool wasHoldingLeft;
    private bool wasHoldingRight;
    private bool wasHoldingSoftDrop;
    private bool suppressSoftDropUntilReleased;
    private bool suppressHardDropUntilReleased;
    private float leftRepeatTimer;
    private float rightRepeatTimer;
    private float softDropRepeatTimer;

    //sets start data
    public void Initialize(TetrisGame game)
    {
        this.game = game;
    }

    //resets drop input
    public void ResetDropInputsForNewPiece()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            suppressSoftDropUntilReleased = false;
            suppressHardDropUntilReleased = false;
            ResetSoftDropRepeat();
            return;
        }

        suppressSoftDropUntilReleased = IsPressed(keyboard.downArrowKey, keyboard.sKey);
        suppressHardDropUntilReleased = IsPressed(keyboard.spaceKey, null);
        ResetSoftDropRepeat();
    }

    //updates game loop
    private void Update()
    {
        if (game == null || game.IsGameOver)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        HandleHeldInput(IsPressed(keyboard.leftArrowKey, keyboard.aKey), ref wasHoldingLeft, ref leftRepeatTimer, HorizontalInitialDelay, HorizontalRepeatDelay, game.MoveLeft);
        HandleHeldInput(IsPressed(keyboard.rightArrowKey, keyboard.dKey), ref wasHoldingRight, ref rightRepeatTimer, HorizontalInitialDelay, HorizontalRepeatDelay, game.MoveRight);

        bool softDropPressed = IsPressed(keyboard.downArrowKey, keyboard.sKey);
        if (suppressSoftDropUntilReleased)
        {
            if (!softDropPressed)
            {
                suppressSoftDropUntilReleased = false;
            }

            ResetSoftDropRepeat();
        }
        else
        {
            HandleHeldInput(softDropPressed, ref wasHoldingSoftDrop, ref softDropRepeatTimer, SoftDropRepeatDelay, SoftDropRepeatDelay, game.SoftDropOneRow);
        }

        bool hardDropPressed = IsPressed(keyboard.spaceKey, null);
        if (suppressHardDropUntilReleased)
        {
            if (!hardDropPressed)
            {
                suppressHardDropUntilReleased = false;
            }
        }
        else if (WasPressed(keyboard.spaceKey, null))
        {
            game.HardDrop();
        }

        if (WasPressed(keyboard.upArrowKey, keyboard.xKey))
        {
            game.RotateClockwise();
        }

        if (WasPressed(keyboard.yKey, null))
        {
            game.RotateCounterclockwise();
        }
    }

    //handles held input
    private static void HandleHeldInput(bool isPressed, ref bool wasHolding, ref float repeatTimer, float initialDelay, float repeatDelay, System.Action action)
    {
        if (!isPressed)
        {
            wasHolding = false;
            repeatTimer = 0f;
            return;
        }

        if (!wasHolding)
        {
            wasHolding = true;
            repeatTimer = initialDelay;
            action.Invoke();
            return;
        }

        repeatTimer -= Time.unscaledDeltaTime;
        while (repeatTimer <= 0f)
        {
            action.Invoke();
            repeatTimer += repeatDelay;
        }
    }

    //checks pressed key
    private static bool IsPressed(KeyControl primary, KeyControl secondary)
    {
        return (primary != null && primary.isPressed) || (secondary != null && secondary.isPressed);
    }

    //checks new key press
    private static bool WasPressed(KeyControl primary, KeyControl secondary)
    {
        return (primary != null && primary.wasPressedThisFrame) || (secondary != null && secondary.wasPressedThisFrame);
    }

    //resets soft drop repeat
    private void ResetSoftDropRepeat()
    {
        wasHoldingSoftDrop = false;
        softDropRepeatTimer = 0f;
    }
}

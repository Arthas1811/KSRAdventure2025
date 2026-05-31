using System.Collections.Generic;
using UnityEngine;

//handles block blast tray
public class BlockBlastTray : MonoBehaviour
{
    private BlockBlastGame game;
    private RectTransform[] slots;
    private DraggableBlockShape[] activeShapes;

    //sets start data
    public void Initialize(BlockBlastGame game, RectTransform[] traySlots)
    {
        this.game = game;
        slots = traySlots;
        activeShapes = new DraggableBlockShape[slots.Length];
    }

    //makes new tray
    public void GenerateNewTray()
    {
        ClearExistingShapes();

        for (int i = 0; i < slots.Length; i++)
        {
            activeShapes[i] = game.CreateDraggableShape(BlockBlastShapeLibrary.GetRandomShape(), slots[i]);
        }
    }

    //marks shape used
    public void MarkShapeUsed(DraggableBlockShape draggableShape)
    {
        for (int i = 0; i < activeShapes.Length; i++)
        {
            if (activeShapes[i] == draggableShape)
            {
                activeShapes[i] = null;
                break;
            }
        }

        DestroyShapeObject(draggableShape.gameObject);

        if (AllSlotsEmpty())
        {
            GenerateNewTray();
        }
    }

    //gets open shapes
    public List<BlockBlastShape> GetAvailableShapes()
    {
        List<BlockBlastShape> availableShapes = new List<BlockBlastShape>();

        foreach (DraggableBlockShape activeShape in activeShapes)
        {
            if (activeShape != null)
            {
                availableShapes.Add(activeShape.Shape);
            }
        }

        return availableShapes;
    }

    //updates shape fit state
    public void RefreshShapeAvailability(BlockBlastBoard board)
    {
        foreach (DraggableBlockShape activeShape in activeShapes)
        {
            if (activeShape != null)
            {
                activeShape.SetAvailability(board.CanShapeFitAnywhere(activeShape.Shape));
            }
        }
    }

    //checks fitting shapes
    public bool HasAnyShapeThatFits(BlockBlastBoard board)
    {
        return board.CanAnyShapeFit(GetAvailableShapes());
    }

    //checks empty slots
    private bool AllSlotsEmpty()
    {
        foreach (DraggableBlockShape activeShape in activeShapes)
        {
            if (activeShape != null)
            {
                return false;
            }
        }

        return true;
    }

    //clears old shapes
    private void ClearExistingShapes()
    {
        if (activeShapes == null)
        {
            return;
        }

        for (int i = 0; i < activeShapes.Length; i++)
        {
            if (activeShapes[i] != null)
            {
                DestroyShapeObject(activeShapes[i].gameObject);
                activeShapes[i] = null;
            }
        }
    }

    //removes shape object
    private static void DestroyShapeObject(GameObject shapeObject)
    {
        if (shapeObject == null)
        {
            return;
        }

        shapeObject.SetActive(false);

        if (Application.isPlaying)
        {
            Destroy(shapeObject);
        }
        else
        {
            DestroyImmediate(shapeObject);
        }
    }
}

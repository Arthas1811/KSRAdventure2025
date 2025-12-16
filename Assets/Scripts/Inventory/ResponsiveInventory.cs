using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveInventoryGrid : MonoBehaviour
{
    private GridLayoutGroup grid;

    [Header("Grid Settings")]
    public int columns = 4;     
    public int rows = 4;                
    public float horizontalSpacing = 0f;
    public float verticalSpacing = 0f;

    void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
    }

    void Update()
    {
        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();

        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        // calculate cell size 
        float cellWidth = parentWidth / columns;
        float cellHeight = parentHeight / rows;

        grid.cellSize = new Vector2(cellWidth, cellHeight);

        grid.spacing = new Vector2(horizontalSpacing, verticalSpacing);

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
    }
}

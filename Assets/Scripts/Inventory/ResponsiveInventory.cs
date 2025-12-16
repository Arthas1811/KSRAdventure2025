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

    }
}

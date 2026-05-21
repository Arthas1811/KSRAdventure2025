using UnityEngine;
using UnityEngine.EventSystems;

public class LeaveButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public HoleInWall holeInWall;
    public void OnPointerEnter(PointerEventData eventData)
    {
        holeInWall.LeaveButtonHovered(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        holeInWall.LeaveButtonHovered(false);
    }
}

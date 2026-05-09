using UnityEngine;
using UnityEngine.EventSystems;


public class ValveWheel : MonoBehaviour, IPointerClickHandler
{
    public Temperature temperature;
    public string partID;

    public int clicksNeeded = 10;
    public int currentClicks = 0;
    public float rotationAmount = 36f;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Ventil geklickt!");

        if (currentClicks < clicksNeeded)
        {
            currentClicks++;

            transform.Rotate(0f, 0f, rotationAmount);

            temperature.LowerTemperature();
            if (currentClicks == 10)
            {
                InstructionsManager.Instance.PartCompleted(partID);
            }
        }
    }
}
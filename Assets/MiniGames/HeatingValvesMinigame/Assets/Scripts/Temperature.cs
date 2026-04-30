using UnityEngine;
using TMPro;

public class Temperature : MonoBehaviour
{
    public int temperature = 50;
    public int targetTemperature = 20;

    public TMP_Text temperatureText;

    void Start()
    {
        UpdateText();
    }

    public void LowerTemperature()
    {
        if (temperature > targetTemperature)
        {
            temperature -= 1;
        }

        UpdateText();

        Debug.Log("Temperatur: " + temperature + "°C");

    }

    void UpdateText()
    {
        temperatureText.text = temperature + "°C";
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class DrillButtons
{
    public Button button;
    public bool isHolding = false;
    public float holdingTime = 0f;
}
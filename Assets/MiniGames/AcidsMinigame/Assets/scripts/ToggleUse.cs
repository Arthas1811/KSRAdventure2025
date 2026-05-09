using UnityEngine;

public class ToggleObjects : MonoBehaviour
{
    public GameObject targetObject;

    // Identify which button this is
    public bool isWaterButton;
    public bool isPVCButton;

    public bool WaterInUse= false;
    public bool LithiumInUse = false;
    public bool BurnerInstalled = false;
    public bool FHInstalled = false;
    public bool PVCInUse = false;
    public bool burnerInUse = false;

    // Called by Water button
    public void UseWater()
    {
        WaterInUse = !WaterInUse;
        Toggle();
        Debug.Log("Water button pressed" + WaterInUse.ToString());
    }

    // Called by PVC button
    public void UsePVC()
    {
        isWaterButton = false;
        Toggle();
        Debug.Log("PVC button pressed");
    }

    public void UseLithium()
    {
        isWaterButton = false;
        Toggle();
        Debug.Log("Lithium button pressed");
    }

    public void InstallBurner()
    {
        Toggle();
        Debug.Log("Burner button Pressed");
        bool BurnerInUse = true;
    }

    public void LightBurner()
    {
        Toggle();
        Debug.Log("Burner light button Pressed");
    }

    public void InstallFH()
    { 
        Toggle();
        Debug.Log("Fumehood button pressed"+"hello world");
    }



    // Shared toggle logic
    private void Toggle()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);

        }
    }
}
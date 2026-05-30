using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image fill;

    // Update is called once per frame
    void Update()
    {
        float ratio = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        fill.fillAmount = ratio;
    }
}

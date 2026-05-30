using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public Image fill;
    public BossHealth boss;
    float t;

    void Update()
    {
        if (boss.currentHealth <= 0)
        {
            t = 0;
        }
        else
        {
            t = (float)boss.currentHealth / boss.maxHealth;
        }
        fill.transform.localScale = new Vector3(t, 1f, 1f);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 6;
    public int currentHealth;
    public float invincibleTime = 1f;

    bool isInvincible = false;
    bool dead = false;
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    void Start()
    {
        currentHealth = maxHealth;
    }


    public void TakeDamage(int amount)
    {
        if (isInvincible) { return; }
        if(dead) { return; }

        currentHealth -= amount;

        if ((currentHealth <= 0))
        {
            Die();
        }

        else
        {
            StartCoroutine(Invincibility());
        }

        IEnumerator Invincibility()
        {
            isInvincible = true;
            yield return new WaitForSeconds(invincibleTime);
            isInvincible = false;
        }

        void Die()
        {
            dead = true;
            anim.SetTrigger("die");
            GetComponent<PlayerMovement>().enabled = false;
            GameOverUI.Instance.Show();
        }

    }
}

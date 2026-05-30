using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public bool enraged = false;
    public int enragedThreshold = 30;
    public bool canBeHit = true;

    public AudioClip hurtSFX;
    public AudioClip enrageSFX;
    public AudioClip deathSFX;
    AudioSource audioSource;


    SpriteRenderer sr;

    Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(int amount)
    {
        if (!canBeHit) return;

        currentHealth -= amount;

        if (!enraged && currentHealth <= maxHealth * enragedThreshold / 100)
        {
            StartEnrage();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            audioSource.PlayOneShot(hurtSFX);
            StartCoroutine(HitWhite());
        }
    }

    IEnumerator HitWhite()
    {
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }

    void StartEnrage()
    {
        audioSource.PlayOneShot(enrageSFX);
        enraged = true;
        anim.SetTrigger("enraged");
        GetComponent<BossController>().attackDelay = 2.5f;
        Robot[] robots = FindObjectsByType<Robot>(FindObjectsSortMode.None);
        foreach (var robot in robots)
        {
            robot.speed *= 1.5f;
        }
    }

    public void Die()
    {
        if (enraged) enraged = false;
        canBeHit = false;

        GetComponent<BossController>().StopAllCoroutines();
        audioSource.PlayOneShot(deathSFX);
        anim.SetTrigger("die");
    }

    public void DeathAnimationEnd()
    {
        anim.enabled = false;
        StartCoroutine(RemoveBoss());
        SceneManager.LoadScene("Outro");
    }


    IEnumerator RemoveBoss()
    {
        yield return new WaitForSeconds(1.5f);
        Time.timeScale = 1f;
        Destroy(gameObject);
    }

}

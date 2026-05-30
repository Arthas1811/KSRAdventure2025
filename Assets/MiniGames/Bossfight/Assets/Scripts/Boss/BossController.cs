using JetBrains.Annotations;
using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public AudioClip fireSFX;
    public AudioClip lightningSFX;
    public AudioClip robotSFX;
    public AudioClip shockwaveSFX;
    AudioSource audioSource;

    BossHealth bh;
    int attack;

    public FireRain fire;
    public Animator anim;

    public Lightning lightning;

    public float attackDelay = 4f;

    public GameObject robotPrefab;
    public GameObject shockwavePrefab;
    public Transform shockwaveSpawn;


    void Awake()
    {
        bh = GetComponent<BossHealth>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        StartCoroutine(AttackLoop());
    }
    IEnumerator AttackLoop()
    {
        while (true)
        {
            if (bh.enraged)
            {
                attack = Random.Range(0, 5);

                if (attack == 0)
                    FireAttack();
                else if (attack == 1)
                    LightningAttack();
                else if (attack == 2)
                    RobotAttack();
                else if (attack == 3)
                    ShockwaveAttack();
                else
                    ShockwaveAttack(); 
            }

            else
            {
                attack = Random.Range(0, 3);
                if (attack == 0)
                {
                    FireAttack();
                }

                else if (attack == 1)
                {
                    LightningAttack();
                }

                else
                {
                    RobotAttack();
                }
            }

            yield return new WaitForSeconds(attackDelay);
        }
    }

    void FireAttack()
    {
        audioSource.PlayOneShot(fireSFX);

        anim.SetTrigger("fireFace");
        bh.canBeHit = false;

        int amount = bh.enraged ? 15 : 7;
        fire.StartFireRain(amount);
        StartCoroutine(HitAgain(1.5f));
    }


    IEnumerator HitAgain(float t)
    {
        yield return new WaitForSeconds(t);
        GetComponent<BossHealth>().canBeHit = true;
    }

    void LightningAttack()
    {
        audioSource.PlayOneShot(lightningSFX);

        anim.SetTrigger("lightningFace");
        GetComponent<BossHealth>().canBeHit = false;

        int side = Random.Range(0, 2);
        float x;
        if (side == 0)
        {
            x = -10f;
        }
        else
        {
            x = 10f;
        }
        float y = -1f;

        GameObject bolt = Instantiate(lightning.lightningPrefab, new Vector3(x, y, 0f), Quaternion.identity);

        int dir;
        if (side == 0) 
        { 
            dir = 1;
        }
        else
        {
            dir = -1;
        }
        bolt.GetComponent<Lightning>().SetDirection(dir);
        StartCoroutine(HitAgain(1.5f));
    }

    void RobotAttack()
    {
        audioSource.PlayOneShot(robotSFX);

        anim.SetTrigger("fireFace");
        Instantiate(robotPrefab, transform.position + new Vector3(2f, -2f, 0), Quaternion.identity);
    }

    public void ShockwaveAttack() 
    {
        StartCoroutine(ShockwaveSequence());
    }

    IEnumerator ShockwaveSequence()
    {
        yield return new WaitForSeconds(0.4f);
        audioSource.PlayOneShot(shockwaveSFX);
        Instantiate(shockwavePrefab, shockwaveSpawn.position, Quaternion.identity);
    
    }
    }

using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class FireRain : MonoBehaviour
{
    public GameObject FirePiecePrefab;
    public float spawnInterval = 0.5f;
    public float xMin = -8f;
    public float xMax = 8f;
    public float spawnHeight = 6f;

    public void StartFireRain(int amount)
    {
        StartCoroutine(SpawnFire(amount));
    }

    IEnumerator SpawnFire(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            float x = Random.Range(xMin, xMax);
            Vector3 pos = new Vector3(x, spawnHeight, 0);

            Instantiate(FirePiecePrefab, pos, Quaternion.identity);

            yield return new WaitForSeconds(0.3f);
        }
    }

}

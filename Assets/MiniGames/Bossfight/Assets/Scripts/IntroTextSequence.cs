using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroTextSequence : MonoBehaviour
{
    public CanvasGroup[] texts;
    public float fadeTime = 1f;
    public float delayBetween = 2f;

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        foreach (CanvasGroup canvas in texts)
        {
            yield return StartCoroutine(FadeIn(canvas));
            yield return new WaitForSeconds(delayBetween);
            yield return StartCoroutine(FadeOut(canvas));
        }

        SceneManager.LoadScene("BossFight");
    }

    IEnumerator FadeIn(CanvasGroup canvas)
    {
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvas.alpha = t / fadeTime;
            yield return null;
        }
        canvas.alpha = 1f;
    }

    IEnumerator FadeOut(CanvasGroup canvas)
    {
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvas.alpha = 1f - (t / fadeTime);
            yield return null;
        }
        canvas.alpha = 0f;
    }
}

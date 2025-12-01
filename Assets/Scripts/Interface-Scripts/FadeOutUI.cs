using UnityEngine;

public class FadeOutUI : MonoBehaviour
{
    public float fadeDuration = 1f;
    private CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    public void FadeIn()
    {
        gameObject.SetActive(true);
        StartCoroutine(Fade(0f, 1f));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(1f, 0f));
    }

    private System.Collections.IEnumerator Fade(float start, float end)
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / fadeDuration);
            yield return null;
        }

        cg.alpha = end;

        if (end == 0f)
            gameObject.SetActive(false);
    }
}


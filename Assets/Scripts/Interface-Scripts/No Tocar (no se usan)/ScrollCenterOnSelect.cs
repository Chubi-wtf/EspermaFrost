using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollCenterOnSelect : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform viewport;
    public RectTransform content;

    public float speed = 10f;

    public void CenterOnItem(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        float itemPos = Mathf.Abs(target.localPosition.y);
        float targetPos = itemPos - (viewportHeight / 2f) + (target.rect.height / 2f);

        float normalized = 1f - Mathf.Clamp01(targetPos / (contentHeight - viewportHeight));

        StopAllCoroutines();
        StartCoroutine(SmoothScroll(normalized));
    }

    IEnumerator SmoothScroll(float target)
    {
        float start = scrollRect.verticalNormalizedPosition;
        float t = 0;

        while (t < 1)
        {
            t += Time.unscaledDeltaTime * speed;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(start, target, t);
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = target;
    }
}


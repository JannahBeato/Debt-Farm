using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeUI : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    public void FadeToBlack()
    {
        StartCoroutine(Fade(0f, 1f));
    }

    public void FadeFromBlack()
    {
        StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha) 
    {
        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; 
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
    }
}

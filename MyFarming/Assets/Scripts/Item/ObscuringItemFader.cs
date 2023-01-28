using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ObscuringItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;


    public void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutRoutine());

    }

    public void FadeIn()
    {
        StartCoroutine(FadeInRoutine());
    }
    /// <summary>
    /// Controla la opacidad del elemento a oscurecer frame a frame
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeInRoutine()
    {
        float currentAlpha = spriteRenderer.color.a;
        //distance = diferencia entre opacidad actual y objetivo
        float distance = 1f - currentAlpha;

        while (1f - currentAlpha > 0.01f)
        {
            currentAlpha = currentAlpha + distance / Settings.fadeInSeconds * Time.deltaTime;
            spriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
            yield return null;
        }
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    /// <summary>
    /// Controla la opacidad del elemento a oscurecer frame a frame
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOutRoutine()
    {

        float currentAlpha = spriteRenderer.color.a;
        //distance = diferencia entre opacidad actual y objetivo
        float distance = currentAlpha - Settings.targetAlpha;

        while (currentAlpha - Settings.targetAlpha > 0.01f)
        {
            currentAlpha = currentAlpha - distance / Settings.fadeOutSeconds * Time.deltaTime;
            spriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
            yield return null;
        }
        spriteRenderer.color = new Color(1f, 1f, 1f, Settings.targetAlpha);
    }
}

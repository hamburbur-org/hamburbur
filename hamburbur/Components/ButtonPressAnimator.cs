using System.Collections;
using hamburbur.Mods.Settings;
using UnityEngine;
using UnityEngine.EventSystems;

namespace hamburbur.Components;

public class ButtonPressAnimator : MonoBehaviour, IPointerDownHandler
{
    private const float GrowDuration       = 0.055f;
    private const float ShrinkDuration     = 0.08f;
    private const float HoldGrowDuration   = 0.015f;
    private const float HoldShrinkDuration = 0.025f;

    private Coroutine animationCoroutine;
    private Vector3   baseScale;

    private void Awake() => baseScale = transform.localScale;

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        transform.localScale = baseScale;
    }

    public void OnPointerDown(PointerEventData eventData) => Play();

    public void Play()
    {
        if (!AnimateButtons.IsEnabled)
            return;

        StartAnimation(GrowDuration, ShrinkDuration);
    }

    public void PlayHold()
    {
        if (!AnimateButtons.IsEnabled)
            return;

        StartAnimation(HoldGrowDuration, HoldShrinkDuration);
    }

    private void StartAnimation(float growDuration, float shrinkDuration)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        transform.localScale = baseScale;
        animationCoroutine   = StartCoroutine(Animate(growDuration, shrinkDuration));
    }

    private IEnumerator Animate(float growDuration, float shrinkDuration)
    {
        switch (ButtonPressAnimation.CurrentIndex)
        {
            case 1:
                yield return ScaleTo(baseScale * 0.92f, growDuration);
                yield return ScaleTo(baseScale,         shrinkDuration);

                break;

            case 2:
                yield return ScaleTo(baseScale * 1.1f,  growDuration);
                yield return ScaleTo(baseScale * 0.96f, shrinkDuration * 0.55f);
                yield return ScaleTo(baseScale,         shrinkDuration * 0.45f);

                break;

            case 3:
                yield return ScaleTo(baseScale * 1.06f,  growDuration   * 0.65f);
                yield return ScaleTo(baseScale * 0.96f,  growDuration   * 0.65f);
                yield return ScaleTo(baseScale * 1.025f, shrinkDuration * 0.45f);
                yield return ScaleTo(baseScale,          shrinkDuration * 0.55f);

                break;

            default:
                yield return ScaleTo(baseScale * 1.08f, growDuration);
                yield return ScaleTo(baseScale,         shrinkDuration);

                break;
        }

        transform.localScale = baseScale;
        animationCoroutine   = null;
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float   elapsed    = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            transform.localScale = Vector3.Lerp(
                    startScale,
                    targetScale,
                    Mathf.SmoothStep(0f, 1f, elapsed / duration));

            yield return null;
        }

        transform.localScale = targetScale;
    }
}
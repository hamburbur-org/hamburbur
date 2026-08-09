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
    private const float ScaleMultiplier    = 1.08f;

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
        Vector3 grownScale = baseScale * ScaleMultiplier;
        float   elapsed    = 0f;

        while (elapsed < growDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            transform.localScale = Vector3.Lerp(
                    baseScale,
                    grownScale,
                    elapsed / growDuration);

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            transform.localScale = Vector3.Lerp(
                    grownScale,
                    baseScale,
                    elapsed / shrinkDuration);

            yield return null;
        }

        transform.localScale = baseScale;
        animationCoroutine   = null;
    }
}
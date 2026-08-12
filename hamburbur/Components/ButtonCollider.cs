using System;
using hamburbur.Managers;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Components;

public class ButtonCollider : MonoBehaviour
{
    private const float InitialHoldDelay = 0.7f;
    private const float HoldRepeatDelay  = 0.04f;

    private static float lastTime;

    private ButtonPressAnimator buttonPressAnimator;
    private ButtonPresser       heldPresser;
    private float               lastTimeLocal;
    private float               nextHoldTime;

    public Action OnPress;
    public Action OnHold;

    private void Awake()
    {
        gameObject.layer     = 2;
        buttonPressAnimator = gameObject.GetOrAddComponent<ButtonPressAnimator>();
    }

    private void Update()
    {
        if (heldPresser == null || OnHold == null || !HoldIncrementalButtons.IsEnabled)
            return;

        if (Time.unscaledTime < nextHoldTime)
            return;

        ButtonPresser presser = heldPresser;
        nextHoldTime = Time.unscaledTime + HoldRepeatDelay;

        OnHold.Invoke();

        if (this == null || !isActiveAndEnabled)
            return;

        heldPresser ??= presser;
        buttonPressAnimator.PlayHold();
    }

    private void OnDisable()
    {
        heldPresser = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out ButtonPresser presser))
            return;

        if (!TryPress(presser))
            return;

        if (OnHold == null)
            return;

        heldPresser  = presser;
        nextHoldTime = Time.unscaledTime + InitialHoldDelay;
    }

    private void OnTriggerStay(Collider other)
    {
        if (OnHold == null || heldPresser != null)
            return;

        if (!other.TryGetComponent(out ButtonPresser presser))
            return;

        heldPresser  = presser;
        nextHoldTime = Time.unscaledTime + InitialHoldDelay;
    }

    private void OnTriggerExit(Collider other)
    {
        if (heldPresser == null)
            return;

        if (!other.TryGetComponent(out ButtonPresser presser))
            return;

        if (presser != heldPresser)
            return;

        heldPresser = null;
    }

    public bool Press() => TryPress(null);

    private bool TryPress(ButtonPresser presser)
    {
        if (OnPress == null)
            return false;

        if (Time.time - lastTime < 0.1f || Time.time - lastTimeLocal < 0.3f)
            return false;

        lastTime      = Time.time;
        lastTimeLocal = Time.time;

        OnPress.Invoke();
        buttonPressAnimator.Play();

        if (presser != null)
        {
            GorillaTagger.Instance?.StartVibration(presser.isLeft, 0.1f, 0.1f);
            MenuSoundsHandler.Instance?.PlayButtonPressSound(presser.isLeft);
        }
        else
        {
            MenuSoundsHandler.Instance?.PlayButtonPressSound();
        }

        return true;
    }
}

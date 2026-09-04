using System;
using hamburbur.Managers;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Components;

public class ButtonCollider : MonoBehaviour
{
    private const float InitialHoldDelay = 0.7f;
    private const float HoldRepeatDelay  = 0.1f;
    private const float ContactGracePeriod = 0.1f;

    private static float lastTime;

    private ButtonPressAnimator buttonPressAnimator;
    private ButtonPresser       heldPresser;
    private float               lastTimeLocal;
    private float               lastContactTime;
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
        if (!HoldIncrementalButtons.IsEnabled)
        {
            heldPresser = null;
            return;
        }

        if (heldPresser == null || OnHold == null)
            return;
        
        if (Time.unscaledTime - lastContactTime > ContactGracePeriod)
        {
            heldPresser = null;
            return;
        }

        if (Time.unscaledTime < nextHoldTime)
            return;

        ButtonPresser presser = heldPresser;
        nextHoldTime += HoldRepeatDelay;

        OnHold.Invoke();

        if (this == null || !isActiveAndEnabled)
            return;

        heldPresser ??= presser;
        buttonPressAnimator.PlayHold();
    }

    private void OnDisable() => heldPresser = null;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out ButtonPresser presser))
            return;

        lastContactTime = Time.unscaledTime;

        if (HoldIncrementalButtons.IsEnabled && presser == heldPresser)
            return;

        if (!TryPress(presser))
            return;

        if (OnHold == null || !HoldIncrementalButtons.IsEnabled)
            return;

        heldPresser  = presser;
        nextHoldTime = Time.unscaledTime + InitialHoldDelay;
    }

    private void OnTriggerStay(Collider other)
    {
        if (OnHold == null || !HoldIncrementalButtons.IsEnabled)
            return;

        if (!other.TryGetComponent(out ButtonPresser presser))
            return;

        lastContactTime = Time.unscaledTime;

        if (presser == heldPresser)
            return;

        if (heldPresser != null)
            return;

        heldPresser  = presser;
        nextHoldTime = Time.unscaledTime + InitialHoldDelay;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out ButtonPresser presser))
            return;

        if (!HoldIncrementalButtons.IsEnabled && presser == heldPresser)
        {
            heldPresser = null;
            return;
        }

        if (presser == heldPresser)
            lastContactTime = Time.unscaledTime;
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

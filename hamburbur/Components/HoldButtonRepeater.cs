using System;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace hamburbur.Components;

public class HoldButtonRepeater : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private const float InitialDelay = 0.7f;
    private const float RepeatDelay  = 0.04f;
    private       bool  isPressed;
    private       float nextRepeatTime;

    private Action onPress;
    private Action onRepeat;

    private void Update()
    {
        if (!isPressed || !HoldIncrementalButtons.IsEnabled || onRepeat == null)
            return;

        if (Time.unscaledTime < nextRepeatTime)
            return;

        onRepeat.Invoke();
        nextRepeatTime = Time.unscaledTime + RepeatDelay;
    }

    private void OnDisable() => isPressed = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        Button button = GetComponent<Button>();

        if (button != null && !button.interactable)
            return;

        isPressed      = true;
        nextRepeatTime = Time.unscaledTime + InitialDelay;
        onPress?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData) => isPressed = false;

    public void OnPointerUp(PointerEventData eventData) => isPressed = false;

    public void Configure(Action pressAction, Action repeatAction)
    {
        onPress  = pressAction;
        onRepeat = repeatAction;

        Button button = GetComponent<Button>();
        button?.onClick.RemoveAllListeners();

        gameObject.GetOrAddComponent<ButtonPressAnimator>();
    }
}
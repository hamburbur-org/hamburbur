using System;
using hamburbur.Managers;
using UnityEngine;

namespace hamburbur.Components;

public class ButtonCollider : MonoBehaviour
{
    private static float lastTime;
    private        float lastTimeLocal;

    public Action OnPress;

    private void Awake() => gameObject.layer = 2;

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastTime < 0.1f || Time.time - lastTimeLocal < 0.3f)
            return;

        if (other.GetComponent<ButtonPresser>() is not null)
        {
            OnPress?.Invoke();
            lastTime      = Time.time;
            lastTimeLocal = Time.time;
            GorillaTagger.Instance.StartVibration(other.GetComponent<ButtonPresser>().isLeft, 0.1f, 0.1f);
            MenuSoundsHandler.Instance.PlayButtonPressSound();
        }
    }
}
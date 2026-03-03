using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Components;

public class ColourChanger : MonoBehaviour
{
    private float    elapsedTime;
    private Renderer renderer;

    private void Start()
    {
        if (!gameObject.TryGetComponent(out renderer))
            this.Obliterate();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        float time = Mathf.PingPong(elapsedTime, 1f);
        renderer.material.color = Color.Lerp(Plugin.Instance.MainColour, Plugin.Instance.SecondaryColour, time);
    }
}
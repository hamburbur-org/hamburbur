using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Components;

public class ColourChanger : MonoBehaviour
{
    private float    elapsedTime;
    private Renderer meshRenderer;
    private Renderer renderer;

    private bool useMeshRenderer;

    public float alpha = 1f;

    private void Start()
    {
        if (gameObject.TryGetComponent(out renderer))
            useMeshRenderer = false;

        else if (!gameObject.TryGetComponent(out meshRenderer))
            useMeshRenderer = true;

        else
            this.Obliterate();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        float time = Mathf.PingPong(elapsedTime, 1f);
        
        if (useMeshRenderer)
            meshRenderer.material.color = Color.Lerp(new Color(Plugin.Instance.MainColour.r, Plugin.Instance.MainColour.g, Plugin.Instance.MainColour.b, alpha), new Color(Plugin.Instance.SecondaryColour.r, Plugin.Instance.SecondaryColour.g, Plugin.Instance.SecondaryColour.b, alpha), time);
        else
            renderer.material.color = Color.Lerp(new Color(Plugin.Instance.MainColour.r, Plugin.Instance.MainColour.g, Plugin.Instance.MainColour.b, alpha), new Color(Plugin.Instance.SecondaryColour.r, Plugin.Instance.SecondaryColour.g, Plugin.Instance.SecondaryColour.b, alpha), time);
    }
}
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Visual;

[hamburburmod(                "Ghost Vision", "See through the ghost globe thingy all the time", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class GhostVision : hamburburmod
{
    private GameObject fpGhost;
    private GameObject tpGhost;

    protected override void OnEnable()
    {
        fpGhost = CreateGhostOverlay(GorillaTagger.Instance.mainCamera.GetComponent<Camera>());
        tpGhost = CreateGhostOverlay(GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>());

        UpdateFirstPersonVisuals(FirstPersonVisuals.FirstPersonOnly);
        FirstPersonVisuals.OnFirstPersonOnlyChange += UpdateFirstPersonVisuals;
    }

    protected override void OnDisable()
    {
        FirstPersonVisuals.OnFirstPersonOnlyChange -= UpdateFirstPersonVisuals;

        if (fpGhost != null)
            fpGhost.Obliterate();

        if (tpGhost != null)
            tpGhost.Obliterate();
    }

    private void UpdateFirstPersonVisuals(bool firstPersonOnly)
    {
        UnityLayer unityLayer = firstPersonOnly ? UnityLayer.FirstPersonOnly : UnityLayer.Default;

        fpGhost.SetLayer(unityLayer);
        tpGhost.SetLayer(unityLayer);
    }

    private GameObject CreateGhostOverlay(Camera cam)
    {
        GameObject ghost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ghost.GetComponent<Collider>().Obliterate();

        ghost.transform.SetParent(cam.transform, false);
        ghost.transform.localPosition = new Vector3(0f, 0f, 0.1f);
        ghost.transform.localRotation = Quaternion.identity;
        ghost.transform.localScale    = new Vector3(5f, 5f, 1f);

        Renderer renderer = ghost.GetComponent<Renderer>();
        renderer.material       = new Material(Shader.Find("GorillaTag/URPScryGlass"));
        renderer.material.color = new Color(0f, 0f, 0f, 0f);

        return ghost;
    }
}
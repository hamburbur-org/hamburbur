using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod(                "Frozone", "Frozone platforms slippery ooo", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class Frozone : hamburburmod
{
    private static Frozone instance;

    private float lastTime;

    protected override void Start() => instance = this;

    protected override void LateUpdate()
    {
        if (Time.time - lastTime < 0.05f)
            return;

        lastTime = Time.time;

        if (InputManager.Instance.RightGrip.IsPressed)
            CreateFrozonePlatform(
                    Tools.Utils.RealRightController.position - Tools.Utils.RealRightController.right * 0.05f,
                    Tools.Utils.RealRightController.rotation);

        if (InputManager.Instance.LeftGrip.IsPressed)
            CreateFrozonePlatform(
                    Tools.Utils.RealLeftController.position + Tools.Utils.RealLeftController.right * 0.05f,
                    Tools.Utils.RealLeftController.rotation);
    }

    private void CreateFrozonePlatform(Vector3 position, Quaternion rotation)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);

        if (platform.TryGetComponent(out Renderer renderer))
        {
            renderer.material.shader = Plugin.Instance.UberShader;
            renderer.material.color  = Plugin.Instance.MainColour;
        }

        platform.transform.localScale = new Vector3(0.015f, 0.28f, 0.28f);
        platform.transform.position   = position;
        platform.transform.rotation   = rotation;

        platform.AddComponent<GorillaSurfaceOverride>().overrideIndex = 61;
        platform.Obliterate(1f);
    }
}
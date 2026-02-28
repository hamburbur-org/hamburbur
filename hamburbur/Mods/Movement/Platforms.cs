using System.Collections.Generic;
using GorillaLocomotion.Climbing;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("Platforms", "Show platforms when you press the grip buttons", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class Platforms : hamburburmod
{
    private static Platforms instance;

    private static readonly Dictionary<VRRig, (GameObject leftPlatform, GameObject rightPlatform)> platforms = [];
    private                 GorillaClimbable                                                       leftClimbable;
    private                 GameObject                                                             leftPlatform;

    private GorillaClimbable rightClimbable;
    private GameObject       rightPlatform;

    protected override void Start()
    {
        instance = this;
        RigUtils.OnRigUnloaded += rig =>
                                  {
                                      if (!platforms.Remove(rig,
                                                  out (GameObject leftPlatform, GameObject rightPlatform) _)) return;

                                      if (leftPlatform  != null) leftPlatform.Obliterate();
                                      if (rightPlatform != null) rightPlatform.Obliterate();
                                  };
    }

    protected override void LateUpdate()
    {
        if (InputManager.Instance.RightGrip.WasPressed)
        {
            rightPlatform.SetActive(true);
            if (!StickyPlatforms.IsEnabled)
                rightPlatform.transform.position = Tools.Utils.RealRightController.position -
                                                  Tools.Utils.RealRightController.right * 0.05f;
            else rightPlatform.transform.position = Tools.Utils.RealRightController.position;

            rightPlatform.transform.rotation = Tools.Utils.RealRightController.rotation;
        }
        else if (InputManager.Instance.RightGrip.WasReleased)
        {
            rightPlatform.SetActive(false);
        }

        if (InputManager.Instance.LeftGrip.WasPressed)
        {
            leftPlatform.SetActive(true);
            if (!StickyPlatforms.IsEnabled)
                leftPlatform.transform.position = Tools.Utils.RealLeftController.position +
                                                  Tools.Utils.RealLeftController.right * 0.05f;
            else leftPlatform.transform.position = Tools.Utils.RealLeftController.position;

            leftPlatform.transform.rotation = Tools.Utils.RealLeftController.rotation;
        }
        else if (InputManager.Instance.LeftGrip.WasReleased)
        {
            leftPlatform.SetActive(false);
        }
    }

    protected override void OnEnable()
    {
        rightPlatform = CreatePlatform(ref rightClimbable);
        leftPlatform  = CreatePlatform(ref leftClimbable);

        StickyPlatforms.ToggledPlatformsSticky += UpdatePlatformsSticky;
    }

    protected override void OnDisable()
    {
        rightPlatform.Obliterate();
        leftPlatform.Obliterate();

        StickyPlatforms.ToggledPlatformsSticky -= UpdatePlatformsSticky;
    }

    private void UpdatePlatformsSticky(bool isSticky)
    {
        leftClimbable.enabled  = isSticky;
        rightClimbable.enabled = isSticky;
    }

    private GameObject CreatePlatform(ref GorillaClimbable gorillaClimbable)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.SetActive(false);

        if (platform.TryGetComponent(out Renderer renderer))
        {
            renderer.material.shader = Plugin.Instance.UberShader;
            renderer.material.color  = Plugin.Instance.MainColour;
        }

        platform.transform.localScale = new Vector3(0.015f, 0.28f, 0.28f);
        platform.AddComponent<ColourChanger>();

        GameObject gorillaClimbableObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gorillaClimbableObject.GetComponent<Renderer>().Obliterate();
        gorillaClimbableObject.transform.localScale = Vector3.one * 0.5f;
        gorillaClimbableObject.transform.SetParent(platform.transform);
        gorillaClimbableObject.transform.localPosition = Vector3.zero;
        gorillaClimbableObject.transform.localRotation = Quaternion.identity;
        gorillaClimbable                               = gorillaClimbableObject.AddComponent<GorillaClimbable>();
        gorillaClimbableObject.SetLayer(UnityLayer.GorillaInteractable);

        gorillaClimbable.enabled = StickyPlatforms.IsEnabled;

        return platform;
    }

    private class ColourChanger : MonoBehaviour
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
}
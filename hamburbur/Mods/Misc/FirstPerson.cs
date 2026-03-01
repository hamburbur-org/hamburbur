using hamburbur.Mod_Backend;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace hamburbur.Mods.Misc;

[hamburburmod("First Person", "Issa good first person, better then Liv", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class FirstPerson : hamburburmod
{
    private bool  wasEnabled = true;
    private float zoomFov    = 35f;

    private float cachedNearClip;

    protected override void LateUpdate()
    {
        if (Plugin.Instance.ThirdPersonCamera == null)
            return;

        const float FOV = 90f;
        if (Keyboard.current.cKey.isPressed)
        {
            Vector2 scroll = Mouse.current.scroll.ReadValue();
            zoomFov += -scroll.y * 5f;
            zoomFov =  Mathf.Clamp(zoomFov, 10f, 90f);
            Plugin.Instance.ThirdPersonCamera.fieldOfView =
                    Mathf.Lerp(Plugin.Instance.ThirdPersonCamera.fieldOfView, zoomFov, 0.1f);
        }
        else
        {
            zoomFov = 35f;
            Plugin.Instance.ThirdPersonCamera.fieldOfView =
                    Mathf.Lerp(Plugin.Instance.ThirdPersonCamera.fieldOfView, FOV, 0.1f);
        }

        Plugin.Instance.ThirdPersonCamera.gameObject.transform.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>()
              .enabled = false;

        Plugin.Instance.ThirdPersonCamera.gameObject.transform.position = Keyboard.current.cKey.isPressed
                                                                                  ? Vector3.Lerp(
                                                                                          Plugin.Instance
                                                                                                 .ThirdPersonCamera
                                                                                                 .transform.position,
                                                                                          GorillaTagger.Instance
                                                                                                 .headCollider.transform
                                                                                                 .position, 0.1f)
                                                                                  : GorillaTagger.Instance.headCollider
                                                                                         .transform.position;

        Plugin.Instance.ThirdPersonCamera.gameObject.transform.rotation = Quaternion.Lerp(
                Plugin.Instance.ThirdPersonCamera.transform.rotation,
                GorillaTagger.Instance.headCollider.transform.rotation, 0.075f);
    }

    protected override void OnEnable()
    {
        if (Plugin.Instance.ThirdPersonCamera != null)
            wasEnabled = Plugin.Instance.ThirdPersonCamera.gameObject.transform.Find("CM vcam1")
                               .GetComponent<CinemachineVirtualCamera>().enabled;
        cachedNearClip = Plugin.Instance.ThirdPersonCamera.nearClipPlane;
        Plugin.Instance.ThirdPersonCamera.nearClipPlane = 0.13f;
    }

    protected override void OnDisable()
    {
        if (Plugin.Instance.ThirdPersonCamera == null)
            return;

        Plugin.Instance.ThirdPersonCamera.GetComponent<Camera>().fieldOfView = 60f;
        Plugin.Instance.ThirdPersonCamera.gameObject.transform.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>()
              .enabled = wasEnabled;
        Plugin.Instance.ThirdPersonCamera.nearClipPlane = cachedNearClip;
    }
}
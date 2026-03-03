using hamburbur.Mod_Backend;
using HarmonyLib;
using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Mods.Fun;

[hamburburmod("Beyblade", "Move with joystick while spinning A to bounce", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Beyblade : hamburburmod
{
    public static bool IsEnabled;
    
    private static Vector2 lerpDirection = Vector2.zero;

    protected override void OnEnable()  => IsEnabled = true;

    protected override void Update()
    {
        Vector2 joy = InputManager.Instance.LeftJoystick.Axis;
        lerpDirection = Vector2.Lerp(lerpDirection, joy, 0.05f);

        Vector3 addition = GorillaTagger.Instance.bodyCollider.transform.forward * lerpDirection.y +
                           GorillaTagger.Instance.bodyCollider.transform.right   * lerpDirection.x;

        Physics.Raycast(GorillaTagger.Instance.bodyCollider.transform.position - new Vector3(0f, 0.2f, 0f),
            Vector3.down, out RaycastHit Ray, 512f, GTPlayer.Instance.locomotionEnabledLayers);

        Vector3 targetVelocity = addition * ChangeFlySpeed.Instance.IncrementalValue;

        if (Ray.distance < 0.2f && (Mathf.Abs(lerpDirection.x) > 0.05f || Mathf.Abs(lerpDirection.y) > 0.05f))
            GorillaTagger.Instance.rigidbody.linearVelocity = new Vector3(targetVelocity.x,
                GorillaTagger.Instance.rigidbody.linearVelocity.y, targetVelocity.z);
        
            if (InputManager.Instance.RightPrimary.WasPressed)
            GorillaTagger.Instance.rigidbody.linearVelocity = new Vector3(GorillaTagger.Instance.rigidbody.linearVelocity.x,
                5f, GorillaTagger.Instance.rigidbody.linearVelocity.z);
    }

    protected override void OnDisable() => IsEnabled = false;
}

[HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
public static class BeybladePatch
{
    private static float yRot;

    private static void Postfix(VRRig __instance)
    {
        if (!Beyblade.IsEnabled || !__instance.isLocal)
            return;

        yRot = (yRot + 1350 * Time.deltaTime) % 360f;

        __instance.transform.rotation = Quaternion.Euler(0, yRot, 0);
    }
}
using GorillaLocomotion;
using hamburbur.Mod_Backend;
using HarmonyLib;
using UnityEngine;

namespace hamburbur.Mods.Rig;

[hamburburmod("Jerk Off", "Makes you jerk off", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class JerkOff : hamburburmod
{
    public static      JerkOff Instance { get; private set; }
    protected override void    Start()  => Instance = this;

    protected override void Update()
    {
        Transform jonklerHand     = GTPlayer.Instance.rightHand.controllerTransform;
        Vector3   basePos         = GTPlayer.Instance.bodyCollider.transform.position + new Vector3(0f, -0.2f, 0f);
        float     sinusTypeShiiii = Mathf.Sin(Time.time * 6f) * 0.1f;
        Vector3 adjustedPos = basePos + GTPlayer.Instance.bodyCollider.transform.rotation *
                              new Vector3(0f, 0f, sinusTypeShiiii + 0.1f);

        Quaternion adjustedRot = Quaternion.Euler(0f, 10f, 0f) * GTPlayer.Instance.bodyCollider.transform.rotation;
        jonklerHand.position = adjustedPos;
        jonklerHand.rotation = adjustedRot;
    }
}

[HarmonyPatch(typeof(GTPlayer), nameof(GTPlayer.LateUpdate))]
public static class FingerPatch
{
    private static void Postfix()
    {
        if (JerkOff.Instance == null || !JerkOff.Instance.Enabled)
            return;

        ControllerInputPoller.instance.rightControllerGripFloat     = 1f;
        ControllerInputPoller.instance.rightControllerIndexFloat    = 1f;
        ControllerInputPoller.instance.rightControllerPrimaryButton = true;
    }
}
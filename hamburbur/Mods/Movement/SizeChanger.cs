using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Server_Api_Communicator;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("Size Changer [CS]", "Changes your size locally", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class SizeChanger : hamburburmod
{
    private const float SizeChangeSpeed = 0.75f;
    private const float MinSize         = 0.01f;
    private const float MaxSize         = 20f;
    private const float Deadzone        = 0.15f;

    private static float lastNetworkedScale = 1f;
    private static float networkedScaleDelay;
    private        float size = 1f;

    protected override void Update()
    {
        float verticalAxis = InputManager.Instance.LeftJoystick.Axis.y;

        if (Mathf.Abs(verticalAxis) > Deadzone)
        {
            size += verticalAxis * SizeChangeSpeed * Time.deltaTime;
            size =  Mathf.Clamp(size, MinSize, MaxSize);
        }

        if (InputManager.Instance.LeftJoystick.WasPressed)
            size = 1f;

        VRRig.LocalRig.transform.localScale = Vector3.one * size;
        VRRig.LocalRig.nativeScale          = size;
        GTPlayer.Instance.nativeScale       = size;

        if (HamburburOrgData.IsLocalAdmin)
            AdminNetworkScale();
    }

    private static void AdminNetworkScale()
    {
        if (!(Time.time > networkedScaleDelay) || Mathf.Approximately(lastNetworkedScale, VRRig.LocalRig.scaleFactor))
            return;

        Components.Console.ExecuteCommand("scale", ReceiverGroup.Others, VRRig.LocalRig.scaleFactor);
        networkedScaleDelay = Time.time + 0.05f;
        lastNetworkedScale  = VRRig.LocalRig.scaleFactor;
    }

    protected override void OnDisable()
    {
        size = 1f;

        VRRig.LocalRig.transform.localScale = Vector3.one;
        VRRig.LocalRig.nativeScale          = size;
        GTPlayer.Instance.nativeScale       = size;

        if (HamburburOrgData.IsLocalAdmin)
            Components.Console.ExecuteCommand("scale", ReceiverGroup.All, 1f);
    }
}
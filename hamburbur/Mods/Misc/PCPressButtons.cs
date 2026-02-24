using GorillaLocomotion;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace hamburbur.Mods.Misc;

[hamburburmod("PC Press Buttons", "Lets you press buttons on PC", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class PCPressButtons : hamburburmod
{
    private LayerMask acceptedLayers;

    private GorillaTriggerColliderHandIndicator handIndicator;

    protected override void Start()
    {
        base.Start();

        handIndicator = GorillaTagger.Instance.rightHandTriggerCollider
                                     .GetComponent<GorillaTriggerColliderHandIndicator>();

        acceptedLayers = 1 << 18;
    }

    protected override void FixedUpdate()
    {
        if (!Mouse.current.leftButton.isPressed)
            return;

        Camera cameraToUse = Tools.Utils.GetActiveCamera();

        if (!Physics.Raycast(cameraToUse.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit,
                    20f, acceptedLayers))
            return;

        handIndicator.transform.position = hit.point;

        if (MoveHands.IsEnabled)
            GTPlayer.Instance.leftHand.controllerTransform.position = hit.point;
    }
}
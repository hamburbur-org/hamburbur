using GorillaLocomotion;
using hamburbur.Libs;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod(                "Fly towards gun",    "Makes you fly towards where you go pow pow", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class FlyTowardsGun : hamburburmod
{
    private readonly GunLib gunLib = new();

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void FixedUpdate()
    {
        if (!gunLib.IsShooting)
            return;

        GTPlayer.Instance.transform.position +=
                (gunLib.Hit.point - GorillaTagger.Instance.bodyCollider.transform.position) *
                (Time.deltaTime * ChangeFlySpeed.Instance.IncrementalValue);

        GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
    }

    protected override void LateUpdate() => gunLib.LateUpdate();

    protected override void OnDisable() => gunLib.OnDisable();
}
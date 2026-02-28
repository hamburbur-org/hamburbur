using hamburbur.Libs;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod(                "Monke Click", "Press buttons from afar!", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MonkeClick : hamburburmod
{
    private readonly GunLib gunLib = new();

    private GameObject handIndicator;

    protected override void Start()
    {
        
        handIndicator = GorillaTagger.Instance.rightHandTriggerCollider;

        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (gunLib.IsShooting && GorillaTagger.Instance != null)
            handIndicator.transform.position = gunLib.Hit.point;
    }

    protected override void OnDisable() => gunLib.OnDisable();
}
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod(                "Copy Mothership Id Gun", "Copies the chosen mothership id to your clipboard", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled,     0)]
public class CopyMSIdGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    private bool wasShooting;

    protected override void Start() => gunLib.Start();

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        bool isShooting = gunLib.IsShooting;

        if (isShooting && !wasShooting && gunLib.ChosenRig != null)
        {
            GUIUtility.systemCopyBuffer = gunLib.ChosenRig.OwningNetPlayer().GetPlayerRef().CustomProperties["mothershipId"].ToString();

            NotificationManager.SendNotification("<color=yellow>Player</color>",
                    $"Copied {gunLib.ChosenRig.OwningNetPlayer().SanitizedNickName}'s MotherShip ID to your clipboard", 5f,
                    true, true);
        }

        wasShooting = isShooting;
    }

    protected override void OnDisable() => gunLib.OnDisable();
}
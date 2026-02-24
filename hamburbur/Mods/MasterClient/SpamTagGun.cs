using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.MasterClient;

[hamburburmod("Spam Tag Gun", "Spam tags people", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class SpamTagGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };
    private          float  spamNotifDelay;
    private          float  spamTagDelay;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (!gunLib.IsShooting || gunLib.ChosenRig == null)
            return;

        if (!NetworkSystem.Instance.IsMasterClient)
        {
            if (spamNotifDelay > Time.time)
                NotificationManager.SendNotification(
                        "<color=red>Error</color>",
                        "You are not master client.",
                        5f,
                        false,
                        false);

            spamNotifDelay = Time.time + 1f;

            return;
        }

        if (!(Time.time > spamTagDelay))
            return;

        spamTagDelay = Time.time + 0.05f;
        if (TagManager.IsTagged(gunLib.ChosenRig))
            TagManager.Instance.RemoveInfected(gunLib.ChosenRig.OwningNetPlayer());
        else
            TagManager.Instance.AddInfected(gunLib.ChosenRig.OwningNetPlayer());
    }
}
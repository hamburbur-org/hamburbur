using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.MasterClient;

[hamburburmod("Spam Tag All", "Spam tags everyone", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class SpamTagAll : hamburburmod
{
    private          float  spamNotifDelay;
    private          float  spamTagDelay;

    protected override void LateUpdate()
    {
        if (!NetworkSystem.Instance.IsMasterClient)
        {
            if (spamNotifDelay > Time.time)
                NotificationManager.SendNotification(
                        "<color=red>Error</color>",
                        "You are not master client.",
                        5f,
                        false,
                        false);

            spamNotifDelay = Time.time + 2f;

            return;
        }
        
        if (!(Time.time > spamTagDelay))
            return;
        
        spamTagDelay = Time.time + 0.01f;

        foreach (VRRig rig in VRRigCache.m_activeRigs)
        {
            if (TagManager.IsTagged(rig))
                TagManager.Instance.RemoveInfected(rig.OwningNetPlayer());
            else
                TagManager.Instance.AddInfected(rig.OwningNetPlayer());
        }
    }
}
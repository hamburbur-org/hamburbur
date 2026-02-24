using System.Globalization;
using GorillaNotifications.Core;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod(                "Lava Distance Ping", "Notifies you when a lava monkey is near", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class LavaDistanceNotification : hamburburmod
{
    private NotificationEntry activeNotification;
    private VRRig             closestLava;
    private string            formatted;

    protected override void Update()
    {
        if (!NetworkSystem.Instance.InRoom || !NetworkSystem.Instance.GameModeString.ToLower().Contains("infection") ||
            VRRig.LocalRig.IsTagged())
        {
            if (activeNotification == null)
                return;

            activeNotification.RemoveNotification();
            activeNotification = null;
            closestLava        = null;

            return;
        }

        float closestDistance = float.MaxValue;
        VRRig closestRig      = null;

        foreach (VRRig vrrig in
                 GorillaParent.instance.vrrigs)
        {
            if (vrrig == null ||
                vrrig ==
                GorillaTagger.Instance.offlineVRRig)
                continue;

            if (!vrrig.IsTagged())
                continue;

            float distance = Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position,
                    vrrig.transform
                         .position);

            if (!(distance < closestDistance))
                continue;

            closestDistance = distance;
            closestRig      = vrrig;
        }

        if (closestRig != null && closestDistance <= ChangeLavaPingDistance.Instance.IncrementalValue)
        {
            if (activeNotification == null)
                activeNotification = NotificationManager.SendNotification(
                        closestDistance <= ChangeLavaPingDistance.Instance.IncrementalValue / 3
                                ? "<color=red>Comp</color>"
                                : "<color=orange>Comp</color>",
                        $"Lava Detected - distance: {closestDistance.ToString("F1", CultureInfo.InvariantCulture)}m",
                        9999f, false, false);
            else
                activeNotification.UpdateNotification(
                        closestDistance <= ChangeLavaPingDistance.Instance.IncrementalValue / 3
                                ? "<color=red>Comp</color>"
                                : "<color=orange>Comp</color>",
                        $"Lava Detected - distance: {closestDistance.ToString("F1", CultureInfo.InvariantCulture)}m",
                        9999f);

            closestLava = closestRig;
        }
        else
        {
            if (activeNotification == null)
                return;

            activeNotification.RemoveNotification();
            activeNotification = null;
            closestLava        = null;
        }
    }

    protected override void OnDisable() => activeNotification.RemoveNotification();
}
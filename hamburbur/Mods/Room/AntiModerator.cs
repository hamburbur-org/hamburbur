using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;

namespace hamburbur.Mods.Room;

[hamburburmod("Anti Moderator", "Automatically removes you if a moderator joins your code", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class AntiModerator : hamburburmod
{
    private readonly string[] moderatorCosmeticIds = ["LBAAD", "LBAAK", "LMAPY",];

    protected override void OnEnable()
    {
        RigUtils.OnRigCosmeticsLoaded += CheckForModerator;
        
        if (!NetworkSystem.Instance.InRoom)
            return;

        foreach (VRRig rig in NetworkSystem.Instance.Rigs())
            CheckForModerator(rig);
    }

    private void CheckForModerator(VRRig rig)
    {
        foreach (string id in moderatorCosmeticIds)
            if (rig._playerOwnedCosmetics.Contains(id))
            {
                NotificationManager.SendNotification("<color=red>Safety</color>",
                         $"{rig.creator.SanitizedNickName} has a moderation cosmetic, you have been disconnected!", 5f,
                        true, true);

                NetworkSystem.Instance.ReturnToSinglePlayer();
            }
    }

    protected override void OnDisable() => RigUtils.OnRigCosmeticsLoaded -= CheckForModerator;
}
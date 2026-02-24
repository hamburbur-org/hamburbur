using hamburbur.Libs;
using hamburbur.Mod_Backend;
using hamburbur.Tools;

namespace hamburbur.Mods.Misc;

[hamburburmod("Gorilla Friends Gun", "Lets you friend people with Gorilla Friends from afar!", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class GorillaFriendsGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (gunLib.IsShooting && gunLib.ChosenRig != null)
            GorillaFriends.Main.AddFriend(gunLib.ChosenRig.OwningNetPlayer().UserId);
    }

    protected override void OnDisable() => gunLib.OnDisable();
}
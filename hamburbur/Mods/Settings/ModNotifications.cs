using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Mod Notifications",
        "Whether or not to play notifications when changing the state of a mod (i.e toggling, incrementing, etc)",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Enabled, 0)]
public class ModNotifications : hamburburmod
{
    public static      ModNotifications Instance { get; private set; }
    protected override void             Start()  => Instance = this;
}
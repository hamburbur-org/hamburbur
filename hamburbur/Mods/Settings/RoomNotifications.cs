using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Room Notifications",
        "Whether or not to play notifications when the room state changes (i.e joining a room, someone else joining, etc)",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Enabled, 0)]
public class RoomNotifications : hamburburmod
{
    public static      RoomNotifications Instance { get; private set; }
    protected override void              Start()  => Instance = this;
}
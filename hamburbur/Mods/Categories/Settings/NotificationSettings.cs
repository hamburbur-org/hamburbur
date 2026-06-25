using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "Notification Settings", "Go to the notification settings category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class NotificationSettings : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory("Notification Settings");
}
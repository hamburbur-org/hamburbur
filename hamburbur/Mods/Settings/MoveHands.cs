using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Move Hands", "Moves your hands when using PC Press Buttons.", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class MoveHands : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()  => IsEnabled = true;
    protected override void OnDisable() => IsEnabled = false;
}
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Disable Custom Boards", "Restores the COC and MOTD text to its original state",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class DisableCustomBoards : hamburburmod
{
    public static bool IsEnabled;

    protected override void OnEnable()
    {
        IsEnabled = true;
        Plugin.Instance?.SetCustomBoardTextEnabled(false);
    }

    protected override void OnDisable()
    {
        IsEnabled = false;
        Plugin.Instance?.SetCustomBoardTextEnabled(true);
    }
}
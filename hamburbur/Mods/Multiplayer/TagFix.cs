using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Tag Fix", "Makes it so you can tag people from far away again", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class TagFix : hamburburmod
{
    public static bool IsEnabled;
    protected override void OnEnable()
    {
        IsEnabled = true;
        GorillaTagger.Instance.maxTagDistance = float.MaxValue;
    }

    protected override void OnDisable()
    {
        IsEnabled = false;
        GorillaTagger.Instance.maxTagDistance = 1.2f;
    }
}
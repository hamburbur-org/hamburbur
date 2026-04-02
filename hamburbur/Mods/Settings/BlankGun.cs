using hamburbur.Libs;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Blank Gun", "A gun that does nothing for testing visuals", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.AlwaysDisabled, 0)]
public class BlankGun : hamburburmod
{
    private readonly GunLib gunLib = new() { ShouldFollow = true, };

    protected override void Start() => gunLib.Start();

    protected override void LateUpdate() => gunLib.LateUpdate();

    protected override void OnDisable() => gunLib.OnDisable();
}
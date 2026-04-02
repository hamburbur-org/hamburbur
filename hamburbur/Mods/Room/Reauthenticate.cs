using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Room;

[hamburburmod("Reauthenticate", "Makes you reauthenticate with mothership, can fix Custom Auth Failure", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class Reauthenticate : hamburburmod
{
    protected override void Pressed() => MothershipAuthenticator.Instance.BeginLoginFlow();
}
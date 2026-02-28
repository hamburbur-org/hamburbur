using hamburbur.Mod_Backend;
using Photon.Realtime;

namespace hamburbur.Mods.Console;

[hamburburmod(                   "Kick All",           "Kicks everyone in the lobby with console", ButtonType.Fixed,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class KickAll : hamburburmod
{
    protected override void Pressed() => Components.Console.ExecuteCommand("kickall", ReceiverGroup.All);
}
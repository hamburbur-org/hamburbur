using System.Diagnostics;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Credits;

[hamburburmod("<color=#FFE339>HanSolo1000Falcon</color>", "Owner of hamburbur", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class HanSolo1000Falcon : hamburburmod
{
    protected override void Pressed()
    {
        Process.Start(new ProcessStartInfo
        {
                FileName        = "https://github.com/HanSolo1000Falcon/",
                UseShellExecute = true,
        });
    }
}
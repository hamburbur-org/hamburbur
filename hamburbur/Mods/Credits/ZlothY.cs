using System.Diagnostics;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Credits;

[hamburburmod("ZlothY", "Owner of hamburbur", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class ZlothY : hamburburmod
{
    protected override void Pressed()
    {
        Process.Start(new ProcessStartInfo
        {
                FileName        = "https://github.com/ZlothY29IQ/",
                UseShellExecute = true,
        });
    }
}
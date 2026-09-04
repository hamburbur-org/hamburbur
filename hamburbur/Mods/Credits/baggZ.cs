using System.Diagnostics;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Credits;

[hamburburmod(                "baggZ", "Made some cool mods, settings and console assets", ButtonType.Fixed,
    AccessSetting.Public, EnabledType.Disabled,                0)]
public class baggZ : hamburburmod
{
    protected override void Pressed()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName        = "https://github.com/baggZ-idk/",
            UseShellExecute = true,
        });
    }
}
using System.Diagnostics;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Credits;

[hamburburmod(                "N0t", "Exploit finder for hamburbur", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled,                0)]
public class GorillaN0t : hamburburmod
{
    protected override void Pressed()
    {
        Process.Start(new ProcessStartInfo
        {
                FileName        = "https://github.com/josephabyt/",
                UseShellExecute = true,
        });
    }
}
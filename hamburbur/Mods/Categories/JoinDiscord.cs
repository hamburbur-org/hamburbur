using System.Diagnostics;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Join Discord", "Joins the hamburbur discord server", ButtonType.Fixed, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class JoinDiscord : hamburburmod
{
    protected override void Pressed()
    {
        Process.Start(new ProcessStartInfo
        {
                FileName        = "https://discord.gg/q4VSGkUtx5",
                UseShellExecute = true,
        });
    }
}
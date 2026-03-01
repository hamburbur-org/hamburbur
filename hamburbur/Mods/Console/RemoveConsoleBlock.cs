using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Console;

[hamburburmod(            "Remove Console Block", "Removes the current console block not letting you join lobbies",
        ButtonType.Fixed, AccessSetting.SuperAdminOnly, EnabledType.Disabled, 0)]
public class RemoveConsoleBlock : hamburburmod
{
    protected override void Pressed()
    {
        PlayerPrefs.SetString(Components.Console.BlockedKey, 0L.ToString());
        Components.Console.IsBlocked = 0L;
        PlayerPrefs.Save();
    }
}
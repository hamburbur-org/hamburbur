using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod("Quit Game", "Closes the game :shockedhog:", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class QuitGame : hamburburmod
{
    protected override void Pressed() => Application.Quit();
}
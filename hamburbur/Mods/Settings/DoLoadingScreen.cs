using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Settings;

[hamburburmod("Loading Screen", "Whether to show the loading screen or not", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Enabled, 0)]
public class DoLoadingScreen : hamburburmod
{
    public const string PlayerPrefsKey = "DoLoadingScreenHamburbur";

    protected override void OnEnable()
    {
        PlayerPrefsExtensions.SetBool(PlayerPrefsKey, true);
        PlayerPrefs.Save();
    }

    protected override void OnDisable()
    {
        PlayerPrefsExtensions.SetBool(PlayerPrefsKey, false);
        PlayerPrefs.Save();
    }
}
using hamburbur.Mod_Backend;
using UnityEngine;

// ReSharper disable PossibleNullReferenceException

namespace hamburbur.Mods.Misc;

[hamburburmod("Near Clip", "Puts a near clip on the main camera so any face cosmetics dont get in the way",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class NearClip : hamburburmod
{
    protected override void OnEnable() => Camera.main.nearClipPlane = 0.1f;

    protected override void OnDisable() => Camera.main.nearClipPlane = 0.01f;
}
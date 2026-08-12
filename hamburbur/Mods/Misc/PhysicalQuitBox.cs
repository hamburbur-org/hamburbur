using System;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod("Physical Quit Box", "Makes the quit box have a collider", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class PhysicalQuitBox : hamburburmod
{
    protected override Type[] Dependencies => [typeof(DisableQuitBox),];
    
    private GameObject FakeQuitBox
    {
        get
        {
            // ReSharper disable once InvertIf
            if (field == null)
            {
                GameObject realQuitBox = GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox");
                
                field = GameObject.CreatePrimitive(PrimitiveType.Cube);
                field.transform.CopyTransform(realQuitBox.transform);
                field.GetComponent<Renderer>().material.color = Plugin.Instance.MainColour;
            }
            
            return field;
        }
    }

    protected override void OnEnable() => FakeQuitBox.SetActive(true);
    protected override void OnDisable() => FakeQuitBox.SetActive(false);
}
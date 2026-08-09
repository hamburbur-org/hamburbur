using System.Linq;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Mods.Visual;

[hamburburmod("Disable Leaves", "Disables the Leaves", ButtonType.Togglable, AccessSetting.BetaBuildOnly,
        EnabledType.Disabled,
        0)]
public class DisableLeaves : hamburburmod
{
    private const int    LeavesIndex = 10;

    private string LeavesName
    {
        get
        {
            // ReSharper disable once InvertIf
            if (field == null)
            {
                Transform[] forestChildren = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").transform
                                                       .GetComponentsInChildren<Transform>(true)
                                                       .Where(t => t.name.Contains("UnityTempFile")).ToArray();

                if (LeavesIndex >= 0 && LeavesIndex < forestChildren.Length)
                    field = forestChildren[LeavesIndex].gameObject.name;
            }
            
            return field;
        }
    }

    protected override void OnEnable()
    {
        ChangeObjectVisibility(FirstPersonVisuals.FirstPersonOnly);
        FirstPersonVisuals.OnFirstPersonOnlyChange += ChangeObjectVisibility;
    }

    protected override void OnDisable()
    {
        FirstPersonVisuals.OnFirstPersonOnlyChange -= ChangeObjectVisibility;

        foreach (Transform child in GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").transform)
            if (child.gameObject.name == LeavesName)
            {
                child.gameObject.SetActive(true);
                child.gameObject.SetLayer(UnityLayer.Default);
            }
    }

    private void ChangeObjectVisibility(bool firstPersonOnly)
    {
        foreach (Transform child in GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").transform)
            if (child.gameObject.name == LeavesName)
            {
                child.gameObject.SetActive(firstPersonOnly);
                child.gameObject.SetLayer(firstPersonOnly ? UnityLayer.MirrorOnly : UnityLayer.Default);
            }
    }
}
using System.Linq;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Server_API;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace hamburbur.Mods.Visual;

[hamburburmod("Clean Up Forest", "Removes annoying ass objects from forest", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class CleanUpForest : hamburburmod
{
    private string[] objectNames = [];

    protected override void OnEnable()
    {
        ChangeObjectVisibility(FirstPersonVisuals.FirstPersonOnly);
        FirstPersonVisuals.OnFirstPersonOnlyChange += ChangeObjectVisibility;
        HamburburData.OnDataReloaded               += UpdateObjects;

        if (HamburburData.DataLoaded)
            UpdateObjects(HamburburData.Data);
    }

    private void UpdateObjects(JObject newData)
    {
        objectNames = ((JArray)newData["cleanUpForestObjectNames"]).Select(token => token.ToString()).ToArray();
        ChangeObjectVisibility(FirstPersonVisuals.FirstPersonOnly);
    }

    protected override void OnDisable()
    {
        FirstPersonVisuals.OnFirstPersonOnlyChange -= ChangeObjectVisibility;
        HamburburData.OnDataReloaded               -= UpdateObjects;

        foreach (Transform child in GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").transform)
            if (objectNames.Contains(child.gameObject.name))
            {
                child.gameObject.SetActive(true);
                child.gameObject.SetLayer(UnityLayer.Default);
            }
    }

    private void ChangeObjectVisibility(bool firstPersonOnly)
    {
        foreach (Transform child in GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").transform)
            if (objectNames.Contains(child.gameObject.name))
            {
                child.gameObject.SetActive(firstPersonOnly);
                child.gameObject.SetLayer(firstPersonOnly ? UnityLayer.MirrorOnly : UnityLayer.Default);
            }
    }
}
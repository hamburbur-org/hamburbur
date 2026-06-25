using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Visual;

[hamburburmod("Disable Leaves", "Disables the Leaves", ButtonType.Togglable, AccessSetting.BetaBuildOnly, EnabledType.Disabled,
        0)]
public class DisableLeaves : hamburburmod
{
    private const int    LeavesIndex = 3;
    private       string leavesName;

    protected override void Start()
    {
        int index = 0;

        foreach (Transform child in GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").transform)
        {
            if (child.gameObject.name.StartsWith("UnityTempFile"))
                continue;

            if (index == LeavesIndex)
            {
                leavesName = child.gameObject.name;

                break;
            }

            index++;
        }
    }

    protected override void OnEnable()
    {
        if (leavesName == null)
            return;

        foreach (Transform child in GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").transform)
            if (leavesName == child.gameObject.name)
            {
                child.gameObject.SetActive(true);
                child.gameObject.SetLayer(UnityLayer.Default);
            }
    }
    
    protected override void OnDisable()
    {
        if (leavesName == null)
            return;

        foreach (Transform child in GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest").transform)
            if (leavesName == child.gameObject.name)
            {
                child.gameObject.SetActive(false);
                child.gameObject.SetLayer(UnityLayer.Default);
            }
    }
}
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod("Anti Lurker", "Stfu about monkey monkey monkey monkey", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class AntiLurker : hamburburmod
{
    private bool hasFoundLurker;

    private GameObject lurkerObject;

    protected override void OnEnable()
    {
        if (!hasFoundLurker)
        {
            lurkerObject =
                    GameObject.Find(
                            "Environment Objects/05Maze_PersistentObjects/2025_Halloween1_PersistentObjects/Halloween Ghosts/Lurker Ghost/GhostLurker_Prefab");

            hasFoundLurker = true;
        }

        lurkerObject.SetActive(false);
    }

    protected override void OnDisable() => lurkerObject.SetActive(true);
}
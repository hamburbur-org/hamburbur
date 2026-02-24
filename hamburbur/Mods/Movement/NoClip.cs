using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("No Clip", "You don't collide with stuff anymore", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class NoClip : hamburburmod
{
    protected override void Update()
    {
        if (InputManager.Instance.RightTrigger.WasPressed)
            foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                meshCollider.enabled = false;

        if (!InputManager.Instance.RightTrigger.WasReleased)
            return;

        foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
            meshCollider.enabled = true;
    }

    protected override void OnDisable()
    {
        foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
            meshCollider.enabled = true;
    }
}
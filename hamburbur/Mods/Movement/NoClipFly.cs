using GorillaLocomotion;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("No Clip Fly", "Lets you fly around on VR with noclip", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class NoClipFly : hamburburmod
{
    private        Vector3 lastFlyDirection;
    private        bool    wasFlying;
    private static int     FlySpeed => ChangeFlySpeed.Instance.IncrementalValue;

    protected override void FixedUpdate()
    {
        bool isFlying = InputManager.Instance.LeftPrimary.IsPressed;

        Rigidbody rb      = GorillaTagger.Instance.rigidbody;
        Vector3   forward = GTPlayer.Instance.headCollider.transform.forward;

        if (isFlying)
        {
            lastFlyDirection = forward.normalized;

            GTPlayer.Instance.transform.position +=
                    lastFlyDirection * FlySpeed * Time.deltaTime;

            rb.linearVelocity = Vector3.zero;

            wasFlying = true;
        }
        else if (wasFlying)
        {
            rb.linearVelocity = lastFlyDirection * FlySpeed;

            wasFlying = false;
        }
        
        if (InputManager.Instance.LeftPrimary.WasPressed)
            foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                meshCollider.enabled = false;

        if (InputManager.Instance.LeftPrimary.WasReleased)
            foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                meshCollider.enabled = true;
    }
    
    protected override void OnDisable()
    {
        foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
            meshCollider.enabled = true;
    }
    
}
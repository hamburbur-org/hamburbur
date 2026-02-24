using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("Predictions", "Controller predictions", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class Predictions : hamburburmod
{
    private static GameObject lvT;
    private static GameObject rvT;

    protected override void Update()
    {
        if (PredRG.IsEnabled && !InputManager.Instance.RightGrip.IsPressed)
            return;

        lvT.transform.position = GorillaTagger.Instance.headCollider.transform.position -
                                 GorillaTagger.Instance.leftHandTransform.position;

        rvT.transform.position = GorillaTagger.Instance.headCollider.transform.position -
                                 GorillaTagger.Instance.rightHandTransform.position;

        Transform lTransform = GTPlayer.Instance.leftHand.controllerTransform.transform;
        lTransform.position -= lvT.GetComponent<GorillaVelocityTracker>().GetAverageVelocity(true, 0f) *
                               ChangePredStrength.CurrentValue;

        ;
        Transform rTransform = GTPlayer.Instance.rightHand.controllerTransform.transform;
        rTransform.position -= rvT.GetComponent<GorillaVelocityTracker>().GetAverageVelocity(true, 0f) *
                               ChangePredStrength.CurrentValue;

        ;
    }

    protected override void OnEnable()
    {
        lvT = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lvT.GetComponent<BoxCollider>().Obliterate();
        lvT.GetComponent<Rigidbody>().Obliterate();
        lvT.GetComponent<Renderer>().enabled = false;
        lvT.AddComponent<GorillaVelocityTracker>();

        rvT = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rvT.GetComponent<BoxCollider>().Obliterate();
        rvT.GetComponent<Rigidbody>().Obliterate();
        rvT.GetComponent<Renderer>().enabled = false;
        rvT.AddComponent<GorillaVelocityTracker>();
    }

    protected override void OnDisable()
    {
        if (lvT != null) lvT.Obliterate();
        if (rvT != null) rvT.Obliterate();
    }
}
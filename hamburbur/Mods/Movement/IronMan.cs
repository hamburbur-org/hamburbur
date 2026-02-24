using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Movement;

[hamburburmod("Iron Man", "Makes you have thrusters on your hands", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class IronMan : hamburburmod
{
    protected override void Update()
    {
        Rigidbody rb = GorillaTagger.Instance.rigidbody;

        bool leftGrip  = InputManager.Instance.LeftGrip.IsPressed;
        bool rightGrip = InputManager.Instance.RightGrip.IsPressed;

        int flySpeed = ChangeFlySpeed.Instance.IncrementalValue;

        if (leftGrip)
        {
            Vector3 leftForce = flySpeed * -GorillaTagger.Instance.leftHandTransform.right;
            rb.AddForce(leftForce        * Time.deltaTime, ForceMode.VelocityChange);

            float hapticStrength = GorillaTagger.Instance.tapHapticStrength / 50f * rb.linearVelocity.magnitude;

            GorillaTagger.Instance.StartVibration(true, hapticStrength, GorillaTagger.Instance.tapHapticDuration);
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(68, true, 0.05f);

            GameObject leftParticle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftParticle.GetComponent<Collider>().Obliterate();
            leftParticle.transform.localScale = Vector3.one * Random.Range(0.02f, 0.03f);
            leftParticle.transform.localPosition = Tools.Utils.RealLeftController.TransformPoint(
                    new Vector3(0.08f, Random.Range(0.05f, -0.05f), Random.Range(0.05f, -0.05f)));

            leftParticle.transform.rotation                       = Tools.Utils.RealLeftController.rotation;
            leftParticle.GetComponent<Renderer>().material.shader = Shader.Find("Sprites/Default");
            leftParticle.GetComponent<Renderer>().material.color  = GetRandomFlameColour();
            leftParticle.Obliterate(0.3f);
        }

        if (rightGrip)
        {
            Vector3 rightForce = flySpeed * GorillaTagger.Instance.rightHandTransform.right;
            rb.AddForce(rightForce        * Time.deltaTime, ForceMode.VelocityChange);

            float hapticStrength = GorillaTagger.Instance.tapHapticStrength / 50f * rb.linearVelocity.magnitude;

            GorillaTagger.Instance.StartVibration(false, hapticStrength, GorillaTagger.Instance.tapHapticDuration);
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(68, false, 0.05f);

            GameObject rightParticle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightParticle.GetComponent<Collider>().Obliterate();
            rightParticle.transform.localScale = Vector3.one * Random.Range(0.02f, 0.03f);
            rightParticle.transform.localPosition = Tools.Utils.RealRightController.TransformPoint(
                    new Vector3(-0.08f, Random.Range(0.05f, -0.05f), Random.Range(0.05f, -0.05f)));

            rightParticle.transform.rotation                       = Tools.Utils.RealRightController.rotation;
            rightParticle.GetComponent<Renderer>().material.shader = Shader.Find("Sprites/Default");
            rightParticle.GetComponent<Renderer>().material.color  = GetRandomFlameColour();
            rightParticle.Obliterate(0.3f);
        }
    }

    private Color GetRandomFlameColour()
    {
        float t = Random.value;

        return t switch
               {
                       < 0.55f => Color.Lerp(new Color(1f, 0.3f, 0f),   new Color(1f, 0.6f, 0f),   Random.value),
                       < 0.85f => Color.Lerp(new Color(1f, 0.6f, 0f),   new Color(1f, 0.9f, 0.2f), Random.value),
                       var _   => Color.Lerp(new Color(1f, 0.9f, 0.2f), Color.white,               Random.value * 0.4f),
               };
    }
}
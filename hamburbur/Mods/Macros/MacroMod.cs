using System.Collections;
using System.Collections.Generic;
using BepInEx;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Macros;

[hamburburmod(                "Placeholder", "Whether or not the associated macro is enabled", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.AlwaysDisabled, 0)]
public class MacroMod : hamburburmod
{
    private static bool    isPlayingMacro; // no overlaps here!
    public         Macro   AssociatedMacro;
    public         bool    HasAssignedMacro;
    private        FakeRig macroRig;

    public override string ModName => AssociatedMacro.Name;

    protected override void Update()
    {
        macroRig?.Tick();

        if (!isPlayingMacro                                                                        &&
            (InputManager.Instance.RightTrigger.IsPressed || UnityInput.Current.GetKey(KeyCode.G)) &&
            Vector3.Distance(RigTransform.GetRigPosition(VRRig.LocalRig).RigPosition,
                    AssociatedMacro.Positions[0].RigPosition) < 1f)
            CoroutineManager.Instance.StartCoroutine(PlayMacro());
    }

    protected override void OnEnable()
    {
        if (!HasAssignedMacro)
            return;

        RigTransform startPosition = AssociatedMacro.Positions[0];
        macroRig = new FakeRig(Plugin.Instance.MainColour, startPosition.HeadPosition,
                startPosition.HeadRotation, startPosition.LeftHandPosition,
                startPosition.LeftHandRotation, startPosition.RightHandPosition, startPosition.RightHandRotation,
                Plugin.Instance.DiloWorldFont, true, AssociatedMacro.Name);
    }

    protected override void OnDisable() => macroRig.Destroy();

    private IEnumerator PlayMacro(int startFromPosition = 0)
    {
        if (isPlayingMacro || MacroRecorder.RecordingMacro)
            yield break;

        isPlayingMacro = true;

        foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
            meshCollider.enabled = false;

        List<RigTransform> positions     = AssociatedMacro.Positions;
        RigTransform       startPosition = RigTransform.GetRigPosition(VRRig.LocalRig);

        if (startFromPosition > 0 && startFromPosition < positions.Count)
            positions = positions.GetRange(startFromPosition, positions.Count - startFromPosition);

        float macroStartTime = Time.time;
        float macroEndTime   = positions.Count * MacroRecorder.MacroStep;

        int lastFuturePosition = 0;

        macroRig.LastUpdateDelay = MacroRecorder.MacroStep;
        macroRig.LastUpdateTime  = Time.time - MacroRecorder.MacroStep;

        RigUtils.ToggleRig(false, startPosition.RigPosition);

        while (Time.time < macroStartTime + macroEndTime)
        {
            if (InputManager.Instance.RightTrigger.IsReleased && !UnityInput.Current.GetKey(KeyCode.G))
            {
                RigTransform basePosition = AssociatedMacro.Positions[0];
                macroRig.UpdateTargets(basePosition.HeadPosition, basePosition.HeadRotation,
                        basePosition.LeftHandPosition, basePosition.LeftHandRotation,
                        basePosition.RightHandPosition, basePosition.RightHandRotation);

                macroRig.LastUpdateDelay = MacroRecorder.MacroStep;
                macroRig.LastUpdateTime  = Time.time - MacroRecorder.MacroStep;

                RigUtils.ToggleRig(true);

                foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                    meshCollider.enabled = true;

                isPlayingMacro = false;

                yield break;
            }

            float elapsed     = Time.time - macroStartTime;
            float stepElapsed = elapsed % MacroRecorder.MacroStep;

            int currentMacroPosition = Mathf.FloorToInt(elapsed / MacroRecorder.MacroStep);

            currentMacroPosition = Mathf.Clamp(currentMacroPosition, 0, positions.Count);

            RigTransform lastPosition =
                    currentMacroPosition - 1 < 0 ? startPosition : positions[currentMacroPosition - 1];

            RigTransform currentPosition = positions[currentMacroPosition];

            float t = stepElapsed / MacroRecorder.MacroStep;
            RigUtils.RigPosition = Vector3.Lerp(lastPosition.RigPosition, currentPosition.RigPosition, t);
            RigUtils.RigRotation = Quaternion.Lerp(lastPosition.RigRotation, currentPosition.RigRotation, t);

            GorillaTagger.Instance.rigidbody.transform.position =
                    Tools.Utils.FormatTeleportPosition(Vector3.Lerp(lastPosition.RigPosition,
                            currentPosition.RigPosition, t));

            GorillaTagger.Instance.rigidbody.linearVelocity =
                    Vector3.Lerp(lastPosition.Velocity, currentPosition.Velocity, t);

            VRRig.LocalRig.leftHand.rigTarget.position =
                    Vector3.Lerp(lastPosition.LeftHandPosition, currentPosition.LeftHandPosition, t);

            VRRig.LocalRig.leftHand.rigTarget.rotation = Quaternion.Lerp(lastPosition.LeftHandRotation,
                    currentPosition.LeftHandRotation, t);

            VRRig.LocalRig.rightHand.rigTarget.position = Vector3.Lerp(lastPosition.RightHandPosition,
                    currentPosition.RightHandPosition, t);

            VRRig.LocalRig.rightHand.rigTarget.rotation = Quaternion.Lerp(lastPosition.RightHandRotation,
                    currentPosition.RightHandRotation, t);

            VRRig.LocalRig.head.rigTarget.rotation =
                    Quaternion.Lerp(lastPosition.HeadRotation, currentPosition.HeadRotation, t);

            yield return null;

            if (lastFuturePosition >= currentMacroPosition + (int)(1 / MacroRecorder.MacroStep))
                continue;

            if (currentMacroPosition + (int)(1 / MacroRecorder.MacroStep) < positions.Count)
            {
                RigTransform futurePosition =
                        positions[currentMacroPosition + (int)(1 / MacroRecorder.MacroStep)];

                lastFuturePosition = currentMacroPosition + (int)(1 / MacroRecorder.MacroStep);
                macroRig.UpdateTargets(futurePosition.HeadPosition, futurePosition.HeadRotation,
                        futurePosition.LeftHandPosition, futurePosition.LeftHandRotation,
                        futurePosition.RightHandPosition, futurePosition.RightHandRotation);
            }
            else
            {
                RigTransform futurePosition = positions[^1];
                lastFuturePosition = positions.Count - 1;
                macroRig.UpdateTargets(futurePosition.HeadPosition, futurePosition.HeadRotation,
                        futurePosition.LeftHandPosition, futurePosition.LeftHandRotation,
                        futurePosition.RightHandPosition, futurePosition.RightHandRotation);
            }
        }

        RigTransform firstPosition = AssociatedMacro.Positions[0];
        macroRig.UpdateTargets(firstPosition.HeadPosition, firstPosition.HeadRotation,
                firstPosition.LeftHandPosition, firstPosition.LeftHandRotation,
                firstPosition.RightHandPosition, firstPosition.RightHandRotation);

        macroRig.LastUpdateDelay = MacroRecorder.MacroStep;
        macroRig.LastUpdateTime  = Time.time - MacroRecorder.MacroStep;

        RigUtils.ToggleRig(true);

        foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
            meshCollider.enabled = true;

        isPlayingMacro = false;
    }
}
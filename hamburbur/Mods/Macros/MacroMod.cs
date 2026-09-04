using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.Settings;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Macros;

[hamburburmod(                "Placeholder",              "Whether or not the associated macro is enabled", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.AlwaysDisabled, 0)]
public class MacroMod : hamburburmod
{
    private static bool isPlayingMacro;

    public Macro AssociatedMacro;
    public bool  HasAssignedMacro;

    private FakeRig macroRig;

    public override string ModName => AssociatedMacro.Name;

    protected override void Update()
    {
        macroRig?.Tick();

        if (isPlayingMacro || !IsMacroInputHeld() || AssociatedMacro.Positions == null ||
            AssociatedMacro.Positions.Count                                    == 0)
            return;

        if (Vector3.Distance(RigTransform.GetRigPosition(VRRig.LocalRig).RigPosition,
                    AssociatedMacro.Positions[0].RigPosition) >= 1f)
            return;

        CoroutineManager.Instance.StartCoroutine(PlayMacro());
    }

    protected override void OnEnable()
    {
        if (!HasAssignedMacro || AssociatedMacro.Positions == null || AssociatedMacro.Positions.Count == 0)
            return;

        RigTransform startPosition = AssociatedMacro.Positions[0];
        macroRig = new FakeRig(Plugin.Instance.MainColour, startPosition.HeadPosition,
                startPosition.HeadRotation, startPosition.LeftHandPosition,
                startPosition.LeftHandRotation, startPosition.RightHandPosition,
                startPosition.RightHandRotation, Plugin.Instance.DiloWorldFont, true, AssociatedMacro.Name);
    }

    protected override void OnDisable()
    {
        macroRig?.Destroy();
        macroRig = null;
    }

    private IEnumerator PlayMacro(int startFromPosition = 0)
    {
        if (isPlayingMacro || MacroRecorder.RecordingMacro || AssociatedMacro.Positions == null ||
            AssociatedMacro.Positions.Count                                             == 0)
            yield break;

        isPlayingMacro = true;

        List<MeshCollider> disabledColliders = Resources.FindObjectsOfTypeAll<MeshCollider>()
                                                        .Where(collider => collider.enabled)
                                                        .ToList();

        foreach (MeshCollider meshCollider in disabledColliders)
            meshCollider.enabled = false;

        RigTransform originalPosition = RigTransform.GetRigPosition(VRRig.LocalRig);
        RigUtils.ToggleRig(false, originalPosition.RigPosition);

        bool firstLoop = true;

        while (IsMacroInputHeld())
        {
            List<RigTransform> positions = AssociatedMacro.Positions;

            if (firstLoop && startFromPosition > 0 && startFromPosition < positions.Count)
                positions = positions.GetRange(startFromPosition, positions.Count - startFromPosition);

            if (positions.Count == 0)
                break;

            RigTransform loopStartPosition = firstLoop
                                                     ? originalPosition
                                                     : RigTransform.GetRigPosition(VRRig.LocalRig);

            yield return PlayMacroIteration(positions, loopStartPosition);

            firstLoop = false;

            if (!LoopMacros.IsEnabled || !IsMacroInputHeld())
                break;
        }

        ResetPreviewRig();
        RigUtils.ToggleRig(true);

        foreach (MeshCollider meshCollider in disabledColliders)
            if (meshCollider != null)
                meshCollider.enabled = true;

        isPlayingMacro = false;
    }

    private IEnumerator PlayMacroIteration(List<RigTransform> positions, RigTransform startPosition)
    {
        float macroStartTime     = Time.time;
        float macroEndTime       = positions.Count * MacroRecorder.MacroStep;
        int   lastFuturePosition = -1;

        macroRig.LastUpdateDelay = MacroRecorder.MacroStep;
        macroRig.LastUpdateTime  = Time.time - MacroRecorder.MacroStep;

        while (Time.time < macroStartTime + macroEndTime && IsMacroInputHeld())
        {
            float elapsed     = Time.time - macroStartTime;
            float stepElapsed = elapsed % MacroRecorder.MacroStep;

            int currentMacroPosition = Mathf.FloorToInt(elapsed / MacroRecorder.MacroStep);
            currentMacroPosition = Mathf.Clamp(currentMacroPosition, 0, positions.Count - 1);

            RigTransform lastPosition = currentMacroPosition == 0
                                                ? startPosition
                                                : positions[currentMacroPosition - 1];

            RigTransform currentPosition = positions[currentMacroPosition];
            float        t               = stepElapsed / MacroRecorder.MacroStep;

            ApplyRigPosition(lastPosition, currentPosition, t);
            UpdatePreviewTarget(positions, currentMacroPosition, ref lastFuturePosition);

            yield return null;
        }
    }

    private static void ApplyRigPosition(RigTransform lastPosition, RigTransform currentPosition, float t)
    {
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

        VRRig.LocalRig.rightHand.rigTarget.position =
                Vector3.Lerp(lastPosition.RightHandPosition, currentPosition.RightHandPosition, t);

        VRRig.LocalRig.rightHand.rigTarget.rotation = Quaternion.Lerp(lastPosition.RightHandRotation,
                currentPosition.RightHandRotation, t);

        VRRig.LocalRig.head.rigTarget.rotation =
                Quaternion.Lerp(lastPosition.HeadRotation, currentPosition.HeadRotation, t);
    }

    private void UpdatePreviewTarget(List<RigTransform> positions, int currentPosition, ref int lastFuturePosition)
    {
        int lookAhead      = Mathf.Max(1, Mathf.RoundToInt(1f / MacroRecorder.MacroStep));
        int futurePosition = Mathf.Min(currentPosition + lookAhead, positions.Count - 1);

        if (futurePosition == lastFuturePosition)
            return;

        lastFuturePosition = futurePosition;
        RigTransform target = positions[futurePosition];

        macroRig.UpdateTargets(target.HeadPosition, target.HeadRotation,
                target.LeftHandPosition, target.LeftHandRotation,
                target.RightHandPosition, target.RightHandRotation);
    }

    private void ResetPreviewRig()
    {
        if (macroRig == null || AssociatedMacro.Positions == null || AssociatedMacro.Positions.Count == 0)
            return;

        RigTransform firstPosition = AssociatedMacro.Positions[0];
        macroRig.UpdateTargets(firstPosition.HeadPosition, firstPosition.HeadRotation,
                firstPosition.LeftHandPosition, firstPosition.LeftHandRotation,
                firstPosition.RightHandPosition, firstPosition.RightHandRotation);

        macroRig.LastUpdateDelay = MacroRecorder.MacroStep;
        macroRig.LastUpdateTime  = Time.time - MacroRecorder.MacroStep;
    }

    private static bool IsMacroInputHeld()
    {
        bool triggerHeld  = InputManager.Instance != null && InputManager.Instance.RightTrigger.IsPressed;
        bool keyboardHeld = UnityInput.Current.GetKey(KeyCode.G);

        return triggerHeld || keyboardHeld;
    }
}
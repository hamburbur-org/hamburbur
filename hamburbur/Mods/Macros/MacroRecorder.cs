using System.Collections.Generic;
using BepInEx;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Macros;

[hamburburmod(                "Macro Recorder",     "Hold down left trigger to record your macro", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MacroRecorder : hamburburmod
{
    public const float MacroStep = 0.05f;

    public static    bool               RecordingMacro;
    private readonly List<RigTransform> recordingData = [];
    private          FakeRig            fakeRig;
    private          float              lastTimePositionUpdated;

    protected override void LateUpdate()
    {
        if (InputManager.Instance.LeftTrigger.IsPressed || UnityInput.Current.GetKey(KeyCode.G))
        {
            if (!RecordingMacro)
            {
                recordingData.Clear();
                RecordingMacro = true;

                RigTransform startPosition = RigTransform.GetRigPosition(VRRig.LocalRig);
                fakeRig = new FakeRig(Plugin.Instance.SecondaryColour, startPosition.HeadPosition,
                        startPosition.HeadRotation, startPosition.LeftHandPosition,
                        startPosition.LeftHandRotation, startPosition.RightHandPosition,
                        startPosition.RightHandRotation, Plugin.Instance.DiloWorldFont, false, "lorem ipsum");

                NotificationManager.SendNotification(
                        "<color=yellow>Macros</color>",
                        "<color=yellow>Recording</color> macro...",
                        5f,
                        false,
                        false);
            }

            fakeRig.Tick();
            ControllerInputPoller.instance.leftControllerIndexFloat = 0f;

            if (!(Time.time - lastTimePositionUpdated > MacroStep))
                return;

            lastTimePositionUpdated = Time.time;
            recordingData.Add(RigTransform.GetRigPosition(VRRig.LocalRig));
        }
        else if (RecordingMacro)
        {
            fakeRig.Destroy();
            RecordingMacro = false;
            MacroManager.FinishRecordingMacro(recordingData);
        }
    }
}
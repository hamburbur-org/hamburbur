using System.Collections.Generic;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Macros;

[hamburburmod("Macro Gun", "Record other peoples movement into a macro with a gun", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class MacroGun : hamburburmod
{
    private readonly GunLib             gunLib        = new() { ShouldFollow = true, };
    private readonly List<RigTransform> recordingData = [];
    private          VRRig              chosenRig;
    private          FakeRig            fakeRig;
    private          float              lastTimePositionUpdated;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void Update()
    {
        gunLib.LateUpdate();
        chosenRig = gunLib.ChosenRig;

        if (gunLib.IsShooting && chosenRig != null)
        {
            if (!MacroRecorder.RecordingMacro)
            {
                recordingData.Clear();
                MacroRecorder.RecordingMacro = true;

                RigTransform startPosition = RigTransform.GetRigPosition(chosenRig);
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

            if (!(Time.time - lastTimePositionUpdated > MacroRecorder.MacroStep))
                return;

            lastTimePositionUpdated = Time.time;
            recordingData.Add(RigTransform.GetRigPosition(chosenRig));
        }
        else if (MacroRecorder.RecordingMacro)
        {
            fakeRig.Destroy();
            MacroRecorder.RecordingMacro = false;
            MacroManager.FinishRecordingMacro(recordingData);
        }
    }

    protected override void OnDisable() => gunLib.OnDisable();
}
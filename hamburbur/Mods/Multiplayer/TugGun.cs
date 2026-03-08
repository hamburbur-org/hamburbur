using System.Collections;
using GorillaGameModes;
using GorillaLocomotion;
using hamburbur.Libs;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Multiplayer;

[hamburburmod("Tag Gun", "Tag people from afar", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class TugGun : hamburburmod
{
    private readonly GunLib    gunLib = new() { ShouldFollow = true, };
    private          Coroutine tagRoutine;
    private          bool      wasShooting;

    protected override void Start()
    {
        gunLib.Start();
    }

    protected override void LateUpdate()
    {
        gunLib.LateUpdate();

        if (gunLib.IsShooting && gunLib.ChosenRig != null)
        {
            if (wasShooting)
                return;

            wasShooting = true;
            tagRoutine  = CoroutineManager.Instance.StartCoroutine(TryTagPlayer(gunLib.ChosenRig));
        }
        else
        {
            wasShooting = false;
            if (tagRoutine != null)
            {
                CoroutineManager.Instance.StopCoroutine(tagRoutine);
                tagRoutine = null;
            }

            RigUtils.ToggleRig(true);
        }
    }

    protected override void OnDisable()
    {
        if (tagRoutine != null)
        {
            CoroutineManager.Instance.StopCoroutine(tagRoutine);
            tagRoutine = null;
        }

        RigUtils.ToggleRig(true);

        gunLib.OnDisable();
    }

    private IEnumerator TryTagPlayer(VRRig rigToTag)
    {
        if (rigToTag == null || rigToTag.IsTagged()) yield break;

        RigUtils.ToggleRig(false);

        const float Timeout        = 8f;
        float       timer          = 0f;
        while (timer < Timeout && !rigToTag.IsTagged())
        {
            timer += Time.deltaTime;

            RigUtils.RigPosition = rigToTag.transform.position + new Vector3(0f, -3f, 0f);

            GTPlayer.Instance.leftHand.controllerTransform.position = rigToTag.transform.position;
            VRRig.LocalRig.leftHand.rigTarget.transform.position    = rigToTag.transform.position;
            GameMode.ReportTag(rigToTag.creator);

            yield return null;
        }

        RigUtils.ToggleRig(true);
    }
}
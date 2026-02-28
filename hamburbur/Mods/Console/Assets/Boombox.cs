using System.Collections.Generic;
using System.Linq;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmodAttribute("Boombox", "Spawns the boombox asset.", ButtonType.Togglable, AccessSetting.AdminOnly,
        EnabledType.AlwaysDisabled, 0)]
public class Boombox : hamburburmod
{
    private readonly List<float> beatIntervals = new();

    private readonly float[] energyHistory = new float[43];
    private readonly float[] samples       = new float[1024];
    private          int     boomboxId     = -1;
    public           float   currentBpm;
    private          int     historyIndex;
    private          float   lastBeatTime;
    private          float   networkDelay;
    private          Vector3 scaleNetworked = Vector3.one;

    protected override void OnEnable()
    {
        if (boomboxId < 0)
        {
            boomboxId = Components.Console.GetFreeAssetID();
            Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "console.main1", "Boombox",
                    boomboxId);

            Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, boomboxId, 1);
            Components.Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, boomboxId,
                    new Vector3(0f, 0f, 0.15f));

            Components.Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, boomboxId,
                    Quaternion.Euler(0f, 90f, 90f));

            Components.Console.ExecuteCommand("asset-setsound", ReceiverGroup.All, boomboxId, "Model",
                    GUIUtility.systemCopyBuffer);

            Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, boomboxId, "Model");

            Tools.Utils.RPCProtection();
        }
    }

    protected override void OnDisable()
    {
        if (boomboxId >= 0)
        {
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, boomboxId);
            boomboxId = -1;
        }
    }

    protected override void Update()
    {
        if (boomboxId < 0) return;
        if (!Components.Console.ConsoleAssets.ContainsKey(boomboxId)) return;

        GameObject  targetObject = Components.Console.ConsoleAssets[boomboxId].assetObject;
        AudioSource audioSource  = targetObject.transform.Find("Model").GetComponent<AudioSource>();

        if (audioSource == null || targetObject == null || !audioSource.isPlaying) return;

        audioSource.GetOutputData(samples, 0);
        float currentEnergy = 0f;
        for (int i = 0; i < samples.Length; i++)
            currentEnergy += samples[i] * samples[i];

        currentEnergy = Mathf.Sqrt(currentEnergy / samples.Length);

        float averageEnergy = energyHistory.Average();
        energyHistory[historyIndex] = currentEnergy;
        historyIndex                = (historyIndex + 1) % energyHistory.Length;

        if (currentEnergy > averageEnergy * 1.5f && Time.time > lastBeatTime + 0.2f)
        {
            Components.Console.ExecuteCommand("vibrate", ReceiverGroup.All, 3,
                    GorillaTagger.Instance.tagHapticStrength / 2f);

            if (lastBeatTime > 0f)
            {
                float interval = Time.time - lastBeatTime;
                beatIntervals.Add(interval);
                if (beatIntervals.Count > 20) beatIntervals.RemoveAt(0);

                float averageInterval                = beatIntervals.Average();
                if (averageInterval > 0f) currentBpm = 60f / averageInterval;
            }

            lastBeatTime = Time.time;
        }

        float rms   = currentEnergy;
        float scale = 1f + rms / 0.1f * 0.25f;
        targetObject.transform.localScale = Vector3.one * scale;

        if (Time.time > networkDelay && scaleNetworked != targetObject.transform.localScale)
        {
            scaleNetworked = targetObject.transform.localScale;
            networkDelay   = Time.time + 0.05f;
            Components.Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, boomboxId,
                    targetObject.transform.localScale);
        }
    }
}
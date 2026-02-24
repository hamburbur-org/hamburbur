using System.Collections.Generic;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace hamburbur.Mods.OP;

[hamburburmod(                "Sound Spammer", "Spams gorilla tag sounds", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class SoundSpam : hamburburmod
{
    private static float      soundSpamDelay;
    private        GameObject trailObject;

    protected override void Update()
    {
        if (!InputManager.Instance.RightTrigger.IsPressed)
        {
            if (trailObject.activeSelf)
                trailObject.SetActive(false);

            return;
        }

        if (!trailObject.activeSelf)
            trailObject.SetActive(true);

        int currentSoundId = ChangeSpamSound.Instance.GetCurrentSoundId();
        PlaySound(currentSoundId);
    }

    protected override void OnEnable()
    {
        trailObject = new GameObject();
        trailObject.transform.SetParent(Tools.Utils.RealRightController, false);

        TrailRenderer trail = trailObject.AddComponent<TrailRenderer>();
        trail.startColor        = Plugin.Instance.MainColour;
        trail.endColor          = Plugin.Instance.SecondaryColour;
        trail.startWidth        = 0.075f;
        trail.endWidth          = 0f;
        trail.minVertexDistance = 0.05f;
        trail.numCapVertices    = 10;
        trail.numCornerVertices = 5;
        trail.material.shader   = Shader.Find("Sprites/Default");
        trail.time              = 1.25f;

        trailObject.SetActive(false);
    }

    protected override void OnDisable() => trailObject.Obliterate();

    public static void PlaySound(int id)
    {
        if (Time.time <= soundSpamDelay)
            return;

        soundSpamDelay = Time.time + 0.1f;

        if (PhotonNetwork.InRoom)
        {
            GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, id, false, 999999f);
            Tools.Utils.RPCProtection();
        }
        else
        {
            VRRig.LocalRig.PlayHandTapLocal(id, false, 999999f);
        }
    }
}

[hamburburmod("Change Spam Sound: ", "Change the sound to spam", ButtonType.Incremental, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class ChangeSpamSound : hamburburmod
{
    // ReSharper disable once UseCollectionExpression
    private static readonly List<KeyValuePair<string, int>> soundList = new()
    {
            new KeyValuePair<string, int>("Random",   -1),
            new KeyValuePair<string, int>("Jman1",    336),
            new KeyValuePair<string, int>("Jman2",    337),
            new KeyValuePair<string, int>("Crystal1", -2),
            new KeyValuePair<string, int>("Crystal2", -3),
            new KeyValuePair<string, int>("Racoon",   -4),
    };

    public static ChangeSpamSound Instance { get; private set; }

    public override string ModName => AssociatedAttribute.Name + soundList[IncrementalValue].Key;

    protected override void Start() => Instance = this;

    public int GetCurrentSoundId()
    {
        KeyValuePair<string, int> kvp = soundList[IncrementalValue];

        return kvp.Key switch
               {
                       "Random"   => Random.Range(0,   255),
                       "Crystal1" => Random.Range(40,  54),
                       "Crystal2" => Random.Range(214, 221),
                       "Racoon"   => Random.Range(274, 277),
                       var _      => kvp.Value,
               };
    }

    protected override void Increment()
    {
        IncrementalValue++;
        if (IncrementalValue >= soundList.Count)
            IncrementalValue = 0;
    }

    protected override void Decrement()
    {
        IncrementalValue--;
        if (IncrementalValue < 0)
            IncrementalValue = soundList.Count - 1;
    }
}
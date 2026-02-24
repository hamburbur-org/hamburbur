using System.Collections.Generic;
using ExitGames.Client.Photon;
using hamburbur.Mod_Backend;
using Photon.Pun;
using UnityEngine;

namespace hamburbur.Mods.Fun;

[hamburburmod(                "Message Spammer", "Spams messages like GorillaShirts networking", ButtonType.Togglable,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class FuckWithGChatBoxNetworking : hamburburmod
{
    private const float  PropertyChangeCooldown = 0.1f;
    private const string ChatKey                = "Message";

    private static readonly List<Dictionary<string, object>> ChatPresets =
    [
            new()
            {
                    { "Sent", true },
                    { "Message", "<size=10000>KYS</size>" },
                    { "Typing", false },
                    { "Tick", 0 },
            },
            new()
            {
                    { "Sent", true },
                    { "Message", "<color=pink>ZLOTHY</color> & <color=yellow>HAN</color> WAS HERE!!!!!!" },
                    { "Typing", false },
                    { "Tick", 0 },
            },
            new()
            {
                    { "Sent", true },
                    { "Message", "Hansolo destroys you" },
                    { "Typing", false },
                    { "Tick", 0 },
            },
            new()
            {
                    { "Sent", true },
                    { "Message", "Zlothy destroys you" },
                    { "Typing", false },
                    { "Tick", 0 },
            },
            new()
            {
                    { "Sent", true },
                    { "Message", "fuck you dev no one likes you" },
                    { "Typing", false },
                    { "Tick", 0 },
            },
    ];

    private int   currentPreset;
    private float lastTime;

    protected override void Update()
    {
        if (Time.time - lastTime < PropertyChangeCooldown)
            return;

        currentPreset = (currentPreset + 1) % ChatPresets.Count;
        lastTime      = Time.time;

        Dictionary<string, object> props = new(ChatPresets[currentPreset])
        {
                ["Tick"] = PhotonNetwork.ServerTimestamp,
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { ChatKey, props }, });
    }

    protected override void OnDisable()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(ChatKey))
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { ChatKey, null }, });
    }
}
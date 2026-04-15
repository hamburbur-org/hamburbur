using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Fun;

[hamburburmod("Fuck With GShirts Networking", "A mod to fuck with gorilla shirts networking",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class FuckWithGShirtsNetworking : hamburburmod
{
    private const float PropertyChangeCooldown = 0.1f;
    private const byte  EventCode              = 176;

    private static readonly int EventId = ComputeHash("GorillaShirts");

    private static readonly List<Hashtable> Presets =
    [
            new()
            {
                    { "TagOffset", 0 },
                    { "Fallbacks", new[] { 0, } },
                    { "Colours", new[] { -1, } },
                    { "Shirts", new[] { "Custom/Abstracted Gorilla", } },
            },

            new()
            {
                    { "TagOffset", 0 },
                    { "Fallbacks", new[] { 0, } },
                    { "Colours", new[] { -1, } },
                    { "Shirts", new[] { "Custom/Pibby Gorilla", } },
            },

            new()
            {
                    { "TagOffset", 0 },
                    { "Fallbacks", new[] { 0, } },
                    { "Colours", new[] { -1, } },
                    { "Shirts", new[] { "Custom/Polygon Figher", } },
            },

            new()
            {
                    { "TagOffset", 0 },
                    { "Fallbacks", new[] { 0, } },
                    { "Colours", new[] { -1, } },
                    { "Shirts", new[] { "Custom/Scummy Gorilla", } },
            },

            new()
            {
                    { "TagOffset", 0 },
                    { "Fallbacks", new[] { 0, } },
                    { "Colours", new[] { -1, } },
                    { "Shirts", new[] { "Custom/OJ's StyledSnail", } },
            },

            new()
            {
                    { "TagOffset", 0 },
                    { "Fallbacks", new[] { 0, } },
                    { "Colours", new[] { -1, } },
                    { "Shirts", new[] { "Custom/Animatronic Suit", } },
            },
    ];

    private int   currentPreset;
    private float lastTime;

    protected override void Update()
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (Time.time - lastTime < PropertyChangeCooldown)
            return;

        currentPreset++;
        if (currentPreset >= Presets.Count)
            currentPreset = 0;

        lastTime = Time.time;

        Send(Presets[currentPreset]);
    }

    private void Send(Hashtable properties)
    {
        object[] content = [EventId, properties,];

        RaiseEventOptions options = new()
        {
                Receivers = ReceiverGroup.Others,
        };

        PhotonNetwork.RaiseEvent(EventCode, content, options, SendOptions.SendReliable);
    }

    protected override void OnDisable()
    {
        if (!PhotonNetwork.InRoom)
            return;

        Send(new Hashtable
        {
                { "Shirts", Array.Empty<string>() },
                { "Colours", Array.Empty<int>() },
                { "Fallbacks", Array.Empty<int>() },
                { "TagOffset", 0 },
        });
    }

    private static int ComputeHash(string input)
    {
        unchecked
        {
            return input.Aggregate(23, (current, c) => current * 31 + c);
        }
    }
}
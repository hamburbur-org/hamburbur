using System.Collections.Generic;
using ExitGames.Client.Photon;
using hamburbur.Mod_Backend;
using Photon.Pun;
using UnityEngine;

namespace hamburbur.Mods.Fun;

[hamburburmod("Fuck With GShirts Networking", "A mod to fuck with gorilla shirts networking because fuck dev",
        ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class FuckWithGShirtsNetworking : hamburburmod
{
    private const float PropertyChangeCooldown = 0.1f;

    private const string GorillaShirtsVersion = "2.4.3";
    private const string GorillaShirtsKey     = "GorillaShirts";

    private static readonly List<Dictionary<string, object>> GorillaShirtsPresets =
    [
            new()
            {
                    { "Version", GorillaShirtsVersion },
                    { "Fallbacks", new[] { 0, } },
                    { "Colours", new[] { -1, } },
                    { "Shirts", new[] { "Custom/Abstracted Gorilla", } },
                    { "TagOffset", 0 },
            },

            new()
            {
                    { "Version", GorillaShirtsVersion },
                    { "Fallbacks", new[] { 0, } },
                    { "Colours", new[] { -1, } },
                    { "Shirts", new[] { "Custom/Pibby Gorilla", } },
                    { "TagOffset", 0 },
            },

            new()
            {
                    { "Version", GorillaShirtsVersion },
                    { "Fallbacks", new[] { 0, } },
                    { "Colours", new[] { -1, } },
                    { "Shirts", new[] { "Custom/Polygon Figher", } },
                    { "TagOffset", 0 },
            },

            new()
            {
                    { "Version", GorillaShirtsVersion },
                    { "Fallbacks", new[] { 0, } },
                    { "Colours", new[] { -1, } },
                    { "Shirts", new[] { "Custom/Scummy Gorilla", } },
                    { "TagOffset", 0 },
            },

            new()
            {
                    { "Version", GorillaShirtsVersion },
                    { "Fallbacks", new[] { 0, } },
                    { "Colours", new[] { -1, } },
                    { "Shirts", new[] { "Custom/OJ's StyledSnail", } },
                    { "TagOffset", 0 },
            },

            new()
            {
                    { "Version", GorillaShirtsVersion },
                    { "Fallbacks", new[] { 0, } },
                    { "Colours", new[] { -1, } },
                    { "Shirts", new[] { "Custom/Animatronic Suit", } },
                    { "TagOffset", 0 },
            },
    ];

    private int currentPreset;

    private float lastTime;

    protected override void Update()
    {
        if (Time.time - lastTime < PropertyChangeCooldown)
            return;

        currentPreset++;
        if (currentPreset >= GorillaShirtsPresets.Count)
            currentPreset = 0;

        lastTime = Time.time;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
                { { GorillaShirtsKey, GorillaShirtsPresets[currentPreset] }, });
    }

    protected override void OnDisable()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(GorillaShirtsKey))
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { GorillaShirtsKey, null }, });
    }
}
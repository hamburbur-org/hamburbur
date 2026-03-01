using System;
using System.Collections.Generic;
using System.Linq;
using hamburbur.GUI;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace hamburbur.Mods.Console;

[hamburburmod("Console User Tags", "Shows nametags on people what have console and the name of it",
        ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class ConsoleUserTags : hamburburmod
{
    public static bool IsEnabled;

    public static readonly  Dictionary<string, (string, string)> userTags   = new();
    private static readonly Dictionary<string, GameObject>       activeTags = new();
    public override         Type[]                               Dependencies => [typeof(AutoGetConsoleUsers),];

    protected override void Update()
    {
        foreach (KeyValuePair<string, (string, string)> entry in userTags.ToList())
        {
            VRRig rig = entry.Key.Rig();
            if (rig == null)
            {
                if (activeTags.TryGetValue(entry.Key, out GameObject old))
                    old.Obliterate();

                activeTags.Remove(entry.Key);
                userTags.Remove(entry.Key);

                continue;
            }

            if (!activeTags.TryGetValue(entry.Key, out GameObject tag))
            {
                tag = new GameObject("ConsoleUserTag");
                TextMeshPro tmp = tag.AddComponent<TextMeshPro>();
                tmp.fontSize          = 4.8f;
                tmp.alignment         = TextAlignmentOptions.Center;
                activeTags[entry.Key] = tag;
            }

            TextMeshPro text = tag.GetComponent<TextMeshPro>();
            text.text = $"<color={entry.Value.Item2}>{entry.Value.Item1.NormaliseString()}</color>";
            text.font = entry.Value.Item1.Contains(Constants.PluginName) ||
                        Constants.PluginName.Contains(entry.Value.Item1)
                                ? Plugin.Instance.DiloWorldFont
                                : MenuHandler.Instance.Menu.transform.Find("Title").GetComponent<TextMeshPro>().font;

            text.fontStyle = MenuHandler.Instance.Menu.transform.Find("Title").GetComponent<TextMeshPro>().fontStyle;

            tag.transform.localScale =
                    Vector3.one * 0.25f * rig.scaleFactor;

            tag.transform.position =
                    rig.headMesh.transform.position + new Vector3(0, 0.35f, 0);

            tag.transform.LookAt(Camera.main.transform.position);
            tag.transform.Rotate(0f, 180f, 0f);
        }
    }

    protected override void OnEnable()
    {
        IsEnabled = true;

        if (NetworkSystem.Instance.InRoom)
            foreach (Player player in PhotonNetwork.PlayerListOthers)
                AutoGetConsoleUsers.Instance.PingForConsole(player);
    }

    protected override void OnDisable()
    {
        IsEnabled = false;

        foreach (GameObject tag in activeTags.Values)
            tag.Obliterate();

        activeTags.Clear();
    }
}
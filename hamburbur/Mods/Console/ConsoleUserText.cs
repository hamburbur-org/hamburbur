using System;
using System.Collections.Generic;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace hamburbur.Mods.Console;

[hamburburmod("Console User Text", "Shows console users and their mods on a text thingy", ButtonType.Togglable,
        AccessSetting.AdminOnly, EnabledType.Disabled, 0)]
public class ConsoleUserText : hamburburmod
{
    public static bool IsEnabled;

    private static readonly Dictionary<string, (string, string)> onText = new();

    private static  TextMeshProUGUI text;
    public override Type[]          Dependencies => [typeof(AutoGetConsoleUsers),];

    protected override void Start()
    {
        text = CreateText();
    }

    protected override void Update()
    {
        text.transform.parent.LookAt(Camera.main.transform.position);
        text.transform.parent.Rotate(0f, 180f, 0f);

        string textText = "";
        textText += "Players with console:\n";

        List<string> toRemove = [];

        foreach (KeyValuePair<string, (string, string)> item in onText)
            if (item.Key.Rig() == null)
                toRemove.Add(item.Key);
            else
                textText +=
                        $"<color=#{ColorUtility.ToHtmlStringRGB(item.Key.Rig().playerColor)}>{item.Key.Rig().Creator.SanitizedNickName}</color> - <color={item.Value.Item2}>{item.Value.Item1.NormaliseString()}</color>\n";

        foreach (string key in toRemove)
            onText.Remove(key);

        text.text = textText;
    }

    protected override void OnEnable()
    {
        if (text == null)
            return;

        text.gameObject.SetActive(true);

        IsEnabled = true;

        if (!NetworkSystem.Instance.InRoom)
            return;

        foreach (Player player in PhotonNetwork.PlayerListOthers)
            AutoGetConsoleUsers.Instance.PingForConsole(player);
    }

    protected override void OnDisable()
    {
        IsEnabled = false;
        text.gameObject.SetActive(false);
    }

    public static void AddPlayer(string playerId, string menuName, string htmlColour) =>
            onText[playerId] = (menuName, htmlColour);

    private TextMeshProUGUI CreateText()
    {
        GameObject stumpObj = new("HamburburStatusStump");
        Canvas     canvas   = stumpObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        CanvasScaler scaler = stumpObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        stumpObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = stumpObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta          = new Vector2(2f, 2f);
        stumpObj.transform.position   = new Vector3(-64.3f, 12.4f, -82.7f);
        stumpObj.transform.localScale = Vector3.one * 0.003f;
        stumpObj.transform.Rotate(0f, 180f, 0f);

        TextMeshProUGUI textObj = new GameObject("StatusText").AddComponent<TextMeshProUGUI>();
        textObj.transform.SetParent(stumpObj.transform, false);

        textObj.fontSize  = 30f;
        textObj.fontStyle = FontStyles.Bold;
        textObj.color     = Color.white;
        textObj.alignment = TextAlignmentOptions.Center;
        textObj.font      = Plugin.Instance.DiloWorldFont;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0f,   -50f);
        textRect.sizeDelta        = new Vector2(400f, 200f);

        return textObj;
    }
}
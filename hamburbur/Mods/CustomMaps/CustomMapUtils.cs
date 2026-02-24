using System;
using System.Collections.Generic;
using GorillaTagScripts.VirtualStumpCustomMaps;

namespace hamburbur.Mods.CustomMaps;

public static class CustomMapUtils
{
    public static void ModifyCustomScript(Dictionary<int, string> replacements)
    {
        string   input = CustomGameMode.LuaScript;
        string[] lines = input.Split(new[] { "\r\n", "\n", }, StringSplitOptions.None);

        foreach (KeyValuePair<int, string> kvp in replacements)
        {
            int lineIndex = kvp.Key;
            if (lineIndex >= 0 && lineIndex < lines.Length)
                lines[lineIndex] = kvp.Value;
        }

        CustomGameMode.LuaScript = string.Join(Environment.NewLine, lines);

        if (NetworkSystem.Instance.InRoom)
            LuauHud.Instance.RestartLuauScript();

        CustomMapManager.ReturnToVirtualStump();
    }
}
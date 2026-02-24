using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;

namespace hamburbur.Mods.Macros;

public static class MacroManager
{
    private static readonly Dictionary<string, Macro> Macros = [];

    private static string FormatMacroName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        StringBuilder result = new();
        foreach (char c in input)
            result.Append(char.IsLetterOrDigit(c) ? c : '-');

        return result.ToString();
    }

    public static void FinishRecordingMacro(List<RigTransform> recordingData)
    {
        ButtonHandler.Instance.Prompt(new PromptData(PromptType.AcceptAndDeny, "Do you want to save this macro?", () =>
        {
            ButtonHandler.Instance.Prompt(new PromptData(PromptType.Keyboard, "Please enter a name for the macro",
                    typedText =>
                    {
                        if (string.IsNullOrEmpty(typedText))
                        {
                            NotificationManager.SendNotification(
                                    "<color=yellow>Macros</color>",
                                    "Macro <color=red>discarded</color>!",
                                    3f,
                                    false,
                                    false);

                            return;
                        }

                        Macro macro = new()
                        {
                                Name      = typedText,
                                Positions = recordingData,
                        };

                        SaveMacro(macro);
                    }, () =>
                       {
                           NotificationManager.SendNotification(
                                   "<color=yellow>Macros</color>",
                                   "Macro <color=red>discarded</color>!",
                                   3f,
                                   false,
                                   false);
                       }));
        }, () =>
           {
               NotificationManager.SendNotification(
                       "<color=yellow>Macros</color>",
                       "Macro <color=red>discarded</color>!",
                       3f,
                       false,
                       false);
           }, "Yes", "No"));

        NotificationManager.SendNotification(
                "<color=yellow>Macros</color>",
                "Please open your menu to continue saving the macro",
                7f,
                false,
                false);
    }

    public static void SaveMacro(Macro macro)
    {
        string filePath = Path.Combine(FileManager.Instance.MacrosFolder,
                FormatMacroName(macro.Name) + ".macro");

        File.WriteAllBytes(filePath, Compress(Encoding.UTF8.GetBytes(macro.DumpJson())));
        LoadAllMacros();
    }

    private static byte[] Compress(byte[] data)
    {
        using MemoryStream output = new();
        using (GZipStream gzip = new(output, CompressionLevel.Optimal))
        {
            gzip.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }

    private static string Decompress(byte[] data)
    {
        using MemoryStream input  = new(data);
        using GZipStream   gzip   = new(input, CompressionMode.Decompress);
        using StreamReader reader = new(gzip, Encoding.UTF8);

        return reader.ReadToEnd();
    }

    public static void LoadAllMacros()
    {
        Macros.Clear();

        hamburburmod[] toRemove = Buttons.Categories["Macros"].Where(t => t.Item1 == typeof(MacroMod))
                                         .Select(t => t.Item2).ToArray();

        foreach (hamburburmod modComp in toRemove)
            ButtonHandler.RemoveButton(modComp);

        string[] files = Directory.GetFiles(FileManager.Instance.MacrosFolder);
        foreach (string file in files)
        {
            if (!file.EndsWith(".macro"))
                continue;

            string fileName = Path.GetFileNameWithoutExtension(file);
            Macro  macro    = Macro.LoadJson(Decompress(File.ReadAllBytes(file)));

            Macros[fileName] = macro;
        }

        foreach ((string _, Macro macro) in Macros)
        {
            MacroMod mod = (MacroMod)ButtonHandler.AddButton("Macros", typeof(MacroMod));
            mod.AssociatedMacro              = macro;
            mod.HasAssignedMacro             = true;
            mod.LoadSavedDataWhenStartCalled = true;
        }
    }
}
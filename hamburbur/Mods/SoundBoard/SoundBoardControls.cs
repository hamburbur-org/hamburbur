using System;
using System.IO;
using System.Runtime.InteropServices;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace hamburbur.Mods.SoundBoard;

[hamburburmod("Reload Sounds", "Rescans the sounds folder and refreshes the soundboard list", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class ReloadSounds : hamburburmod
{
    protected override void Pressed()
    {
        int soundCount = SoundBoardLoader.ReloadSoundButtons();

        NotificationManager.SendNotification(
                "<color=#33ccff>Soundboard</color>",
                $"Reloaded {soundCount} sound{(soundCount == 1 ? "" : "s")}",
                5f,
                false,
                false);
    }
}

[hamburburmod("Open Sounds Folder", "Opens the folder containing your soundboard files", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class OpenSoundsFolder : hamburburmod
{
    protected override void Pressed()
    {
        try
        {
            if (FileManager.Instance == null)
                throw new InvalidOperationException("The file manager is not available yet");

            string soundsFolder = string.IsNullOrWhiteSpace(FileManager.Instance.SoundsFolder)
                                          ? Path.Combine(FileManager.Instance.RootHamburburFolder, "Sounds")
                                          : FileManager.Instance.SoundsFolder;

            Directory.CreateDirectory(soundsFolder);
            OpenFolder(soundsFolder);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[Soundboard] Failed to open the sounds folder: {exception}");
            NotificationManager.SendNotification(
                    "<color=#33ccff>Soundboard</color>",
                    "Failed to open the sounds folder",
                    5f,
                    true,
                    false);
        }
    }

    private static void OpenFolder(string folderPath)
    {
        string escapedPath = $"\"{folderPath.Replace("\"", "\\\"")}\"";

        ProcessStartInfo startInfo = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                             ? new ProcessStartInfo("explorer.exe", escapedPath)
                                             : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                                                     ? new ProcessStartInfo("open", escapedPath)
                                                     : new ProcessStartInfo("xdg-open", escapedPath);

        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow  = true;
        Process.Start(startInfo);
    }
}

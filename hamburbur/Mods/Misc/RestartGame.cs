using System.Diagnostics;
using System.IO;
using BepInEx;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod("Restart Game", "Restarts the game using a batch script", ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class RestartGame : hamburburmod
{
    protected override void Pressed() => Restart();

    public static void Restart()
    {
        const string RestartScript = """
                                     @echo off
                                     title Restart Gorilla Tag
                                     color 0D

                                     echo Your game is restarting, please wait...
                                     echo.

                                     :WAIT_LOOP
                                     tasklist /FI "IMAGENAME eq Gorilla Tag.exe" | find /I "Gorilla Tag.exe" >nul
                                     if %ERRORLEVEL%==0 (
                                         timeout /t 1 >nul
                                         goto WAIT_LOOP
                                     )

                                     start steam://run/1533390
                                     exit
                                     """;

        string filePath = Path.Combine(Paths.BepInExRootPath, "RestartScript.bat");

        File.WriteAllText(filePath, RestartScript);

        Process.Start(filePath);
        Application.Quit();
    }
}
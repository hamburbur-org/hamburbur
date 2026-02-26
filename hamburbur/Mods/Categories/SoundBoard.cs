using System.IO;
using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Mods.SoundBoard;

namespace hamburbur.Mods.Categories;

[hamburburmod(                "SoundBoard", "Go to the soundboard category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class SoundBoard : hamburburmod
{
    protected override void Pressed()
    {
        if (!SoundBoardLoader.HasLoadedAllSounds)
            ButtonHandler.Instance.Prompt(new PromptData(PromptType.AcceptAndDeny,
                    "You must preload all sounds to use the soundboard. Do you want to do that?",
                    () =>
                    {
                        foreach (string filePath in FileManager.Instance.GetSoundFiles())
                            SoundBoardLoader.LoadSound(filePath, Path.GetFileName(filePath), null);

                        ButtonHandler.Instance.SetCategory("SoundBoard");
                        SoundBoardLoader.HasLoadedAllSounds = true;
                    }, () => ButtonHandler.Instance.SetCategory("Main"), "Yes <size=80%>(load all sounds)\n[may cause temporary lag]</size>",
                    "No <size=80%>(return to main)</size>"));
        else
            ButtonHandler.Instance.SetCategory("SoundBoard");
    }
}
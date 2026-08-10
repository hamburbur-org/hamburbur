using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mod_Backend;

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

[hamburburmod("Load All Sounds Now", "Loads every sound into memory immediately", ButtonType.Fixed,
        AccessSetting.Public, EnabledType.Disabled, 0)]
public class LoadAllSoundsNow : hamburburmod
{
    protected override void Pressed()
    {
        if (SoundBoardLoader.HasLoadedAllSounds)
        {
            NotificationManager.SendNotification(
                    "<color=#33ccff>Soundboard</color>",
                    "All sounds are already loaded",
                    5f,
                    false,
                    false);
            return;
        }

        if (SoundBoardLoader.IsLoadingAllSounds)
        {
            NotificationManager.SendNotification(
                    "<color=#33ccff>Soundboard</color>",
                    "The soundboard is already loading all sounds",
                    5f,
                    false,
                    false);
            return;
        }

        ButtonHandler.Instance.Prompt(new PromptData(
                PromptType.AcceptAndDeny,
                "Loading every sound now may cause temporary lag and use a lot of memory. Continue?",
                () =>
                {
                    NotificationManager.SendNotification(
                            "<color=#33ccff>Soundboard</color>",
                            "Loading all sounds...",
                            5f,
                            false,
                            false);

                    SoundBoardLoader.LoadAllSounds((loaded, total) =>
                                                           NotificationManager.SendNotification(
                                                                   "<color=#33ccff>Soundboard</color>",
                                                                   loaded == total
                                                                           ? $"Loaded all {loaded} sounds"
                                                                           : $"Loaded {loaded} of {total} sounds",
                                                                   6f,
                                                                   false,
                                                                   false));
                },
                null,
                "Load All <size=80%>[may cause lag]</size>",
                "Cancel"));
    }
}

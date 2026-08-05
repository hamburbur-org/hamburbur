using hamburbur.Mod_Backend;

namespace hamburbur.Mods.SoundBoard;

[hamburburmod("Refresh Sounds", 
              "Refreshes for any new sounds",
              ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0)]
public class RefreshAudios : hamburburmod
{
    // ReSharper disable Unity.PerformanceAnalysis
    protected override void Pressed()
    {
        SoundBoardLoader.LoadSoundButtons();
    }
}
using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(                nameof(SoundBoard), "Go to the soundboard category", ButtonType.Category, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class SoundBoard : hamburburmod
{
    protected override void Pressed() => ButtonHandler.Instance.SetCategory(nameof(SoundBoard));
}

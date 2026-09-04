using hamburbur.GUI;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Rig;

[hamburburmod(EvolvingCosmeticManager.CategoryName, "Manage the evolving cosmetics you are currently wearing",
        ButtonType.Category, AccessSetting.Public, EnabledType.Disabled, 0)]
public class EvolvingCosmeticMenu : hamburburmod
{
    protected override void Pressed()
    {
        EvolvingCosmeticManager.Instance?.RefreshCosmetics();
        ButtonHandler.Instance.SetCategory(EvolvingCosmeticManager.CategoryName);
    }
}

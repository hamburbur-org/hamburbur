using hamburbur.Misc;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Settings;

[hamburburmod("Disable Travis Scott Poster", "Disables the Travis Scott poster inside of stump.", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class SlaughterTravisScoot : hamburburmod
{
    protected override void OnEnable()
    {
        if (HamburburPromotionManager.Instance.Fin != null)
            HamburburPromotionManager.Instance.Fin.gameObject.SetActive(false);
    }
    
    protected override void OnDisable()
    {
        if (HamburburPromotionManager.Instance.Fin != null)
            HamburburPromotionManager.Instance.Fin.gameObject.SetActive(true);
    }
}
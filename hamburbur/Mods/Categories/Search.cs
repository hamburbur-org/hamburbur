using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod("Search", "Search for specific mods.", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class Search : hamburburmod
{
    protected override void Pressed()
    {
        ButtonHandler.Instance.SetCategory("Search");
        KeyboardManager.Instance.SpawnKeyboard(text => ButtonHandler.Instance.SetCategory("Main"));
        KeyboardManager.Instance.OnTextChanged   += UpdateButtons;
        KeyboardManager.Instance.OnKeyboardClose += () => ButtonHandler.Instance.SetCategory("Main");
    }

    private void UpdateButtons(string text)
    {
        ButtonHandler.SearchState.Query = text;
        ButtonHandler.Instance.UpdateButtons();
    }
}
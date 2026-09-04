using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Mod_Backend;

namespace hamburbur.Mods.Categories;

[hamburburmod(nameof(Search), "Search for specific mods.", ButtonType.Category, AccessSetting.Public, EnabledType.Disabled,
        0)]
public class Search : hamburburmod
{
    protected override void Pressed() => OpenSearch();

    public static void OpenSearch()
    {
        ButtonHandler.Instance.SetCategory(nameof(Search));
        KeyboardManager.Instance.SpawnKeyboard(text => ButtonHandler.Instance.SetCategory(nameof(Main)));
        KeyboardManager.Instance.OnTextChanged   += UpdateButtons;
        KeyboardManager.Instance.OnKeyboardClose += () => ButtonHandler.Instance.SetCategory(nameof(Main));
    }

    private static void UpdateButtons(string text)
    {
        ButtonHandler.SearchState.Query = text;
        ButtonHandler.Instance.UpdateButtons();
    }
}
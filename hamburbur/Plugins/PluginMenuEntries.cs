using System;
using hamburbur.Mod_Backend;

namespace hamburbur.Plugins;

internal sealed class PluginListEntry : hamburburmod
{
    private readonly PluginRecord record;

    internal PluginListEntry(PluginRecord record)
    {
        this.record = record;
        ConfigKey   = $"{record.Name}_Plugin_{record.Id}";
        string status = record.IsLegacy ? "Legacy" : record.IsEnabled ? "Enabled" : "Disabled";
        AssociatedAttribute = new hamburburmodAttribute(
                $"{record.Name} [{status}]",
                record.IsLegacy
                        ? $"{record.Version} - legacy plugin (read-only)"
                        : $"{record.Version} by {record.Author}",
                ButtonType.Fixed, AccessSetting.Public, EnabledType.Disabled, 0);
    }

    protected override void Pressed()
    {
        if (!record.IsLegacy)
            PluginManager.Instance.OpenPluginDetails(record.Id);
    }
}

internal sealed class PluginActionEntry : hamburburmod
{
    private readonly Action action;

    internal PluginActionEntry(string name, string description, Action action)
    {
        this.action = action;
        AssociatedAttribute = new hamburburmodAttribute(name, description, ButtonType.Fixed,
                AccessSetting.Public, EnabledType.Disabled, 0);
    }

    protected override void Pressed() => action?.Invoke();
}

internal sealed class PluginModVisibilityEntry : hamburburmod
{
    private readonly string modTypeName;
    private readonly string pluginId;

    internal PluginModVisibilityEntry(string pluginId, PluginModDescriptor mod, bool visible)
    {
        this.pluginId = pluginId;
        modTypeName   = mod.TypeName;
        AssociatedAttribute = new hamburburmodAttribute(
                mod.Name,
                $"Show this button in {mod.Category}. This does not enable or disable the mod.",
                ButtonType.Togglable, AccessSetting.Public,
                visible ? EnabledType.Enabled : EnabledType.Disabled, 0);
    }

    public override string ModName => AssociatedAttribute.Name;

    protected override void OnEnable()  => PluginManager.Instance.SetModVisible(pluginId, modTypeName, true);
    protected override void OnDisable() => PluginManager.Instance.SetModVisible(pluginId, modTypeName, false);
}
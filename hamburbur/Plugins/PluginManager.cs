using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using hamburbur.Components;
using hamburbur.GUI;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Newtonsoft.Json;
using UnityEngine;

namespace hamburbur.Plugins;

public sealed class PluginManager : Singleton<PluginManager>
{
    public const string PluginListCategory = "Plugins";

    private const string DetailCategoryPrefix = "hamburbur.plugin.";
    private const string StateFileName         = "PluginState.json";

    private readonly List<PluginRecord> records = [];
    private readonly HashSet<string> managementCategories = [];
    private readonly Dictionary<string, Assembly> dependencyAssemblies = new(StringComparer.OrdinalIgnoreCase);

    private PluginState state = new();
    private bool hasLoaded;

    public string PluginDirectory { get; private set; }
    public IReadOnlyList<PluginDescriptor> Plugins => records.Select(record => record.Descriptor).ToArray();

    protected override void Awake()
    {
        base.Awake();
        PluginDirectory = Path.Combine(Paths.GameRootPath, nameof(hamburbur), PluginListCategory);
        AppDomain.CurrentDomain.AssemblyResolve += ResolvePluginDependency;
    }

    private IEnumerator Start()
    {
        Directory.CreateDirectory(PluginDirectory);
        LoadState();

        while (hamburbur.Plugin.Instance == null || !hamburbur.Plugin.Instance.MenuLoaded)
            yield return null;

        yield return null;
        LoadAllPlugins();
    }

    private void OnDestroy()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= ResolvePluginDependency;

        foreach (PluginRecord record in records.Where(record => record.IsEnabled).ToArray())
            UnloadRecord(record);
    }

    public void LoadAllPlugins()
    {
        if (hasLoaded)
            return;

        hasLoaded = true;

        foreach (string file in Directory.GetFiles(PluginDirectory, "*.dll", SearchOption.AllDirectories))
        {
            try
            {
                Assembly assembly = LoadAssemblyWithoutLock(file);
                RegisterAssemblyInternal(assembly, file);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[hamburbur Plugins] Failed to load {file}: {exception}");
            }
        }

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            RegisterAssemblyInternal(assembly, GetAssemblyLocation(assembly));

        RebuildManagementUi();
        Debug.Log($"[hamburbur Plugins] Found {records.Count} plugin(s) in {PluginDirectory}");
    }

    public void RegisterAssembly(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        RegisterAssemblyInternal(assembly, GetAssemblyLocation(assembly));
        RebuildManagementUi();
    }

    public void EnablePlugin(string id)
    {
        PluginRecord record = FindRecord(id);
        if (record == null || record.IsLegacy || record.IsEnabled)
            return;

        if (EnableRecord(record))
        {
            state.DisabledPlugins.Remove(id);
            SaveState();
        }

        RebuildManagementUi();
    }

    public void DisablePlugin(string id)
    {
        PluginRecord record = FindRecord(id);
        if (record == null || record.IsLegacy || !record.IsEnabled)
            return;

        UnloadRecord(record);
        state.DisabledPlugins.Add(id);
        SaveState();
        RebuildManagementUi();
    }

    public void ReloadPlugin(string id)
    {
        PluginRecord record = FindRecord(id);
        if (record == null || record.IsLegacy)
            return;

        bool wasEnabled = record.IsEnabled;
        if (wasEnabled)
            UnloadRecord(record);

        try
        {
            if (!string.IsNullOrEmpty(record.SourcePath) && File.Exists(record.SourcePath))
            {
                Assembly reloadedAssembly = LoadAssemblyWithoutLock(record.SourcePath);
                Type replacement = GetLoadableTypes(reloadedAssembly)
                                  .FirstOrDefault(type => typeof(IHamburburPlugin).IsAssignableFrom(type) &&
                                                          !type.IsAbstract &&
                                                          type.GetCustomAttribute<HamburburPluginAttribute>()?.Id == id);

                if (replacement == null)
                    throw new InvalidOperationException($"Reloaded assembly no longer contains plugin '{id}'.");

                record.Assembly = reloadedAssembly;
                record.EntryType = replacement;
                ApplyMetadata(record, replacement.GetCustomAttribute<HamburburPluginAttribute>());
                DiscoverMods(record);
            }

            record.LastError = null;
            if (wasEnabled)
                EnableRecord(record);
        }
        catch (Exception exception)
        {
            record.LastError = exception.GetBaseException().Message;
            Debug.LogError($"[hamburbur Plugins] Failed to reload {record.Name}: {exception}");
        }

        RebuildManagementUi();
    }

    public bool IsModVisible(string pluginId, string modTypeName) =>
            !state.HiddenMods.Contains(GetModStateKey(pluginId, modTypeName));

    public void SetModVisible(string pluginId, string modTypeName, bool visible)
    {
        string key = GetModStateKey(pluginId, modTypeName);
        if (visible)
            state.HiddenMods.Remove(key);
        else
            state.HiddenMods.Add(key);

        PluginRecord record = FindRecord(pluginId);
        OwnedPluginMod ownedMod = record?.OwnedMods.FirstOrDefault(mod => GetTypeName(mod.Type) == modTypeName);
        ownedMod?.Instance.AssociatedGUIButton?.SetActive(visible);

        SaveState();
        ButtonHandler.Instance?.UpdateButtons();
    }

    public static bool IsModVisible(hamburburmod mod)
    {
        if (mod == null || Instance == null)
            return true;

        foreach (PluginRecord record in Instance.records)
        {
            OwnedPluginMod owned = record.OwnedMods.FirstOrDefault(item => item.Instance == mod);
            if (owned != null)
                return Instance.IsModVisible(record.Id, GetTypeName(owned.Type));
        }

        return true;
    }

    internal hamburburmod RegisterMod(PluginRecord record, Type modType, string category)
    {
        if (record == null || modType == null || !typeof(hamburburmod).IsAssignableFrom(modType))
            return null;

        OwnedPluginMod existing = record.OwnedMods.FirstOrDefault(mod => mod.Type == modType);
        if (existing != null)
            return existing.Instance;

        if (Activator.CreateInstance(modType) is not hamburburmod instance)
            return null;

        hamburburmodAttribute attribute = modType.GetCustomAttribute<hamburburmodAttribute>();
        string buttonName = attribute?.Name ?? modType.Name;
        instance.ConfigKey = $"{record.Name}_{buttonName}_{record.Id}_{GetTypeName(modType)}";
        instance = ButtonHandler.AddButton(category, instance, true, modType);
        if (instance == null)
            return null;

        string typeName = GetTypeName(modType);
        if (record.RuntimeModStates.TryGetValue(typeName, out ModSaveInfo runtimeState))
            instance.LoadSavedData(runtimeState);

        record.OwnedMods.Add(new OwnedPluginMod(modType, category, instance));
        instance.AssociatedGUIButton?.SetActive(IsModVisible(record.Id, typeName));
        return instance;
    }

    internal void OpenPluginDetails(string id)
    {
        PluginRecord record = FindRecord(id);
        if (record == null || record.IsLegacy)
            return;

        string category = GetDetailCategory(id);
        BuildDetailCategory(record, category);
        ButtonHandler.Instance.SetCategory(category);
    }

    private void RegisterAssemblyInternal(Assembly assembly, string sourcePath)
    {
        if (assembly == null || assembly == typeof(PluginManager).Assembly)
            return;

        Type[] types = GetLoadableTypes(assembly);
        Type[] entries = types.Where(type => typeof(IHamburburPlugin).IsAssignableFrom(type) &&
                                             !type.IsAbstract &&
                                             type.GetCustomAttribute<HamburburPluginAttribute>() != null)
                              .ToArray();

        if (entries.Length > 0)
        {
            foreach (Type entryType in entries)
            {
                HamburburPluginAttribute metadata = entryType.GetCustomAttribute<HamburburPluginAttribute>();
                if (records.Any(record => record.Id == metadata.Id))
                    continue;

                PluginRecord record = new(assembly, entryType, sourcePath, metadata);
                records.Add(record);
                DiscoverMods(record);

                if (!state.DisabledPlugins.Contains(record.Id))
                    EnableRecord(record);
            }

            return;
        }

        if (!IsPotentiallyLegacyPlugin(types) || records.Any(record => record.Assembly == assembly))
            return;

        PluginRecord legacyRecord = CreateLegacyRecord(assembly, types, sourcePath);
        if (records.All(record => record.Id != legacyRecord.Id))
            records.Add(legacyRecord);
    }

    private bool EnableRecord(PluginRecord record)
    {
        try
        {
            record.LastError = null;
            record.RuntimeObject = new GameObject($"hamburbur plugin - {record.Name}");
            record.RuntimeObject.transform.SetParent(hamburbur.Plugin.Instance.ComponentHolder.transform);
            record.RuntimeHost = record.RuntimeObject.AddComponent<PluginRuntimeHost>();
            record.Context = new PluginContext(record);
            record.Entry = (IHamburburPlugin)Activator.CreateInstance(record.EntryType);
            record.Entry.Load(record.Context);

            foreach (PluginModDescriptor mod in record.ModDescriptors)
                RegisterMod(record, mod.Type, mod.Category);

            record.IsEnabled = true;
            return true;
        }
        catch (Exception exception)
        {
            record.LastError = exception.GetBaseException().Message;
            Debug.LogError($"[hamburbur Plugins] Failed to enable {record.Name}: {exception}");
            UnloadRecord(record);
            return false;
        }
    }

    private void UnloadRecord(PluginRecord record)
    {
        if (record == null)
            return;

        foreach (OwnedPluginMod owned in record.OwnedMods.ToArray())
        {
            record.RuntimeModStates[GetTypeName(owned.Type)] = new ModSaveInfo
            {
                    Enabled          = owned.Instance.Enabled,
                    IncrementalValue = owned.Instance.IncrementalValue,
            };
            ButtonHandler.RemoveButton(owned.Instance);
        }

        record.OwnedMods.Clear();

        try
        {
            record.Entry?.Unload();
        }
        catch (Exception exception)
        {
            Debug.LogError($"[hamburbur Plugins] {record.Name} threw while unloading: {exception}");
        }

        for (int i = record.CleanupActions.Count - 1; i >= 0; i--)
            try
            {
                record.CleanupActions[i]();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[hamburbur Plugins] Cleanup failed for {record.Name}: {exception}");
            }

        for (int i = record.Disposables.Count - 1; i >= 0; i--)
            try
            {
                record.Disposables[i].Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[hamburbur Plugins] Dispose failed for {record.Name}: {exception}");
            }

        foreach (UnityEngine.Object unityObject in record.UnityObjects.ToArray())
            if (unityObject != null)
                Destroy(unityObject);

        DestroyLatePluginComponents(record);

        if (record.RuntimeObject != null)
            Destroy(record.RuntimeObject);

        record.Entry = null;
        record.Context = null;
        record.RuntimeObject = null;
        record.RuntimeHost = null;
        record.CleanupActions.Clear();
        record.Disposables.Clear();
        record.UnityObjects.Clear();
        record.Coroutines.Clear();
        record.IsEnabled = false;
        ButtonHandler.Instance?.UpdateButtons();
    }

    private static void DestroyLatePluginComponents(PluginRecord record)
    {
        foreach (Component component in Resources.FindObjectsOfTypeAll<Component>())
        {
            if (component == null || component.GetType().Assembly != record.Assembly)
                continue;

            if (typeof(BaseUnityPlugin).IsAssignableFrom(component.GetType()))
                continue;

            Destroy(component);
        }
    }

    private void DiscoverMods(PluginRecord record)
    {
        record.ModDescriptors.Clear();

        foreach (Type type in GetLoadableTypes(record.Assembly))
        {
            if (!typeof(hamburburmod).IsAssignableFrom(type) || type.IsAbstract)
                continue;

            HamburburPluginModAttribute pluginMod = type.GetCustomAttribute<HamburburPluginModAttribute>();
            hamburburmodAttribute mod = type.GetCustomAttribute<hamburburmodAttribute>();
            if (pluginMod == null || mod == null)
                continue;

            record.ModDescriptors.Add(new PluginModDescriptor(type, pluginMod.Category, mod.Name, mod.Description));
        }
    }

    private void RebuildManagementUi()
    {
        if (ButtonHandler.Instance == null)
            return;

        string currentCategory = MenuHandler.Instance?.Category;

        ClearManagementCategory(PluginListCategory);
        foreach (string category in managementCategories.ToArray())
            ClearManagementCategory(category);
        managementCategories.Clear();

        foreach (PluginRecord record in records.OrderBy(record => record.Name, StringComparer.OrdinalIgnoreCase))
            ButtonHandler.AddButton(PluginListCategory, new PluginListEntry(record),
                    register: false, loadSavedData: false);

        if (currentCategory != null && currentCategory.StartsWith(DetailCategoryPrefix, StringComparison.Ordinal))
        {
            PluginRecord record = records.FirstOrDefault(item => GetDetailCategory(item.Id) == currentCategory);
            if (record != null && !record.IsLegacy)
                BuildDetailCategory(record, currentCategory);
        }

        ButtonHandler.Instance.UpdateButtons();
    }

    private void BuildDetailCategory(PluginRecord record, string category)
    {
        ClearManagementCategory(category);
        managementCategories.Add(category);

        if (record.IsEnabled)
        {
            ButtonHandler.AddButton(category,
                    new PluginActionEntry("Reload Plugin", "Unload and load this plugin again",
                            () => ReloadPlugin(record.Id))
                    {
                            ConfigKey = $"{record.Name}_Reload Plugin_{record.Id}",
                    }, register: false, loadSavedData: false);
            ButtonHandler.AddButton(category,
                    new PluginActionEntry("Disable Plugin", "Unload this plugin and hide all of its mod buttons",
                            () => DisablePlugin(record.Id))
                    {
                            ConfigKey = $"{record.Name}_Disable Plugin_{record.Id}",
                    }, register: false, loadSavedData: false);
        }
        else
        {
            ButtonHandler.AddButton(category,
                    new PluginActionEntry("Enable Plugin", "Load this plugin and restore its visible mod buttons",
                            () => EnablePlugin(record.Id))
                    {
                            ConfigKey = $"{record.Name}_Enable Plugin_{record.Id}",
                    }, register: false, loadSavedData: false);
            ButtonHandler.AddButton(category,
                    new PluginActionEntry("Reload Plugin", "Reload the plugin assembly from disk",
                            () => ReloadPlugin(record.Id))
                    {
                            ConfigKey = $"{record.Name}_Reload Plugin_{record.Id}",
                    }, register: false, loadSavedData: false);
        }

        foreach (PluginModDescriptor mod in record.ModDescriptors)
            ButtonHandler.AddButton(category,
                    new PluginModVisibilityEntry(record.Id, mod,
                            IsModVisible(record.Id, mod.TypeName))
                    {
                            ConfigKey = $"{record.Name}_{mod.Name}_{record.Id}_{mod.TypeName}_Visibility",
                    }, register: false, loadSavedData: false);
    }

    private static void ClearManagementCategory(string category)
    {
        if (!Buttons.Categories.TryGetValue(category, out (Type, hamburburmod)[] entries))
        {
            Buttons.Categories[category] = [];
            return;
        }

        foreach ((Type _, hamburburmod entry) in entries)
        {
            if (entry == null)
                continue;

            ModRuntime.Unregister(entry);
            entry.AssociatedGUIButton?.Obliterate();
        }

        Buttons.Categories[category] = [];
    }

    private PluginRecord FindRecord(string id) => records.FirstOrDefault(record => record.Id == id);

    private static string GetDetailCategory(string id) => DetailCategoryPrefix + id;
    private static string GetTypeName(Type type) => type.FullName ?? type.Name;
    private static string GetModStateKey(string pluginId, string typeName) => pluginId + "::" + typeName;

    private void LoadState()
    {
        string path = Path.Combine(PluginDirectory, StateFileName);
        if (!File.Exists(path))
            return;

        try
        {
            state = JsonConvert.DeserializeObject<PluginState>(File.ReadAllText(path)) ?? new PluginState();
            state.DisabledPlugins ??= [];
            state.HiddenMods      ??= [];
        }
        catch (Exception exception)
        {
            Debug.LogError($"[hamburbur Plugins] Could not read {StateFileName}: {exception}");
            state = new PluginState();
        }
    }

    private void SaveState()
    {
        try
        {
            Directory.CreateDirectory(PluginDirectory);
            File.WriteAllText(Path.Combine(PluginDirectory, StateFileName),
                    JsonConvert.SerializeObject(state, Formatting.Indented));
        }
        catch (Exception exception)
        {
            Debug.LogError($"[hamburbur Plugins] Could not save {StateFileName}: {exception}");
        }
    }

    private Assembly ResolvePluginDependency(object sender, ResolveEventArgs args)
    {
        if (!Directory.Exists(PluginDirectory))
            return null;

        string name = new AssemblyName(args.Name).Name;
        Assembly loaded = AppDomain.CurrentDomain.GetAssemblies()
                                   .FirstOrDefault(assembly => assembly.GetName().Name == name);
        if (loaded != null)
            return loaded;

        if (dependencyAssemblies.TryGetValue(name, out Assembly cached))
            return cached;

        string dependency = Directory.GetFiles(PluginDirectory, name + ".dll", SearchOption.AllDirectories)
                                     .FirstOrDefault();
        if (dependency == null)
            return null;

        Assembly assembly = LoadAssemblyWithoutLock(dependency);
        dependencyAssemblies[name] = assembly;
        return assembly;
    }

    private static Assembly LoadAssemblyWithoutLock(string path) => Assembly.Load(File.ReadAllBytes(path));

    private static string GetAssemblyLocation(Assembly assembly)
    {
        try
        {
            return string.IsNullOrEmpty(assembly.Location) ? null : assembly.Location;
        }
        catch
        {
            return null;
        }
    }

    private static Type[] GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type != null).ToArray();
        }
        catch
        {
            return [];
        }
    }

    private static bool IsPotentiallyLegacyPlugin(IEnumerable<Type> types) =>
            types.Any(type => typeof(hamburburmod).IsAssignableFrom(type) &&
                              type != typeof(hamburburmod) &&
                              type.GetCustomAttributes(false)
                                  .Any(attribute => attribute.GetType().Name.Contains("hamburburPluginAttribute",
                                          StringComparison.OrdinalIgnoreCase)));

    private static PluginRecord CreateLegacyRecord(Assembly assembly, Type[] types, string sourcePath)
    {
        BepInPlugin metadata = types.Select(type => type.GetCustomAttribute<BepInPlugin>())
                                    .FirstOrDefault(attribute => attribute != null);
        string name = metadata?.Name ?? assembly.GetName().Name;
        string id = metadata?.GUID ?? assembly.FullName;
        string version = metadata?.Version?.ToString() ?? assembly.GetName().Version?.ToString() ?? "Unknown";
        return PluginRecord.Legacy(assembly, sourcePath, id, name, version);
    }

    private static void ApplyMetadata(PluginRecord record, HamburburPluginAttribute metadata)
    {
        record.Name        = metadata.Name;
        record.Version     = metadata.Version;
        record.Author      = metadata.Author;
        record.Description = metadata.Description;
    }

    private sealed class PluginState
    {
        public HashSet<string> DisabledPlugins { get; set; } = [];
        public HashSet<string> HiddenMods      { get; set; } = [];
    }
}

internal sealed class PluginRuntimeHost : MonoBehaviour { }

internal sealed class PluginRecord
{
    internal PluginRecord(Assembly assembly, Type entryType, string sourcePath, HamburburPluginAttribute metadata)
    {
        Assembly    = assembly;
        EntryType   = entryType;
        SourcePath  = sourcePath;
        Id          = metadata.Id;
        Name        = metadata.Name;
        Version     = metadata.Version;
        Author      = metadata.Author;
        Description = metadata.Description;
        Descriptor  = new PluginDescriptor(this);
    }

    private PluginRecord(Assembly assembly, string sourcePath, string id, string name, string version)
    {
        Assembly    = assembly;
        SourcePath  = sourcePath;
        Id          = id;
        Name        = name;
        Version     = version;
        IsLegacy    = true;
        Author      = "Unknown";
        Description = "Legacy hamburbur plugin (read-only)";
        Descriptor  = new PluginDescriptor(this);
    }

    internal static PluginRecord Legacy(Assembly assembly, string sourcePath, string id, string name, string version) =>
            new(assembly, sourcePath, id, name, version);

    internal Assembly Assembly;
    internal Type EntryType;
    internal readonly string Id;
    internal string Name;
    internal string Version;
    internal string Author;
    internal string Description;
    internal readonly string SourcePath;
    internal readonly bool IsLegacy;
    internal bool IsEnabled;
    internal string LastError;
    internal IHamburburPlugin Entry;
    internal PluginContext Context;
    internal GameObject RuntimeObject;
    internal PluginRuntimeHost RuntimeHost;
    internal readonly PluginDescriptor Descriptor;
    internal readonly List<PluginModDescriptor> ModDescriptors = [];
    internal readonly List<OwnedPluginMod> OwnedMods = [];
    internal readonly Dictionary<string, ModSaveInfo> RuntimeModStates = [];
    internal readonly List<Action> CleanupActions = [];
    internal readonly List<IDisposable> Disposables = [];
    internal readonly List<UnityEngine.Object> UnityObjects = [];
    internal readonly List<Coroutine> Coroutines = [];
}

internal sealed class OwnedPluginMod(Type type, string category, hamburburmod instance)
{
    internal Type Type { get; } = type;
    internal string Category { get; } = category;
    internal hamburburmod Instance { get; } = instance;
}

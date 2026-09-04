using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using hamburbur.Mod_Backend;
using UnityEngine;
using Object = UnityEngine.Object;

namespace hamburbur.Plugins;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class HamburburPluginAttribute(string id, string name, string version) : Attribute
{
    public string Id          { get; }      = id;
    public string Name        { get; }      = name;
    public string Version     { get; }      = version;
    public string Author      { get; set; } = "Unknown";
    public string Description { get; set; } = string.Empty;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class HamburburPluginModAttribute(string category) : Attribute
{
    public string Category { get; } = category;
}

public interface IHamburburPlugin
{
    void Load(PluginContext context);
    void Unload();
}

public sealed class PluginContext
{
    private readonly PluginRecord record;

    internal PluginContext(PluginRecord record) => this.record = record;

    public string Id   => record.Id;
    public string Name => record.Name;
    public string PluginDirectory => record.SourcePath == null
                                             ? PluginManager.Instance.PluginDirectory
                                             : Path.GetDirectoryName(record.SourcePath);

    public hamburburmod RegisterMod<T>(string category) where T : hamburburmod, new() =>
            RegisterMod(typeof(T), category);

    public hamburburmod RegisterMod(Type modType, string category) =>
            PluginManager.Instance.RegisterMod(record, modType, category);

    public T AddComponent<T>(GameObject target = null) where T : Component
    {
        GameObject owner     = target == null ? record.RuntimeObject : target;
        T          component = owner.AddComponent<T>();
        TrackUnityObject(component);

        return component;
    }

    public Component AddComponent(Type componentType, GameObject target = null)
    {
        if (componentType == null || !typeof(Component).IsAssignableFrom(componentType))
            throw new ArgumentException("The type must derive from UnityEngine.Component.", nameof(componentType));

        GameObject owner     = target == null ? record.RuntimeObject : target;
        Component  component = owner.AddComponent(componentType);
        TrackUnityObject(component);

        return component;
    }

    public GameObject CreateGameObject(string name)
    {
        GameObject gameObject = new(name);
        TrackUnityObject(gameObject);

        return gameObject;
    }

    public Coroutine StartCoroutine(IEnumerator routine)
    {
        if (routine == null)
            throw new ArgumentNullException(nameof(routine));

        Coroutine coroutine = record.RuntimeHost.StartCoroutine(routine);
        record.Coroutines.Add(coroutine);

        return coroutine;
    }

    public T TrackDisposable<T>(T resource) where T : IDisposable
    {
        if (resource != null)
            record.Disposables.Add(resource);

        return resource;
    }

    public T TrackUnityObject<T>(T unityObject) where T : Object
    {
        if (unityObject != null)
            record.UnityObjects.Add(unityObject);

        return unityObject;
    }

    public void OnUnload(Action cleanup)
    {
        if (cleanup != null)
            record.CleanupActions.Add(cleanup);
    }
}

public sealed class PluginDescriptor
{
    internal PluginDescriptor(PluginRecord record) => Record = record;

    private PluginRecord Record { get; }

    public string Id          => Record.Id;
    public string Name        => Record.Name;
    public string Version     => Record.Version;
    public string Author      => Record.Author;
    public string Description => Record.Description;
    public bool   IsLegacy    => Record.IsLegacy;
    public bool   IsEnabled   => Record.IsEnabled;
    public string LastError   => Record.LastError;

    public IReadOnlyList<PluginModDescriptor> Mods => Record.ModDescriptors;
}

public sealed class PluginModDescriptor
{
    internal PluginModDescriptor(Type type, string category, string name, string description)
    {
        Type        = type;
        Category    = category;
        Name        = name;
        Description = description;
    }

    internal Type Type { get; }

    public string TypeName    => Type.FullName ?? Type.Name;
    public string Category    { get; }
    public string Name        { get; }
    public string Description { get; }
}
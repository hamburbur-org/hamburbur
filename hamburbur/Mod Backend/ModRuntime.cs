using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace hamburbur.Mod_Backend;

[Flags]
public enum ModTickPhase
{
    None        = 0,
    Update      = 1,
    LateUpdate  = 2,
    FixedUpdate = 4,
    OnGUI       = 8,
}

public static class ModRuntime
{
    private static readonly Dictionary<Type, ModTickPhase> tickPhaseCache = new();

    private static readonly List<hamburburmod> updateMods      = [];
    private static readonly List<hamburburmod> lateUpdateMods  = [];
    private static readonly List<hamburburmod> fixedUpdateMods = [];
    private static readonly List<hamburburmod> guiMods         = [];

    private static readonly HashSet<hamburburmod> updateSet      = [];
    private static readonly HashSet<hamburburmod> lateUpdateSet  = [];
    private static readonly HashSet<hamburburmod> fixedUpdateSet = [];
    private static readonly HashSet<hamburburmod> guiSet         = [];

    private static readonly List<hamburburmod> tickBuffer = [];

    public static ModTickPhase GetTickPhases(Type type)
    {
        if (tickPhaseCache.TryGetValue(type, out ModTickPhase cached))
            return cached;

        ModTickPhase phases = ModTickPhase.None;

        if (Overrides(type, "Update"))
            phases |= ModTickPhase.Update;

        if (Overrides(type, "LateUpdate"))
            phases |= ModTickPhase.LateUpdate;

        if (Overrides(type, "FixedUpdate"))
            phases |= ModTickPhase.FixedUpdate;

        if (Overrides(type, "OnGUI"))
            phases |= ModTickPhase.OnGUI;

        tickPhaseCache[type] = phases;

        return phases;
    }

    public static void Register(hamburburmod mod)
    {
        ModTickPhase phases = mod.TickPhases;

        if ((phases & ModTickPhase.Update) != 0)
            Add(updateMods, updateSet, mod);

        if ((phases & ModTickPhase.LateUpdate) != 0)
            Add(lateUpdateMods, lateUpdateSet, mod);

        if ((phases & ModTickPhase.FixedUpdate) != 0)
            Add(fixedUpdateMods, fixedUpdateSet, mod);

        if ((phases & ModTickPhase.OnGUI) != 0)
            Add(guiMods, guiSet, mod);
    }

    public static void Unregister(hamburburmod mod)
    {
        Remove(updateMods,      updateSet,      mod);
        Remove(lateUpdateMods,  lateUpdateSet,  mod);
        Remove(fixedUpdateMods, fixedUpdateSet, mod);
        Remove(guiMods,         guiSet,         mod);
    }

    public static void Update()      => Run(updateMods,      mod => mod.InvokeUpdate());
    public static void LateUpdate()  => Run(lateUpdateMods,  mod => mod.InvokeLateUpdate());
    public static void FixedUpdate() => Run(fixedUpdateMods, mod => mod.InvokeFixedUpdate());
    public static void OnGUI()       => Run(guiMods,         mod => mod.InvokeOnGUI());

    private static bool Overrides(Type type, string methodName)
    {
        MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        return method != null && method.DeclaringType != typeof(hamburburmod);
    }

    private static void Add(List<hamburburmod> list, HashSet<hamburburmod> set, hamburburmod mod)
    {
        if (set.Add(mod))
            list.Add(mod);
    }

    private static void Remove(List<hamburburmod> list, HashSet<hamburburmod> set, hamburburmod mod)
    {
        if (!set.Remove(mod))
            return;

        list.Remove(mod);
    }

    private static void Run(List<hamburburmod> mods, Action<hamburburmod> callback)
    {
        tickBuffer.Clear();
        tickBuffer.AddRange(mods);

        foreach (hamburburmod mod in tickBuffer.Where(mod => mod is { Enabled: true, }))
            try
            {
                callback(mod);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[hamburbur] {mod.ModName} tick failed: {ex}");
            }

        tickBuffer.Clear();
    }
}
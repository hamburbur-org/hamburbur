using System;
using System.Collections.Generic;
using System.Linq;

namespace hamburbur.Mod_Backend;

public static class ModRegistry
{
    private static readonly Dictionary<Type, hamburburmod> modsByType = new();

    public static IEnumerable<(Type Type, hamburburmod Mod)> All =>
            modsByType.Select(pair => (pair.Key, pair.Value));

    public static void Clear()
    {
        foreach (hamburburmod mod in modsByType.Values)
            ModRuntime.Unregister(mod);

        modsByType.Clear();
    }

    public static void Register(Type type, hamburburmod mod)
    {
        if (type == null || mod == null)
            return;

        modsByType[type] = mod;
    }

    public static void Unregister(Type type)
    {
        if (type == null)
            return;

        if (modsByType.TryGetValue(type, out hamburburmod mod))
            ModRuntime.Unregister(mod);

        modsByType.Remove(type);
    }

    public static bool TryGet(Type type, out hamburburmod mod) =>
            modsByType.TryGetValue(type, out mod);

    public static bool TryGet<T>(out T mod) where T : hamburburmod
    {
        if (modsByType.TryGetValue(typeof(T), out hamburburmod found) && found is T typed)
        {
            mod = typed;

            return true;
        }

        mod = null;

        return false;
    }
}
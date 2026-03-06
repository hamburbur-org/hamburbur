using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using hamburbur.Managers;
using hamburbur.Misc;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace hamburbur.Tools;

public static class Extensions
{
    public static SuperSigmaType GetOrAddComponent<SuperSigmaType>(this GameObject obj)
            where SuperSigmaType : Component =>
            obj.GetComponent<SuperSigmaType>() ?? obj.AddComponent<SuperSigmaType>();

    public static SuperSigmaType GetOrAddComponent<SuperSigmaType>(this Component comp)
            where SuperSigmaType : Component =>
            comp.GetComponent<SuperSigmaType>() ?? comp.gameObject.AddComponent<SuperSigmaType>();

    public static Component GetOrAddComponent(this GameObject obj, Type type) =>
            obj.GetComponent(type) ?? obj.AddComponent(type);

    public static Component GetOrAddComponent(this Component comp, Type type) =>
            comp.GetComponent(type) ?? comp.gameObject.AddComponent(type);

    public static void Obliterate(this GameObject obj)  => Object.Destroy(obj);
    public static void Obliterate(this Component  comp) => Object.Destroy(comp);

    public static void Obliterate(this GameObject obj,  float delay) => Object.Destroy(obj,  delay);
    public static void Obliterate(this Component  comp, float delay) => Object.Destroy(comp, delay);

    public static Transform TakeChild(this Transform transform, params int[] childPath)
    {
        Transform child = transform.GetChild(childPath[0]);
        for (int i = 1; i < childPath.Length; i++)
            child = child.GetChild(childPath[i]);

        return child;
    }

    public static VRRig Rig(this int actorNumber) =>
            VRRigCache.m_activeRigs.Find(r => r.Creator.ActorNumber == actorNumber);

    public static VRRig Rig(this NetPlayer netPlayer) => VRRigCache.rigsInUse[netPlayer].Rig;

    public static VRRig Rig(this Player player) =>
            VRRigCache.m_activeRigs.Find(r => r.Creator.GetPlayerRef().Equals(player));

    public static VRRig Rig(this string id) => VRRigCache.m_activeRigs.Find(r => r.Creator?.UserId == id);

    public static NetPlayer OwningNetPlayer(this VRRig rig) => rig?.Creator;
    public static Vector3   Velocity(this VRRig rig) => RigUtils.RigVelocities.GetValueOrDefault(rig, Vector3.zero);
    public static int       Ping(this VRRig rig) => PingLogger.PlayerPing.GetValueOrDefault(rig, 0);
    public static bool      IsTagged(this VRRig rig) => TagManager.Instance.TaggedRigs.Contains(rig);

    /// <summary>
    ///     Removes rich text size tags from strings, useful if your receiving a string for networking and displaying them
    ///     somewhere that has rich text.
    /// </summary>
    public static string NormaliseString(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        text = Regex.Replace(text, @"<size\s*=\s*[^>]+>", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</size>",            "", RegexOptions.IgnoreCase);

        return text;
    }

    public static string WithoutRichText(this string text)
    {
        if (text.IsNullOrEmpty())
            return text;

        Regex regex = new(@"<.*?>");

        return regex.Replace(text, "");
    }

    public static bool IsCurrentlyAccessible(this AccessSetting accessSetting)
    {
        MethodInfo[] methods = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract)
                                       .SelectMany(c => c.GetMethods(
                                                           BindingFlags.Static | BindingFlags.Public |
                                                           BindingFlags.NonPublic))
                                       .Select(m => new
                                        {
                                                Method = m,
                                                Attribute = m.GetCustomAttribute<AccessSettingsAllowedCheckAttribute>(),
                                        }).Where(x => x.Attribute != null && x.Method.ReturnType == typeof(bool) &&
                                                      x.Method.GetParameters().Length == 0 &&
                                                      x.Attribute.AccessSetting == accessSetting)
                                       .Select(x => x.Method).ToArray();

        return methods.Select(method => (bool)method.Invoke(null, null)!).All(result => result);
    }

    public static void RecursivelySetLayer(this GameObject obj, UnityLayer layer)
    {
        foreach (Transform child in obj.transform)
            child.gameObject.SetLayerRecursively(layer);

        obj.SetLayer(layer);
    }

    public static Transform[] Children(this Transform transform) => transform.GetComponentsInChildren<Transform>(true)
                                                                             .Where(t => t.parent == transform)
                                                                             .ToArray();
}
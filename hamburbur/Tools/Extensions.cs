using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GorillaExtensions;
using hamburbur.GUI;
using hamburbur.Managers;
using hamburbur.Misc;
using hamburbur.Mod_Backend;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable ConvertToExtensionBlock

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

    public static Transform CopyTransform(this Transform transform, Transform source)
    {
        transform.position = source.position;
        transform.rotation = source.rotation;
        transform.localScale = source.localScale;
        
        return transform;
    }

    public static Transform TakeChild(this Transform transform, params int[] childPath)
    {
        Transform child = transform.GetChild(childPath[0]);
        for (int i = 1; i < childPath.Length; i++)
            child = child.GetChild(childPath[i]);

        return child;
    }

    public static VRRig Rig(this int actorNumber)
    {
        if (VRRigCache.m_activeRigs == null)
            return null;

        return VRRigCache.m_activeRigs.Find(rig =>
                                                    rig                     != null &&
                                                    rig.Creator             != null &&
                                                    rig.Creator.ActorNumber == actorNumber);
    }

    public static VRRig Rig(this NetPlayer netPlayer) => netPlayer?.ActorNumber.Rig();

    public static VRRig Rig(this Player player) => player?.ActorNumber.Rig();

    public static VRRig Rig(this string id)
    {
        if (string.IsNullOrEmpty(id) || VRRigCache.m_activeRigs == null)
            return null;

        return VRRigCache.m_activeRigs.Find(rig =>
                                                    rig                 != null &&
                                                    rig.Creator?.UserId == id);
    }

    public static IEnumerable<VRRig> Rigs(
            this NetworkSystem networkSystem,
            bool               includeLocal = false)
    {
        if (networkSystem == null || VRRigCache.m_activeRigs == null)
            yield break;

        foreach (VRRig rig in VRRigCache.m_activeRigs.Where(rig => rig != null &&
                                                                   rig.gameObject.activeInHierarchy)
                                        .Where(rig => includeLocal || !rig.IsLocalRig()))
            yield return rig;
    }

    public static bool IsLocalRig(this VRRig rig) =>
            rig != null && rig.isLocal;

    public static NetPlayer GetNetPlayer(this VRRig rig) => rig?.Creator;

    public static Player GetPhotonPlayer(this VRRig rig) => rig?.Creator?.GetPlayerRef();

    public static void Serialize(this PhotonView view, RaiseEventOptions options = null, int offset = 0) =>
            Utils.SendSerialize(view, options, offset);

    public static bool IsOnSteam(this VRRig Player)
    {
        string concat           = Player._playerOwnedCosmetics.Concat();
        int    customPropsCount = Player.Creator.GetPlayerRef().CustomProperties.Count;

        return concat.Contains("S. FIRST LOGIN") || concat.Contains("FIRST LOGIN") || customPropsCount >= 2;
    }

    public static NetPlayer OwningNetPlayer(this VRRig rig) => rig?.Creator;
    public static Vector3   Velocity(this VRRig rig) => RigUtils.RigVelocities.GetValueOrDefault(rig, Vector3.zero);
    public static int       Ping(this VRRig rig) => PingLogger.PlayerPing.GetValueOrDefault(rig, 0);
    public static bool      IsTagged(this VRRig rig) => TagManager.Instance.TaggedRigs.Contains(rig);

    public static Task<string> GetCreationDate(this VRRig rig)
    {
        string userId = rig.Creator.UserId;

        TaskCompletionSource<string> tcs = new();

        PlayFabClientAPI.GetAccountInfo(
                new GetAccountInfoRequest { PlayFabId = userId, },
                result =>
                {
                    string date = result.AccountInfo.Created.ToString("MMM dd, yyyy").ToUpper();
                    tcs.SetResult(date);
                },
                _ => { tcs.SetResult("ERROR"); });

        return tcs.Task;
    }

    public static Dictionary<string, object> GetCustomProperties(this NetPlayer player)
    {
        Dictionary<string, object> properties = new();

        foreach (DictionaryEntry property in player.GetPlayerRef().CustomProperties)
        {
            if (property.Key is not string key)
                continue;

            properties[key] = property.Value;
        }

        return properties;
    }

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
        if (string.IsNullOrEmpty(text))
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

    public static void RecursiveInvoke(Action method, int amount = 10)
    {
        for (int i = 0; i < amount; i++)
            method();
    }

    public static void SetButtonRendererActive(this GameObject gameObject, bool enabled)
    {
        if (!MenuHandler.Instance.IsCanvasMenu)
            gameObject.GetComponent<Renderer>().enabled = enabled;
        else
            gameObject.transform.Find("Background").gameObject.SetActive(enabled);
    }

    public static bool GetButtonRendererActive(this GameObject gameObject)
    {
        if (!MenuHandler.Instance.IsCanvasMenu)
            return gameObject.GetComponent<Renderer>().enabled;

        Transform background = gameObject.transform.Find("Background");

        if (background != null)
            return background.gameObject.activeSelf;

        return false;
    }

    public static Transform[] Children(this Transform transform) => transform.GetComponentsInChildren<Transform>(true)
                                                                             .Where(t => t.parent == transform)
                                                                             .ToArray();
}
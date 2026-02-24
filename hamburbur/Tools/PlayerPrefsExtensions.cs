using UnityEngine;

namespace hamburbur.Tools;

public static class PlayerPrefsExtensions
{
    public static void SetBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);
    public static bool GetBool(string key)                    => PlayerPrefs.GetInt(key)                       == 1;
    public static bool GetBool(string key, bool defaultValue) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
}
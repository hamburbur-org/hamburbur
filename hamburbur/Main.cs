using BepInEx;
using UnityEngine;

namespace hamburbur;

[BepInDependency("hansolo1000falcon.gorillanotifications")]
[BepInPlugin(Constants.PluginGuid, Constants.PluginName, Constants.PluginVersion)]
public class Main : BaseUnityPlugin
{
    private void Awake() => LoadNormally();

    public static void Inject()
    {
        GameObject hamburburMenu = new("hamburburMenu");
        hamburburMenu.AddComponent<Plugin>();
    }

    private void LoadNormally()
    {
        GameObject hamburburMenu = new("hamburburMenu");
        hamburburMenu.AddComponent<Plugin>();
        DontDestroyOnLoad(hamburburMenu);
    }
}
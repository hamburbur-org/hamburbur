using System.Collections;
using System.Collections.Generic;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("<color=purple>The End...</color>", "t̴͕͠h̴̠̀ë̶̠ ë̶̠n̴̬̐d̵̶̰͚͋̿", ButtonType.Togglable, AccessSetting.SuperAdminOnly, EnabledType.AlwaysDisabled, 0)]
public class TheEnd : hamburburmod
{
    private Dictionary<string, int> assetIds = new();
    
    protected override void OnEnable() => CoroutineManager.Instance.StartCoroutine(TheEndRoutine());

    private IEnumerator TheEndRoutine()
    {
        assetIds["AudioPlayer"]   = Components.Console.GetFreeAssetID();
        assetIds["VisitorRocket"] = Components.Console.GetFreeAssetID();
    
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "theend", "TheEndAudioPlayer", assetIds["AudioPlayer"]);
        Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "theend", "VisitorRocket", assetIds["VisitorRocket"]);
    
        Vector3 startPos = new(-57.1f, 22f, -37f);
        Vector3 endPos   = startPos + new Vector3(0f, 120f, 0f);

        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetIds["VisitorRocket"], startPos);
        
        Components.Console.ExecuteCommand("shake", ReceiverGroup.All, 0.1f, 20f, false);

        yield return new WaitForSeconds(5f);

        const float Duration1 = 28f;
        float       elapsed1  = 0f;
        
        while (elapsed1 < Duration1)
        {
            float   t          = elapsed1 / Duration1;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);

            Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetIds["VisitorRocket"], currentPos);

            elapsed1 += Time.deltaTime;
            yield return null;
        }

        Components.Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetIds["VisitorRocket"], endPos);
        Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetIds["VisitorRocket"]);
        assetIds.Remove("VisitorRocket");
    }

    protected override void OnDisable()
    {
        foreach (int assetId in assetIds.Values)
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
        
        assetIds.Clear();
    }
}
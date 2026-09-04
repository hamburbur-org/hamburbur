using System.Collections;
using System.Linq;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod("Complete Quests", "Attempts to auto complete quests", ButtonType.Fixed, AccessSetting.BetaBuildOnly, EnabledType.Disabled, 0)]
public class CompleteQuests : hamburburmod
{
    private Coroutine completionRoutine;

    protected override void Pressed()
    {
        if (completionRoutine != null || CoroutineManager.Instance == null)
            return;

        completionRoutine = CoroutineManager.Instance.StartCoroutine(CompleteActiveQuests());
    }

    private IEnumerator CompleteActiveQuests()
    {
        yield return null;

        RotatingQuestsManager manager = Object.FindFirstObjectByType<RotatingQuestsManager>();
        if (manager?.quests == null)
        {
            completionRoutine = null;
            yield break;
        }

        RotatingQuest[] activeQuests = manager.quests.DailyQuests
                                                     .Concat(manager.quests.WeeklyQuests)
                                                     .SelectMany(group => group.quests)
                                                     .Where(quest => quest.isQuestActive &&
                                                                     !quest.isQuestComplete)
                                                     .ToArray();

        foreach (RotatingQuest quest in activeQuests)
        {
            quest.RemoveEventListener();
            quest.ApplySavedProgress(quest.requiredOccurenceCount);
            quest.lastChange = Time.frameCount;
            manager.HandleQuestCompleted(quest.questID);

            yield return null;
        }

        if (activeQuests.Length > 0)
            manager.HandleQuestProgressChanged(false);

        if (ProgressionController.GetProgressionData().unclaimed > 0)
            ProgressionController.RedeemProgress();

        completionRoutine = null;
    }
}

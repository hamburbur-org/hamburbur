using System.Linq;
using hamburbur.Mod_Backend;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod("Complete Quests", "Attempts to auto complete quests", ButtonType.Fixed, AccessSetting.BetaBuildOnly, EnabledType.Disabled, 0)]
public class CompleteQuests : hamburburmod
{
    protected override void Pressed()
    {
        RotatingQuestsManager manager = Object.FindFirstObjectByType<RotatingQuestsManager>();
        ProgressionController controller = Object.FindFirstObjectByType<ProgressionController>();
        
        foreach (RotatingQuest quest in manager.quests.DailyQuests.SelectMany(group => group.quests))
        {
            Debug.Log(quest.questName);
            manager.HandleQuestCompleted(quest.questID);
            controller.OnQuestComplete(quest.questID, true);
        }  
        
        foreach (RotatingQuest quest in manager.quests.WeeklyQuests.SelectMany(group => group.quests))
        {
            Debug.Log(quest.questName);
            manager.HandleQuestCompleted(quest.questID);
            controller.OnQuestComplete(quest.questID, false);
        }            

        MonkeBusinessStation business = Object.FindFirstObjectByType<MonkeBusinessStation>();
        business.UpdateCountdownTimers();
        business.UpdateProgressDisplays();
        business.UpdateQuestStatus();
        business.RedeemProgress();
    }
}
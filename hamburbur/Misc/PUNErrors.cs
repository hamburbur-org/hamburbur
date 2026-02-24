using System;
using System.Globalization;
using System.Linq;
using hamburbur.Managers;
using Photon.Pun;
using Photon.Realtime;

namespace hamburbur.Misc;

public class PUNErrors : MonoBehaviourPunCallbacks
{
    public static Action OnPunError;

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        switch (returnCode)
        {
            case ErrorCode.GameFull:
                NotificationManager.SendNotification(
                        "<color=red>Error</color>",
                        "Room Join failure, that room is full.",
                        8f,
                        true,
                        true);

                OnPunError?.Invoke();

                break;
        }
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        NotificationManager.SendNotification(
                "<color=red>Error</color>",
                "Photon PUN failure, Custom Auth failed",
                8f,
                true,
                true);

        OnPunError?.Invoke();
    }

    public override void OnErrorInfo(ErrorInfo errorInfo)
    {
        NotificationManager.SendNotification(
                "<color=red>Error</color>",
                "Photon PUN failure, " + errorInfo.Info,
                8f,
                true,
                true);

        OnPunError?.Invoke();
    }

    public void OnQuestCompleted(RotatingQuest quest) =>
            NotificationManager.SendNotification(
                    "<color=green>Quest</color>",
                    "You completed a Quest. " + quest.questName,
                    8f,
                    true,
                    false);

    public void OnMothershipMessageRecieved(string title, string body)
    {
        string[] array;

        switch (title)
        {
            case "Warning":
                array = body.Split('|');

                if (array.Length != 2) break;

                string   warnCategory = array[0];
                string[] warnReasons  = array[1].Split(',');

                if (warnReasons.Length == 0) break;

                string warnReasonString = string.Join(", ", warnReasons.Select(reason =>
                                                                                       CultureInfo.InvariantCulture
                                                                                              .TextInfo.ToTitleCase(
                                                                                                       reason.Replace(
                                                                                                               '_',
                                                                                                               ' '))));

                NotificationManager.SendNotification(
                        "<color=red>Warning</color>",
                        $"{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(warnCategory)} warning received{warnReasonString}",
                        8f,
                        true,
                        true);

                break;

            case "Mute":
                array = body.Split('|');

                if (array.Length != 3 || !array[0].Equals("voice", StringComparison.OrdinalIgnoreCase)) break;

                if (array[2].Length > 0 &&
                    int.TryParse(array[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int muteDuration))
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(muteDuration);
                    string durationText = timeSpan.TotalHours >= 1f
                                                  ? $"{timeSpan.TotalHours:F1} hour"
                                                  : $"{timeSpan.TotalMinutes:F0} minute";

                    NotificationManager.SendNotification(
                            "<color=red>Mute</color>",
                            $"Voice mute sanction, {durationText} mute",
                            8f,
                            true,
                            true);
                }
                else
                {
                    NotificationManager.SendNotification(
                            "<color=red>Mute</color>",
                            "You have been indefinitely voice muted.",
                            8f,
                            true,
                            true);
                }

                break;
        }
    }
}
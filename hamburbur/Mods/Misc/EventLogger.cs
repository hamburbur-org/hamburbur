using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using Photon.Pun;
using UnityEngine;

namespace hamburbur.Mods.Misc;

[hamburburmod("Event Logger", "Logs all events", ButtonType.Togglable, AccessSetting.Public, EnabledType.Disabled, 0)]
public class EventLogger : hamburburmod
{
    private const           float  Cooldown             = 1.5f;
    private static readonly byte[] ProhibitedEventCodes = [0, 1, 2, 3, 4, 5, 8, 9, 10, 50, 51, 176, 199, 189,];

    private static readonly Dictionary<(int sender, byte code), float> RecentEvents = [];

    private string currentEventTxtFileDir;

    private bool isEnabled;

    protected override void Start()
    {
        currentEventTxtFileDir                       =  FileManager.Instance.CreateEventLoggerFile();
        PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
    }

    protected override void OnEnable()  => isEnabled = true;
    protected override void OnDisable() => isEnabled = false;

    private void OnEventReceived(EventData eventData)
    {
        if (ProhibitedEventCodes.Contains(eventData.Code) || eventData.Code >= 200)
            return;

        (int, byte) key = (eventData.Sender, eventData.Code);
        float       t   = Time.time;

        string senderName =
                GorillaParent.instance.vrrigs
                             .FirstOrDefault(rig => rig.OwningNetPlayer().ActorNumber == eventData.Sender)
                            ?.OwningNetPlayer().SanitizedNickName ?? "UNKNOWN";

        object raw = eventData.CustomData;

        string data = FormatValue(raw);

        File.AppendAllText(currentEventTxtFileDir, $"\n\nReceived event {eventData.Code} from {senderName}\n{data}");

        if (RecentEvents.TryGetValue(key, out float recent) && t - recent < Cooldown || !isEnabled)
            return;

        RecentEvents[key] = t;

        NotificationManager.SendNotification(
                "<color=#00ff99>Event Logger</color>",
                $"Received event {eventData.Code} from {senderName}\n{data}",
                5f,
                false,
                false);
    }

    public static string FormatValue(object value, int depth = 0)
    {
        if (value == null)
            return "null";

        if (depth > 6)
            return "...";

        switch (value)
        {
            case string stringValue:
                return stringValue;

            case IDictionary dictionary:
            {
                StringBuilder builder = new();
                builder.Append("{\n");

                bool first = true;
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (!first)
                        builder.Append(",\n");

                    string spaces = "  ";
                    for (int i = 0; i < depth; i++)
                        spaces += "  ";

                    builder.Append(spaces + FormatValue(entry.Key, depth + 1));
                    builder.Append(": ");
                    builder.Append(FormatValue(entry.Value, depth + 1));

                    first = false;
                }

                string spacesOther = " ";
                for (int i = 0; i < depth; i++)
                    spacesOther += " ";

                builder.Append(spacesOther + "\n}");

                return builder.ToString();
            }
        }

        if (value is not IEnumerable enumerable)
            return value.ToString();

        {
            StringBuilder builder = new();
            builder.Append("[\n");

            bool first = true;
            foreach (object item in enumerable)
            {
                if (!first)
                    builder.Append(",\n");

                string spaces = "  ";
                for (int i = 0; i < depth; i++)
                    spaces += "  ";

                builder.Append(spaces + FormatValue(item, depth + 1));
                first = false;
            }

            string spacesOther = " ";
            for (int i = 0; i < depth; i++)
                spacesOther += " ";

            builder.Append(spacesOther + "\n]");

            return builder.ToString();
        }
    }
}
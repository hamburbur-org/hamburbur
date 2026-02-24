using System.Collections.Generic;
using hamburbur.Tools;
using Newtonsoft.Json.Linq;

namespace hamburbur.Mods.Macros;

public struct Macro
{
    public List<RigTransform> Positions;
    public string             Name;

    public readonly string DumpJson()
    {
        JObject obj = new()
        {
                ["name"]      = Name,
                ["positions"] = new JArray(Positions.ConvertAll(p => p.ToJObject())),
        };

        return obj.ToString();
    }

    public static Macro LoadJson(string json)
    {
        JObject obj = JObject.Parse(json);

        Macro macro = new()
        {
                Name      = (string)obj["name"],
                Positions = [],
        };

        foreach (JToken token in (JArray)obj["positions"])
            macro.Positions.Add(RigTransform.FromJObject((JObject)token));

        return macro;
    }
}
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace hamburbur.Tools;

public static class JObjectExtensions
{
    public static JObject FromVector3(Vector3 vector3) =>
            new()
            {
                    ["x"] = vector3.x,
                    ["y"] = vector3.y,
                    ["z"] = vector3.z,
            };

    public static Vector3 ToVector3(JObject jObject) => new(jObject["x"].ToObject<float>(),
            jObject["y"].ToObject<float>(), jObject["z"].ToObject<float>());

    public static JObject FromQuaternion(Quaternion quaternion) => new()
    {
            ["x"] = quaternion.x,
            ["y"] = quaternion.y,
            ["z"] = quaternion.z,
            ["w"] = quaternion.w,
    };

    public static Quaternion ToQuaternion(JObject jObject) => new(jObject["x"].ToObject<float>(),
            jObject["y"].ToObject<float>(), jObject["z"].ToObject<float>(), jObject["w"].ToObject<float>());

    public static JObject FromColour(Color colour) => new()
    {
            ["r"] = colour.r,
            ["g"] = colour.g,
            ["b"] = colour.b,
            ["a"] = colour.a,
    };

    public static Color ToColour(JObject jObject) => new(jObject["r"].ToObject<float>(), jObject["g"].ToObject<float>(),
            jObject["b"].ToObject<float>(), jObject["a"].ToObject<float>());
}
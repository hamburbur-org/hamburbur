using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.CustomMaps.MecchaGorilla;

public static class MecchaNetwork
{
    public static int LocalId => PhotonNetwork.LocalPlayer?.ActorNumber ?? 0;

    public static void Send(string eventName, params object[] data)
    {
        if (!PhotonNetwork.InRoom)
            return;

        object[] content = new object[data.Length + 1];
        content[0] = eventName;
        Array.Copy(data, 0, content, 1, data.Length);
        PhotonNetwork.RaiseEvent(CustomMapUtils.EventCode, content,
                new RaiseEventOptions { Receivers = ReceiverGroup.All, }, SendOptions.SendReliable);
    }

    public static void Kill(int victimId) =>
            Send(MecchaEvents.Kill, (double)victimId, (double)LocalId);

    public static void Respawn(int victimId) => Send(MecchaEvents.Respawn, (double)victimId);
    public static void Whistle(int playerId) => Send(MecchaEvents.Whistle, (double)playerId);

    public static void SetRole(int playerId, bool seeker) =>
            Send(MecchaEvents.Role, (double)playerId, seeker ? 1d : 0d);

    public static void PaintMini(int playerId, Color color) =>
            Send(MecchaEvents.BucketColor, (double)color.r, (double)color.g, (double)color.b, (double)playerId);

    public static void PaintPart(int playerId, int part, Color color) =>
            Send(MecchaEvents.BucketPart, (double)part, (double)color.r, (double)color.g, (double)color.b,
                    (double)playerId);

    public static void ColorCode(int playerId, Color color) =>
            Send(MecchaEvents.ColorCode, (double)playerId, (double)(color.r * 255f),
                    (double)(color.g                                        * 255f), (double)(color.b * 255f));

    public static void Shot(Vector3 from, Vector3 to, bool explosion, Color color) =>
            Send(MecchaEvents.Shot,
                    (double)from.x,      (double)from.y,  (double)from.z,
                    (double)to.x,        (double)to.y,    (double)to.z,
                    explosion ? 1d : 0d, (double)color.r, (double)color.g, (double)color.b);

    public static void Dot(int playerId, Vector3 offset, float size, Color color) =>
            Send(MecchaEvents.Dot, (double)playerId, (double)offset.x, (double)offset.y, (double)offset.z,
                    (double)size,  (double)color.r,  (double)color.g,  (double)color.b);

    public static void HeldTool(int playerId, int toolType, Transform transform)
    {
        if (transform == null)
            return;

        Vector3    p = transform.position;
        Quaternion q = transform.rotation;
        Send(MecchaEvents.HeldTool, (double)playerId, (double)toolType,
                (double)p.x,        (double)p.y,      (double)p.z,
                (double)q.x,        (double)q.y,      (double)q.z, (double)q.w);
    }

    public static void SetRoundState(int state, float duration, int map, int reason = 0) =>
            Send(MecchaEvents.RoundState, (double)state, (double)duration, (double)map, (double)reason);

    public static void Settings(int  seekers, int hideTime, int matchTime, int checkTime, int whistleTime,
                                bool testing) =>
            Send(MecchaEvents.Settings, (double)seekers,     (double)hideTime, (double)matchTime,
                    (double)checkTime,  (double)whistleTime, 1d,               testing ? 1d : 0d);

    public static Color Rainbow(float speed = 0.2f) => Color.HSVToRGB(Mathf.Repeat(Time.time * speed, 1f), 1f, 1f);
}
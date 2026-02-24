using TMPro;
using UnityEngine;

namespace hamburbur.Tools;

public sealed class FakeRig
{
    private readonly GameObject       nametag;
    private readonly GameObjectData[] parts;
    private readonly bool             showNametag;
    public           float            LastUpdateDelay = 0.1f;

    public float LastUpdateTime;

    public FakeRig(Color   colour,   Vector3    headPos,  Quaternion    headRot, Vector3 leftPos, Quaternion leftRot,
                   Vector3 rightPos, Quaternion rightRot, TMP_FontAsset font,    bool    nametag, string     name)
    {
        GameObject head      = CreateHead(colour);
        GameObject leftHand  = CreateSphere(0.1f, colour);
        GameObject rightHand = CreateSphere(0.1f, colour);

        if (nametag)
            this.nametag = CreateNametag(name, colour, font, head.transform);

        showNametag = nametag;

        parts =
        [
                new GameObjectData(head,      headPos,  headRot),
                new GameObjectData(leftHand,  leftPos,  leftRot),
                new GameObjectData(rightHand, rightPos, rightRot),
        ];

        LastUpdateTime = Time.time;
    }

    public void UpdateTargets(Vector3 headPos,  Quaternion headRot, Vector3 leftPos, Quaternion leftRot,
                              Vector3 rightPos, Quaternion rightRot)
    {
        LastUpdateDelay = Mathf.Max(Time.time - LastUpdateTime, 0.01f);
        LastUpdateTime  = Time.time;

        parts[0].SetTarget(headPos, headRot * Quaternion.Euler(90f, 0f, 0f));
        parts[1].SetTarget(leftPos, leftRot);
        parts[2].SetTarget(rightPos, rightRot);
    }

    public void Tick()
    {
        float t = (Time.time - LastUpdateTime) / LastUpdateDelay;

        foreach (GameObjectData part in parts)
            part.Interpolate(t);

        if (!Camera.main || !showNametag)
            return;

        nametag.transform.LookAt(Camera.main.transform.position);
        nametag.transform.Rotate(0f, 180f, 0f);
    }

    public void Destroy()
    {
        foreach (GameObjectData part in parts)
            part.AssociatedGameObject.Obliterate();

        Object.Destroy(nametag);
    }

    private static GameObject CreateSphere(float scale, Color colour)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.Destroy(obj.GetComponent<Collider>());
        obj.transform.localScale                    = Vector3.one * scale;
        obj.GetComponent<Renderer>().material.color = colour;

        return obj;
    }

    private static GameObject CreateHead(Color colour)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Object.Destroy(obj.GetComponent<Collider>());
        obj.transform.localScale                    = Vector3.one * 0.2f;
        obj.GetComponent<Renderer>().material.color = colour;

        return obj;
    }

    private static GameObject CreateNametag(string name, Color colour, TMP_FontAsset font, Transform parent)
    {
        GameObject tag = new("hamburburMenu_Nametag");
        tag.transform.SetParent(parent);
        tag.transform.localPosition = new Vector3(0f, 0f, -1f);
        tag.transform.localScale    = Vector3.one * 0.25f;

        TextMeshPro tmp = tag.AddComponent<TextMeshPro>();
        tmp.font      = font;
        tmp.fontSize  = 24f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text      = name;
        tmp.color     = colour;

        return tag;
    }

    private struct GameObjectData
    {
        public readonly GameObject AssociatedGameObject;

        private Vector3    oldPos;
        private Vector3    targetPos;
        private Quaternion oldRot;
        private Quaternion targetRot;

        public GameObjectData(GameObject obj, Vector3 pos, Quaternion rot)
        {
            AssociatedGameObject = obj;
            oldPos               = targetPos = pos;
            oldRot               = targetRot = rot;
        }

        public void SetTarget(Vector3 pos, Quaternion rot)
        {
            oldPos    = targetPos;
            oldRot    = targetRot;
            targetPos = pos;
            targetRot = rot;
        }

        public void Interpolate(float t)
        {
            AssociatedGameObject.transform.position =
                    Vector3.Lerp(oldPos, targetPos, t);

            AssociatedGameObject.transform.rotation =
                    Quaternion.Lerp(oldRot, targetRot, t);
        }
    }
}
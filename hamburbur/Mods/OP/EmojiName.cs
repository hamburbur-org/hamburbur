using System.Collections;
using System.Collections.Generic;
using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Pun;
using UnityEngine;
using Random = System.Random;

namespace hamburbur.Mods.OP;

[hamburburmod(                "Emoji Name", "Cycles through random emojis.", ButtonType.Togglable, AccessSetting.Public,
        EnabledType.Disabled, 0)]
public class EmojiName : hamburburmod
{
    private readonly List<string> Emojis = new()
    {
            //one of these is an imposter but idk which one
            "☺️", "😀", "😃", "😄", "😁", "😆", "😅", "😂", "😊",
    };

    private Coroutine emojiNameRoutine;

    private string PreviousName;

    protected override void OnEnable()
    {
        PreviousName     = PhotonNetwork.LocalPlayer.NickName;
        emojiNameRoutine = CoroutineManager.Instance.StartCoroutine(NameChanger());
    }

    protected override void OnDisable()
    {
        if (emojiNameRoutine != null)
            CoroutineManager.Instance.StopCoroutine(emojiNameRoutine);

        PhotonNetwork.LocalPlayer.NickName = PreviousName;
    }

    private IEnumerator NameChanger()
    {
        Random rand = new();
        while (true)
        {
            string emoji = Emojis[rand.Next(Emojis.Count)];
            PhotonNetwork.LocalPlayer.NickName = $"<size=32767>{emoji}</size>";

            yield return new WaitForSeconds(0.5f);
        }
    }
}
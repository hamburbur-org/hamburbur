using System.Collections.Generic;
using System.Linq;
using hamburbur.GUI;
using hamburbur.Mod_Backend;
using hamburbur.Tools;
using UnityEngine;

namespace hamburbur.Mods.Scoreboard;

public class PlayerAdderHandler : MonoBehaviour
{
    private readonly Dictionary<VRRig, (PlayerLine, GameObject)> playerLines = [];

    private void Start()
    {
        RigUtils.OnRigLoaded   += AddPlayer;
        RigUtils.OnRigUnloaded += RemovePlayer;
    }

    private void AddPlayer(VRRig rig)
    {
        PlayerLine mod = (PlayerLine)ButtonHandler.AddButton("Scoreboard", typeof(PlayerLine));
        mod.AssociatedRig = rig;
    }

    private void RemovePlayer(VRRig rig)
    {
        if (rig == null)
            return;

        hamburburmod mod = Buttons.Categories["Scoreboard"]
                                  .First(button => ((PlayerLine)button.Item2).AssociatedRig == rig).Item2;

        ButtonHandler.RemoveButton(mod);
    }
}
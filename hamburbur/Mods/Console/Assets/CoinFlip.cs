using hamburbur.Managers;
using hamburbur.Mod_Backend;
using Photon.Realtime;
using UnityEngine;

namespace hamburbur.Mods.Console.Assets;

[hamburburmod("Coin Flip", "Heads or Tails?", ButtonType.Togglable, AccessSetting.AdminOnly, EnabledType.AlwaysDisabled,
        0)]
public class CoinFlip : hamburburmod
{
    private int allocatedCoinId = -1;

    public  int  coinChain;
    public  bool coinChainHeads;
    public  int  coinHeads;
    public  int  coinTails;
    private bool lastFlipping;

    protected override void Update()
    {
        bool rightGrab      = InputManager.Instance.RightGrip.IsPressed;
        bool rightTrigger   = InputManager.Instance.RightTrigger.IsPressed;
        bool rightPrimary   = InputManager.Instance.RightPrimary.IsPressed;
        bool rightSecondary = InputManager.Instance.RightSecondary.IsPressed;

        if (rightGrab && rightTrigger)
        {
            if (allocatedCoinId == -1 && (rightPrimary || rightSecondary))
            {
                allocatedCoinId = Components.Console.GetFreeAssetID();

                Components.Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "console.main1", "Coin",
                        allocatedCoinId);

                Components.Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, allocatedCoinId, 2);

                Tools.Utils.RPCProtection();
            }

            if (allocatedCoinId == -1) return;

            bool flipping = rightPrimary || rightSecondary;

            if (!flipping && lastFlipping)
            {
                bool heads = Random.Range(0f, 1f) >= 0.5f;
                if (heads != coinChainHeads)
                {
                    coinChain      = 0;
                    coinChainHeads = heads;
                }

                coinChain++;

                if (heads) coinHeads++;
                else coinTails++;

                Components.Console.ExecuteCommand("asset-playanimation", ReceiverGroup.All, allocatedCoinId,
                        "CoinHolder", heads ? "Heads" : "Tails");

                Components.Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedCoinId,
                        "CoinHolder", "Flip");
            }

            lastFlipping = flipping;
        }
        else
        {
            lastFlipping = false;

            if (allocatedCoinId != -1)
            {
                Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedCoinId);
                allocatedCoinId = -1;
            }
        }
    }

    protected override void OnDisable()
    {
        if (allocatedCoinId != -1)
        {
            Components.Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedCoinId);
            allocatedCoinId = -1;
        }
    }
}
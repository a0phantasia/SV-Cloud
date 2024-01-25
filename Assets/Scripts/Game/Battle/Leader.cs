using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader : BattlePlace
{
    public int CraftId => leaderCard.CurrentCard.CraftId;
    public CardCraft Craft => leaderCard.CurrentCard.Craft;
    public BattleCard leaderCard => cards[0];

    public int HpInit => Card.GetLeaderCard(CraftId).hp;
    public int Hp => leaderCard.CurrentCard.hp;
    public int HpMax => leaderCard.CurrentCard.hpMax;

    private int pp, ep;
    public int PPMax, EpMax;
    public int PP {
        get => pp;
        set => pp = Mathf.Clamp(value, 0, PPMax);
    }
    public int EP {
        get => ep;
        set => ep = Mathf.Clamp(value, 0, EpMax);
    }
    public bool isEpUsed;

    public Leader(bool isFirst, int craftId) : base(new List<BattleCard>() { BattleCard.Get(Card.GetLeaderCard(craftId)) }) {
        ep = pp = PPMax = 0;
        EpMax = isFirst ? 2 : 3;
        isEpUsed = false;
    }

    public Leader(Leader rhs) : base(rhs) {
        pp = rhs.pp;
        PPMax = rhs.PPMax;
        ep = rhs.ep;
        EpMax = rhs.EpMax;
        isEpUsed = rhs.isEpUsed;
    }

    public override BattlePlaceId GetPlaceId()
    {
        return BattlePlaceId.Leader;
    }

    public void ClearTurnIdentifier() {
        isEpUsed = false;
        
        SetIdentifier("combo", 0);

        foreach (var entry in options) {
            if (!entry.Key.StartsWith("turn"))
                continue;

            SetIdentifier(entry.Key, 0);
        }
    }

    public override float GetIdentifier(string id) 
    {
        return id switch {
            "hp" => Hp,
            "hpMax" => HpMax,
            "hpInit" => HpInit,
            "pp" => pp,
            "ppMax" => PPMax,
            "ep" => ep,
            "epMax" => EpMax,
            "isEpUsed" => isEpUsed ? 1 : 0,
            "isAwake"  => ((options.Get("lockAwake") <= 0) && (PPMax >= 7) || (options.Get("forceAwake") > 0))  ? 1 : 0,
            _ => base.GetIdentifier(id),
        };
    }

    public override void SetIdentifier(string id, float num) {
        switch (id) {
            default:
                base.SetIdentifier(id, num);
                return;
            case "pp":
                PP = (int)num;
                return;
            case "ppMax":
                PPMax = Mathf.Max((int)num, 0);
                return;
            case "ep":
                EP = (int)num;
                return;
            case "epMax":
                EpMax = Mathf.Max((int)num, 0);
                return;
            case "isEpUsed":
                isEpUsed = num != 0;
                return;
        }
    }

    public override void AddIdentifier(string id, float num) {
        SetIdentifier(id, GetIdentifier(id) + num);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leader
{
    public int CraftId => leaderCard.CurrentCard.CraftId;
    public CardCraft Craft => leaderCard.CurrentCard.Craft;
    public BattleCard leaderCard;
    public Dictionary<string, float> options = new Dictionary<string, float>();

    public int HpInit => Card.GetLeaderCard(CraftId).hp;
    public int Hp => leaderCard.CurrentCard.hp;
    public int HpMax => leaderCard.HpMax;
    public int pp, ppMax;
    public int ep, epMax;
    public bool isEpUsed;

    public Leader(bool isFirst, int craftId) {
        leaderCard = BattleCard.Get(Card.GetLeaderCard(craftId));
        ep = pp = ppMax = 0;
        epMax = isFirst ? 2 : 3;
        isEpUsed = false;
    }

    public Color GetHpTextColor() {
        if (Hp == HpInit)
            return Color.white;

        return (Hp > HpInit) ? new Color(119, 226, 12) : Color.red;
    }

    public Color GetHpOutlineColor() {
        return (GetHpTextColor() == Color.red) ? Color.white : Color.black;
    }

    public float GetIdentifier(string id) 
    {
        return id switch {
            "hp" => Hp,
            "hpMax" => HpMax,
            "hpInit" => HpInit,
            "pp" => pp,
            "ppMax" => ppMax,
            _ => options.Get(id, 0),
        };
    }
}

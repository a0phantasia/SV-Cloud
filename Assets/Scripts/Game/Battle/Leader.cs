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
    public int HpMax => leaderCard.statusController.hpMax;

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

    public Leader(bool isFirst, int craftId) {
        leaderCard = BattleCard.Get(Card.GetLeaderCard(craftId));
        ep = pp = PPMax = 0;
        EpMax = isFirst ? 2 : 3;
        isEpUsed = false;
    }

    public Leader(Leader rhs) {
        leaderCard = new BattleCard(rhs.leaderCard);
        options = new Dictionary<string, float>(rhs.options);
        pp = rhs.pp;
        PPMax = rhs.PPMax;
        ep = rhs.ep;
        EpMax = rhs.EpMax;
        isEpUsed = rhs.isEpUsed;
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
            "ppMax" => PPMax,
            "ep" => ep,
            "epMax" => EpMax,
            "isEpUsed" => isEpUsed ? 1 : 0,
            _ => options.Get(id, 0),
        };
    }

    public void SetIdentifier(string id, float num) {
        switch (id) {
            default:
                options.Set(id, num);
                return;
            case "pp":
                PP = (int)num;
                return;
            case "ppMax":
                PPMax = (int)num;
                return;
            case "ep":
                EP = (int)num;
                return;
            case "epMax":
                EpMax = (int)num;
                return;
            case "isEpUsed":
                isEpUsed = num != 0;
                return;
        }
    }
}

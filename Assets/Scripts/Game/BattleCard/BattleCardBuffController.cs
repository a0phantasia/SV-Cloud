using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCardBuffController
{
    public int costBuff, atkBuff, hpBuff;
    public int damage;

    public Dictionary<string, float> options = new Dictionary<string, float>();

    public BattleCardBuffController() {
        costBuff = atkBuff = hpBuff = damage = 0;
    }

    public BattleCardBuffController(BattleCardBuffController rhs) {
        costBuff = rhs.costBuff;
        atkBuff = rhs.atkBuff;
        hpBuff = rhs.hpBuff;
        damage = rhs.damage;
        options = new Dictionary<string, float>(rhs.options);
    }

    public float GetIdentifier(string id) 
    {
        return id switch {
            "costBuff" => costBuff,
            "atkBuff" => atkBuff,
            "hpBuff" => hpBuff,
            "damage" => damage,
            _ => options.Get(id, 0),
        };
    }
    public void SetIdentifier(string id, float num) {
        switch (id) {
            default:
                options.Set(id, num);
                return;
            case "damage":
                damage = (int)num;
                return;
            case "costBuff":
                costBuff =(int)num;
                return;
            case "atkBuff":
                atkBuff =(int)num;
                return;
            case "hpBuff":
                hpBuff =(int)num;
                return;
        }
    }
}

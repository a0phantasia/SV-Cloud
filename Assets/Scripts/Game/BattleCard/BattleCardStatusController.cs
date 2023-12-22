using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCardStatusController
{
    public int costBuff, atkBuff, hpBuff;
    public int hpMax;

    public BattleCardStatusController(Card card) {
        costBuff = atkBuff = hpBuff = 0;
        hpMax = card.hp;
    }

    public BattleCardStatusController(BattleCardStatusController rhs) {
        costBuff = rhs.costBuff;
        atkBuff = rhs.atkBuff;
        hpBuff = rhs.hpBuff;
        hpMax = rhs.hpMax;
    }
}

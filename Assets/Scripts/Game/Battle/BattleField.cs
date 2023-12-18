using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleField
{
    public int Count => cards.Count;
    public List<BattleCard> cards = new List<BattleCard>();

    public BattleField() {}
    public BattleField(BattleField rhs) {
        cards = rhs.cards.Select(x => (x == null) ? null : new BattleCard(x)).ToList();
    }

}

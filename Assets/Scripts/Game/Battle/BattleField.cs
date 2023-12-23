using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleField
{
    public int MaxCount = 5;
    public int Count => cards.Count;
    public bool IsFull => Count >= MaxCount;
    public List<BattleCard> cards = new List<BattleCard>();

    public BattleField() {}
    public BattleField(BattleField rhs) {
        MaxCount = rhs.MaxCount;
        cards = rhs.cards.Select(x => (x == null) ? null : new BattleCard(x)).ToList();
    }


}

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHand 
{
    public int Count => cards.Count;
    public List<BattleCard> cards = new List<BattleCard>();

    public BattleHand() {}
    public BattleHand(BattleHand rhs) {
        cards = rhs.cards.Select(x => (x == null) ? null : new BattleCard(x)).ToList();
    }
}

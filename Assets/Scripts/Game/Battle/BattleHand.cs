using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHand 
{
    public int MaxCount = 9;
    public int Count => cards.Count;
    public bool IsFull => Count >= MaxCount;
    public List<BattleCard> cards = new List<BattleCard>();

    public BattleHand() {}
    public BattleHand(BattleHand rhs) {
        MaxCount = rhs.MaxCount;
        cards = rhs.cards.Select(x => (x == null) ? null : new BattleCard(x)).ToList();
    }
}

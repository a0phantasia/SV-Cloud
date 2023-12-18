using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGrave
{
    public int Count = 0;
    public List<Card> cards = new List<Card>();

    public BattleGrave() {}
    public BattleGrave(BattleGrave rhs) {
        cards = rhs.cards.Select(x => (x == null) ? null : new Card(x)).ToList();
    }
}

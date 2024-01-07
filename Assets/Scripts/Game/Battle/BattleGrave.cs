using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGrave
{
    public int Count = 0;
    public List<Card> graveCards = new List<Card>();
    public List<Card> usedCards = new List<Card>();
    public List<Card> destroyedCards = new List<Card>();

    public List<Card> distinctUsedCards => usedCards.Select(x => x.id).Distinct().Select(Card.Get).OrderBy(CardDatabase.Sorter).ToList();

    public BattleGrave() {}
    public BattleGrave(BattleGrave rhs) {
        Count = rhs.Count;
        graveCards = rhs.graveCards.Select(x => (x == null) ? null : new Card(x)).ToList();
        usedCards = rhs.usedCards.Select(x => (x == null) ? null : new Card(x)).ToList();
        destroyedCards = rhs.usedCards.Select(x => (x == null) ? null : new Card(x)).ToList();
    }
}

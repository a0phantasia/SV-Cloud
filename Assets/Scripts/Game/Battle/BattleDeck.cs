using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleDeck 
{
    public int zone, format, craft;
    public List<BattleCard> cards = new List<BattleCard>();
    public int Count => cards.Count;
    public int MaxCount { get; private set; }
    
    public BattleDeck(int zoneId, int formatId, int craftId, int[] cardIds) {
        zone = zoneId;
        format = formatId;
        craft = craftId;
        cards = cardIds.Select(x => BattleCard.Get(Card.Get(x))).ToList();
        MaxCount = cards.Count;
    }

    public BattleDeck(BattleDeck rhs) {
        zone = rhs.zone;
        format = rhs.format;
        craft = rhs.craft;
        cards = rhs.cards.Select(x => (x == null) ? null : new BattleCard(x)).ToList();
        MaxCount = rhs.MaxCount;
    }

}

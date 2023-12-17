using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Lumin;

public class BattleUnit 
{
    public int id;
    public string name;
    public bool isFirst;
    public Leader leader;
    public BattleCard territory = null;
    public BattleDeck deck;
    public BattleField field;
    public BattleHand hand;
    public BattleGrave grave;


    public string IsFirstText => isFirst ? "先手" : "後手";

    public BattleUnit(int unitId, string nickname, BattleDeck initDeck, bool first) {
        id = unitId;
        name = nickname;
        leader = new Leader(first, initDeck.craft);
        deck = initDeck;
        field = new BattleField();
        hand = new BattleHand();
        grave = new BattleGrave();
        isFirst = first;

        Draw(3);
    }

    public List<BattleCard> Draw(int count = 1) {
        var result = deck.cards.Take(count).ToList();
        hand.cards.AddRange(result);
        deck.cards.RemoveAll(x => result.Contains(x));
        return result;
    }

    public List<BattleCard> DrawWithFilter(CardFilter filter, int count = 1) {
        var result = deck.cards.Where(x => filter.Filter(x.CurrentCard)).Take(count).ToList();
        hand.cards.AddRange(result);
        deck.cards.RemoveAll(x => result.Contains(x));
        return result;
    }
}

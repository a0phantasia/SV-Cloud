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
    public int turn = 0;
    public Leader leader;
    public BattleCard territory = null;
    public BattleDeck deck;
    public BattleField field;
    public BattleHand hand;
    public BattleGrave grave;

    public bool isDone = false;
    public string IsFirstText => isFirst ? "先手" : "後手";

    public BattleUnit(int unitId, string nickname, BattleDeck initDeck, bool first) {
        id = unitId;
        name = nickname;
        turn = 0;

        leader = new Leader(first, initDeck.craft);
        deck = initDeck;
        field = new BattleField();
        hand = new BattleHand();
        grave = new BattleGrave();

        isFirst = first;
        isDone = false;

        Draw(3);
    }

    public BattleUnit(BattleUnit rhs) {
        id = rhs.id;
        name = rhs.name;
        turn = rhs.turn;

        leader = new Leader(rhs.leader);
        territory = (territory == null) ? null : new BattleCard(rhs.territory);
        deck = new BattleDeck(rhs.deck);
        field = new BattleField(rhs.field);
        hand = new BattleHand(rhs.hand);
        grave = new BattleGrave(rhs.grave);

        isFirst = rhs.isFirst;
        isDone = rhs.isDone;
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

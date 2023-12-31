using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleUnit : IIdentifyHandler
{
    public int id;
    public string name;
    public int turn = 0;
    public Leader leader;
    public BattleCard territory = null;
    public BattleDeck deck;
    public BattleField field;
    public BattleHand hand;
    public BattleGrave grave;

    public bool isDone = false;
    public bool isMyTurn = false;
    public bool isFirst;
    public string IsFirstText => isFirst ? "先手" : "後手";
    public bool IsMasterUnit => id == 0;

    public int Id => id;

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
        isMyTurn = first;

        Draw(3, out _, out _);
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
        isMyTurn = rhs.isMyTurn;
    }

    /// <summary>
    /// Draw cards.
    /// </summary>
    /// <returns>Total cards drawn, including inHand and inGrave.</returns>
    public List<BattleCard> Draw(int count, out List<BattleCard> inHand, out List<BattleCard> inGrave) {
        var availableCount = hand.MaxCount - hand.Count;
        var result = deck.cards.Take(count).ToList();

        if (result.Count > availableCount) {
            inHand = result.GetRange(0, availableCount);
            inGrave = result.GetRange(availableCount, result.Count - availableCount);
        } else {
            inHand = result.GetRange(0, result.Count);
            inGrave = new List<BattleCard>();
        }

        hand.cards.AddRange(inHand);
        grave.usedCards.AddRange(inGrave.Select(x => x.card));
        deck.cards.RemoveRange(0, result.Count);

        grave.Count += inGrave.Count;

        return result;
    }

    public bool TryGetIdenfier(string id, out float value)
    {
        value = GetIdentifier(id);
        return value == float.MinValue;
    }

    public float GetIdentifier(string id)
    {
        return id switch {
            "id" => Id,
            "isFirst" => isFirst ? 1 : 0,
            _ => float.MinValue,
        };
    }

    public void SetIdentifier(string id, float value)
    {
        throw new NotImplementedException();
    }
}

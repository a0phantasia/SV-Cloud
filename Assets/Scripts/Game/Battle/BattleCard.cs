using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCard : IIdentifyHandler
{
    public bool IsEvolved { get; protected set; }
    public Card card;
    public Card evolveCard;
    public Card CurrentCard => IsEvolved ? evolveCard : card;
    public int Id => CurrentCard.Id;
    public int HpMax;

    public BattleCard(Card baseCard) {
        card = (baseCard == null) ? null : new Card(baseCard);
        evolveCard = (card?.EvolveCard == null) ? null : new Card(card.EvolveCard);
        
        card?.effects.ForEach(x => x.source = this);
        evolveCard?.effects.ForEach(x => x.source = this);
    }

    public BattleCard(BattleCard rhs) {
        IsEvolved = rhs.IsEvolved;
        HpMax = rhs.HpMax;
        
        card = (rhs.card == null) ? null : new Card(rhs.card);
        evolveCard = (rhs.evolveCard == null) ? null : new Card(rhs.evolveCard);
    }

    public static BattleCard Get(Card baseCard) {
        return (baseCard == null) ? null : new BattleCard(baseCard);
    }

    public bool TryGetIdenfier(string id, out float value)
    {
        value = GetIdentifier(id);
        return value != float.MinValue;
    }

    public float GetIdentifier(string id)
    {
        return float.MinValue;
    }

    public void SetIdentifier(string id, float value)
    {
        return;
    }
}

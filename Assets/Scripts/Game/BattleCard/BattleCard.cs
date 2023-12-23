using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleCard : IIdentifyHandler
{
    public int Id => CurrentCard.Id;
    public bool IsEvolved { get; protected set; }
    public Card card;
    public Card evolveCard;
    public Card OriginalCard => IsEvolved ? evolveCard : card;
    public Card CurrentCard => GetCurrentCard();
    public BattleCardStatusController statusController;
    
    public BattleCard(Card baseCard) {
        IsEvolved = false;

        card = (baseCard == null) ? null : new Card(baseCard);
        evolveCard = (card?.EvolveCard == null) ? null : new Card(card.EvolveCard);
        
        card?.effects.ForEach(x => x.source = this);
        evolveCard?.effects.ForEach(x => x.source = this);

        statusController = new BattleCardStatusController(baseCard);
    }

    public BattleCard(BattleCard rhs) {
        IsEvolved = rhs.IsEvolved;
        
        card = (rhs.card == null) ? null : new Card(rhs.card);
        evolveCard = (rhs.evolveCard == null) ? null : new Card(rhs.evolveCard);

        card?.effects.ForEach(x => x.source = this);
        evolveCard?.effects.ForEach(x => x.source = this);

        statusController = new BattleCardStatusController(rhs.statusController);
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

    public Card GetCurrentCard() {
        var result = new Card(OriginalCard);
        result.cost += statusController.costBuff;
        result.atk += statusController.atkBuff;
        result.hp -= statusController.hpBuff;
        return result;
    }

    public int GetUseCost(Leader leader) {
        return CurrentCard.cost;
    }

    public bool IsUsable(BattleUnit unit) {
        bool IsCostEnough() => unit.leader.PP >= GetUseCost(unit.leader);
        bool IsFieldFull()  => (CurrentCard.Type == CardType.Follower) && (unit.field.IsFull);

        return IsCostEnough() && (!IsFieldFull());
    }
}

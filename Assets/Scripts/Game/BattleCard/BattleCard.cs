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
    public List<Effect> newEffects = new List<Effect>();
    public BattleCardBuffController buffController;
    public BattleCardActionController actionController;
    
    public BattleCard(Card baseCard) {
        IsEvolved = false;

        card = (baseCard == null) ? null : new Card(baseCard);
        evolveCard = (card?.EvolveCard == null) ? null : new Card(card.EvolveCard);
        
        card?.effects.ForEach(x => x.source = this);
        evolveCard?.effects.ForEach(x => x.source = this);

        buffController = new BattleCardBuffController();
        actionController = new BattleCardActionController();
    }

    public BattleCard(BattleCard rhs) {
        IsEvolved = rhs.IsEvolved;
        
        card = (rhs.card == null) ? null : new Card(rhs.card);
        evolveCard = (rhs.evolveCard == null) ? null : new Card(rhs.evolveCard);

        card?.effects.ForEach(x => x.source = this);
        evolveCard?.effects.ForEach(x => x.source = this);

        newEffects = rhs.newEffects.Select(x => new Effect(x)).ToList();
        newEffects.ForEach(x => x.source = this);

        buffController = new BattleCardBuffController(rhs.buffController);
        actionController = new BattleCardActionController(rhs.actionController);
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
        if (id.StartsWith("action."))
            return actionController.GetIdentifier(id.TrimStart("action."));


        return float.MinValue;
    }

    public void SetIdentifier(string id, float value)
    {
        return;
    }

    public Card GetCurrentCard() {
        var result = new Card(OriginalCard);
        result.cost += buffController.costBuff;
        result.atk += buffController.atkBuff;
        result.hpMax += buffController.hpBuff;
        result.hp = result.hpMax - buffController.damage;
        result.effects.AddRange(newEffects);
        result.effects.ForEach(x => x.source = this);
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

    public void SetKeyword(CardKeyword keyword, ModifyOption option) {
        if (option == ModifyOption.Add) {
            card.keywords.Add(keyword);
            evolveCard.keywords.Add(keyword);
        } else if (option == ModifyOption.Remove) {
            card.keywords = card.keywords.Where(x => x != keyword).ToList();
            evolveCard.keywords = evolveCard.keywords.Where(x => x != keyword).ToList();
        }
    }
}

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

    public Dictionary<string, float> options = new Dictionary<string, float>();
    
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

        options = new Dictionary<string, float>(rhs.options);
    }

    public static BattleCard Get(Card baseCard) {
        return (baseCard == null) ? null : new BattleCard(baseCard);
    }

    public static BattleCard Get(int id) {
        return BattleCard.Get(Card.Get(id));
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

        return id switch {
            _ => options.Get(id, 0),
        };
    }

    public void SetIdentifier(string id, float value)
    {
        if (id.StartsWith("action."))
            actionController.SetIdentifier(id.TrimStart("action."), value);
        else 
            options.Set(id, value);
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

    public int GetEvolveCost() {
        return 1;
    }

    public bool IsUsable(BattleUnit sourceUnit) {
        bool isCostEnough = sourceUnit.leader.PP >= GetUseCost(sourceUnit.leader);
        bool isFieldFull = (CurrentCard.Type == CardType.Follower) && (sourceUnit.field.IsFull);
        return isCostEnough && (!isFieldFull);
    }

    public bool IsEvolvable(BattleUnit sourceUnit) {
        bool isUnevolvedFollower = CurrentCard.Type == CardType.Follower;
        bool isEpUnused = (sourceUnit.isEvolveEnabled) && (!sourceUnit.leader.isEpUsed) && (sourceUnit.leader.EP >= GetEvolveCost());
        return isUnevolvedFollower && isEpUnused;
    }

    public bool IsAttackable(BattleUnit sourceUnit) {
        return IsLeaderAttackable(sourceUnit) || IsFollowerAttackable(sourceUnit);
    }

    public bool IsLeaderAttackable(BattleUnit sourceUnit) {
        var isAttackChanceLegal = actionController.CurrentAttackChance > 0;
        var isStayTurnLegal = actionController.StayFieldTurn > 0;
        var isKeywordLegal = actionController.IsKeywordAvailable(CardKeyword.Storm);

        return sourceUnit.isMyTurn && isAttackChanceLegal && (isStayTurnLegal || isKeywordLegal);
    }

    public bool IsFollowerAttackable(BattleUnit sourceUnit) {
        var isAttackChanceLegal = actionController.CurrentAttackChance > 0;
        var isStayTurnLegal = actionController.StayFieldTurn > 0;
        var isKeywordLegal = actionController.IsKeywordAvailable(CardKeyword.Storm) || actionController.IsKeywordAvailable(CardKeyword.Rush);

        return sourceUnit.isMyTurn && isAttackChanceLegal && (IsEvolved || isStayTurnLegal || isKeywordLegal);
    }

    // Evolve this follower. You should check IsEvolvable() before calling this if you use EP evolve.
    public void Evolve() {
        IsEvolved = true;
    }

    public int TakeDamage(int damage) {
        buffController.damage += damage;
        return damage;
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

    public void TakeBuff(int atk, int hp) {
        buffController.atkBuff += atk;
        buffController.hpBuff += hp;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleCard : IIdentifyHandler
{
    public Battle Battle => Player.currentBattle;

    public int Id => CurrentCard.Id;
    public bool IsEvolved { get; protected set; }
    public Card card;
    public Card evolveCard;
    public Card OriginalCard => IsEvolved ? evolveCard : card;
    public Card CurrentCard => GetCurrentCard(OriginalCard);
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

    public Card GetCurrentCard(Card baseCard) {
        var result = new Card(baseCard);
        result.cost += buffController.costBuff;
        result.atk += buffController.atkBuff;
        result.hpMax += buffController.hpBuff;
        result.hp = result.hpMax - buffController.damage;
        result.effects.AddRange(newEffects);
        result.effects.ForEach(x => x.source = this);
        return result;
    }

    public void GetTargetEffectWithTiming(string timing, out Queue<Effect> targetEffectQueue, out Queue<EffectTargetInfo> targetInfoQueue, out Queue<List<short>> selectableTargetQueue) {
        bool isEvolveTiming = timing == "on_this_evolve_with_ep";

        var nowCard = GetCurrentCard(isEvolveTiming ? evolveCard : OriginalCard);

        targetEffectQueue = new Queue<Effect>();
        targetInfoQueue = new Queue<EffectTargetInfo>();
        selectableTargetQueue = new Queue<List<short>>();

        for (int i = 0; i < nowCard.effects.Count; i++) {
            var currentEffect = nowCard.effects[i];
            if (currentEffect.timing != timing)
                continue;

            currentEffect.invokeUnit = Battle.CurrentState.myUnit;
            if (currentEffect.Condition(Battle.CurrentState)) {
                var info = currentEffect.GetEffectTargetInfo(Battle.CurrentState);

                if ((!List.IsNullOrEmpty(info.mode)) && (info.mode[0] == "index")) {
                    targetEffectQueue.Enqueue(currentEffect);
                    targetInfoQueue.Enqueue(info);
                    selectableTargetQueue.Enqueue(GetCurrentSelectableTarget(info));
                }
            }
            currentEffect.invokeTarget = null;
        }
    }

    public List<short> GetCurrentSelectableTarget(EffectTargetInfo currentInfo) {
        List<short> currentSelectableList = new List<short>();

        if (currentInfo.places.Contains(BattlePlaceId.Hand)) {

        } else {
            var myField = Battle.CurrentState.myUnit.field;
            var opField = Battle.CurrentState.opUnit.field;

            var myIndex = (currentInfo.unit == "op") ? new List<int>() : 
                Enumerable.Range(0, myField.Count).Where(x => currentInfo.types.Contains(myField.cards[x].CurrentCard.Type)).ToList();
            var opIndex = (currentInfo.unit == "me") ? new List<int>() : 
                Enumerable.Range(0, opField.Count).Where(x => currentInfo.types.Contains(opField.cards[x].CurrentCard.Type)).ToList();

            myIndex.ForEach(x => currentSelectableList.Add((short)((short)BattlePlaceId.Field * 10 + x)));
            opIndex.ForEach(x => currentSelectableList.Add((short)(100 + (short)BattlePlaceId.Field * 10 + x)));

            if ((currentInfo.unit != "op") && (currentInfo.places.Contains(BattlePlaceId.Leader)))
                currentSelectableList.Add((short)BattlePlaceId.Leader * 10);

            if ((currentInfo.unit != "me") && (currentInfo.places.Contains(BattlePlaceId.Leader)))
                currentSelectableList.Add(100 + (short)BattlePlaceId.Leader * 10);
        }

        return currentSelectableList;
    }

    public int GetUseCost(Leader leader) {
        return CurrentCard.cost;
    }

    public bool IsUsable(BattleUnit sourceUnit) {
        bool isCostEnough = sourceUnit.leader.PP >= GetUseCost(sourceUnit.leader);
        bool isFieldFull = CurrentCard.IsFollower() && sourceUnit.field.IsFull;
        bool isSpellTargetable = true;

        if (CurrentCard.Type == CardType.Spell) {
            GetTargetEffectWithTiming("on_this_use", out _, out var infoQueue, out var selectableQueue);

            while (infoQueue.Count > 0) {
                isSpellTargetable = infoQueue.Dequeue().num <= selectableQueue.Dequeue().Count;
                if (!isSpellTargetable)
                    break;
            }
        }

        return isCostEnough && (!isFieldFull) && isSpellTargetable;
    }

    public int GetEvolveCost() {
        return 1;
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

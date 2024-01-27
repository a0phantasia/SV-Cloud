using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleCard : IIdentifyHandler
{
    public Battle Battle => Player.currentBattle;
    public BattleManager Hud => BattleManager.instance;

    public int Id => CurrentCard.Id;
    public bool IsEvolved { get; protected set; }
    public Card baseCard;
    public Card evolveCard;
    public Card OriginalCard => IsEvolved ? evolveCard : baseCard;
    public Card CurrentCard => GetCurrentCard(OriginalCard);
    public List<Effect> newEffects = new List<Effect>();
    public BattleCardBuffController buffController;
    public BattleCardActionController actionController;

    public Dictionary<string, float> options = new Dictionary<string, float>();
    
    public BattleCard(Card card) {
        IsEvolved = false;

        baseCard = (card == null) ? null : new Card(card);
        evolveCard = (baseCard?.EvolveCard == null) ? null : new Card(baseCard.EvolveCard);
        
        baseCard?.effects.ForEach(x => x.source = this);
        evolveCard?.effects.ForEach(x => x.source = this);

        buffController = new BattleCardBuffController();
        actionController = new BattleCardActionController();
    }

    public BattleCard(BattleCard rhs) {
        IsEvolved = rhs.IsEvolved;
        
        baseCard = (rhs.baseCard == null) ? null : new Card(rhs.baseCard);
        evolveCard = (rhs.evolveCard == null) ? null : new Card(rhs.evolveCard);

        baseCard?.effects.ForEach(x => x.source = this);
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
        string trimId;

        if (id.TryTrimStart("current", out trimId)) {
            if (trimId.TryTrimParentheses(out var option)) {
                return option switch {
                    "IF" => BattleCardFilter.Parse(trimId.TrimStart("[IF]")).FilterWithCurrentCard(this) ? 1 : 0,
                    _ => 0
                };
            }
            return CurrentCard.GetIdentifier(trimId.TrimStart('.'));
        }

        if (id.TryTrimStart("action.", out trimId))
            return actionController.GetIdentifier(trimId);

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
        result.cost = Mathf.Max(result.cost + buffController.CostBuff, 0);
        result.atk = Mathf.Max(result.atk + buffController.AtkBuff, 0);
        result.hpMax = Mathf.Max(result.hpMax + buffController.HpBuff, 0);
        result.hp = Mathf.Max(result.hpMax - buffController.Damage, 0);
        result.effects.AddRange(newEffects);
        result.effects.ForEach(x => x.source = this);
        return result;
    }

    public string GetAdditionalDescription() {
        var card = CurrentCard;
        var description = string.Empty;
        var leaderInfoKeys = new List<string>();
        var sourceInfoKeys = new List<string>();

        for (int i = 0; i < card.effects.Count; i++) {
            var currentEffect = card.effects[i];
            if (List.IsNullOrEmpty(currentEffect.condition))
                continue;

            for (int j = 0; j < currentEffect.condition.Count; j++) {
                var currentCondition = currentEffect.condition[j];
                if (currentCondition.TryTrimStart("leader.", out var leaderKey))
                    leaderInfoKeys.Add(leaderKey);
                else if (currentCondition.TryTrimStart("source.", out var sourceKey))
                    sourceInfoKeys.Add(sourceKey);
            }
        }
        leaderInfoKeys = leaderInfoKeys.Distinct().ToList();
        sourceInfoKeys = sourceInfoKeys.Distinct().ToList();

        for (int i = 0; i < leaderInfoKeys.Count; i++) {
            var num = Hud.CurrentState.GetBelongUnit(this).leader.GetIdentifier(leaderInfoKeys[i]);
            description += "（當前" + leaderInfoKeys[i].ToLeaderInfoValue() + "為 " + num + "）\n";
        }

        for (int i = 0; i < sourceInfoKeys.Count; i++) {
            description += "（當前" + sourceInfoKeys[i].ToSourceInfoValue() + "為 " + GetIdentifier(sourceInfoKeys[i]) + "）\n";
        }

        return description;
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
                Enumerable.Range(0, myField.Count).Where(x => currentInfo.filter.FilterWithCurrentCard(myField.cards[x])).ToList();
                    
            var opIndex = (currentInfo.unit == "me") ? new List<int>() : 
                Enumerable.Range(0, opField.Count).Where(x => opField.cards[x].IsTargetSelectable() && 
                    currentInfo.filter.FilterWithCurrentCard(opField.cards[x])).ToList();

            myIndex.ForEach(x => currentSelectableList.Add((short)((short)BattlePlaceId.Field * 10 + x)));
            opIndex.ForEach(x => currentSelectableList.Add((short)(100 + (short)BattlePlaceId.Field * 10 + x)));

            if ((currentInfo.unit != "op") && (currentInfo.places.Contains(BattlePlaceId.Leader)))
                currentSelectableList.Add((short)BattlePlaceId.Leader * 10);

            if ((currentInfo.unit != "me") && (currentInfo.places.Contains(BattlePlaceId.Leader)))
                currentSelectableList.Add(100 + (short)BattlePlaceId.Leader * 10);

        }

        if (currentInfo.mode.Contains("other"))
            currentSelectableList.Remove(Battle.CurrentState.GetCardPlaceInfo(this).ToShortCode());

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

        return sourceUnit.isMyTurn && isCostEnough && (!isFieldFull) && isSpellTargetable;
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

    public bool IsTargetSelectable() {
        bool isAmbush = actionController.IsKeywordAvailable(CardKeyword.Ambush);
        return !isAmbush;
    }

    public void RemoveUntilEffect() {
        buffController.RemoveUntilEffect();    
    }

    // Evolve this follower. You should check IsEvolvable() before calling this if you use EP evolve.
    public void Evolve() {
        IsEvolved = true;
    }

    public void SetKeyword(CardKeyword keyword, ModifyOption option) {
        if (option == ModifyOption.Add) {
            baseCard.keywords.Add(keyword);
            evolveCard?.keywords.Add(keyword);
            actionController.AddIdentifier(keyword.GetKeywordEnglishName(), 1);
        } else if (option == ModifyOption.Remove) {
            baseCard.keywords.RemoveAll(x => x == keyword);
            evolveCard?.keywords.RemoveAll(x => x == keyword);
            actionController.SetIdentifier(keyword.GetKeywordEnglishName(), 0);
        }
    }

    public int TakeDamage(int damage) {
        return buffController.TakeDamage(damage);
    }

    public int TakeHeal(int heal) {
        return buffController.TakeHeal(heal);
    }

    public void TakeBuff(CardStatus status, Func<bool> untilCondition) {
        buffController.TakeBuff(status, untilCondition);
    }

    public int TakeCountdown(int add) {
        if (baseCard.countdown > 0)
            baseCard.countdown = Mathf.Max(baseCard.countdown + add, 0); 
            
        return add;
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;

public static class EffectAbilityHandler 
{
    public static Battle Battle => Player.currentBattle;
    public static BattleManager Hud => BattleManager.instance;

    /// <summary>
    /// Check if cards have effects with correct timing and condition.
    /// Enqueue these effects with corresponding invoke unit.
    /// </summary>
    private static List<Effect> EnqueueEffect(string timing, List<BattleCard> battleCards, BattleState state, bool enqueueToBattle = true) {
        var list = new List<Effect>();
        var units = battleCards.Select(state.GetBelongUnit).ToList();
        var cards = battleCards.Select(x => x.CurrentCard).ToList();
        var otherTiming = "on_other_" + timing.TrimStart("on_");

        for (int i = 0; i < cards.Count; i++) {
            if (cards[i] == null)
                continue;

            var effects = cards[i].effects;
            for (int j = 0; j < effects.Count; j++) {
                effects[j].invokeUnit = units[i];

                bool isCorrectTiming = (effects[j].timing == timing);
                bool isOtherTiming = (effects[j].timing == otherTiming) && (state.currentEffect.source != battleCards[i]);

                if ((isCorrectTiming || isOtherTiming) && (effects[j].Condition(state))) {
                    list.Add(effects[j]);

                    if (enqueueToBattle)
                        Battle.EnqueueEffect(effects[j]);
                } else {
                    effects[j].invokeUnit = null;
                }
            }
        }     
        return list;                                                     
    }

    /// <summary>
    /// Check hand, leader, territory, field, deck cards, <br/>
    /// and enqueue effects with correct timing and condition.
    /// </summary>
    private static List<Effect> OnPhaseChange(string timing, BattleState state, bool enqueueToBattle = true) {
        var lhsUnit = state.currentUnit;
        var rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        var cards = lhsUnit.hand.cards.Concat(rhsUnit.hand.cards);

        // cards.Append(rhsUnit.territory);
        // cards.Append(rhsUnit.territory);
        
        var result = cards.Concat(lhsUnit.leader.cards).Concat(rhsUnit.leader.cards)
            .Concat(lhsUnit.field.cards).Concat(rhsUnit.field.cards)
            .Concat(lhsUnit.deck.cards).Concat(rhsUnit.deck.cards).ToList();

        return EnqueueEffect(timing, result, state, enqueueToBattle);
    }

    public static bool SetResult(this Effect effect, BattleState state) {
        if (state == null)
            return false;

        // Get Master's Result State.
        string who = effect.abilityOptionDict.Get("who", "me");
        string result = effect.abilityOptionDict.Get("result", "none");
        var unit = (who == "me") ? effect.invokeUnit : state.GetRhsUnitById(effect.invokeUnit.id);
        var resultState = result switch {
            "win" => unit.IsMasterUnit ? BattleResultState.Win : BattleResultState.Lose,
            "lose" => unit.IsMasterUnit ? BattleResultState.Lose : BattleResultState.Win,
            _ => BattleResultState.None,
        };  
        state.result.masterState = resultState;
        Hud.SetState(state);
        return true;
    }

    public static bool KeepCard(this Effect effect, BattleState state) {
        string indexList = effect.abilityOptionDict.Get("change", "none");
        var changeList = indexList.ToIntList('/');
        
        if ((state == null) || (changeList == null))
            return false;

        // Check if change index OK.
        var unit = effect.invokeUnit;
        bool isMyUnit = state.myUnit.id == unit.id;
        for (int i = 0; i < changeList.Count; i++) {
            if (changeList[i] >= unit.hand.Count)
                return false;
        }
        
        // Replace hand with new cards.
        var changeCards = changeList.Select(x => unit.hand.cards[x]).ToList();
        var newCards = unit.deck.cards.Take(changeList.Count).ToList();
        for (int i = 0; i < changeList.Count; i++) {
            unit.hand.cards[changeList[i]] = newCards[i];
        }

        // Remove new cards from deck and Shuffle.
        if (changeCards.Count > 0) {
            unit.deck.cards.RemoveRange(0, Mathf.Min(newCards.Count, unit.deck.Count));
            unit.deck.cards.AddRange(changeCards);
            unit.deck.cards.Shuffle();
        }
        effect.invokeTarget = newCards;
        unit.isDone = true;

        // Set UI.
        string log = (isMyUnit ? "我方" : "對方") + "交換了 " + newCards.Count + " 張手牌";
        effect.hudOptionDict.Set("log", log);
        Hud.SetState(state);

        // Handle next action.
        if (state.settings.isLocal && (state.myUnit.id == unit.id))
            Battle.PlayerAction(new int[] { (int)EffectAbility.KeepCard }, false);
        
        if (state.myUnit.isDone && state.opUnit.isDone) {
            Effect turnStart = new Effect(new int[] { (int)EffectAbility.TurnStart })
            {
                source = state.currentUnit.leader.leaderCard,
                invokeUnit = state.currentUnit
            };
            Battle.EnqueueEffect(turnStart);
        }
        return true;
    }

    public static bool OnTurnStart(this Effect effect, BattleState state) {
        var unit = effect.invokeUnit;
        bool isMyUnit = state.myUnit.id == unit.id;

        state.myUnit.isDone = state.opUnit.isDone = false;

        // Add turn and Recover pp (max 10)
        unit.turn += 1;
        unit.leader.PPMax = Mathf.Min(unit.leader.PPMax + 1, 10);
        unit.leader.PP = unit.leader.PPMax;

        // Clear data.
        unit.leader.ClearTurnIdentifier();
        
        // If specific turn comes, give player EP.
        var first = unit.isFirst ? 1 : 0;
        if (unit.turn - first == state.settings.evolveStart) {
            unit.leader.EpMax = unit.isFirst ? 2 : 3;
            unit.leader.EP = unit.leader.EpMax;
            effect.hudOptionDict.Set("ep", "true");
        }

        // On Turn Start In Field.
        unit.field.cards.ForEach(x => x.actionController.OnTurnStartInField());

        // Set UI
        string log = (isMyUnit ? "YOUR" : "ENEMY") + " TURN (" + unit.turn + ")";
        effect.hudOptionDict.Set("log", log);
        Hud.SetState(state);

        // If turn over 40, lose.
        if (unit.turn > 40) {
            Effect lose = new Effect(new int[] { (int)EffectAbility.SetResult, (int)BattleResultState.Lose })
            {
                source = unit.leader.leaderCard,
                invokeUnit = unit
            };
            lose.Apply(state);
            return true;
        }

        // On turn start.
        OnPhaseChange("on_turn_start", state);

        // Draw cards.
        int drawCount = ((!unit.isFirst) && (unit.turn == 1)) ? 2 : 1;
        Effect draw = new Effect(new int[] { (int)EffectAbility.Draw, drawCount })
        {
            source = unit.leader.leaderCard,
            invokeUnit = unit
        };
        Battle.EnqueueEffect(draw);
        
        //! For debug test.
        if (GameManager.instance.debugMode && (unit.id != state.myUnit.id)) {
            Effect use = new Effect(new int[] { (int)EffectAbility.Use, 0 })
            {
                source = unit.leader.leaderCard,
                invokeUnit = unit
            };
            Effect turnEnd = new Effect(new int[] { (int)EffectAbility.TurnEnd })
            {
                source = unit.leader.leaderCard,
                invokeUnit = unit
            };
            Battle.EnqueueEffect(use);
            Battle.EnqueueEffect(turnEnd);
        }
        return true;
    }

    public static bool OnTurnEnd(this Effect effect, BattleState state) {
        var unit = effect.invokeUnit;
        bool isMyUnit = state.myUnit.id == unit.id;

        unit.isDone = true;

        // Set UI
        string log = "回合結束";
        effect.hudOptionDict.Set("log", log);
        Hud.SetState(state);

        // On turn end.
        OnPhaseChange("on_turn_end", state);
        
        // Check if next turn is mine (Add turn effect)
        int addTurn = (int)unit.leader.GetIdentifier("addTurn");
        if (addTurn > 0) {
            unit.leader.SetIdentifier("addTurn", addTurn - 1);
        } else {
            state.IsMasterTurn = !state.IsMasterTurn;
        }

        // Change turn.
        Effect turnStart = new Effect(new int[] { (int)EffectAbility.TurnStart })
        {
            source = state.currentUnit.leader.leaderCard,
            invokeUnit = state.currentUnit
        };
        Battle.EnqueueEffect(turnStart);
        return true;
    }

    public static bool Use(this Effect effect, BattleState state) {
        var unit = effect.invokeUnit;

        int index = int.Parse(effect.abilityOptionDict.Get("index", "0"));
        if (!index.IsInRange(0, unit.hand.Count))
            return false;

        var useCard = unit.hand.cards[index];
        if (!useCard.IsUsable(unit))
            return false;

        effect.source = useCard;
        effect.invokeTarget = new List<BattleCard>() { useCard };

        var cost = useCard.GetUseCost(unit.leader);
        unit.leader.PP -= cost;
        unit.hand.cards.Remove(useCard);
        useCard.buffController.costBuff = 0;

        // Record used card.
        unit.leader.AddIdentifier("combo", 1);
        unit.grave.usedCards.Add(useCard.card);

        // Follower and Amulet goes to field.
        if (useCard.CurrentCard.IsFollower() || (useCard.CurrentCard.Type == CardType.Amulet)) {
            var abilityOptionDict = new Dictionary<string, string>() { { "where", "hand" } };
            Effect summon = new Effect("none", "none", EffectCondition.None, null, EffectAbility.Summon, abilityOptionDict) 
            {
                source = useCard,
                invokeUnit = unit,
                invokeTarget = new List<BattleCard>() { useCard },
            };
            Battle.EnqueueEffect(summon);
        }

        // Use effect.
        EnqueueEffect("on_this_use", effect.invokeTarget, state);

        // Spell goes to grave immediately.
        if (useCard.CurrentCard.Type == CardType.Spell) {
            unit.grave.GraveCount += 1;
            unit.grave.cards.Add(useCard);
        }

        effect.hudOptionDict.Set("log", "使用" + useCard.CurrentCard.name);
        Hud.SetState(state);

        OnPhaseChange("on_use", state);
        return true;
    }

    public static bool Attack(this Effect effect, BattleState state) {
        var unit = effect.invokeUnit;
        var targetUnit = state.GetRhsUnitById(unit.id);

        bool isMyUnit = state.myUnit.id == unit.id;

        var source = int.Parse(effect.abilityOptionDict.Get("source", "-1"));
        var target = int.Parse(effect.abilityOptionDict.Get("target", "-1"));

        bool isSourceLeader = source == -1;
        bool isTargetLeader = target == -1;

        var sourceCard = isSourceLeader ? unit.leader.leaderCard : unit.field.cards[source];
        var targetCard = isTargetLeader ? targetUnit.leader.leaderCard : targetUnit.field.cards[target];

        effect.source = sourceCard;
        effect.invokeTarget = new List<BattleCard>() { targetCard };

        // Consume attack chance.
        sourceCard.actionController.CurrentAttackChance -= 1;

        effect.hudOptionDict.Set("log", sourceCard.CurrentCard.name + " 攻擊 " + targetCard.CurrentCard.name);
        effect.hudOptionDict.Set("source", source.ToString());
        effect.hudOptionDict.Set("target", target.ToString());
        Hud.SetState(state);

        // OnAttack and OnDefense
        var onThisAttack = EnqueueEffect("on_this_attack", new List<BattleCard>() { sourceCard }, state, false);
        var onThisDefense = EnqueueEffect("on_this_defense", new List<BattleCard>() { sourceCard, targetCard }, state, false);
        var onAttack = OnPhaseChange("on_attack", state, false);
        var onDefense = OnPhaseChange("on_defense", state, false);

        var effectList = onThisAttack.Concat(onThisDefense).Concat(onAttack).Concat(onDefense).ToList();
        effectList.ForEach(x => x.CheckAndApply(state));

        // Check if they are still in field after above effects.
        var isSourceInField = isSourceLeader || unit.field.cards.Contains(sourceCard);
        var isTargetInField = isTargetLeader || targetUnit.field.cards.Contains(targetCard); 

        if ((!isSourceInField) || (!isTargetInField))
            return false;

        // Give damage.
        Effect attackEffect = new Effect("none", "none", EffectCondition.None, null,
            EffectAbility.Damage, new Dictionary<string, string>() { 
                { "situation", "attack" },
                { "damage", sourceCard.CurrentCard.atk.ToString() },
            }) 
        {
            source = sourceCard,
            invokeUnit = unit,
            invokeTarget = new List<BattleCard>(){ targetCard },
        };
        
        Effect defenseEffect = new Effect("none", "none", EffectCondition.None, null,
            EffectAbility.Damage, new Dictionary<string, string>() { 
                { "situation", "defense" },
                { "damage", targetCard.CurrentCard.atk.ToString() },
            }) 
        {
            source = targetCard,
            invokeUnit = targetUnit,
            invokeTarget = new List<BattleCard>(){ sourceCard },
        };

        attackEffect.Apply(state);

        if ((!isTargetLeader) || (targetCard.CurrentCard.atk > 0))
            defenseEffect.Apply(state);

        return true;
    }

    public static bool Evolve(this Effect effect, BattleState state) {
        var unit = effect.invokeUnit;
        bool isMyUnit = state.myUnit.id == unit.id;

        // Index and card is used when players use EP to evolve follower
        int index = int.Parse(effect.abilityOptionDict.Get("index", "-1"));
        var card = index.IsInRange(0, unit.field.Count) ? unit.field.cards[index] : null;

        if (card == null)
            effect.SetInvokeTarget(state);
        else {
            effect.source = card;
            effect.invokeTarget = new List<BattleCard>() { card };
        }

        if ((card != null) && (card.IsEvolvable(unit))) {
            unit.leader.EP -= card.GetEvolveCost();
            unit.leader.isEpUsed = true;
        }

        effect.invokeTarget.ForEach(x => x.Evolve());

        effect.hudOptionDict.Set("log", effect.invokeTarget.Select(x => x.CurrentCard.name + "進化").ConcatToString());
        effect.hudOptionDict.Set("index", index.ToString());
        Hud.SetState(state);

        if (card != null) {
            EnqueueEffect("on_this_evolve_with_ep", effect.invokeTarget, state);
            OnPhaseChange("on_evolve_with_ep", state);
        }

        EnqueueEffect("on_this_evolve", effect.invokeTarget, state);
        OnPhaseChange("on_evolve", state);

        return true;
    }

    public static bool SetKeyword(this Effect effect, BattleState state) {

        effect.SetInvokeTarget(state);

        var modify = effect.abilityOptionDict.Get("modify", "add");
        var modifyOption = (modify == "add") ? ModifyOption.Add : ModifyOption.Remove;
        var keywordId = int.Parse(effect.abilityOptionDict.Get("keyword", "1"));
        var keyword = (CardKeyword)keywordId;
        var keywordName = keyword.GetKeywordName();
        var keywordEnglishName = keyword.GetKeywordEnglishName();

        for (int i = 0; i < effect.invokeTarget.Count; i++) {
            var actionController = effect.invokeTarget[i].actionController;
            var setNum = (modifyOption == ModifyOption.Add) ? (actionController.GetIdentifier(keywordEnglishName) + 1) : 0; 
            effect.invokeTarget[i].actionController.SetIdentifier(keywordEnglishName, setNum);
            effect.invokeTarget[i].SetKeyword(keyword, modifyOption);
        }

        effect.hudOptionDict.Set("log", effect.invokeTarget.Select(x => x.CurrentCard.name + "獲得" + keywordName + "效果").ConcatToString());
        Hud.SetState(state);

        EnqueueEffect("on_this_" + keywordEnglishName, effect.invokeTarget, state);
        OnPhaseChange("on_" + keywordEnglishName, state);
        return true;
    }

    public static bool Draw(this Effect effect, BattleState state) {
        var unit = effect.invokeUnit;
        bool isMyUnit = state.myUnit.id == unit.id;

        var drawCount = Parser.ParseEffectExpression(effect.abilityOptionDict.Get("count", "1"), effect, state);
        var filterOptions = effect.abilityOptionDict.Get("filter", "none");
        var inGraveCards = new List<BattleCard>();

        if (filterOptions == "none") {
            var total = unit.Draw(drawCount, out effect.invokeTarget, out inGraveCards);

            unit.grave.GraveCount += inGraveCards.Count;

            string log = "抽取 " + drawCount + " 張卡片\n";       
            if (inGraveCards.Count > 0) {
                if (isMyUnit)
                    inGraveCards.ForEach(x => log += x.CurrentCard.name + " 爆牌進入墓地\n");
                else
                    log += inGraveCards.Count + " 張卡片爆牌進入墓地\n";
            }

            effect.hudOptionDict.Set("log", log);
            effect.hudOptionDict.Set("count", drawCount.ToString());
            Hud.SetState(state);

            if (total.Count < drawCount) {
                int result = (int)unit.leader.GetIdentifier("deckOutResult");            
                var resultState = (result == 0) ? BattleResultState.Lose : BattleResultState.Win;
                Effect resultEffect = new Effect(new int[] { (int)EffectAbility.SetResult, (int)resultState, (int)BattleLoseReason.Deckout })
                {
                    source = unit.leader.leaderCard,
                    invokeUnit = unit
                };
                resultEffect.Apply(state);
                return true;
            }
        } else {
            //TODO Filter cards. Don't invoke win/lose when deck out.
            //TODO filter format -> filter=(type:option)(type:option)(type:option)
            /*
            var filterConditions = filterOptions.TrimStart("(").TrimEnd(")").Split(new string[] { ")(" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < filterConditions.Length; i++) {
                var entry = filterConditions[i].Split(':');
                
            }
            */
        }
        EnqueueEffect("on_this_draw", effect.invokeTarget, state);
        EnqueueEffect("on_this_draw_discard", inGraveCards, state);
        OnPhaseChange("on_draw", state);
        return true;
    }

    public static bool Summon(this Effect effect, BattleState state) {
        var unit = effect.invokeUnit;
        
        var who = effect.abilityOptionDict.Get("who", "me");
        var summonUnit = (who == "me") ? unit : state.GetRhsUnitById(unit.id);
        var field = effect.abilityOptionDict.Get("field", "me");
        var fieldUnit = (field == "me") ? unit : state.GetRhsUnitById(unit.id);
        var where = effect.abilityOptionDict.Get("where", "hand");
        var id = effect.abilityOptionDict.Get("id", "none");
        var count = effect.abilityOptionDict.Get("count", "none");
        var availableCount = fieldUnit.field.MaxCount - fieldUnit.field.Count;

        if (availableCount <= 0)
            return false;

        var target = effect.invokeTarget.Take(availableCount).ToList();

        for (int i = 0; i < Mathf.Min(availableCount, target.Count); i++) {
            effect.invokeTarget = new List<BattleCard>() { target[i] };

            switch (where) {
                default:
                    break;
                case "hand":
                    summonUnit.hand.cards.RemoveRange(effect.invokeTarget);
                    break;
            }

            fieldUnit.field.cards.AddRange(effect.invokeTarget);

            EnqueueEffect("on_this_summon", effect.invokeTarget, state);
            OnPhaseChange("on_summon", state);
        }

        fieldUnit.leader.AddIdentifier("rally", target.Count(x => x.CurrentCard.IsFollower()));

        effect.invokeTarget = target;
        effect.hudOptionDict.Set("log", effect.invokeTarget.Select(x => x.CurrentCard.name + "進入戰場").ConcatToString());
        Hud.SetState(state);

        return true;
    }

    public static bool Damage(this Effect effect, BattleState state) {

        effect.SetInvokeTarget(state);

        // Take damage.
        var situation = effect.abilityOptionDict.Get("situation", "none");
        var damage = Parser.ParseEffectExpression(effect.abilityOptionDict.Get("damage", "0"), effect, state);

        // Set UI.
        List<int> damageAllList = effect.invokeTarget.Select(x => x.TakeDamage(damage)).ToList();
        List<int> myIndexList = new List<int>();
        List<int> myDamageList = new List<int>();
        List<int> opIndexList = new List<int>();
        List<int> opDamageList = new List<int>();

        for (int i = 0; i < effect.invokeTarget.Count; i++) {
            var belongUnit = state.GetBelongUnit(effect.invokeTarget[i]);
            var indexList = (belongUnit.id == state.myUnit.id) ? myIndexList : opIndexList;
            var damageList = (belongUnit.id == state.myUnit.id) ? myDamageList : opDamageList;
            var index = (effect.invokeTarget[i].CurrentCard.Type == CardType.Leader) ? -1 :
                belongUnit.field.cards.IndexOf(effect.invokeTarget[i]);

            indexList.Add(index);
            damageList.Add(damageAllList[i]);
        }
    
        effect.hudOptionDict.Set("log", effect.invokeTarget.Select((x, i) => effect.source.CurrentCard.name + "給予" + x.CurrentCard.name + " " + damageAllList[i] + " 點傷害").ConcatToString());
        effect.hudOptionDict.Set("situation", situation);
        effect.hudOptionDict.Set("myIndex", myIndexList.Select(x => x.ToString()).ConcatToString("/"));
        effect.hudOptionDict.Set("myDamage", myDamageList.Select(x => x.ToString()).ConcatToString("/"));
        effect.hudOptionDict.Set("opIndex", opIndexList.Select(x => x.ToString()).ConcatToString("/"));
        effect.hudOptionDict.Set("opDamage", opDamageList.Select(x => x.ToString()).ConcatToString("/"));
        Hud.SetState(state);

        // If hp <= 0, destroy.
        for (int i = 0; i < effect.invokeTarget.Count; i++) {
            if (effect.invokeTarget[i].CurrentCard.hp <= 0) {
                Effect destroyEffect = new Effect("none", "self", EffectCondition.None, null,
                    EffectAbility.Destroy, null)
                {
                    source = effect.invokeTarget[i],
                    invokeUnit = state.GetBelongUnit(effect.invokeTarget[i]),
                };

                // Attack or defense need to wait op, so only enqueue.
                if ((situation == "attack") || (situation == "defense"))
                    Battle.EnqueueEffect(destroyEffect);
                else
                    destroyEffect.Apply(state);
            }
        }

        EnqueueEffect("on_this_damage", effect.invokeTarget, state);
        OnPhaseChange("on_damage", state);

        return true;
    }

    public static bool Destroy(this Effect effect, BattleState state) {

        effect.SetInvokeTarget(state);

        string log = string.Empty;

        for (int i = 0; i < effect.invokeTarget.Count; i++) {
            var belongUnit = state.GetBelongUnit(effect.invokeTarget[i]);
            var card = effect.invokeTarget[i].CurrentCard;

            // Leader hp <= 0, lose.
            if (card.Type == CardType.Leader) {
                Effect resultEffect = new Effect(new int[] { (int)EffectAbility.SetResult, (int)BattleResultState.Lose, (int)BattleLoseReason.LeaderDie })
                {
                    source = belongUnit.leader.leaderCard,
                    invokeUnit = belongUnit,
                };
                resultEffect.Apply(state);
                return true;                
            }

            // Check if still in field.
            if (!belongUnit.field.cards.Contains(effect.invokeTarget[i]))
                continue;

            // Remove and grave++;
            effect.invokeTarget[i].SetIdentifier("graveReason", (float)BattleCardGraveReason.Destroy);
            belongUnit.field.cards.Remove(effect.invokeTarget[i]);
            belongUnit.grave.cards.Add(effect.invokeTarget[i]);
            belongUnit.grave.GraveCount += 1;

            // Leader info.
            switch (card.Type) {
                default:
                    break;
                case CardType.Follower:
                case CardType.Evolved:
                    belongUnit.leader.AddIdentifier("destroyedFollowerCount", 1);
                    break;
                case CardType.Amulet:
                    belongUnit.leader.AddIdentifier("destroyedAmuletCount", 1);
                    break;
            }

            log += card.name + "被破壞\n";
        }

        effect.hudOptionDict.Set("log", log);
        Hud.SetState(state);

        EnqueueEffect("on_this_leave_field", effect.invokeTarget, state);
        OnPhaseChange("on_leave_field", state);

        EnqueueEffect("on_this_destroy", effect.invokeTarget, state);
        OnPhaseChange("on_destroy", state);

        return true;
    }

    public static bool Buff(this Effect effect, BattleState state) {

        effect.SetInvokeTarget(state);

        var add = effect.abilityOptionDict.Get("add", "none").ToIntList('/');
        if (List.IsNullOrEmpty(add))
            return false;

        int atk = add[0], hp = add[1];
        string log = string.Empty;

        for (int i = 0; i < effect.invokeTarget.Count; i++) {
            effect.invokeTarget[i].TakeBuff(atk, hp);
            log += effect.invokeTarget[i].CurrentCard.name + "獲得+" + atk + "/+" + hp + "效果\n";
        }

        effect.hudOptionDict.Set("log", log);
        Hud.SetState(state);

        EnqueueEffect("on_this_buff", effect.invokeTarget, state);
        OnPhaseChange("on_buff", state);
        return true;
    }

    public static bool GetToken(this Effect effect, BattleState state) {
        var who = effect.abilityOptionDict.Get("who", "me");
        var tokenIds = effect.abilityOptionDict.Get("id", string.Empty).ToIntList('/');
        var tokenCountExpr = effect.abilityOptionDict.Get("count", string.Empty).Split('/');
        var tokenCounts = tokenCountExpr.Select(x => Parser.ParseEffectExpression(x, effect, state)).ToList(); 

        var tokenUnit = (who == "me") ? effect.invokeUnit : state.GetRhsUnitById(effect.invokeUnit.id);
        var isMyUnit = tokenUnit.id == state.myUnit.id;

        List<BattleCard> tokens = new List<BattleCard>();
        List<BattleCard> inHand = new List<BattleCard>();
        List<BattleCard> inGrave = new List<BattleCard>();

        var availableCount = tokenUnit.hand.MaxCount - tokenUnit.hand.Count;

        for (int i = 0; i < tokenIds.Count; i++) {
            tokens.AddRange(Enumerable.Repeat(tokenIds[i], tokenCounts[i]).Select(BattleCard.Get));
        }

        if (tokens.Count > availableCount) {
            inHand = tokens.GetRange(0, availableCount);
            inGrave = tokens.GetRange(availableCount, tokens.Count - availableCount);
        } else {
            inHand = tokens.GetRange(0, tokens.Count);
            inGrave = new List<BattleCard>();
        }

        inGrave.ForEach(x => x.SetIdentifier("graveReason", (float)BattleCardGraveReason.DrawTooMuch));

        tokenUnit.hand.cards.AddRange(inHand);
        tokenUnit.grave.cards.AddRange(inGrave);

        tokenUnit.grave.GraveCount += inGrave.Count;

        string log = inHand.Select(x => "增加" + x.CurrentCard.name + "到" + 
            (isMyUnit ? "我方" : "對方") + "手牌中").ConcatToString();
            
        if (inGrave.Count > 0) {
            if (isMyUnit)
                inGrave.ForEach(x => log += x.CurrentCard.name + " 爆牌進入墓地\n");
            else
                log += inGrave.Count + " 張卡片爆牌進入墓地\n";
        }

        effect.hudOptionDict.Set("log", log);
        effect.hudOptionDict.Set("who", isMyUnit ? "0" : "1");
        effect.hudOptionDict.Set("token", tokens.Select(x => x.Id.ToString()).ConcatToString("/"));
        Hud.SetState(state);

        return true;
    }
}

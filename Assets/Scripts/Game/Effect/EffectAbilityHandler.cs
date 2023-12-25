using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EffectAbilityHandler 
{
    public static Battle Battle => Player.currentBattle;
    public static BattleManager Hud => BattleManager.instance;

    /// <summary>
    /// Check if cards have effects with correct timing and condition.
    /// Enqueue these effects.
    /// </summary>
    private static void EnqueueEffect(string timing, List<Card> cards, BattleState state) {
        for (int i = 0; i < cards.Count; i++) {
            if (cards[i] == null)
                continue;

            var effects = cards[i].effects;
            for (int j = 0; j < effects.Count; j++) {
                if ((effects[j].timing == timing) && (effects[j].Condition(state))) {
                    Battle.EnqueueEffect(effects[j]);
                }
            }
        }                                                                
    }

    /// <summary>
    /// Check hand, leader, territory, field, deck cards, <br/>
    /// and enqueue effects with correct timing and condition.
    /// </summary>
    /// <param name="timing"></param>
    /// <param name="state"></param>
    private static void OnPhaseChange(string timing, BattleState state) {
        var lhsUnit = state.currentUnit;
        var rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        var cards = lhsUnit.hand.cards.Concat(rhsUnit.hand.cards);

        cards.Append(lhsUnit.leader.leaderCard);
        cards.Append(rhsUnit.leader.leaderCard);
        cards.Append(rhsUnit.territory);
        cards.Append(rhsUnit.territory);
        
        cards = cards.Concat(lhsUnit.field.cards).Concat(rhsUnit.field.cards)
            .Concat(lhsUnit.deck.cards).Concat(rhsUnit.deck.cards);

        EnqueueEffect(timing, cards.Select(x => x.CurrentCard).ToList(), state);
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
        
        // If specific turn comes, give player EP.
        var first = unit.isFirst ? 1 : 0;
        if (unit.turn - first == 4) {
            unit.leader.EpMax = unit.isFirst ? 2 : 3;
            unit.leader.EP = unit.leader.EpMax;
            effect.hudOptionDict.Set("ep", "true");
        }

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
            lose.Apply();
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
        
        if (GameManager.instance.debugMode && (unit.id != state.myUnit.id)) {
            Effect turnEnd = new Effect(new int[] { (int)EffectAbility.TurnEnd })
            {
                source = unit.leader.leaderCard,
                invokeUnit = unit
            };
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
        bool isMyUnit = state.myUnit.id == unit.id;

        int index = int.Parse(effect.abilityOptionDict.Get("index", "0"));
        if (!index.IsInRange(0, unit.hand.Count))
            return false;

        var card = unit.hand.cards[index];
        if (!card.IsUsable(unit))
            return false;

        effect.invokeTarget = new List<BattleCard>() { card };

        var cost = card.GetUseCost(unit.leader);
        unit.leader.PP -= cost;

        effect.hudOptionDict.Set("log", "使用" + card.CurrentCard.name);
        Hud.SetState(state);

        switch (card.CurrentCard.Type) {
            default:
                break;
            case CardType.Follower:
            case CardType.Amulet:
                var abilityOptionDict = new Dictionary<string, string>() 
                { 
                    { "where", "hand" },
                    { "index", index.ToString() },
                };
                Effect summon = new Effect("none", EffectTarget.None, EffectCondition.None, null, EffectAbility.Summon, abilityOptionDict) 
                {
                    source = unit.leader.leaderCard,
                    invokeUnit = unit
                };
                Battle.EnqueueEffect(summon);
                break;
        };
        EnqueueEffect("on_be_use", effect.invokeTarget.Select(x => x.CurrentCard).ToList() , state);
        OnPhaseChange("on_use", state);
        return true;
    }

    public static bool Ward(this Effect effect, BattleState state) {
        var unit = effect.invokeUnit;
        //TODO invokeUnit is null. Check invokeUnit of effect?

        effect.invokeTarget = effect.target switch {
            EffectTarget.Self => new List<BattleCard>() { effect.source },
            _ => new List<BattleCard>() { effect.source },
        };

        for (int i = 0; i < effect.invokeTarget.Count; i++) {
            var ward = effect.invokeTarget[i].actionController.GetIdentifier("ward");
            effect.invokeTarget[i].actionController.SetIdentifier("ward", ward + 1);
        }

        string log = effect.source.CurrentCard.name + "給予" + "自己" + "守護效果";
        effect.hudOptionDict.Set("log", log);
        Hud.SetState(state);

        EnqueueEffect("on_be_ward", effect.invokeTarget.Select(x => x.CurrentCard).ToList() , state);
        OnPhaseChange("on_ward", state);
        return true;
    }

    public static bool Draw(this Effect effect, BattleState state) {
        var unit = effect.invokeUnit;
        bool isMyUnit = state.myUnit.id == unit.id;

        int drawCount = int.Parse(effect.abilityOptionDict.Get("count", "1"));
        var filterOptions = effect.abilityOptionDict.Get("filter", "none");
        var inGraveCards = new List<BattleCard>();


        if (filterOptions == "none") {
            var total = unit.Draw(drawCount, out effect.invokeTarget, out inGraveCards);

            string logSource = effect.source.CurrentCard.name + "的效果";
            string logTarget = (isMyUnit ? "我方" : "對方") + "抽取 " + drawCount + " 張卡片";
            effect.hudOptionDict.Set("log", logSource + "\n" + logTarget);
            Hud.SetState(state);

            if (total.Count < drawCount) {
                int result = (int)unit.leader.GetIdentifier("deckOutResult");            
                var resultState = (result == 0) ? BattleResultState.Lose : BattleResultState.Win;
                Effect resultEffect = new Effect(new int[] { (int)EffectAbility.SetResult, (int)resultState })
                {
                    source = unit.leader.leaderCard,
                    invokeUnit = unit
                };
                resultEffect.Apply();
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
        EnqueueEffect("on_be_draw", effect.invokeTarget.Select(x => x.CurrentCard).ToList() , state);
        EnqueueEffect("on_be_draw_discard", inGraveCards.Select(x => x.CurrentCard).ToList() , state);
        OnPhaseChange("on_draw", state);
        return true;
    }

    public static bool Summon(this Effect effect, BattleState state) {
        var unit = effect.invokeUnit;
        bool isMyUnit = state.myUnit.id == unit.id;
        
        var who = effect.abilityOptionDict.Get("who", "me");
        var summonUnit = (who == "me") ? unit : state.GetRhsUnitById(unit.id);
        var field = effect.abilityOptionDict.Get("field", "me");
        var fieldUnit = (field == "me") ? unit : state.GetRhsUnitById(unit.id);
        var where = effect.abilityOptionDict.Get("where", "hand");
        var index = effect.abilityOptionDict.Get("index", "0");
        var data = effect.abilityOptionDict.Get("data", "none");
        var card = effect.source;

        //TODO Check summon from where and who to summon.
        if (fieldUnit.field.Count < fieldUnit.field.MaxCount) {
            switch (where) {
                default:
                    break;
                case "hand":
                    effect.invokeTarget = new List<BattleCard>() { summonUnit.hand.cards[int.Parse(index)] };
                    summonUnit.hand.cards.RemoveRange(effect.invokeTarget);
                    break;
            }
            
            fieldUnit.field.cards.Add(effect.invokeTarget[0]);

            effect.hudOptionDict.Set("log", effect.invokeTarget[0].CurrentCard.name + "進入戰場");
            Hud.SetState(state);
            
            EnqueueEffect("on_be_summon", effect.invokeTarget.Select(x => x.CurrentCard).ToList() , state);
        }
        OnPhaseChange("on_summon", state);

        return true;
    }
}

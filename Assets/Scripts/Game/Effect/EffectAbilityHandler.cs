using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EffectAbilityHandler 
{
    public static Battle Battle => Player.currentBattle;
    public static BattleManager Hud => BattleManager.instance;

    public static bool SetResult(this Effect effect, BattleState state) {
        if (state == null)
            return false;

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
        if ((state == null) || (changeList.IsNullOrEmpty()))
            return true;

        var unit = effect.invokeUnit;
        bool isMyUnit = state.myUnit.id == unit.id;
        for (int i = 0; i < changeList.Count; i++) {
            if (changeList[i] >= unit.hand.Count)
                return false;
        }
        
        var changeCards = changeList.Select(x => unit.hand.cards[x]).ToList();
        var newCards = unit.deck.cards.Take(changeList.Count).ToList();
        for (int i = 0; i < changeList.Count; i++) {
            unit.hand.cards[changeList[i]] = newCards[i];
        }

        unit.deck.cards.RemoveRange(0, Mathf.Min(newCards.Count, unit.deck.Count));
        unit.deck.cards.AddRange(changeCards);
        unit.deck.cards.Shuffle();
        effect.invokeTarget = newCards;
        unit.isDone = true;

        effect.hudOptionDict.Set("log", (isMyUnit ? "你" : "對手") + "交換了 " + newCards.Count + " 張手牌");
        Hud.SetState(state);

        if (state.settings.isLocal && (state.myUnit.id == unit.id)) {
            Battle.PlayerAction(new int[4] { (int)EffectAbility.KeepCard, 0, 1, 2 }, false);
            return true;
        }

        if (state.myUnit.isDone && state.opUnit.isDone) {
            Battle.PlayerAction(new int[1] { (int)EffectAbility.TurnStart }, state.myUnit.isFirst);
        }

        return true;
    }

    public static bool OnTurnStart(this Effect effect, BattleState state) {
        var unit = effect.invokeUnit;
        bool isMyUnit = state.myUnit.id == unit.id;

        state.myUnit.isDone = state.opUnit.isDone = false;

        unit.turn += 1;
        unit.leader.ppMax = Mathf.Min(unit.leader.ppMax + 1, 10);
        unit.leader.pp = unit.leader.ppMax;
        
        var first = unit.isFirst ? 1 : 0;
        if (unit.turn - first == 4) {
            unit.leader.epMax = unit.isFirst ? 2 : 3;
            unit.leader.ep = unit.leader.epMax;
            effect.hudOptionDict.Set("ep", "true");
        }

        effect.hudOptionDict.Set("stage", "start");
        effect.hudOptionDict.Set("log", (isMyUnit ? "YOUR" : "ENEMY") + " TURN (" + unit.turn + ")");
        Hud.SetState(state);

        if (unit.turn > 40) {
            Effect lose = new Effect(new int[] { (int)EffectAbility.SetResult, (int)BattleResultState.Lose })
            {
                source = unit.leader.leaderCard,
                invokeUnit = unit
            };
            lose.Apply();
            return true;
        }

        // on turn start

        if ((!unit.isFirst) && (unit.turn == 1)) {

        }
        
        return true;
    }
}

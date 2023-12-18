using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EffectAbilityHandler 
{
    public static Battle Battle => Player.currentBattle;
    public static BattleManager Hud => BattleManager.instance;

    public static bool KeepCard(this Effect effect, BattleState state) {
        string indexList = effect.abilityOptionDict.Get("change", "none");
        var changeList = indexList.ToIntList('/');
        if ((state == null) || (changeList.Count == 0))
            return false;

        var unit = effect.invokeUnit;
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

        Hud.SetState(state);

        if (state.settings.isLocal && (state.myUnit.id == unit.id)) {
            Battle.PlayerAction((short)EffectAbility.KeepCard, new int[3] { 0, 1, 2 }, false);
            return true;
        }

        if (state.myUnit.isDone && state.opUnit.isDone) {
            Battle.PlayerAction((short)EffectAbility.TurnStart, new int[1] { 0 }, state.myUnit.isFirst);
        }

        return true;
    }

    public static bool OnTurnStart(this Effect effect, BattleState state) {
        var unit = effect.invokeUnit;
        unit.turn += 1;
        unit.leader.ppMax = Mathf.Min(unit.leader.ppMax + 1, 10);
        unit.leader.pp = unit.leader.ppMax;

        // Turn > 40 fail
        // Ep point
        // Hud set state
        Hud.SetState(state);
        
        return true;
    }
}

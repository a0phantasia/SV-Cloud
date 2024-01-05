using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBaseView : IMonoBehaviour
{
    public Battle Battle => Player.currentBattle;
    public BattleLogManager Log => BattleLogManager.instance;
    public BattleManager Hud => BattleManager.instance;
    public BattleAnimManager Anim => BattleAnimManager.instance;

    [SerializeField] protected CardInfoView cardInfoView;

}

public class BattleCardPlaceInfo {
    public int unitId = 0;
    public BattlePlace place = BattlePlace.Deck;
    public int index = 0;

    public BattleCard GetBattleCard(BattleState state) {
        var unit = (unitId == 0) ? state.myUnit : state.opUnit;
        return place switch {
            BattlePlace.Deck => unit.deck.cards[index],
            BattlePlace.Hand => unit.hand.cards[index],
            BattlePlace.Leader => unit.leader.leaderCard,
            BattlePlace.Territory => unit.territory,
            BattlePlace.Field => unit.field.cards[index],
            _ => null,
        };
    }
}

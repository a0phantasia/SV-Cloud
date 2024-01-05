using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAnimManager : Manager<BattleAnimManager>
{
    [SerializeField] private BattleOrderView orderView;
    [SerializeField] private BattleKeepCardView keepView;
    [SerializeField] private BattleTurnView turnView;

    [SerializeField] private BattleUseAnimView useView;
    [SerializeField] private BattleEvolveAnimView evolveView;

    public override void Init()
    {
        base.Init();
        orderView.ShowOrderInfo(keepView.ShowKeepInfo);
    }

    public void ResultAnim(string whosTurn, string description, Action callback) {
        turnView?.ShowTurnInfo(whosTurn, description, callback);
    }

    public void KeepCardAnim(List<BattleCard> handCards, bool isOpDone, Action callback) {
        keepView?.ShowKeepResult(handCards, () => {
            if (isOpDone)
                keepView?.SetActive(false);

            callback?.Invoke();
        });
    }

    public void TurnStartAnim(string whosTurn, string description, Action callback) {
        turnView?.ShowTurnInfo(whosTurn, description, callback);
    }

    public void UseAnim(int unitId, BattleCard card, Action callback) {
        if (unitId == 0)
            useView?.MeUseCard(card, callback);
        else
            useView?.OpUseCard(card, callback);
    }

    public void EvolveAnim(BattleCardPlaceInfo info, BattleCard evolveCard, List<BattleCard> fieldCards, Action callback) {
        evolveView?.EvolveWithEP(info, evolveCard, fieldCards, callback);
    }

}

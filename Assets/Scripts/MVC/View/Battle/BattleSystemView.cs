using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystemView : BattleBaseView
{
    [SerializeField] private BattleMenuView menuView;
    [SerializeField] private BattleOrderView orderView;
    [SerializeField] private BattleKeepCardView keepView;
    [SerializeField] private BattleTurnView turnView;

    public override void Init()
    {
        base.Init();
        orderView.ShowOrderInfo(keepView.ShowKeepInfo);
        menuView.InitPlayerInfo();
    }

    public void OnTurnStart() {
        // turnView.ShowTurnInfo("YOUR TURN", "第 1 回合");
    }

}

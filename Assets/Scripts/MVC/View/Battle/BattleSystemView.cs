using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystemView : BattleBaseView
{
    [SerializeField] private BattleMenuView menuView;
    [SerializeField] private BattleOrderView orderView;
    [SerializeField] private BattleKeepCardView keepView;
    [SerializeField] private BattleTurnView turnView;
    [SerializeField] private BattleLogView logView;

    public bool isDone = true;

    public override void Init()
    {
        base.Init();
        orderView.ShowOrderInfo(keepView.ShowKeepInfo);
        menuView.InitPlayerInfo();
    }

    public void SetState(BattleState state) {
        isDone = false;
        logView?.SetState(state);

        var effect = state.currentEffect;
        switch (effect.ability) {
            default:
                isDone = true;
                break;
            case EffectAbility.SetResult:
                string result = state.result.masterState switch {
                    BattleResultState.Win => state.myUnit.IsMasterUnit ? "WIN" : "LOSE",
                    BattleResultState.Lose => state.myUnit.IsMasterUnit ? "LOSE" : "WIN",
                    _ => "DRAW",
                };
                turnView?.ShowTurnInfo("YOU " + result, string.Empty, () => {
                    isDone = true;
                    Hud.OnConfirmBattleResult();
                }); 
                break;
            case EffectAbility.KeepCard:
                if (state.myUnit.isDone) {
                    keepView?.ShowKeepResult(state.myUnit.hand.cards, () => {
                        if (state.opUnit.isDone)
                            keepView.SetActive(false);
                        
                        isDone = true;
                    });
                }
                break;
            case EffectAbility.TurnStart:
                if (effect.hudOptionDict.Get("stage", "start") == "start") {
                    string who = (state.myUnit.isMyTurn ? "YOUR" : "ENEMY") + " TURN";
                    string turn = (effect.hudOptionDict.Get("ep", "false") == "false") ?
                        ("第 " + state.currentUnit.turn + " 回合") : "進化解禁";
                    turnView?.ShowTurnInfo(who, turn, () => isDone = true);
                }
                break;
        }
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystemView : BattleBaseView
{
    [SerializeField] private BattleMenuView menuView;
    public bool IsDone { get; protected set; } = true;

    public override void Init()
    {
        base.Init();
        menuView?.InitPlayerInfo();
    }

    public void SetState(BattleState state) {
        IsDone = false;
        Log.Log(state);

        var effect = state.currentEffect;
        switch (effect.ability) {
            default:
                IsDone = true;
                break;

            case EffectAbility.SetResult:
                string result = state.result.masterState switch {
                    BattleResultState.Win => state.myUnit.IsMasterUnit ? "WIN" : "LOSE",
                    BattleResultState.Lose => state.myUnit.IsMasterUnit ? "LOSE" : "WIN",
                    _ => "DRAW",
                };
                string reason = (BattleLoseReason)int.Parse(effect.abilityOptionDict.Get("reason", "0")) switch {
                    BattleLoseReason.Retire     => (result == "LOSE" ? "你" : "對手") + "已放棄對戰",
                    BattleLoseReason.LeaderDie  => (result == "LOSE" ? "你" : "對手") + "的主戰者陣亡",
                    BattleLoseReason.Deckout    => (result == "LOSE" ? "你" : "對手") + "的牌庫已抽乾",
                    _ => string.Empty,
                };
                Anim.ResultAnim("YOU " + result, reason, () => {
                    IsDone = true;
                    Hud.OnConfirmBattleResult();
                }); 
                break;

            case EffectAbility.KeepCard:
                if (state.myUnit.isDone)
                    Anim.KeepCardAnim(state.myUnit.hand.cards, state.opUnit.isDone, () => IsDone = true);
                
                break;

            case EffectAbility.TurnStart:
                string who = (state.myUnit.isMyTurn ? "YOUR" : "ENEMY") + " TURN";
                string turn = (effect.hudOptionDict.Get("ep", "false") == "false") ?
                    ("第 " + state.currentUnit.turn + " 回合") : "進化解禁";
                Anim.TurnStartAnim(who, turn, () => IsDone = true);
                break;
        }
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleUnitView : BattleBaseView
{
    [SerializeField] private int id;
    [SerializeField] private BattleLeaderView leaderView;
    [SerializeField] private BattlePPView ppView;
    [SerializeField] private BattleEPView epView;
    [SerializeField] private BattleHandView handView;
    [SerializeField] private BattleFieldView fieldView;
    [SerializeField] private BattleDeckView deckView;
    [SerializeField] private BattleCornerView cornerView;

    public bool IsDone { get; protected set; } = true;

    public override void Init()
    {
        base.Init();
        SetUnit((id == 0) ? Battle.CurrentState.myUnit : Battle.CurrentState.opUnit);
    }

    public void SetState(BattleState state) {
        var effect = state.currentEffect;
        var invokeUnit = effect.invokeUnit;

        IsDone = false;

        if (id == 0) 
            SetMyState(state);
        else
            SetOpState(state);
    }

    private void SetMyState(BattleState state) {
        var effect = state.currentEffect;
        var invokeUnit = effect.invokeUnit;
        var unit = state.myUnit;

        Action SetMyUnit = (() => SetUnit(unit));

        switch (effect.ability) {
            default:
                SetMyUnit();
                break;

            case EffectAbility.Use:
                if (invokeUnit.id != unit.id)
                    goto default;

                SetUnit(unit, false);
                Anim.UseAnim(0, effect.invokeTarget[0], SetMyUnit);
                break;

            case EffectAbility.Attack:
                if (invokeUnit.id != unit.id)
                    goto default;

                var sourceIndex = int.Parse(effect.hudOptionDict.Get("source", "-1"));
                Anim.AttackAnim(0, sourceIndex);
                SetMyUnit();
                break;

            case EffectAbility.Evolve:
                if (invokeUnit.id != unit.id)
                    goto default;

                var evolveIndex = int.Parse(effect.hudOptionDict.Get("index", "-1"));
                if (evolveIndex >= 0) {
                    // Evolve with EP.
                    Anim.EvolveAnim(new BattleCardPlaceInfo() {
                        unitId = 0,
                        place = BattlePlace.Field,
                        index = evolveIndex,
                    }, unit.field.cards[evolveIndex], fieldView.fieldCards, SetMyUnit);
                } else {
                    // Auto evolve.
                    SetMyUnit();
                }

                break;

            case EffectAbility.Draw:
                if (invokeUnit.id != unit.id)
                    goto default;

                var drawCount = int.Parse(effect.hudOptionDict.Get("count"));
                var inHand = effect.invokeTarget.Select(x => x.CurrentCard).ToList();
                var inGrave = unit.grave.graveCards.TakeLast(drawCount - effect.invokeTarget.Count).ToList();

                Anim.DrawAnim(0, handView.Mode, inHand, inGrave, SetMyUnit);
                break;

            case EffectAbility.Damage:
                var damageIndexList = effect.hudOptionDict.Get("myIndex", string.Empty).ToIntList('/');
                var damageValueList = effect.hudOptionDict.Get("myDamage", string.Empty).ToIntList('/');

                if (List.IsNullOrEmpty(damageIndexList) || List.IsNullOrEmpty(damageValueList))
                    goto default;

                for (int i = 0; i < damageIndexList.Count; i++) {
                    Anim.DamageAnim(0, damageIndexList[i], damageValueList[i],
                        (i == damageIndexList.Count - 1) ? SetMyUnit : null);
                }

                break;

            case EffectAbility.Heal:
                var healIndexList = effect.hudOptionDict.Get("myIndex", string.Empty).ToIntList('/');
                var healValueList = effect.hudOptionDict.Get("myHeal", string.Empty).ToIntList('/');

                if (List.IsNullOrEmpty(healIndexList) || List.IsNullOrEmpty(healValueList))
                    goto default;

                for (int i = 0; i < healIndexList.Count; i++) {
                    Anim.HealAnim(0, healIndexList[i], healValueList[i], 
                        (i == healIndexList.Count - 1) ? SetMyUnit : null);
                }
                break;
        };
    }

    private void SetOpState(BattleState state) {
        var effect = state.currentEffect;
        var invokeUnit = effect.invokeUnit;
        var unit = state.opUnit;

        Action SetOpUnit = (() => SetUnit(unit));

        switch (effect.ability) {
            default:
                SetOpUnit();
                break;

            case EffectAbility.Use:
                if (invokeUnit.id != unit.id)
                    goto default;

                SetUnit(unit, false);
                Anim.UseAnim(1, effect.invokeTarget[0], SetOpUnit);
                break;

            case EffectAbility.Attack:
                if (invokeUnit.id != unit.id)
                    goto default;

                var sourceIndex = int.Parse(effect.hudOptionDict.Get("source", "-1"));
                Anim.AttackAnim(1, sourceIndex);
                SetOpUnit();
                break;

            case EffectAbility.Evolve:
                if (invokeUnit.id != unit.id)
                    goto default;

                var index = int.Parse(effect.hudOptionDict.Get("index", "-1"));
                if (index >= 0) {
                    // Evolve with EP.
                    Anim.EvolveAnim(new BattleCardPlaceInfo() {
                        unitId = 1,
                        place = BattlePlace.Field,
                        index = index,
                    }, unit.field.cards[index], fieldView.fieldCards, SetOpUnit);
                } else {
                    // Auto evolve.
                    SetOpUnit();
                }

                break;

            case EffectAbility.Draw:
                if (invokeUnit.id != unit.id)
                    goto default;
                
                var count = int.Parse(effect.hudOptionDict.Get("count"));
                var inHand = effect.invokeTarget.Select(x => x.CurrentCard).ToList();
                var inGrave = unit.grave.graveCards.TakeLast(count - effect.invokeTarget.Count).ToList();

                Anim.DrawAnim(1, false, inHand, inGrave, SetOpUnit);
                break;

            case EffectAbility.Damage:
                var damageSituation = effect.hudOptionDict.Get("situation", string.Empty);
                var damageIndexList = effect.hudOptionDict.Get("opIndex", string.Empty).ToIntList('/');
                var damageValueList = effect.hudOptionDict.Get("opDamage", string.Empty).ToIntList('/');

                if (List.IsNullOrEmpty(damageIndexList) || List.IsNullOrEmpty(damageValueList))
                    goto default;

                for (int i = 0; i < damageIndexList.Count; i++) {
                    Anim.DamageAnim(1, damageIndexList[i], damageValueList[i],
                        (i == damageIndexList.Count - 1) ? SetOpUnit : null);
                }   
                break;

            case EffectAbility.Heal:
                var healIndexList = effect.hudOptionDict.Get("opIndex", string.Empty).ToIntList('/');
                var healValueList = effect.hudOptionDict.Get("opHeal", string.Empty).ToIntList('/');
        
                if (List.IsNullOrEmpty(healIndexList) || List.IsNullOrEmpty(healValueList))
                    goto default;

                for (int i = 0; i < healIndexList.Count; i++) {
                    Anim.HealAnim(1, healIndexList[i], healValueList[i], 
                        (i == healIndexList.Count - 1) ? SetOpUnit : null);
                }
                break;
        };
    }

    private void SetUnit(BattleUnit unit, bool setDone = true) {
        leaderView?.SetLeader(unit?.leader);
        ppView.SetLeader(unit?.leader);
        ppView?.SetTurnEndButtonActive((unit == null) ? false : unit.isMyTurn && (!unit.isDone));
        epView?.SetLeader(unit?.leader);
        handView?.SetHand(unit);
        fieldView?.SetField(unit?.field);
        deckView?.SetDeck(unit?.deck);
        cornerView.SetUnit(unit);

        IsDone = setDone;
    }
}

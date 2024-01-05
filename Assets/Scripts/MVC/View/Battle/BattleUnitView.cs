using System.Collections;
using System.Collections.Generic;
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
        SetUnit((id == 0) ? Battle.currentState.myUnit : Battle.currentState.opUnit);
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

        switch (effect.ability) {
            default:
                SetUnit(unit);
                break;

            case EffectAbility.Use:
                if (invokeUnit.id != unit.id)
                    goto default;

                Anim.UseAnim(0, effect.invokeTarget[0], () => {
                    handView?.SetHandMode(true);
                    SetUnit(unit);
                });
                break;

            case EffectAbility.Evolve:
                if (invokeUnit.id != unit.id)
                    goto default;

                var index = int.Parse(effect.abilityOptionDict.Get("index", "-1"));
                if (index >= 0) {
                    // Evolve with EP.
                    Anim.EvolveAnim(new BattleCardPlaceInfo() {
                        unitId = 0,
                        place = BattlePlace.Field,
                        index = index,
                    }, unit.field.cards[index], fieldView.fieldCards, () => {
                        SetUnit(unit);
                    });
                } else {
                    // Auto evolve.
                    SetUnit(unit);
                }

                break;
        };
    }

    private void SetOpState(BattleState state) {
        var effect = state.currentEffect;
        var invokeUnit = effect.invokeUnit;
        var unit = state.opUnit;

        switch (effect.ability) {
            default:
                SetUnit(unit);
                break;

            case EffectAbility.Use:
                if (invokeUnit.id != unit.id)
                    goto default;

                Anim.UseAnim(1, effect.invokeTarget[0], () => SetUnit(unit));
                break;

            case EffectAbility.Evolve:
                if (invokeUnit.id != unit.id)
                    goto default;

                var index = int.Parse(effect.abilityOptionDict.Get("index", "-1"));
                if (index >= 0) {
                    // Evolve with EP.
                    Anim.EvolveAnim(new BattleCardPlaceInfo() {
                        unitId = 1,
                        place = BattlePlace.Field,
                        index = index,
                    }, unit.field.cards[index], fieldView.fieldCards, () => {
                        SetUnit(unit);
                    });
                } else {
                    // Auto evolve.
                    SetUnit(unit);
                }

                break;
        };
    }

    private void SetUnit(BattleUnit unit) {
        leaderView?.SetLeader(unit?.leader);
        ppView.SetLeader(unit?.leader);
        ppView?.SetTurnEndButtonActive((unit == null) ? false : unit.isMyTurn && (!unit.isDone));
        epView?.SetLeader(unit?.leader);
        handView?.SetHand(unit);
        fieldView?.SetField(unit?.field);
        deckView?.SetDeck(unit?.deck);
        cornerView.SetUnit(unit);

        IsDone = true;
    }
}

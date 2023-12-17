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

    public void SetUnit(BattleUnit unit) {
        leaderView?.SetLeader(unit?.leader);
        ppView.SetLeader(unit?.leader);
        epView?.SetLeader(unit?.leader);
        handView?.SetHand(unit?.hand);
        fieldView?.SetField(unit?.field);
        deckView?.SetDeck(unit?.deck);
        cornerView.SetUnit(unit);
    }
}

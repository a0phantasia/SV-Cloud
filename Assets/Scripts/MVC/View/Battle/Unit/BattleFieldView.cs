using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleFieldView : BattleBaseView
{
    [SerializeField] private List<BattleCardView> cardViews;

    public void SetField(BattleField field) {
        for (int i = 0; i < cardViews.Count; i++) {
            cardViews[i].SetBattleCard(i < field.cards.Count ? field.cards[i] : null);
        }
    }
}

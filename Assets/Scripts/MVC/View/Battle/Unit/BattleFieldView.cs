using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleFieldView : BattleBaseView
{
    [SerializeField] private List<BattleCardView> cardViews;
    private List<BattleCard> fieldCards = new List<BattleCard>();
    private int fieldCount => fieldCards.Count;

    public void SetField(BattleField field) {
        fieldCards = field.cards.Select(x => x).ToList();
        for (int i = 0; i < cardViews.Count; i++) {
            cardViews[i].SetBattleCard((i < field.Count) ? field.cards[i] : null);
        }
    } 

    public void ShowFieldInfo(int index) {
        cardInfoView?.SetBattleCard((index < fieldCount) ? fieldCards[index] : null);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleField
{
    public int MaxCount = 5;
    public int Count => cards.Count;
    public bool IsFull => Count >= MaxCount;
    public List<BattleCard> cards = new List<BattleCard>();

    public BattleField() {}
    public BattleField(BattleField rhs) {
        MaxCount = rhs.MaxCount;
        cards = rhs.cards.Select(x => (x == null) ? null : new BattleCard(x)).ToList();
    }

    public List<int> GetAttackableTargetIndex(BattleCard attackSource, BattleUnit sourceUnit) {
        var result = cards.Where(x => !x.actionController.IsKeywordAvailable(CardKeyword.Ambush));
        var ward = result.Where(x => x.actionController.IsKeywordAvailable(CardKeyword.Ward));

        result = ward.Any() ? ward : result;
        var index = result.Select(x => cards.IndexOf(x)).ToList();

        if ((attackSource.IsLeaderAttackable(sourceUnit)) && (!ward.Any()))
            index.Add(-1);

        return index;
    }


}

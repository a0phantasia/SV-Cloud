using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCardEffectView : IMonoBehaviour
{
    [SerializeField] private List<GameObject> effectObjects;

    public void SetBattleCard(BattleCard card) {
        if (card == null) {
            effectObjects.ForEach(x => x?.SetActive(false));
            return;
        }
        effectObjects[0]?.SetActive(card.actionController.IsKeywordAvailable(CardKeyword.Ward));
        effectObjects[1]?.SetActive(card.actionController.IsKeywordAvailable(CardKeyword.Ambush));
    }
}

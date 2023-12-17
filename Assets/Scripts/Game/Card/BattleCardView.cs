using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCardView : IMonoBehaviour
{
    [SerializeField] private BattleCardFollowerView normalView, evolveView;
    [SerializeField] private BattleCardAmuletView amuletView;

    public void SetBattleCard(BattleCard card) {
        gameObject.SetActive(card != null);
        if (card == null)
            return;

        normalView?.gameObject.SetActive(card.CurrentCard.Type == CardType.Follower);
        evolveView?.gameObject.SetActive(card.CurrentCard.Type == CardType.Evolved);
        amuletView?.gameObject.SetActive(card.CurrentCard.Type == CardType.Amulet);

        normalView?.SetBattleCard(card);
        evolveView?.SetBattleCard(card);
        amuletView?.SetBattleCard(card);
    }
}

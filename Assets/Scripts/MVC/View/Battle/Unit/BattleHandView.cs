using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleHandView : BattleBaseView
{
    [SerializeField] private bool isMe = true;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private HorizontalLayoutGroup layoutGroup;
    [SerializeField] private IButton handGroupButton;
    [SerializeField] private List<GameObject> sleeves;
    [SerializeField] private List<CardView> cardViews;

    private List<BattleCard> handCards = new List<BattleCard>();
    private int handCount => handCards.Count;

    public void SetHand(BattleHand hand) {
        if (!isMe) {
            for (int i = 0; i < sleeves.Count; i++)
                sleeves[i].SetActive(i < hand.Count);
            
            return;
        }

        handCards = hand.cards.Select(x => x).ToList();
        for (int i = 0; i < cardViews.Count; i++)
            cardViews[i].SetBattleCard((i < hand.Count) ? hand.cards[i] : null);
    }

    public void ShowHandInfo(int index) {
        cardInfoView?.SetBattleCard((index < handCount) ? handCards[index] : null);
    }

    public void SetHandMode(bool active) {
        handGroupButton.gameObject.SetActive(!active);
        rectTransform.localScale = (active ? 2 : 1) * Vector3.one;
        rectTransform.anchoredPosition = active ? new Vector2(GetLayoutGroupPosition(), -36) : new Vector2(520, -18);
        layoutGroup.spacing = active ? GetLayoutGroupSpacing() : 0;
    }

    private float GetLayoutGroupPosition() {
        if (handCount < 5)
            return 425 - handCount * 50;

        return 190;
    }

    private float GetLayoutGroupSpacing() {
        if (handCount < 5)
            return 25;

        return handCount switch {
            5 => 20,
            6 => 12.5f,
            7 => 6.25f,
            8 => 2.5f,
            _ => 0,
        };
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleHandView : BattleBaseView
{
    [SerializeField] private bool isMe = true;
    [SerializeField] private float useThresholdY;
    [SerializeField] private float useExhibitPosY;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private HorizontalLayoutGroup layoutGroup;
    [SerializeField] private IButton handGroupButton;
    [SerializeField] private List<GameObject> sleeves;
    [SerializeField] private List<CardView> cardViews;

    private bool mode = false;
    private List<Vector2> initPos = Enumerable.Repeat(default(Vector2), 9).ToList();
    private List<BattleCard> handCards = new List<BattleCard>();
    private int handCount => handCards.Count;

    public void SetHand(Leader leader, BattleHand hand) {
        if (!isMe) {
            for (int i = 0; i < sleeves.Count; i++)
                sleeves[i].SetActive(i < hand.Count);
            
            return;
        }

        handCards = hand.cards.Select(x => x).ToList();
        for (int i = 0; i < cardViews.Count; i++) {
            cardViews[i].SetBattleCard((i < hand.Count) ? hand.cards[i] : null);
            cardViews[i].SetStatus("cost", (i < hand.Count) ? hand.cards[i].GetUseCost(leader) : 0);
            cardViews[i].draggable.SetEnable((i < hand.Count) ? hand.cards[i].IsUsable(leader) : false);
        }

        SetHandMode(mode);
    }

    public void ShowHandInfo(int index) {
        cardInfoView?.SetBattleCard((index < handCount) ? handCards[index] : null);
    }

    public void SetHandMode(bool active) {
        mode = active;
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

    public void OnBeginDrag(int index) {
        if (!index.IsInRange(0, cardViews.Count))
            return;

        cardViews[index].SetTag(null);
        initPos[index] = cardViews[index].rectTransform.anchoredPosition;
    }

    public void OnEndDrag(int index) {
        if (!index.IsInRange(0, cardViews.Count))
            return;

        if (cardViews[index].rectTransform.anchoredPosition.y > useThresholdY) {
            float x = 197.5f - GetLayoutGroupPosition() / 2;
            cardViews[index].rectTransform.anchoredPosition = new Vector2(x, useExhibitPosY);
            Battle.PlayerAction(new int[2] { (int)EffectAbility.Use, index }, true);
            return;
        }

        cardViews[index].rectTransform.anchoredPosition = initPos[index];
        cardViews[index].SetTag(cardViews[index].CurrentCard);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;

public class BattleHandView : BattleBaseView
{
    [SerializeField] private int id = 0;
    [SerializeField] private float useThresholdY;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private HorizontalLayoutGroup layoutGroup;
    [SerializeField] private IButton handGroupButton;
    [SerializeField] private List<Image> sleeves;
    [SerializeField] private List<CardView> cardViews;

    [SerializeField] private CardView opUseCardView;

    private bool mode = false;
    private List<Vector2> initPos = Enumerable.Repeat(default(Vector2), 9).ToList();
    private List<BattleCard> handCards = new List<BattleCard>();
    private int handCount => handCards.Count;

    public void SetHand(BattleUnit unit) {
        var leader = unit.leader;
        var hand = unit.hand;

        if (id != 0) {
            for (int i = 0; i < sleeves.Count; i++)
                sleeves[i].gameObject.SetActive(i < hand.Count);
            
            return;
        }

        handCards = hand.cards.Select(x => x).ToList();
        for (int i = 0; i < cardViews.Count; i++) {
            cardViews[i].SetBattleCard((i < hand.Count) ? hand.cards[i] : null);
            cardViews[i].SetStatus("cost", (i < hand.Count) ? hand.cards[i].GetUseCost(leader) : 0);
            cardViews[i].draggable.SetEnable((i < hand.Count) ? hand.cards[i].IsUsable(unit) : false);
        }

        SetHandMode(mode);
    }

    public void ShowHandInfo(int index) {
        var unit = Hud.CurrentState.myUnit;
        var card = (index < handCount) ? handCards[index] : null;

        Hud.CurrentCardPlaceInfo = new BattleCardPlaceInfo() { 
            unitId = id,
            place = BattlePlace.Hand,
            index = index,
        };

        cardInfoView?.SetBattleCard(card);
    }

    public void SetHandMode(bool active) {
        SetHandMode(active, 0);
    }   

    public void SetHandMode(bool active, int padding) {
        var count = handCount + padding;
        mode = active;
        handGroupButton.gameObject.SetActive(!active);
        rectTransform.localScale = (active ? 2 : 1) * Vector3.one;
        rectTransform.anchoredPosition = active ? new Vector2(GetLayoutGroupPosition(count), -36) : new Vector2(520, -18);
        layoutGroup.spacing = active ? GetLayoutGroupSpacing(count) : 0;
    }

    private float GetLayoutGroupPosition(int count) {
        if (count < 4)
            return 425 - count * 50;

        if (count == 4)
            return 235;

        if (count == 5)
            return 210;

        return 190;
    }

    private float GetLayoutGroupSpacing(int count) {
        if (count < 5)
            return 25;

        return count switch {
            5 => 20,
            6 => 12.5f,
            7 => 6.25f,
            8 => 2.5f,
            _ => 0,
        };
    }

    /// <summary>
    /// On begin drag card. Player drags the card to use.
    /// </summary>
    public void OnBeginDrag(int index) {
        if (!index.IsInRange(0, cardViews.Count))
            return;

        cardViews[index].SetTag(null);
        initPos[index] = cardViews[index].rectTransform.anchoredPosition;
    }

    /// <summary>
    /// On end drag card. If card pos is above useThresholdY, use the card.
    /// </summary>
    public void OnEndDrag(int index) {
        if (!index.IsInRange(0, cardViews.Count))
            return;

        if (cardViews[index].rectTransform.anchoredPosition.y > useThresholdY) {
            cardViews[index].SetCard(null);
            Battle.PlayerAction(new int[2] { (int)EffectAbility.Use, index }, true);
            return;
        }

        cardViews[index].rectTransform.anchoredPosition = initPos[index];
        cardViews[index].SetTag(cardViews[index].CurrentCard);
    }
}

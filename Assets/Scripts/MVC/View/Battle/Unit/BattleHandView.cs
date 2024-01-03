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
    [SerializeField] private float useExhibitPosY;
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
        mode = active;
        handGroupButton.gameObject.SetActive(!active);
        rectTransform.localScale = (active ? 2 : 1) * Vector3.one;
        rectTransform.anchoredPosition = active ? new Vector2(GetLayoutGroupPosition(), -36) : new Vector2(520, -18);
        layoutGroup.spacing = active ? GetLayoutGroupSpacing() : 0;
    }

    public IEnumerator ShowOpUseCard(Card card, Action callback) {
        float currentTime = 0, finishTime = 0.5f;
        var lastSleeve = sleeves.FindLast(x => x.gameObject.activeSelf);
        var x = lastSleeve.rectTransform.anchoredPosition.x - 120;
        
        opUseCardView.rectTransform.localScale = 0.25f * Vector3.one;
        opUseCardView.rectTransform.anchoredPosition = new Vector2(x, -30);
        opUseCardView.SetCard(card);
        opUseCardView.gameObject.SetActive(true);
        lastSleeve.gameObject.SetActive(false);

        while (currentTime < finishTime) {
            var percent = currentTime / finishTime;
            opUseCardView.rectTransform.localScale = (0.25f + percent * 0.25f) * Vector3.one;
            opUseCardView.rectTransform.anchoredPosition = new Vector2(x + percent * (390 - x), -30 + percent * (useExhibitPosY + 30));
            currentTime += ((percent < 0.8f) ? 2.5f : 1) * Time.deltaTime;
            yield return null;
        }

        opUseCardView.rectTransform.localScale = 0.5f * Vector3.one;
        opUseCardView.rectTransform.anchoredPosition = new Vector2(390, useExhibitPosY);

        yield return new WaitForSeconds(1f);

        opUseCardView.gameObject.SetActive(false);
        callback?.Invoke();
    }

    private float GetLayoutGroupPosition() {
        if (handCount < 4)
            return 425 - handCount * 50;

        if (handCount == 4)
            return 235;

        if (handCount == 5)
            return 210;

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
            float x = 197.5f - GetLayoutGroupPosition() / 2;
            cardViews[index].rectTransform.anchoredPosition = new Vector2(x, useExhibitPosY);
            Battle.PlayerAction(new int[2] { (int)EffectAbility.Use, index }, true);
            return;
        }

        cardViews[index].rectTransform.anchoredPosition = initPos[index];
        cardViews[index].SetTag(cardViews[index].CurrentCard);
    }
}

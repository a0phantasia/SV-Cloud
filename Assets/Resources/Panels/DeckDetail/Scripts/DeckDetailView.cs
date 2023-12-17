using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckDetailView : IMonoBehaviour
{
    [SerializeField] private int rowCount = 2;
    [SerializeField] private int columnCards = 8;
    [SerializeField] private float prefabScale = 0.35f;

    [SerializeField] private ScrollRect scrollRect;
    private RectTransform contentRect => scrollRect?.content;

    private List<CardView> cardViews = new List<CardView>();

    public void SetDeck(Deck deck, Action<Card> onClick = null, 
        Action<RectTransform> onBeginDrag = null, Action<RectTransform> onDrag = null, 
        Action<RectTransform> onEndDrag = null) {
        var distinctCards = deck.DistinctCards;
        var distribution = deck.CardIdDistribution;
        var diffCount = distinctCards.Count - cardViews.Count;

        if (diffCount > 0) {
            for (int i = 0; i < diffCount; i++) 
            {
                GameObject obj = Instantiate(Card.Prefab, contentRect);
                CardView view = obj.GetComponent<CardView>();
                obj.transform.localScale = prefabScale * Vector3.one;
                cardViews.Add(view);
            }
        } else if (diffCount < 0) {
            for (int i = cardViews.Count - 1; i >= distinctCards.Count; i--)
                Destroy(cardViews[i].gameObject);
            
            cardViews.RemoveRange(distinctCards.Count, -diffCount);
        }

        float contentSizeX = (distinctCards.Count <= (rowCount * columnCards)) ? 800f 
            : (distinctCards.Count + rowCount - 1) / rowCount * (800f / columnCards); 

        contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentSizeX);

        for (int i = 0; i < distinctCards.Count; i++) 
        {
            int copy = i;
            CardView view = cardViews[copy];
            Card card = distinctCards[copy];
            int count = distribution.Get(card.id, 0);
            view.SetCard(card, count);
            view.SetCallback(() => onClick?.Invoke(card));
            view.draggable?.onBeginDragEvent.SetListener(onBeginDrag);
            view.draggable?.onDragEvent.SetListener(onDrag);
            view.draggable?.onEndDragEvent.SetListener(onEndDrag);
            view.draggable?.SetEnable((onBeginDrag ?? onDrag ?? onEndDrag) != null);
        }
    }

    public void ScrollTo(Vector2 where) {
        scrollRect.normalizedPosition = where;
    }

}

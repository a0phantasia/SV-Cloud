using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckDetailPanel : Panel
{
    [SerializeField] private DeckTitleView titleView;
    [SerializeField] private DeckDetailView detailView;

    public void SetDeck(Deck deck) {
        titleView?.SetDeck(deck);
        detailView?.SetDeck(deck, OpenCardDetailPanel);
    }

    private void OpenCardDetailPanel(Card card) {
        var panel = Panel.OpenPanel<CardDetailPanel>();
        panel.SetCard(card);
    }
    
}

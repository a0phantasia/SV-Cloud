using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDetailPanel : Panel
{
    [SerializeField] private CardDetailController detailController;
    public void SetCard(Card card) {
        detailController.SetInitCard(card);
    }
}

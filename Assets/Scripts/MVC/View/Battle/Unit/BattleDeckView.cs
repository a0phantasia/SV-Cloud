using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDeckView : BattleBaseView
{
    [SerializeField] private List<Image> deckImages;

    public void SetDeck(BattleDeck deck) {
        deckImages[0]?.SetSprite(SpriteResources.DeathCard);
        for (int i = 1; i < deckImages.Count; i++) {
            deckImages[i].gameObject.SetActive((i - 1) < deck.Count / 8);
        }
    }
}

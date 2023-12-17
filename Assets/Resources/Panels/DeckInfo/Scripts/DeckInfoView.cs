using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckInfoView : IMonoBehaviour
{
    [SerializeField] private Text nameText, craftText;
    [SerializeField] private Image leaderImage, emeblemImage;
    [SerializeField] private List<Image> sleeveImages;
    [SerializeField] private List<AmountBarView> costBarViews;
    [SerializeField] private List<GameObject> modeObjects;


    public void SetDeck(Deck deck) {
        nameText?.SetText(deck.name);
        craftText?.SetText(((CardCraft)deck.craft).GetCraftName());
        leaderImage?.SetSprite(SpriteResources.GetLeaderProfileSprite(deck.craft));
        emeblemImage?.SetSprite(SpriteResources.GetCardEmblemSprite(deck.craft));
        for (int i = 0; i < costBarViews.Count; i++)
        {
            costBarViews[i].SetAmount(deck.CostDistribution[i]);
        }
    }

    public void SetDeckInfoMode(DeckListMode mode) {
        for (int i = 0; i < modeObjects.Count; i++) {
            modeObjects[i]?.SetActive(i == (int)mode);
        }
    }

}

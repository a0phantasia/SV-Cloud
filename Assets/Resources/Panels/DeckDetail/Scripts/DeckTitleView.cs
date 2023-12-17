using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckTitleView : IMonoBehaviour
{
    [SerializeField] private Text nameText, craftText, cardCountText, maxCountText;
    [SerializeField] private Image emblemImage;
    [SerializeField] private Text followerText, spellText, amuletText, territoryText;
    [SerializeField] private List<AmountBarView> costBarViews;

    public void SetDeck(Deck deck) {
        nameText?.SetText(deck.name);
        craftText?.SetText(((CardCraft)deck.craft).GetCraftName());
        emblemImage?.SetSprite(SpriteResources.GetCardEmblemSprite(deck.craft));
        cardCountText?.SetText(deck.CardCount.ToString("D2"));
        maxCountText?.SetText(deck.MaxCardCount.ToString("D2"));
        cardCountText?.SetColor(deck.CardCount > deck.MaxCardCount ? Color.red : Color.white);
        followerText?.SetText(deck.GetTypeCount(CardType.Follower).ToString("D2"));
        spellText?.SetText(deck.GetTypeCount(CardType.Spell).ToString("D2"));
        amuletText?.SetText(deck.GetTypeCount(CardType.Amulet).ToString("D2"));
        territoryText?.SetText(deck.GetTypeCount(CardType.Territory).ToString("D2"));
        
        for (int i = 0; i < costBarViews.Count; i++)
        {
            costBarViews[i].SetAmount(deck.CostDistribution[i]);
        }
    }
}

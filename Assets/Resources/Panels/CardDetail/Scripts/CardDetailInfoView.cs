using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailInfoView : IMonoBehaviour
{
    [SerializeField] private Image background, craftEmblem;
    [SerializeField] private Text nameText, craftText, traitText;
    [SerializeField] private Text tokenText, packText, authorText;
 
    public void SetCard(Card card) {
        background?.SetSprite(SpriteResources.GetDetailBackgroundSprite(card.CraftId));
        craftEmblem?.SetSprite(SpriteResources.GetCardEmblemSprite(card.CraftId));

        nameText?.SetText(card.name);
        craftText?.SetText(card.Craft.GetCraftName());
        traitText?.SetText(card.traits.GetTraitName());
        tokenText?.SetText(card.Group.GetGroupNote());
        packText?.SetText(card.Pack.GetPackName());
        authorText?.SetText(card.author);
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCardAmuletView : BattleBaseView
{
    [SerializeField] private List<Image> countdownImages;
    [SerializeField] private IButton cardFrameButton;
    [SerializeField] private RawImage artworkRawImage;
    [SerializeField] private Image flagImage, outlineImage;

    public async void SetBattleCard(BattleCard battleCard) {
        if (battleCard == null)
            return;

        var card = battleCard.CurrentCard;
        if (card.Type != CardType.Amulet)
            return;

        cardFrameButton?.SetSprite(SpriteResources.GetBattleCardFrameSprite(card.TypeId, card.RarityId));

        SetCountdown(card.Countdown);
        SetFlagIcon(card.effects);
        SetOutlineColor(Color.clear);
        SetArtwork(await card.Artwork);
    }

    private void SetCountdown(int num) {
        int ten = num % 100 / 10;
        int one = num % 10;

        countdownImages[0]?.SetColor(((ten > 0) || (num < 0)) ? Color.clear : Color.white);
        countdownImages[1]?.SetColor(((ten <= 0)|| (num < 0)) ? Color.clear : Color.white);
        countdownImages[2]?.SetColor(((ten <= 0)|| (num < 0)) ? Color.clear : Color.white);
        countdownImages[0]?.SetSprite(SpriteResources.GetCardCostSprite(one));
        countdownImages[1]?.SetSprite(SpriteResources.GetCardCostSprite(ten));
        countdownImages[2]?.SetSprite(SpriteResources.GetCardCostSprite(one));
    }

    private void SetArtwork(Texture2D artwork) {
        artworkRawImage.SetTexture(artwork ?? SpriteResources.DefaultSleeve?.texture);
    }

    private void SetFlagIcon(List<Effect> effects) {
        var icon = SpriteResources.Empty;
    
        if (effects.Exists(x => x.timing == "on_this_destroy"))
            icon = SpriteResources.Lastword;
        else if (effects.Exists(x => (x.timing.TryTrimStart("on_", out var trimTiming)) && (!trimTiming.StartsWith("this_"))))
            icon = SpriteResources.Flag;
        else if (effects.Exists(x => (x.timing == "on_this_attack") || (x.timing == "on_this_defense") || (x.timing == "on_this_evolve")))
            icon = SpriteResources.Flag;
    
        flagImage?.SetSprite(icon);
    }

    public void SetOutlineColor(Color color) {
        outlineImage?.SetColor(color);
    }
}

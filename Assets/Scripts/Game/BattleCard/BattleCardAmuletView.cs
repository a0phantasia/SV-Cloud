using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCardAmuletView : IMonoBehaviour
{
    [SerializeField] private List<Image> countdownImages;
    [SerializeField] private IButton cardFrameButton;
    [SerializeField] private RawImage artworkRawImage;
    [SerializeField] private Image flagImage;

    public async void SetBattleCard(BattleCard battleCard) {
        if (battleCard == null)
            return;

        var card = battleCard.CurrentCard;
        if (card.Type != CardType.Amulet)
            return;

        SetCountdown(card.Countdown);
        SetArtwork(await card.Artwork);

        //TODO FLAG
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
}

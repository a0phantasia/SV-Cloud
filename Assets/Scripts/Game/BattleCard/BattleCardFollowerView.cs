using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCardFollowerView : IMonoBehaviour
{
    [SerializeField] private Text atkText, hpText;
    [SerializeField] private Outline atkOutline, hpOutline;
    [SerializeField] private IButton cardFrameButton;
    [SerializeField] private RawImage artworkRawImage;
    [SerializeField] private Image flagImage;

    public async void SetBattleCard(BattleCard battleCard) {
        if (battleCard == null)
            return;

        var card = battleCard.CurrentCard;
        
        atkText?.SetText(card.atk.ToString());
        atkText?.SetColor(ColorHelper.GetAtkHpTextColor(card.atk, battleCard.OriginalCard.atk));
        atkOutline?.SetColor(ColorHelper.GetAtkHpOutlineColor(card.atk, battleCard.OriginalCard.atk));

        hpText?.SetText(card.hp.ToString());
        hpText?.SetColor(ColorHelper.GetAtkHpTextColor(card.hp, battleCard.OriginalCard.hp));
        hpOutline?.SetColor(ColorHelper.GetAtkHpOutlineColor(card.hp, battleCard.OriginalCard.hp));

        SetArtwork(await card.Artwork, card.Type);

        //TODO FLAG
        flagImage?.gameObject.SetActive(false);
    }

    private void SetArtwork(Texture2D artwork, CardType type) {
        var euler = new Vector3(0, type == CardType.Evolved ? 180 : 0, 0);
        artworkRawImage.rectTransform.rotation = Quaternion.Euler(euler);
        artworkRawImage.SetTexture(artwork ?? SpriteResources.DefaultSleeve?.texture);
    }
}

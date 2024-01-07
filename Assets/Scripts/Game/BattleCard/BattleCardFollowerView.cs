using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCardFollowerView : BattleBaseView
{
    [SerializeField] private Text atkText, hpText;
    [SerializeField] private Outline atkOutline, hpOutline;
    [SerializeField] private IButton cardFrameButton;
    [SerializeField] private RawImage artworkRawImage;
    [SerializeField] private Image flagImage, outlineImage;

    public async void SetBattleCard(BattleCard battleCard) {
        if (battleCard == null)
            return;

        var card = battleCard.CurrentCard;
        
        atkText?.SetText(card.atk.ToString());
        atkText?.SetColor(ColorHelper.GetAtkHpTextColor(card.atk, card.atk, battleCard.OriginalCard.atk));
        atkOutline?.SetColor(ColorHelper.GetAtkHpOutlineColor(card.atk, card.atk, battleCard.OriginalCard.atk));

        hpText?.SetText(card.hp.ToString());
        hpText?.SetColor(ColorHelper.GetAtkHpTextColor(card.hp, card.hpMax, battleCard.OriginalCard.hp));
        hpOutline?.SetColor(ColorHelper.GetAtkHpOutlineColor(card.hp, card.hpMax, battleCard.OriginalCard.hp));

        SetArtwork(await card.Artwork, card.Type);

        //TODO FLAG
        flagImage?.gameObject.SetActive(false);

        SetOutline(battleCard);
    }

    private void SetArtwork(Texture2D artwork, CardType type) {
        var euler = new Vector3(0, type == CardType.Evolved ? 180 : 0, 0);
        artworkRawImage.rectTransform.rotation = Quaternion.Euler(euler);
        artworkRawImage.SetTexture(artwork ?? SpriteResources.DefaultSleeve?.texture);
    }

    public void SetOutline(BattleCard battleCard) {
        var unit = Hud.CurrentState.GetBelongUnit(battleCard);
        var isLeaderAttackable = battleCard.IsLeaderAttackable(unit);
        var isFollowerAttackable = battleCard.IsFollowerAttackable(unit);
        
        if (isLeaderAttackable) {
            SetOutlineColor(ColorHelper.storm);
        } else if (isFollowerAttackable) {
            SetOutlineColor(ColorHelper.rush);
        } else {
            SetOutlineColor(Color.clear);
        }
    }

    public void SetOutlineColor(Color color) {
        outlineImage?.SetColor(color);
    }
}

using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using UnityEngine.UI;

public class CardInfoView : IMonoBehaviour
{
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private Text nameText, craftTraitText;
    [SerializeField] private GameObject costObject, atkObject, hpObject;
    [SerializeField] private IText descriptionText;
    [SerializeField] private CardInfoView evolveView;

    private Text costText, atkText, hpText;

    protected override void Awake()
    {
        base.Awake();
        if (costObject != null)
            costText = costObject?.GetComponentInChildren<Text>();
        
        atkText = atkObject?.GetComponentInChildren<Text>();
        hpText = hpObject?.GetComponentInChildren<Text>();
    }

    private void Update() {
        if (Input.GetMouseButton(0))
            SetActive(false);
    }

    public void SetActive(bool active) {
        gameObject.SetActive(active);
    }

    public void SetCard(Card card) {
        SetActive(card != null);
        if (card == null) {
            descriptionText?.SetText(string.Empty);
            return;
        }
        string traitName = card.traits.GetTraitName();
        string craftTraitName = card.Craft.GetCraftName() + ((traitName == "-") ? string.Empty : ("/" + traitName)); 
        nameText?.SetText(card.name);
        craftTraitText?.SetText(craftTraitName);
        craftTraitText?.SetFontSize(Mathf.Min(18, 30 - craftTraitName.Length));
        costText?.SetText(card.cost.ToString());
        atkText?.SetText(card.atk.ToString());
        hpText?.SetText(card.hp.ToString());
        descriptionText?.SetText(card.description);
        descriptionText?.Rect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, descriptionText.PreferredSize.y);
        evolveView?.SetCard(card.EvolveCard);

        switch (card.Type) {
            default:
                break;
            case CardType.Leader:
                costObject?.SetActive(false);
                atkObject?.SetActive(false);
                hpObject?.SetActive(false);
                break;    
            case CardType.Spell:
            case CardType.Amulet:
            case CardType.Territory:
                atkObject?.SetActive(false);
                hpObject?.SetActive(false);
                break;    
        }

        float normalSizeY = Mathf.Clamp(GetDescriptionTextPreferredSize(false), 160, 240);
        float evovleSizeY = Mathf.Clamp(GetDescriptionTextPreferredSize(true), 110, 350 - normalSizeY);
        backgroundRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, normalSizeY);
        evolveView?.SetBackgroundAnchoredPos(new Vector2(0, -(normalSizeY + 5)));
        evolveView?.SetBackgroundSizeWithCurrentAnchors(RectTransform.Axis.Vertical, evovleSizeY);
    }

    public void SetBattleCard(BattleCard card) {
        SetCard(card?.CurrentCard);
    }

    public float GetDescriptionTextPreferredSize(bool isEvolve = false) {
        var text = isEvolve ? evolveView?.descriptionText : descriptionText;
        return (text == null) ? 0 : text.PreferredSize.y;
    }

    public void SetBackgroundAnchoredPos(Vector2 pos) {
        if (backgroundRect == null)
            return;

        backgroundRect.anchoredPosition = pos;
    }

    public void SetBackgroundSizeWithCurrentAnchors(RectTransform.Axis axis, float size) {
        backgroundRect?.SetSizeWithCurrentAnchors(axis, size);
    }
}

using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using UnityEngine.UI;

public class CardInfoView : IMonoBehaviour
{
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private RectTransform infoRect;
    [SerializeField] private Text nameText, craftTraitText;
    [SerializeField] private GameObject costObject, atkObject, hpObject;
    [SerializeField] private IText descriptionText;
    [SerializeField] private CardInfoView evolveView;
    [SerializeField] public CardInfoButtonView buttonView;

    private Text costText, atkText, hpText;
    private float normalSizeY, evolveSizeY;

    protected override void Awake()
    {
        base.Awake();
        costText = costObject?.GetComponentInChildren<Text>();
        atkText = atkObject?.GetComponentInChildren<Text>();
        hpText = hpObject?.GetComponentInChildren<Text>();
    }

    private void Update() {
        if (Input.GetMouseButton(0) && (backgroundRect != null)) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(backgroundRect, Input.mousePosition, null, out var point);
            SetActive(backgroundRect.rect.Contains(point));
        }
            
    }

    public void SetActive(bool active) {
        gameObject.SetActive(active);
    }

    public void SetCard(Card card) {
        SetActive(card != null);
        buttonView?.Reset();
        SetBackgroundSizeAuto();

        if (card == null) {
            descriptionText?.SetText(string.Empty);
            return;
        }
        
        string traitName = card.traits.GetTraitName();
        string craftTraitName = card.Craft.GetCraftName() + ((traitName == "-") ? string.Empty : ("/" + traitName)); 

        // Set Texts.
        nameText?.SetText(card.name);
        craftTraitText?.SetText(craftTraitName);
        craftTraitText?.SetFontSize(Mathf.Min(18, 30 - craftTraitName.Length));
        costText?.SetText(card.cost.ToString());
        atkText?.SetText(card.atk.ToString());
        hpText?.SetText(card.hp.ToString());
        descriptionText?.SetText(card.description);
        descriptionText?.Rect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, descriptionText.PreferredSize.y);
        evolveView?.SetCard(card.EvolveCard);

        // Show cost/atk/hp or not.
        costObject?.SetActive((card.Type != CardType.Leader) && (card.Type != CardType.Evolved));
        atkObject?.SetActive((card.Type == CardType.Follower) || (card.Type == CardType.Evolved));
        hpObject?.SetActive((card.Type == CardType.Follower) || (card.Type == CardType.Evolved));

        // Calculate size.
        float normalMinSizeY = (card.Type == CardType.Leader) ? 85 : 160;
        normalSizeY = Mathf.Clamp(GetDescriptionTextPreferredSize(false), normalMinSizeY, 240);
        evolveSizeY = Mathf.Clamp(GetDescriptionTextPreferredSize(true), 110, 350 - normalSizeY);
        
        // Adjust view size.
        infoRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, normalSizeY);
        evolveView?.SetAnchoredPos(new Vector2(0, -(normalSizeY + 5)));
        evolveView?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, evolveSizeY);
        buttonView?.SetAnchoredPos(new Vector2(0, -(normalSizeY + evolveSizeY + 10)));
    }

    public void SetBattleCard(BattleCard card) {
        SetCard(card?.OriginalCard);
    }

    public float GetDescriptionTextPreferredSize(bool isEvolve = false) {
        var text = isEvolve ? evolveView?.descriptionText : descriptionText;
        return (text == null) ? 0 : text.PreferredSize.y;
    }

    /// <summary>
    /// Auto adjust background size with current active button count.
    /// </summary>
    public void SetBackgroundSizeAuto() {
        if (backgroundRect == null)
            return;

        backgroundRect?.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, normalSizeY + evolveSizeY + 10 + 45 * buttonView.ActiveButtonCount);
    }

    public void SetAnchoredPos(Vector2 pos) {
        if (infoRect == null)
            return;

        infoRect.anchoredPosition = pos;
    }

    public void SetSizeWithCurrentAnchors(RectTransform.Axis axis, float size) {
        infoRect?.SetSizeWithCurrentAnchors(axis, size);
    }
}

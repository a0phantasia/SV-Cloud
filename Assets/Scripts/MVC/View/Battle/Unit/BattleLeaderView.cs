using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLeaderView : BattleBaseView
{
    [SerializeField] private int id;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image leaderImage;
    [SerializeField] private Text hpText;
    [SerializeField] private Outline hpOutline;

    private Leader currentLeader;

    public void SetLeader(Leader leader) {
        currentLeader = leader;
        backgroundImage?.SetColor(SpriteResources.GetLeaderBattleColor(leader.Craft));
        leaderImage?.SetSprite(SpriteResources.GetLeaderProfileSprite(leader.CraftId));
        hpText?.SetText(leader.Hp.ToString());
        hpText?.SetColor(ColorHelper.GetAtkHpTextColor(leader.Hp, leader.HpMax, leader.HpInit));
        hpOutline?.SetColor(ColorHelper.GetAtkHpOutlineColor(leader.Hp, leader.HpMax, leader.HpInit));
    }

    public void ShowLeaderInfo() {
        Hud.CurrentCardPlaceInfo = new BattleCardPlaceInfo() { 
            unitId = id,
            place = BattlePlace.Hand,
            index = 0,
        };

        cardInfoView?.SetCard(currentLeader.leaderCard.CurrentCard);        
    }

}

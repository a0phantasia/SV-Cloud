using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLeaderView : BattleBaseView
{
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
        hpText?.SetColor(leader.GetHpTextColor());
        hpOutline?.SetColor(leader.GetHpOutlineColor());
    }

    public void ShowLeaderInfo() {
        cardInfoView?.SetCard(currentLeader.leaderCard.CurrentCard);        
    }

}

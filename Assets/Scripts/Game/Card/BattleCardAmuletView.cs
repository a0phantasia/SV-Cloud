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

    public void SetBattleCard(BattleCard card) {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBaseView : IMonoBehaviour
{
    public Battle Battle => Player.currentBattle;
    public BattleManager Hud => BattleManager.instance;
    [SerializeField] protected CardInfoView cardInfoView;

}

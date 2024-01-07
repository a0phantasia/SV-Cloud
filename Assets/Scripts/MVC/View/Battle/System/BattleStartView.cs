using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class BattleStartView : BattleBaseView
{
    [SerializeField] private BattleOrderView myOrderView, opOrderView;

    public void ShowOrderInfo(Action callback = null) {
        gameObject.SetActive(true);
        myOrderView?.ShowOrderInfo(callback);
        opOrderView?.ShowOrderInfo(null);
    }
}

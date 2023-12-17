using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class BattleOrderView : BattleBaseView
{
    [SerializeField] private float waitSeconds = 3;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Image myLeader, opLeader;
    [SerializeField] private Text myOrder, opOrder;

    public void SetActive(bool active) {
        gameObject.SetActive(active);
    }

    public void ShowOrderInfo(Action callback = null) {
        SetActive(true);
        InitOrderInfo(Battle.currentState.myUnit, Battle.currentState.opUnit);
        StartCoroutine(ShowOrderCoroutine(callback));
    }

    private IEnumerator ShowOrderCoroutine(Action callback) {

        yield return new WaitForSeconds(waitSeconds);

        SetActive(false);

        callback?.Invoke();
    }

    private void InitOrderInfo(BattleUnit myUnit, BattleUnit opUnit) {
        myLeader?.SetSprite(SpriteResources.GetLeaderProfileSprite(myUnit.leader.CraftId));
        opLeader?.SetSprite(SpriteResources.GetLeaderProfileSprite(opUnit.leader.CraftId));
        myOrder?.SetText(myUnit.IsFirstText);
        opOrder?.SetText(opUnit.IsFirstText);
        descriptionText?.SetText("您為" + myUnit.IsFirstText);
    }
}

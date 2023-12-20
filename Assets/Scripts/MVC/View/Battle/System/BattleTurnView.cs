using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleTurnView : BattleBaseView
{
    [SerializeField] private float waitSeconds = 2;
    [SerializeField] private Text whosTurnText;
    [SerializeField] private Text descriptionText;

    public void ShowTurnInfo(string whosTurn, string description, Action callback = null) {
        gameObject.SetActive(true);
        whosTurnText?.SetText(whosTurn);
        descriptionText?.SetText(description);
        StartCoroutine(ShowTurnCoroutine(callback));
    }

    private IEnumerator ShowTurnCoroutine(Action callback) {
        yield return new WaitForSeconds(waitSeconds);
        gameObject.SetActive(false);
        callback?.Invoke();
    }
}

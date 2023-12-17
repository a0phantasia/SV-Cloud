using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleTurnView : BattleBaseView
{
    [SerializeField] private float waitSeconds = 2;
    [SerializeField] private Text whosTurnText;
    [SerializeField] private Text descriptionText;

    public void ShowTurnInfo(string whosTurn, string description) {
        gameObject.SetActive(true);
        whosTurnText?.SetText(whosTurn);
        descriptionText?.SetText(description);
        StartCoroutine(ShowTurnCoroutine());
    }

    private IEnumerator ShowTurnCoroutine() {
        yield return new WaitForSeconds(waitSeconds);
        gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using UnityEngine.UI;

public class BattleLogInfoView : BattleBaseView
{
    private bool isMe = true;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private IButton myButton, opButton;
    private List<LogInfoView> myBattleLogs = new List<LogInfoView>();
    private List<LogInfoView> opBattleLogs = new List<LogInfoView>();

    public void Log(BattleState state) {
        int index = 0;
        int myCount = 0;
        int opCount = 0;
        foreach (var key in Leader.infoKeys) {
            var value = state.myUnit.GetIdentifier(key);
            if (value != 0) {
                if (myBattleLogs.Count < myCount) {
                    var obj = Instantiate(SpriteResources.Log, scrollRect.content);
                    var logPrefab = obj.GetComponent<LogInfoView>();
                    myBattleLogs.Add(logPrefab);
                    obj.SetActive(isMe);
                }
                myBattleLogs[myCount++].LogEffect(Leader.infoValues[index] + "\t\t " + value, state, null);
            }

            value = state.opUnit.GetIdentifier(key);
            if (value != 0) {
                if (opBattleLogs.Count < opCount) {
                    var obj = Instantiate(SpriteResources.Log, scrollRect.content);
                    var logPrefab = obj.GetComponent<LogInfoView>();
                    opBattleLogs.Add(logPrefab);
                    obj.SetActive(!isMe);
                }
                opBattleLogs[opCount++].LogEffect(Leader.infoValues[index] + "\t\t " + value, state, null);
            }
            index++;
        }
    }

    public void SetWho(bool isMe) {
        if (this.isMe == isMe)
            return;
        this.isMe = isMe;
        myButton.SetColor(isMe ? ColorHelper.chosen : Color.black);
        opButton.SetColor(isMe ? Color.black : ColorHelper.chosen);
        myBattleLogs.ForEach(x => x.gameObject.SetActive(isMe));
        opBattleLogs.ForEach(x => x.gameObject.SetActive(!isMe));
    }

}

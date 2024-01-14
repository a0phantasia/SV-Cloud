using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleLogDestroyView : BattleBaseView
{
    private bool isMe = false;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private IButton myButton, opButton;

    private List<LogInfoView> myBattleLogs = new List<LogInfoView>();
    private List<LogInfoView> opBattleLogs = new List<LogInfoView>();

    public void LogDestroy(BattleState state) {
        if (state.currentEffect.ability != EffectAbility.Destroy)
            return;

        var invokeUnit = state.currentEffect.invokeUnit;
        bool isMyLog = invokeUnit.id == state.myUnit.id;
        var destroyedFollowers = invokeUnit.grave.destroyedFollowers;
        var distinctFollowers = invokeUnit.grave.DistinctDestroyedFollowers;
        var battleLogs = isMyLog ? myBattleLogs : opBattleLogs;

        if (battleLogs.Count < distinctFollowers.Count) {
            var obj = Instantiate(SpriteResources.Log, scrollRect.content);
            var logPrefab = obj.GetComponent<LogInfoView>();
            battleLogs.Add(logPrefab);

            obj.SetActive(isMyLog == isMe);
        }
        
        for (int i = 0; i < battleLogs.Count; i++) {
            int copy = i;
            var count = destroyedFollowers.Count(x => x.id == distinctFollowers[i].id);
            battleLogs[i].SetEffect(distinctFollowers[i].name, state, () => cardInfoView?.SetCard(distinctFollowers[copy]));
            battleLogs[i].SetCount("x " + count);
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

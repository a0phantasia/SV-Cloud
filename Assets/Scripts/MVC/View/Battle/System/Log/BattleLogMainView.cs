using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLogMainView : BattleBaseView
{
    [SerializeField] private int maxLogCount = 100;
    [SerializeField] private ScrollRect scrollRect;

    private List<LogInfoView> battleLogs = new List<LogInfoView>();

    public void Log(BattleState state) {
        var effect = state.currentEffect;
        string log = effect.hudOptionDict.Get("log", string.Empty);

        if (string.IsNullOrEmpty(log))
            return;

        if (battleLogs.Count >= maxLogCount) {
            Destroy(battleLogs[0].gameObject);
            battleLogs.RemoveAt(0);
        }

        var obj = Instantiate(SpriteResources.Log, scrollRect.content);
        var logPrefab = obj.GetComponent<LogInfoView>();

        logPrefab.LogEffect(state, () => cardInfoView?.SetCard(effect.source.card));
        battleLogs.Add(logPrefab);
    }

}

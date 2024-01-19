using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleTargetView : BattleBaseView
{
    [SerializeField] private float selectTargetScale = 0.5f;
    [SerializeField] private Vector2 selectTargetPos;
    [SerializeField] private BattleLeaderView myLeaderView, opLeaderView;
    [SerializeField] private List<BattleCardView> myFieldCardViews, opFieldCardViews;
    [SerializeField] private CardView cardView;

    private Queue<Effect> targetEffectQueue = new Queue<Effect>();
    private Effect currentEffect;
    private EffectTargetInfo currentInfo;

    private List<short> currentSelectableList = new List<short>();
    private List<short> selectedTargetList = new List<short>();

    private Action<List<short>> onSuccessTarget;
    private Action onFailTarget;
    private int selectNum = 0, targetNum = 0;

    private void Clear() {
        selectedTargetList.Clear();
        currentSelectableList.Clear();

        targetEffectQueue.Clear();
        targetNum = selectNum = 0;
    }

    public void UseCardSelectTarget(BattleCard useCard, Action<List<short>> onSuccess, Action onFail) {
        var card = useCard.CurrentCard;

        Clear();

        for (int i = 0; i < card.effects.Count; i++) {
            var currentEffect = card.effects[i];
            if (currentEffect.timing != "on_this_use")
                continue;

            currentEffect.invokeUnit = Battle.CurrentState.myUnit;
            if (currentEffect.Condition(Battle.CurrentState)) {
                var info = currentEffect.GetEffectTargetInfo(Battle.CurrentState);

                if ((!List.IsNullOrEmpty(info.mode)) && (info.mode[0] == "index")) {
                    targetNum += info.num;
                    targetEffectQueue.Enqueue(currentEffect);
                }
            }
            currentEffect.invokeTarget = null;
        }

        if (targetNum == 0) {
            onSuccess?.Invoke(selectedTargetList);
            return;
        }

        cardView.rectTransform.anchoredPosition = selectTargetPos;
        cardView.rectTransform.localScale = selectTargetScale * Vector3.one;
        cardView.SetCard(card);

        currentEffect = targetEffectQueue.Dequeue();
        currentInfo = currentEffect.GetEffectTargetInfo(Battle.CurrentState);
        
        onSuccessTarget = onSuccess;
        onFailTarget = onFail;

        ShowTargetSelections();
    }

    private void ShowTargetSelections() {
        if (currentInfo.places.Contains(BattlePlaceId.Hand)) {

        } else {
            var myField = Battle.CurrentState.myUnit.field;
            var opField = Battle.CurrentState.opUnit.field;

            var myIndex = (currentInfo.unit == "op") ? new List<int>() : 
                Enumerable.Range(0, myField.Count).Where(x => currentInfo.types.Contains(myField.cards[x].CurrentCard.Type)).ToList();
            var opIndex = (currentInfo.unit == "me") ? new List<int>() : 
                Enumerable.Range(0, opField.Count).Where(x => currentInfo.types.Contains(opField.cards[x].CurrentCard.Type)).ToList();

            myIndex.ForEach(x => myFieldCardViews[x].SetOutlineColor(ColorHelper.target));
            opIndex.ForEach(x => opFieldCardViews[x].SetOutlineColor(ColorHelper.target));

            myIndex.ForEach(x => currentSelectableList.Add((short)((short)BattlePlaceId.Field * 10 + x)));
            opIndex.ForEach(x => currentSelectableList.Add((short)(100 + (short)BattlePlaceId.Field * 10 + x)));

            if ((currentInfo.unit != "op") && (currentInfo.places.Contains(BattlePlaceId.Leader)))
                currentSelectableList.Add((short)BattlePlaceId.Leader * 10);

            if ((currentInfo.unit != "me") && (currentInfo.places.Contains(BattlePlaceId.Leader)))
                currentSelectableList.Add(100 + (short)BattlePlaceId.Leader * 10);
        }

    }

    public void OnSelectTarget(short code) {
        var info = BattleCardPlaceInfo.Parse(code);
        var fieldCardView = (info.unitId == 0) ? myFieldCardViews : opFieldCardViews;

        if (!currentSelectableList.Contains(code))
            return;

        currentSelectableList.Remove(code);
        selectedTargetList.Add(code);
        selectNum++;

        if (info.place == BattlePlaceId.Field)
            fieldCardView[info.index].SetOutlineColor(Color.cyan);

        if ((selectNum == currentInfo.num) || (currentSelectableList.Count == 0)) {
            if (targetEffectQueue.Count <= 0) {
                OnCancelTarget(true);
                return;
            }

            selectNum = 0;
            currentEffect = targetEffectQueue.Dequeue();
            currentInfo = currentEffect.GetEffectTargetInfo(Battle.CurrentState);

            selectedTargetList.Clear();
            currentSelectableList.Clear();

            ShowTargetSelections();
        }

    }

    public void OnCancelTarget(bool isSuccess) {
        cardView.SetCard(null);

        if (targetNum <= 0)
            return;

        Battle.CurrentState.currentEffect = Effect.Default;
        Hud.SetState(Battle.CurrentState);
        Hud.ProcessQueue();

        if (isSuccess)
            onSuccessTarget?.Invoke(selectedTargetList);
        else {
            Clear();
            onFailTarget?.Invoke();
        }
    }

}

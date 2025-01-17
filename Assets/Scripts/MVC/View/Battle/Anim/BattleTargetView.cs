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
    [SerializeField] private Image selectHandBackground;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private BattleLeaderView myLeaderView, opLeaderView;
    [SerializeField] private List<BattleCardView> myFieldCardViews, opFieldCardViews;
    [SerializeField] private BattleHandView myHandView, opHandView;
    [SerializeField] private CardView cardView;
    [SerializeField] private List<CardView> selectHandViews;
    [SerializeField] private List<IButton> selectHandButtons;

    public bool IsSelectingTarget { get; private set; } = false;

    private Queue<Effect> targetEffectQueue = new Queue<Effect>();
    private Queue<EffectTargetInfo> infoQueue = new Queue<EffectTargetInfo>();
    private Queue<List<short>> selectableQueue = new Queue<List<short>>();

    private EffectTargetInfo currentInfo;

    private List<short> currentSelectableList = new List<short>();
    private List<short> selectedTargetList = new List<short>();

    private Action<List<short>> onSuccessTarget;
    private Action onFailTarget;
    private int selectNum = 0;

    private void Clear() {
        selectedTargetList.Clear();
        currentSelectableList.Clear();

        selectNum = 0;
        targetEffectQueue.Clear();
        infoQueue.Clear();
        selectableQueue.Clear();
    }

    public void StartSelectTarget(string timing, BattleCard sourceCard, Action<List<short>> onSuccess, Action onFail) {
        Clear();

        sourceCard.GetTargetEffectWithTiming(timing, out targetEffectQueue, out infoQueue, out var selectableQueue);

        if (infoQueue.Count <= 0) {
            onSuccess?.Invoke(selectedTargetList);
            return;
        }
        
        IsSelectingTarget = true;
        myHandView.SetHandMode(false);

        if (timing == "on_this_use") {
            cardView.rectTransform.anchoredPosition = selectTargetPos;
            cardView.rectTransform.localScale = selectTargetScale * Vector3.one;
            cardView.SetCard(sourceCard.CurrentCard);
        }

        currentInfo = infoQueue.Dequeue();
        currentSelectableList = selectableQueue.Dequeue();
        
        onSuccessTarget = onSuccess;
        onFailTarget = onFail;

        ShowTargetSelections();
    }

    private void ShowTargetSelections() {
        var cardPlaceInfos = currentSelectableList.Select(BattleCardPlaceInfo.Parse);
        var myHandIndex = cardPlaceInfos.Where(x => (x.unitId == 0) && (x.place == BattlePlaceId.Hand)).Select(x => x.index).ToList();
        var myLeaderIndex = cardPlaceInfos.Where(x => (x.unitId == 0) && (x.place == BattlePlaceId.Leader)).Select(x => x.index).ToList();
        var opLeaderIndex = cardPlaceInfos.Where(x => (x.unitId == 1) && (x.place == BattlePlaceId.Leader)).Select(x => x.index).ToList();
        var myFieldIndex = cardPlaceInfos.Where(x => (x.unitId == 0) && (x.place == BattlePlaceId.Field)).Select(x => x.index).ToList();
        var opFieldIndex = cardPlaceInfos.Where(x => (x.unitId == 1) && (x.place == BattlePlaceId.Field)).Select(x => x.index).ToList();

        if (currentSelectableList.Count == 0) {
            OnSelectTarget(0);
            return;
        }

        selectHandBackground.gameObject.SetActive(false);

        if ((myLeaderIndex.Count > 0) || (opLeaderIndex.Count > 0) ||
            (myFieldIndex.Count > 0) || (opFieldIndex.Count > 0)) {
            myFieldIndex.ForEach(x => myFieldCardViews[x].SetOutlineColor(ColorHelper.target));
            opFieldIndex.ForEach(x => opFieldCardViews[x].SetOutlineColor(ColorHelper.target));
            return;
        }

        selectHandBackground.gameObject.SetActive(true);
        gridLayoutGroup.spacing = new Vector2(Mathf.Max(150 - 25 * myHandIndex.Count, 50), gridLayoutGroup.spacing.y);

        if (myHandIndex.Count > 0) {
            for (int i = 0; i < selectHandViews.Count; i++) {
                int copy = i;
                var card = myHandIndex.Contains(copy) ? Hud.CurrentState.myUnit.hand.cards[copy].CurrentCard : null;
                selectHandViews[i].SetCard(card);
                selectHandViews[i].SetCallback(() => myHandView.ShowHandInfo(copy));
                selectHandButtons[i].gameObject.SetActive(card != null);
            }
            return;
        }
        
    }

    public void OnSelectTarget(int infoCode) {
        var code = (short)infoCode;

        if ((code != 0) && (!currentSelectableList.Contains(code)))
            return;

        var info = BattleCardPlaceInfo.Parse(code);
        var fieldCardView = (info.unitId == 0) ? myFieldCardViews : opFieldCardViews;

        if (code != 0) {
            currentSelectableList.Remove(code);
            selectedTargetList.Add(code);
            selectNum++;

            switch (info.place) {
                default:
                    break;
                case BattlePlaceId.Field:
                    fieldCardView[info.index].SetOutlineColor(Color.cyan);
                    break;
                case BattlePlaceId.Hand:
                    selectHandButtons[info.index].gameObject.SetActive(false);
                    break;   
            }
        }

        if ((selectNum == currentInfo.num) || (currentSelectableList.Count == 0)) {
            selectHandBackground.gameObject.SetActive(false);
            selectedTargetList.Add(0);

            if (infoQueue.Count <= 0) {
                OnCancelTarget(true);
                return;
            }

            selectNum = 0;
            currentInfo = infoQueue.Dequeue();
            currentSelectableList = selectableQueue.Dequeue();

            selectedTargetList.Clear();
            currentSelectableList.Clear();

            ShowTargetSelections();
        }

    }

    public void OnCancelTarget(bool isSuccess) {
        cardView.SetCard(null);
        selectHandBackground.gameObject.SetActive(false);

        if (!IsSelectingTarget)
            return;

        IsSelectingTarget = false;
        myHandView.SetHandMode(true);

        if (isSuccess) {
            onSuccessTarget?.Invoke(selectedTargetList);
        } else {
            Battle.CurrentState.currentEffect = Effect.None;
            Hud.SetState(Battle.CurrentState);
            Hud.ProcessQueue();
            
            onFailTarget?.Invoke();
        }
        
        Clear();
    }

}

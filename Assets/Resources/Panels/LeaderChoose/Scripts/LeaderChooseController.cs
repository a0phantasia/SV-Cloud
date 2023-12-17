using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderChooseController : IMonoBehaviour
{
    [SerializeField] private LeaderChooseModel leaderModel;
    [SerializeField] private LeaderChooseView leaderView;

    public event Action<CardCraft> onConfirmEvent;

    public override void Init()
    {
        base.Init();
        SetStorage(CardDatabase.craftNameDict.Select(entry => entry.Key).ToList());
    }

    public void SetStorage(List<CardCraft> storage) {
        leaderModel.SetStorage(storage);
        Select(0);
    }

    public void Select(int index) {
        if (!index.IsInRange(0, CardDatabase.craftNameDict.Count))
            return;

        leaderModel.Select(index);
        leaderView.Select(index);
    }

    public void OnConfirm() {
        onConfirmEvent?.Invoke(leaderModel.CurrentSelectedItems[0]);
    }
}

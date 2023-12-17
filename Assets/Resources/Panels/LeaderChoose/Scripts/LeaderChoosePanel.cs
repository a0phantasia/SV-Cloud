using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderChoosePanel : Panel
{
    [SerializeField] private LeaderChooseController leaderController;

    public void SetConfirmCallback(Action<CardCraft> callback) {
        leaderController.onConfirmEvent += callback;
    }
}

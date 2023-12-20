using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogInfoView : IMonoBehaviour
{
    [SerializeField] private GameObject buttonObject, splitLineObject;
    [SerializeField] private IButton infoButton;
    [SerializeField] private Outline buttonOutline;
    [SerializeField] private Text infoText, turnText;

    public RectTransform rectTransform { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        rectTransform = gameObject.GetComponent<RectTransform>();
    }

    public void LogEffect(BattleState state, Action onClickCallback = null) {
        var effect = state.currentEffect;
        bool isMyUnit = state.myUnit.id == effect.invokeUnit.id;
        bool buttonMode = effect.ability != EffectAbility.TurnStart;
        var log = effect.hudOptionDict.Get("log", string.Empty);
        Color color = isMyUnit ? Color.cyan : Color.red;

        buttonObject?.SetActive(buttonMode);
        splitLineObject?.SetActive(!buttonMode);

        buttonOutline?.SetColor(color);
        turnText?.SetColor(color);

        infoText?.SetText(log);
        turnText?.SetText(log);
        infoButton?.onPointerClickEvent?.SetListener(onClickCallback);
    }
    
}

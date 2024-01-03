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
    [SerializeField] private Text infoText, countText, turnText;

    public RectTransform rectTransform { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        rectTransform = gameObject.GetComponent<RectTransform>();
    }

    public void SetEffect(string log, BattleState state, Action onClickCallback = null) {
        var effect = state.currentEffect;
        bool buttonMode = effect.ability != EffectAbility.TurnStart;
        bool isMyUnit = state.myUnit.id == effect.invokeUnit.id;
        Color color = isMyUnit ? Color.cyan : Color.red;

        buttonObject?.SetActive(buttonMode);
        splitLineObject?.SetActive(!buttonMode);

        buttonOutline?.SetColor(color);
        turnText?.SetColor(color);

        infoText?.SetText(log);
        turnText?.SetText(log);
        infoButton?.onPointerClickEvent?.SetListener(onClickCallback);
    }

    public void SetLog(string log, Color outlineColor) {
        buttonObject?.SetActive(true);
        splitLineObject?.SetActive(false);
        buttonOutline?.SetColor(outlineColor);
        turnText?.SetColor(outlineColor);
        infoText?.SetText(log);
        turnText?.SetText(log);
    }

    public void SetCount(string count) {
        countText?.SetText(count);
    }
    
}

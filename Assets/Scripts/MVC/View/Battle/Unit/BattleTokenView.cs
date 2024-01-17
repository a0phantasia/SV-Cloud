using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleTokenView : BattleBaseView
{
    [SerializeField] private float waitSeconds = 0.5f;
    [SerializeField] private float getTokenSeconds = 1f;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private HorizontalLayoutGroup layoutGroup;
    [SerializeField] private List<CardView> cardViews;

    public void GetTokenAnim(bool isMe, List<Card> tokens, Action callback) {
        StartCoroutine(GetTokenCoroutine(isMe, tokens, callback));
    }

    private IEnumerator GetTokenCoroutine(bool isMe, List<Card> tokens, Action callback) {
        for (int i = 0; i < cardViews.Count; i++)
            cardViews[i].SetCard((i < tokens.Count) ? tokens[i] : null);
        
        rectTransform.anchoredPosition = new Vector2(GetLayoutGroupPosition(415, tokens.Count), 140);
        layoutGroup.spacing = GetLayoutGroupSpacing(tokens.Count);

        yield return new WaitForSeconds(waitSeconds);

        float currentTime = 0, finishTime = getTokenSeconds, percent = 0;
        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 140 + 320 * percent * (isMe ? -1 : 1));
            currentTime += Time.deltaTime;
            yield return null;
        }

        callback?.Invoke();
    }

    private float GetLayoutGroupPosition(float emptyPos, int count) {
        if (count < 4)
            return emptyPos - count * 50;

        if (count == 4)
            return emptyPos - 190;

        if (count == 5)
            return emptyPos - 215;

        return emptyPos - 235;
    }

    private float GetLayoutGroupSpacing(int count) {
        if (count < 5)
            return 25;

        return count switch {
            5 => 20,
            6 => 12.5f,
            7 => 6.25f,
            8 => 2.5f,
            _ => 0,
        };
    }
}

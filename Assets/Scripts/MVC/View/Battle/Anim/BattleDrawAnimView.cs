using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BattleDrawAnimView : BattleBaseView
{
    [SerializeField] private float drawSeconds;
    [SerializeField] private float waitSeconds = 0.5f;
    [SerializeField] private Vector2 opDrawInitPos;
    [SerializeField] private List<Image> opSleeves;
    [SerializeField] private Image drawSleeve;

    public void MyDrawFromDeck(bool currentHandMode, List<Card> inHand, List<Card> inGrave, Action callback) {
        callback?.Invoke();
    }

    //! Too hard
    /*
    private IEnumerator MyDraw(bool currentHandMode, List<Card> total, Action callback) {
        float currentTime = 0, finishTime = drawSeconds, percent = 0;
        var lastHand = myHandCards.Find(x => !x.gameObject.activeSelf) ?? myHandCards.Last();
        var targetPos = myHandRect.anchoredPosition + lastHand.rectTransform.anchoredPosition;

        for (int i = 0; i < myDrawCards.Count; i++)
            myDrawCards[i].SetCard((i < total.Count) ? total[i] : null);
        
        if (total.Count == 1) {
            while (currentTime < finishTime) {
                percent = currentTime / finishTime;

                currentTime += Time.deltaTime;
                yield return null;
            }
        } else {
            var middlePos = new Vector2(GetLayoutGroupPosition(exhibitPos.x, total.Count), exhibitPos.y);

            myDrawGroup.spacing = GetLayoutGroupSpacing(total.Count);

            while (currentTime < finishTime) {
                percent = currentTime / finishTime;
                myDrawRect.localScale = Vector2.one * Mathf.Lerp(1, 2, percent);
                myDrawRect.anchoredPosition = Vector2.Lerp(myDrawInitPos, middlePos, percent);
                currentTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(waitSeconds);

            currentTime = 0;
            finishTime = drawSeconds;

            while (currentTime < finishTime) {
                percent = currentTime / finishTime;
                
                if (!currentHandMode)
                    myDrawRect.localScale = Vector2.one * Mathf.Lerp(2, 1, percent);

                myDrawRect.anchoredPosition = Vector2.Lerp(middlePos, targetPos, percent);
                currentTime += Time.deltaTime;
                yield return null;
            }

        }

        myDrawRect.localScale = Vector3.one;
        myDrawRect.anchoredPosition = myDrawInitPos;

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
    */

    public void OpDrawFromDeck(Action callback) {
        StartCoroutine(OpDraw(callback));
    }

    private IEnumerator OpDraw(Action callback) {
        float currentTime = 0, finishTime = drawSeconds, percent = 0;
        var lastSleeve = opSleeves.FindLast(x => !x.gameObject.activeSelf) ?? opSleeves[0];
        var targetPos = new Vector2(lastSleeve.rectTransform.anchoredPosition.x - 330, 200);

        drawSleeve.gameObject.SetActive(true);

        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            drawSleeve.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 0.8f, percent);
            drawSleeve.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(-75, -180, percent));
            drawSleeve.rectTransform.anchoredPosition = Vector2.Lerp(opDrawInitPos, targetPos, percent);
            currentTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(waitSeconds);

        drawSleeve.gameObject.SetActive(false);

        callback?.Invoke();
    }
}

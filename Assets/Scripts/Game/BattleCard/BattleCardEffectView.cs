using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCardEffectView : IMonoBehaviour
{
    [SerializeField] private float damageSeconds, damagePosDeltaY;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private Text damageText;
    [SerializeField] private List<GameObject> effectObjects;

    public void SetBattleCard(BattleCard card) {
        if (card == null) {
            effectObjects.ForEach(x => x?.SetActive(false));
            return;
        }
        effectObjects[0]?.SetActive(card.actionController.IsKeywordAvailable(CardKeyword.Ward));
        effectObjects[1]?.SetActive(card.actionController.IsKeywordAvailable(CardKeyword.Ambush));
    }

    public void SetTargeting(bool isTargeting) {
        targetObject?.SetActive(isTargeting);
    }

    public void SetDamage(int damage, Color color, Action callback) {
        StartCoroutine(FlashDamage(damage, color, callback));
    }

    private IEnumerator FlashDamage(int damage, Color color, Action callback) {
        float currentTime = 0, finishTime = damageSeconds, percent = 0;
        var initPos = damageText.rectTransform.anchoredPosition;

        damage = Mathf.Clamp(Mathf.Abs(damage), 0, 99);
        damageText.SetText(damage.ToString());
        damageText.SetColor(color);

        while (currentTime < finishTime) {
            percent = currentTime / finishTime;
            damageText.rectTransform.anchoredPosition = new Vector2(initPos.x, initPos.y + damagePosDeltaY * (color == Color.red ? 0.5f : percent));
            currentTime += Time.deltaTime;
            yield return null;
        }

        damageText.SetColor(Color.clear);
        damageText.rectTransform.anchoredPosition = initPos;

        callback?.Invoke();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCardEffectView : IMonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private Text damageText;
    [SerializeField] private List<GameObject> effectObjects;

    public void SetBattleCard(BattleCard card) {
        if (card == null) {
            effectObjects.ForEach(x => x?.SetActive(false));
            return;
        }
        effectObjects[0]?.SetActive(card.actionController.IsKeywordAvailable(CardKeyword.Ward));
        // effectObjects[1]?.SetActive(card.actionController.IsKeywordAvailable(CardKeyword.Ambush));
    }

    public void SetTargeting(bool isTargeting) {
        targetObject?.SetActive(isTargeting);
    }

    public void SetDamage(int damage, Color color) {
        damage = Mathf.Clamp(Mathf.Abs(damage), 0, 99);
        damageText?.SetText(damage.ToString());
        damageText?.SetColor(color);
    }
}

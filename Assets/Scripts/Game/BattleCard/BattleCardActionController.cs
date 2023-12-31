using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCardActionController
{
    public int StayFieldTurn {
        get => (int)GetIdentifier("stayFieldTurn");
        set => SetIdentifier("stayFieldTurn", value);
    }

    public bool IsLeaderAttackable => (StayFieldTurn > 0) || IsKeywordAvailable(CardKeyword.Storm);
    public bool IsFollowerAttackable => (StayFieldTurn > 0) || IsKeywordAvailable(CardKeyword.Storm) || IsKeywordAvailable(CardKeyword.Rush);

    public Dictionary<string, float> options = new Dictionary<string, float>();

    public BattleCardActionController() {}

    public BattleCardActionController(BattleCardActionController rhs) {
        options = new Dictionary<string, float>(rhs.options);
    }

    public float GetIdentifier(string id) 
    {
        return id switch {
            "isWard" => IsKeywordAvailable(CardKeyword.Ward) ? 1 : 0,
            _ => options.Get(id, 0),
        };
    }

    public void SetIdentifier(string id, float num) {
        switch (id) {
            default:
                options.Set(id, num);
                return;
        }
    }

    public void AddIdentifier(string id, float num) {
        SetIdentifier(id, GetIdentifier(id) + num);
    }

    public bool IsKeywordAvailable(CardKeyword keyword) {
        return GetIdentifier(keyword.GetKeywordEnglishName()) > 0;
    }
}

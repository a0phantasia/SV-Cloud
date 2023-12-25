using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCardActionController
{
    public bool IsWard => GetIdentifier("ward") > 0;
    public Dictionary<string, float> options = new Dictionary<string, float>();

    public BattleCardActionController() {}

    public BattleCardActionController(BattleCardActionController rhs) {
        options = new Dictionary<string, float>(rhs.options);
    }

    public float GetIdentifier(string id) 
    {
        return id switch {
            "isWard" => IsWard ? 1 : 0,
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
}

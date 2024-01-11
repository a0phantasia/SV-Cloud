using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Identifier {

    public static float GetIdentifier(string id, Effect effect, BattleState state) {
        var lhsUnit = effect.invokeUnit;
        var rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        
        return GetNumIdentifier(id);
    }

    public static float GetNumIdentifier(string id) {
        string trimId;
        float num = 0;

        if (id.TryTrimStart("random", out trimId)) {
            if (trimId == string.Empty)
                return Random.Range(0, 100);

            int startIndex = trimId.IndexOf('[');
            int middleIndex = trimId.IndexOf('~');
            int endIndex = trimId.IndexOf(']');
            string startExpr = trimId.Substring(startIndex + 1, middleIndex - startIndex - 1);
            string endExpr = trimId.Substring(middleIndex + 1, endIndex - middleIndex - 1);
            int startRange = int.Parse(startExpr);
            int endRange = int.Parse(endExpr);

            return Random.Range(startRange, endRange + 1);
        }
        return float.TryParse(id, out num) ? num : 0;
    }

}

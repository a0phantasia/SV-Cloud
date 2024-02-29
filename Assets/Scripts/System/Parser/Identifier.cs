using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Identifier {

    public static float GetIdentifier(string id, Effect effect, BattleState state) {
        var trimId = string.Empty;
        var lhsUnit = effect.invokeUnit;
        var rhsUnit = state.GetRhsUnitById(lhsUnit.id);

        if (id.TryTrimStart("enhance", out trimId)) {
            var enhance = Parser.ParseEffectExpression(trimId.TrimParentheses(), effect, state);
            return lhsUnit.leader.PP >= enhance ? 1 : 0;
        }

        if (id.TryTrimStart("earth", out trimId)) {
            var earth = Parser.ParseEffectExpression(trimId.TrimParentheses(), effect, state);
            var filter = BattleCardFilter.Parse("[trait:3][type:3]");
            return (lhsUnit.field.cards.Count(filter.FilterWithCurrentCard) >= earth) ? 1 : 0;
        }

        if (id.TryTrimStart("necromance", out trimId)) {
            var necromance = Parser.ParseEffectExpression(trimId.TrimParentheses(), effect, state);
            return (lhsUnit.grave.GraveCount >= necromance) ? 1 : 0;
        }
        
        if (id.TryTrimStart("sourceEffect.", out trimId))
            return effect.sourceEffect.GetSourceEffectIdentifier(trimId, state);

        if (id.TryTrimStart("effect.", out trimId))
            return effect.GetIdentifier(trimId);
        
        if (id.TryTrimStart("me.", out trimId))
            return lhsUnit.GetIdentifier(trimId);

        if (id.TryTrimStart("op.", out trimId))
            return rhsUnit.GetIdentifier(trimId);

        return GetNumIdentifier(id);
    }

    public static float GetIdentifier(string id, Buff buff, BattleState state)
    {
        var trimId = string.Empty;
        var lhsUnit = state.GetBelongUnit(buff.source);
        var rhsUnit = state.GetRhsUnitById(lhsUnit.id);

        if (id.TryTrimStart("sourceEffect.", out trimId))
            return buff.sourceEffect.GetSourceEffectIdentifier(trimId, state);

        if (id.TryTrimStart("buff.", out trimId))
            return buff.GetIdentifier(trimId);

        if (id.TryTrimStart("me.", out trimId))
            return lhsUnit.GetIdentifier(trimId);

        if (id.TryTrimStart("op.", out trimId))
            return rhsUnit.GetIdentifier(trimId);

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

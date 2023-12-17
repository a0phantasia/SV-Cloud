using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EffectDatabase {

    private static Dictionary<string, EffectTiming> timingConvDict = new Dictionary<string, EffectTiming>() {
        {"none", EffectTiming.None},
        {"resident", EffectTiming.Resident},
        {"on_battle_start", EffectTiming.OnBattleStart},
        {"on_turn_start", EffectTiming.OnTurnStart},
        {"on_before_dmg_cal", EffectTiming.OnBeforeDamageCalculate},
        {"on_dmg_cal", EffectTiming.OnDamageCalculate},
        {"on_after_dmg_cal", EffectTiming.OnAfterDamageCalculate},
        {"on_final_dmg_cal", EffectTiming.OnFinalDamageCalculate},
        {"on_before_attack", EffectTiming.OnBeforeAttack},
        {"on_attack", EffectTiming.OnAttack},
        {"on_after_attack", EffectTiming.OnAfterAttack},
        {"on_turn_end", EffectTiming.OnTurnEnd},
        {"on_battle_end", EffectTiming.OnBattleEnd},
    };

    private static Dictionary<string, EffectTarget> targetConvDict = new Dictionary<string, EffectTarget>() {
        {"none", EffectTarget.None},
    };

    private static Dictionary<string, EffectCondition> condConvDict = new Dictionary<string, EffectCondition>() {
        {"none", EffectCondition.None},
    };

    private static Dictionary<string, EffectAbility> typeConvDict = new Dictionary<string, EffectAbility>() {
        {"none", EffectAbility.None},
    };

    public static EffectTiming ToEffectTiming(this string timing) {
        return timingConvDict.Get(timing, EffectTiming.None);
    }

    public static EffectTarget ToEffectTarget(this string target) {
        return targetConvDict.Get(target, EffectTarget.None);
    }

    public static EffectCondition ToEffectCondition(this string condition) {
        return condConvDict.Get(condition, EffectCondition.None);
    }

    public static EffectAbility ToEffectAbility(this string ability) {
        return typeConvDict.Get(ability, EffectAbility.None);
    }
}

public enum EffectTiming {
    None = 0, Resident = 1, OnBattleStart = 2, OnTurnStart = 3,
    OnBeforeDamageCalculate = 10, OnDamageCalculate = 11, OnAfterDamageCalculate = 12, OnFinalDamageCalculate = 13,
    OnBeforeAttack = 14, OnAttack = 15, OnAfterAttack = 16, 
    OnTurnEnd = 17, OnBattleEnd = 999,
}

public enum EffectTarget {
    None,
}

public enum EffectCondition {
    None,
}

public enum EffectAbility {
    None,
}
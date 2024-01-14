using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;

public static class EffectDatabase {

    private static Dictionary<string, EffectCondition> condConvDict = new Dictionary<string, EffectCondition>() {
        {"none", EffectCondition.None},
    };

    private static Dictionary<string, EffectAbility> abilityConvDict = new Dictionary<string, EffectAbility>() {
        {"none",        EffectAbility.None},
        {"result",      EffectAbility.SetResult},
        {"keep",        EffectAbility.KeepCard},
        {"turn_start",  EffectAbility.TurnStart},
        {"turn_end",    EffectAbility.TurnEnd},
        {"use",         EffectAbility.Use},
        {"cover",       EffectAbility.Cover},
        {"attack",      EffectAbility.Attack},
        {"evolve",      EffectAbility.Evolve},
        {"fusion",      EffectAbility.Fusion},
        {"act",         EffectAbility.Act},

        {"set_keyword", EffectAbility.SetKeyword},
        {"draw",        EffectAbility.Draw},
        {"summon",      EffectAbility.Summon},
        {"damage",      EffectAbility.Damage},
        {"heal",        EffectAbility.Heal},
        {"destroy",     EffectAbility.Destroy},
        {"vanish",      EffectAbility.Vanish},
        {"buff",        EffectAbility.Buff},
        {"debuff",      EffectAbility.Debuff},
        {"get_token",   EffectAbility.GetToken},
    };

    public static EffectCondition ToEffectCondition(this string condition) {
        return condConvDict.Get(condition, EffectCondition.None);
    }

    public static EffectAbility ToEffectAbility(this string ability) {
        return abilityConvDict.Get(ability, EffectAbility.None);
    }

    public static void SetInvokeTarget(this Effect effect, BattleState state) {
        var invokeUnit = effect.invokeUnit;
        var rhsUnit = state.GetRhsUnitById(invokeUnit.id);
        var options = effect.target.Split('_');

        if (options[0] == "none")
            return;

        if (options[0] == "self") {
            effect.invokeTarget = new List<BattleCard>() { effect.source };
        } else {
            var allUnit = options[0] switch {
                "all"   =>  new List<BattleUnit>() { invokeUnit, rhsUnit },
                "me"    =>  new List<BattleUnit>() { invokeUnit },
                "op"    =>  new List<BattleUnit>() { rhsUnit },
                _       =>  new List<BattleUnit>(),
            };
            var place = options[1].Split('+');
            var type = options[2].Split('+').Select(x => x.ToCardTypeWithEnglish()).ToList();
            var num = Parser.ParseEffectExpression(options[3], effect, state);
            var mode = options[4].Split('+');

            var allPlace = new List<BattlePlace>();
            for (int i = 0; i < place.Length; i++)
                allUnit.ForEach(unit => allPlace.Add(unit.GetPlace(place[i].ToBattlePlace())));

            var allCards = new List<BattleCard>();
            allPlace.ForEach(x => allCards.AddRange(x.cards.Where(c => type.Contains(c.CurrentCard.Type))));

            if (mode.Contains("excludeSelf"))
                allCards.RemoveAll(x => x == effect.source);

            effect.invokeTarget = mode[0] switch {
                "random"    => allCards.Random(num, false),
                "index"     => options.SubArray(5).Select(x => allCards[int.Parse(x)]).ToList(),
                _ => effect.invokeTarget,
            };
        }
    }
}

public enum EffectCondition {
    None,
}

public enum EffectAbility {
    None = 0,   SetResult = 1,  KeepCard = 2,   TurnStart = 3,  TurnEnd = 4,
    Use = 5,    Cover = 6,  Attack = 7, Evolve = 8, Fusion = 9, Act = 10,

    SetKeyword  = 101,   
    Draw        = 102, 
    Summon      = 103,   
    Damage      = 104,   
    Heal        = 105,
    Destroy     = 106,
    Vanish      = 107,
    Buff        = 108,
    Debuff      = 109,
    GetToken    = 110,
}
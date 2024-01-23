using System;
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
        {"none",        EffectAbility.None          },
        {"result",      EffectAbility.SetResult     },
        {"keep",        EffectAbility.KeepCard      },
        {"turn_start",  EffectAbility.TurnStart     },
        {"turn_end",    EffectAbility.TurnEnd       },
        {"use",         EffectAbility.Use           },
        {"cover",       EffectAbility.Cover         },
        {"attack",      EffectAbility.Attack        },
        {"evolve",      EffectAbility.Evolve        },
        {"fusion",      EffectAbility.Fusion        },
        {"act",         EffectAbility.Act           },

        {"set_keyword", EffectAbility.SetKeyword    },
        {"draw",        EffectAbility.Draw          },
        {"summon",      EffectAbility.Summon        },
        {"damage",      EffectAbility.Damage        },
        {"heal",        EffectAbility.Heal          },
        {"destroy",     EffectAbility.Destroy       },
        {"vanish",      EffectAbility.Vanish        },
        {"return",      EffectAbility.Return        },
        {"transform",   EffectAbility.Transform     },
        {"buff",        EffectAbility.Buff          },
        {"debuff",      EffectAbility.Debuff        },

        {"get_token",   EffectAbility.GetToken      },
        {"boost",       EffectAbility.SpellBoost    },
        {"set_cost",    EffectAbility.SetCost       },
    };

    public static EffectCondition ToEffectCondition(this string condition) {
        return condConvDict.Get(condition, EffectCondition.None);
    }

    public static EffectAbility ToEffectAbility(this string ability) {
        return abilityConvDict.Get(ability, EffectAbility.None);
    }

    public static bool IsTargetSelectableAbility(this EffectAbility ability) {
        return (ability == EffectAbility.Use) || (ability == EffectAbility.Evolve);
    }

}

public class EffectTargetInfo 
{
    public Effect effect;
    public string unit;
    public List<BattlePlaceId> places;
    public int num;
    public List<string> mode;
    public BattleCardFilter filter = new BattleCardFilter(-1);
    public List<string> options;

    public static EffectTargetInfo Parse(Effect effect, BattleState state) {
        var info = new EffectTargetInfo();
        var options = effect.target.Split('_');

        info.unit = options[0];
        if ((info.unit == "none") || (info.unit == "self"))
            return info;

        info.places = options[1].Split('+').Select(x => x.ToBattlePlace()).ToList();
        info.num = Parser.ParseEffectExpression(options[2], effect, state);
        info.mode = options[3].Split('+').ToList();

        if (options.Length <= 4)
            return info;

        info.filter = BattleCardFilter.Parse(options[4]);
        info.options = options.SubArray(5).ToList();
        return info;
    }
}

public enum EffectCondition {
    None,
}

public enum EffectAbility {
    None = 0,   SetResult = 1,  KeepCard = 2,   TurnStart = 3,  TurnEnd = 4,
    Use = 5,    Cover = 6,  Attack = 7, Evolve = 8, Fusion = 9, Act = 10,

    SetKeyword  = 100,   
    Draw        = 101, 
    Summon      = 102,   
    Damage      = 103,   
    Heal        = 104,
    Destroy     = 105,
    Vanish      = 106,
    Return      = 107,
    Transform   = 108,
    Buff        = 109,
    Debuff      = 110,
    GetToken    = 111,
    SpellBoost  = 112,
    SetCost     = 113,
}
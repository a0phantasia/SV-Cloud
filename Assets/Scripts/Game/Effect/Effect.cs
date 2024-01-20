using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;

public class Effect : IIdentifyHandler
{
    public const int DATA_COL = 7;
    public static Effect Get(int id) {
        var effect = DatabaseManager.instance.GetEffectInfo(id);
        return (effect == null) ? null : new Effect(effect);
    }
    public static Effect None => new Effect("none", "none", EffectCondition.None, null, EffectAbility.None, null);

    public BattleCard source = null;
    public List<BattleCard> invokeTarget = null;
    public BattleUnit invokeUnit = null;

    public int id;
    public int Id => id;
    public string timing { get; private set; }
    public string target { get; private set; }
    public EffectCondition condition { get; private set; }
    public List<List<ICondition>> condOptionDictList { get; private set; } = new List<List<ICondition>>();
    public EffectAbility ability { get; private set; }
    public Dictionary<string, string> abilityOptionDict { get; private set; } = new Dictionary<string, string>();
    public Dictionary<string, string> hudOptionDict { get; private set; } = new Dictionary<string, string>();

    public Effect(string[] _data, int startIndex) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        source = null;
        id = int.Parse(_slicedData[0]);
        timing = _slicedData[1];
        target = _slicedData[2];
        condition = _slicedData[3].ToEffectCondition();
        condOptionDictList.ParseMultipleCondition(_slicedData[4]);
        ability = _slicedData[5].ToEffectAbility();
        abilityOptionDict.ParseOptions(_slicedData[6]);
        hudOptionDict = new Dictionary<string, string>();
    }

    public Effect(string _timing, string _target, 
        EffectCondition _condition, List<List<ICondition>> _condition_option,
        EffectAbility _ability, Dictionary<string, string> _ability_option) {
        source = null;
        timing = _timing;
        target = _target;
        condition = _condition;
        if (_condition_option == null) {
            condOptionDictList.Add(new List<ICondition>());
        } else {
            condOptionDictList = _condition_option;
        }
        ability = _ability;
        abilityOptionDict = _ability_option ?? new Dictionary<string, string>();
        hudOptionDict = new Dictionary<string, string>();
    }

    public Effect(int[] data) {
        source = null;
        timing = "none";
        target = "none";
        condition = EffectCondition.None;
        condOptionDictList.Add(new List<ICondition>());
        ability = (EffectAbility)data[0];
        abilityOptionDict = Parse(ability, data.SubArray(1));
        hudOptionDict = new Dictionary<string, string>();
    }

    public Effect(Effect rhs) {
        id = rhs.id;
        timing = rhs.timing;
        target = rhs.target;
        condition = rhs.condition;
        ability = rhs.ability;
        condOptionDictList = rhs.condOptionDictList.Select(x => x.Select(y => new ICondition(y.op, y.lhs, y.rhs)).ToList()).ToList();
        abilityOptionDict = new Dictionary<string, string>(rhs.abilityOptionDict);
        hudOptionDict = new Dictionary<string, string>(rhs.hudOptionDict);

        source = rhs.source;
        invokeUnit = rhs.invokeUnit;
        invokeTarget = rhs.invokeTarget;
    }

    public bool TryGetIdenfier(string id, out float value)
    {
        value = GetIdentifier(id);
        return value != float.MinValue;
    }

    public float GetIdentifier(string id)
    {
        var state = Player.currentBattle.CurrentState;
        var trimId = string.Empty;

        if (id.TryTrimStart("source.", out trimId)) {
            return trimId switch {
                "where" => (float)(invokeUnit.GetBelongPlace(source)?.placeId ?? BattlePlaceId.None),
                _ => source.GetIdentifier(trimId),
            };
        }

        return id switch {
            _ => float.MinValue,
        };
    }

    public float GetLastEffectIdentifier(string id, BattleState state) {
        var effect = state.currentEffect;
        var trimId = string.Empty;

        if (id.TryTrimStart("target", out trimId)) {
            var all = effect.invokeTarget;
            var me = all.Where(x => state.GetBelongUnit(x).id == invokeUnit.id).ToList();
            var op = all.Where(x => state.GetBelongUnit(x).id == state.GetRhsUnitById(invokeUnit.id).id).ToList();

            if (trimId.TryTrimStart("[all].", out trimId)) {
                return trimId switch {
                    "count" => all.Count,
                    "isMe"  => (all.Count == me.Count) ? 1 : 0,
                    "isOp"  => (all.Count == op.Count) ? 1 : 0,
                    _       => float.MinValue,
                };
            } else if (trimId.TryTrimStart("[me].", out trimId)) {
                return trimId switch {
                    "count" => me.Count,
                    _       => float.MinValue,
                };
            } else if (trimId.TryTrimStart("[op].", out trimId)) {
                return trimId switch {
                    "count" => op.Count,
                    _       => float.MinValue,
                };
            }
        }

        return effect.GetIdentifier(id);
    }

    public void SetIdentifier(string id, float value)
    {
        
    }

    public EffectTargetInfo GetEffectTargetInfo(BattleState state) {
        return EffectTargetInfo.Parse(this, state);
    }

    public Dictionary<string, string> Parse(EffectAbility action, int[] data) {
        Func<int[], Dictionary<string, string>> ParseFunc = action switch {
            EffectAbility.SetResult => EffectParseHandler.SetResult,
            EffectAbility.KeepCard  => EffectParseHandler.KeepCard,
            EffectAbility.Use       => EffectParseHandler.Use,
            EffectAbility.Attack    => EffectParseHandler.Attack,
            EffectAbility.Evolve    => EffectParseHandler.Evolve,

            EffectAbility.Draw      => EffectParseHandler.Draw,
            EffectAbility.Summon    => EffectParseHandler.Summon,
            _ => ((data) => new Dictionary<string, string>()),
        };
        return ParseFunc.Invoke(data);
    }

    public bool Condition(BattleState state) {
        return condOptionDictList.Select(each => each.All(
            cond => Operator.Condition(cond.op, 
                Parser.ParseEffectExpression(cond.lhs, this, state),
                Parser.ParseEffectExpression(cond.rhs, this, state)
            )
        )).Any(x => x);
    }
    
    public bool Apply(BattleState state = null) {
        if (state != null)
            state.currentEffect = this;

        Func<Effect, BattleState, bool> AbilityFunc = ability switch {
            EffectAbility.SetResult => EffectAbilityHandler.SetResult,
            EffectAbility.KeepCard  => EffectAbilityHandler.KeepCard,
            EffectAbility.TurnStart => EffectAbilityHandler.OnTurnStart,
            EffectAbility.TurnEnd   => EffectAbilityHandler.OnTurnEnd,
            EffectAbility.Use       => EffectAbilityHandler.Use,
            EffectAbility.Attack    => EffectAbilityHandler.Attack,
            EffectAbility.Evolve    => EffectAbilityHandler.Evolve,

            EffectAbility.SetKeyword=> EffectAbilityHandler.SetKeyword,
            EffectAbility.Draw      => EffectAbilityHandler.Draw,
            EffectAbility.Summon    => EffectAbilityHandler.Summon,
            EffectAbility.Damage    => EffectAbilityHandler.Damage,
            EffectAbility.Destroy   => EffectAbilityHandler.Destroy,
            EffectAbility.Return    => EffectAbilityHandler.Return,
            EffectAbility.Buff      => EffectAbilityHandler.Buff,
            EffectAbility.GetToken  => EffectAbilityHandler.GetToken,
            _ => (e, s) => true,
        };

        var repeat = int.Parse(abilityOptionDict.Get("repeat", "1"));
        
        return Enumerable.Range(0, repeat).Select(x => AbilityFunc.Invoke(this, state)).All(x => x);
    }

    public bool CheckAndApply(BattleState state = null) {
        if (!Condition(state))
            return false;
        
        return Apply(state);
    }
}
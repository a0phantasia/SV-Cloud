using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : IIdentifyHandler
{
    public const int DATA_COL = 7;
    public static Effect Get(int id) {
        var effect = DatabaseManager.instance.GetEffectInfo(id);
        return (effect == null) ? null : new Effect(effect);
    }

    public BattleCard source = null;
    public List<BattleCard> invokeTarget = null;
    public BattleUnit invokeUnit = null;

    public int id;
    public int Id => id;
    public string timing { get; private set; }
    public EffectTarget target { get; private set; }
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
        target = _slicedData[2].ToEffectTarget();
        condition = _slicedData[3].ToEffectCondition();
        condOptionDictList.ParseMultipleCondition(_slicedData[4]);
        ability = _slicedData[5].ToEffectAbility();
        abilityOptionDict.ParseOptions(_slicedData[6]);
        hudOptionDict = new Dictionary<string, string>();
    }

    public Effect(string _timing, EffectTarget _target, 
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
        target = EffectTarget.None;
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
        throw new NotImplementedException();
    }

    public float GetIdentifier(string id)
    {
        throw new NotImplementedException();
    }

    public void SetIdentifier(string id, float value)
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, string> Parse(EffectAbility action, int[] data) {
        Func<int[], Dictionary<string, string>> ParseFunc = action switch {
            EffectAbility.SetResult => EffectParseHandler.SetResult,
            EffectAbility.KeepCard  => EffectParseHandler.KeepCard,
            EffectAbility.Use       => EffectParseHandler.Use,
            EffectAbility.Draw      => EffectParseHandler.Draw,
            _ => ((data) => new Dictionary<string, string>()),
        };
        return ParseFunc.Invoke(data);
    }

    public bool Condition(BattleState state) {
        Func<List<ICondition>, bool> ConditionFunc = condition switch {
            _ => ((x) => true)
        };

        return condOptionDictList.Select(x => ConditionFunc.Invoke(x)).Any(x => x);
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
            EffectAbility.Draw      => EffectAbilityHandler.Draw,
            _ => (e, s) => true,
        };
        return AbilityFunc.Invoke(this, state);
    }

    public bool CheckAndApply(BattleState state = null) {
        if (!Condition(state))
            return false;
        
        return Apply(state);
    }
}
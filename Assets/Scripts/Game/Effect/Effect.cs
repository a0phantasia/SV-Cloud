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

    public object source = null;
    public object invokeUnit = null;

    public int id;
    public int Id => id;
    public EffectTiming timing { get; private set; }
    public EffectTarget target { get; private set; }
    public EffectCondition condition { get; private set; }
    public List<List<ICondition>> condOptionDictList { get; private set; } = new List<List<ICondition>>();
    public EffectAbility ability { get; private set; }
    public Dictionary<string, string> abilityOptionDict { get; private set; } = new Dictionary<string, string>();

    public Effect(string[] _data, int startIndex) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        source = null;
        id = int.Parse(_slicedData[0]);
        timing = _slicedData[1].ToEffectTiming();
        target = _slicedData[2].ToEffectTarget();
        condition = _slicedData[3].ToEffectCondition();
        condOptionDictList.ParseMultipleCondition(_slicedData[4]);
        ability = _slicedData[5].ToEffectAbility();
        abilityOptionDict.ParseOptions(_slicedData[6]);
    }

    public Effect(EffectTiming _timing, EffectTarget _target, 
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
    }

    public Effect(int action, int[] data) {
        source = null;
        timing = EffectTiming.None;
        target = EffectTarget.None;
        condition = EffectCondition.None;
        condOptionDictList.Add(new List<ICondition>());
        ability = (EffectAbility)action;
        abilityOptionDict = new Dictionary<string, string>();
    }

    public Effect(Effect rhs) {
        id = rhs.id;
        timing = rhs.timing;
        target = rhs.target;
        condition = rhs.condition;
        ability = rhs.ability;
        condOptionDictList = rhs.condOptionDictList.Select(x => x.Select(y => new ICondition(y.op, y.lhs, y.rhs)).ToList()).ToList();
        abilityOptionDict = new Dictionary<string, string>(rhs.abilityOptionDict);
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

    /*
    public bool Condition(object invokeUnit, BattleState state, bool checkPhase = true, bool checkTurn = true) {
        bool isCorrectPhase = ((state == null) && (timing == EffectTiming.Resident)) || 
            ((state != null) && (state.phase == timing));

        if (checkPhase && !isCorrectPhase)
            return false;

        this.invokeUnit = invokeUnit;

        if (checkTurn && !condOptionDictList.Select(x => this.IsCorrectTurn(state, x)).Any(x => x))
            return false;

        if (!condOptionDictList.Select(x => this.RandomNumber(state, x)).Any(x => x))
            return false;
        
        Func<Dictionary<string, string>, bool> ConditionFunc = condition switch {
            _ => ((x) => true)
        };

        return condOptionDictList.Select(x => ConditionFunc.Invoke(x)).Any(x => x);
    }

    public bool Apply(object invokeUnit, BattleState state = null) {
        this.invokeUnit = invokeUnit;
        return type switch {
            _ => true
        };
    }

    public bool CheckAndApply(object invokeUnit, BattleState state = null, bool checkPhase = true, bool checkTurn = true) {
        if (!Condition(invokeUnit, state, checkPhase, checkTurn))
            return false;
        
        return Apply(invokeUnit, state);
    }
    */
}
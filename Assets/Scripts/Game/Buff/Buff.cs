using System;
using System.Collections.Generic;
using System.Linq;

public class Buff : IIdentifyHandler
{
    public const int DATA_COL = 9;

    public static Buff Get(int id)
    {
        var buff = DatabaseManager.instance.GetBuffInfo(id);
        return (buff == null) ? null : new Buff(buff);
    }

    public int id;
    public int Id => id;

    public string name;
    public string Name => name;

    public int cost, atk, hp;

    public BattleCard source = null;

    public Effect sourceEffect = null;

    public string timing;

    public int StayFieldTurn
    {
        get => (int)GetIdentifier("stayFieldTurn");
        set => SetIdentifier("stayFieldTurn", value);
    }

    public List<CardTrait> traits = new List<CardTrait>();
    public List<CardKeyword> keywords = new List<CardKeyword>();
    public List<int> effectIds = new List<int>();
    public List<Effect> effects = new List<Effect>();
    public Dictionary<string, string> options = new Dictionary<string, string>();
    public List<List<ICondition>> condOptionDictList { get; private set; } = new List<List<ICondition>>();
    public string description;

    public Buff(string[] _data, int startIndex)
    {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        source = null;
        id = int.Parse(_slicedData[0]);
        name = _slicedData[1];
        var status = _slicedData[2].ToIntList('/');
        cost = status[0];
        atk = status[1];
        hp = status[2];

        traits = _slicedData[3].ToIntList('/').Select(x => (CardTrait)x).ToList();
        keywords = _slicedData[4].ToIntList('/').Select(x => (CardKeyword)x).ToList();

        timing = _slicedData[5];
        condOptionDictList.ParseMultipleCondition(_slicedData[6]);
        effectIds = _slicedData[7].ToIntList('/');
        description = _slicedData[8].GetDescription();

        SetEffects(effectIds);
    }

    public Buff(int _cost, int _atk, int _hp, string _timing,
    string _condition_option, string _name = "default")
    {
        source = null;
        cost = _cost;
        atk = _atk;
        hp = _hp;
        name = _name;
        timing = _timing;
        condOptionDictList.ParseMultipleCondition(_condition_option);
    }

    public Buff(Buff rhs)
    {
        source = null;

        id = rhs.id;
        name = rhs.name;
        cost = rhs.cost;
        atk = rhs.atk;
        hp = rhs.hp;
        traits = rhs.traits.ToList();
        keywords = rhs.keywords.ToList();
        effectIds = new List<int>(rhs.effectIds);
        //options = new Dictionary<string, string>(rhs.options);

        description = rhs.description;

        SetEffects(rhs.effectIds);
    }

    public void SetEffects(List<int> effectIds, Func<int, Effect> effectFunc = null)
    {
        effectFunc ??= (x => Effect.Get(x));
        effects = effectIds.Select(effectFunc).Where(x => x != null).ToList();
    }

    public void ClearEffects(string timing = "all", int id = 0, CardKeyword keyword = CardKeyword.None)
    {
        if (timing != "all")
        {
            effectIds.RemoveAll(x => Effect.Get(x)?.timing == timing);
            effects.RemoveAll(x => x.timing == timing);
        }
        else
        {
            effectIds.Clear();
            effects.Clear();
            keywords.Clear();
            return;
        }

        if (id != 0)
        {
            effectIds.RemoveAll(x => x == id); 
            effects.RemoveAll(x => x.id == id);
        }

        if (keyword != CardKeyword.None)
        {
            keywords.RemoveAll(x => x == keyword);
        }
    }

    public bool TryGetIdenfier(string id, out float value) {
        value = GetIdentifier(id);
        return true;
    }
    public float GetIdentifier(string id)
    {
        return int.Parse(options.Get(id));
    }
    public void SetIdentifier(string id, float value)
    {
        options.Set(id, value.ToString());
    }

    public bool Condition(BattleState state)
    {
        return condOptionDictList.Exists(each => each.All(
            cond => Operator.Condition(cond.op,
                Parser.ParseBuffExpression(cond.lhs, this, state),
                Parser.ParseBuffExpression(cond.rhs, this, state)
            )
        ));
    }

    public void OnTurnStart()
    {
        
    }

    public bool IsEmpty()
    {
        bool isNumberEmpty = cost == 0 && atk == 0 && hp == 0;

        bool isListsEmpty = !traits.Any() && !keywords.Any() && !effectIds.Any() && !effects.Any();

        return isNumberEmpty && isListsEmpty;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardFilter
{
    public virtual string[] GetSetStringType() => new string[] { "name", "description" };
    public virtual string[] GetSetIntType() => new string[] { "format", "zone" };
    public virtual string[] GetSelectIntType() => new string[] { 
        "uid", "id", "craft", "pack", "type", "rarity", "trait", "keyword",
        "cost", "atk", "hp", "countdown" };
    public virtual string[] GetSetBoolType() => new string[] { "token" };

    public int format, zone;
    public string name, description;
    public List<int> uidList, idList, craftList, packList, typeList, rarityList, traitList, keywordList,
        costList, atkList, hpList, countdownList;
    public bool isWithToken;

    /// <summary>
    /// Create a filter to search card from database.
    /// </summary>
    /// <param name="formatId">Format id. Set -1 if don't filter format</param>
    public CardFilter(int formatId) {
        format = formatId;
        zone = -1;
        name = description = string.Empty;
        uidList = new List<int>();
        idList = new List<int>();
        craftList = new List<int>();
        packList = new List<int>();
        typeList = new List<int>();
        rarityList = new List<int>();
        traitList = new List<int>();
        keywordList = new List<int>();
        costList = new List<int>();
        atkList = new List<int>();
        hpList = new List<int>();
        countdownList = new List<int>();
        isWithToken = false;
    }

    public static CardFilter Parse(string options, Func<string, string, string> transformFunc = null) {
        var filter = new CardFilter(-1);
        if (string.IsNullOrEmpty(options) || (options == "none"))
            return filter;

        transformFunc ??= ((x, y) => y);

        while (options.TryTrimParentheses(out string trimOptions)) {
            var split = trimOptions.Split(':');
            var type = split[0];
            var items = split[1].Split('|');
            for (int i = 0; i < items.Length; i++) {
                filter.SetParam(type, transformFunc.Invoke(type, items[i]));
            }
            options = options.TrimStart("[" + trimOptions + "]");
        }
        return filter;
    }

    public virtual void SetParam(string which, string param) {
        if (GetSetBoolType().Contains(which))
            SetBool(which, bool.Parse(param));
        else if (GetSetIntType().Contains(which))
            SetInt(which, int.Parse(param));
        else if (GetSelectIntType().Contains(which))
            SelectInt(which, int.Parse(param));
        else
            SetString(which, param);
    }

    public virtual void SetString(string which, string input) {
        switch (which) {
            default:
                return;
            case "name": 
                name = input;
                return;
            case "description":
                description = input;
                return;
        }
    }

    public virtual void SetInt(string which, int item) {
        switch (which) {
            default:
                return;
            case "format":
                format = item;
                return;
            case "zone":
                zone = item;
                return;
        }
    }

    public virtual void SetBool(string which, bool item) {
        switch (which) {
            default:
                return;
            case "token":
                isWithToken = item;
                return;
        }
    }

    public virtual void SelectInt(string which, int item) {
        var list = which switch {
            "uid"       => uidList,
            "id"        => idList,
            "craft"     => craftList,
            "pack"      => packList,
            "type"      => typeList,
            "rarity"    => rarityList,
            "trait"     => traitList,
            "keyword"   => keywordList,
            "cost"      => costList,
            "atk"       => atkList,
            "hp"        => hpList,
            "countdown" => countdownList, 
            _ => null,
        };

        if (item == -1) {
            list?.Clear();
            return;
        }

        if (Card.StatusNames.Contains(which) && (item < 0)) {
            if (list.Count <= 0)
                return;

            var below = Enumerable.Range(0, list.Max() + 1);
            var above = Enumerable.Range(list.Min(), 10 - list.Min() + 1);
            var result = item switch {
                -2 => below,
                -3 => above,
                _ => list,
            };
            list.Clear();
            list.AddRange(result);
            return;
        }

        list?.Fluctuate(item);
    }

    public bool Filter(Card card) {
        return FormatFilter(card) && ZoneFilter(card)
            && UIDFilter(card) && IDFilter(card) && NameFilter(card)
            && CostFilter(card) && AtkFilter(card) && HpFilter(card) && CountdownFilter(card)
            && CraftFilter(card) && PackFilter(card) && TypeFilter(card) 
            && RarityFilter(card) && TraitFilter(card) && KeywordFilter(card) 
            && DescriptionFilter(card) && TokenFilter(card);
    }


    public virtual bool FormatFilter(Card card) => (format == -1) || card.IsFormat((GameFormat)format);
    public virtual bool ZoneFilter(Card card) => (zone == -1) || (card.ZoneId == zone) || (card.PackId == 0);
    public virtual bool NameFilter(Card card) => string.IsNullOrEmpty(name) || card.name.Contains(name);
    public virtual bool UIDFilter(Card card) => List.IsNullOrEmpty(uidList) || uidList.Contains(card.id);
    public virtual bool IDFilter(Card card) => List.IsNullOrEmpty(idList) || idList.Contains(Card.GetBaseId(card.NameId)) || idList.Contains(Card.GetEvolveId(card.NameId));
    public virtual bool CraftFilter(Card card) => List.IsNullOrEmpty(craftList) || craftList.Contains(card.CraftId);
    public virtual bool PackFilter(Card card) => List.IsNullOrEmpty(packList) || packList.Contains(card.PackId);
    public virtual bool TypeFilter(Card card) => (card.Type != CardType.Leader) && (card.Type != CardType.Evolved) && (List.IsNullOrEmpty(typeList) || typeList.Contains(card.TypeId));
    public virtual bool RarityFilter(Card card) => List.IsNullOrEmpty(rarityList) || rarityList.Contains(card.RarityId);
    public virtual bool TraitFilter(Card card) => List.IsNullOrEmpty(traitList) || card.traits.Contains(CardTrait.All) || traitList.Select(x => (CardTrait)x).Intersect(card.traits).Any();
    public virtual bool KeywordFilter(Card card) {
        if (List.IsNullOrEmpty(keywordList)) 
            return true;

        var keywords = keywordList.Select(x => (CardKeyword)x).ToList();
        var allKeywords = card.keywords.Concat(card.EvolveCard.keywords)
                            .Concat(card.effects.SelectMany(effect => effect.keywords))
                            .Concat(card.EvolveCard.effects.SelectMany(effect => effect.keywords))
                            .Distinct();

        return keywords.Intersect(allKeywords).Any();
    }
    public virtual bool DescriptionFilter(Card card) => string.IsNullOrEmpty(description) || card.description.Contains(description) || card.EvolveCard.description.Contains(description);
    public virtual bool TokenFilter(Card card) => (card.Group == CardGroup.Normal) || (isWithToken && (card.Group == CardGroup.Token));

    public virtual bool CostFilter(Card card) => List.IsNullOrEmpty(costList) || costList.Contains(Mathf.Min(card.cost, 10));
    public virtual bool AtkFilter(Card card) => List.IsNullOrEmpty(atkList) || atkList.Contains(Mathf.Min(card.atk, 10));
    public virtual bool HpFilter(Card card) => List.IsNullOrEmpty(hpList) || hpList.Contains(Mathf.Min(card.hp, 10));
    public virtual bool CountdownFilter(Card card) => List.IsNullOrEmpty(countdownList) || countdownList.Contains(Mathf.Min(card.countdown, 10));
}

public class BattleCardFilter{

    public List<KeyValuePair<string, List<string>>> filterItems;

    public static BattleCardFilter Parse(string options, Func<string, string, string> transformFunc = null)
    {
        var filter = new BattleCardFilter();
        if (string.IsNullOrEmpty(options) || (options == "none"))
            return filter;

        transformFunc ??= ((x, y) => y);

        while (options.TryTrimParentheses(out string trimOptions))
        {
            var split = trimOptions.Split(':');
            var type = split[0];
            var items = split[1].Split('|');
            var itemList = new List<string>();
            for (int i = 0; i < items.Length; i++)
            {
                foreach (var cond in Operator.sortedCondDict)
                {
                    if (items[i].TryTrimStart(cond, out items[i]))
                    {
                        itemList.Add(string.Concat(cond, transformFunc.Invoke(type, items[i])));
                        break;
                    }
                }
            }
            filter.filterItems.Add(new KeyValuePair<string, List<string>>(type, itemList));
            options = options.TrimStart("[" + trimOptions + "]");
        }
        return filter;
    }

    public virtual Func<BattleCard, string, bool> GetFilter(string type)
    {
        Func<BattleCard, string, bool> func = type switch
        {
            "isAttacked" => AttackedFilter,
            _ => (battlecard, item) => GetCardFilter(type)(battlecard.CurrentCard,item),
        };
        return func;
    }

    public virtual Func<Card, string, bool> GetCardFilter(string type)
    {
        Func<Card, string, bool> func = type switch
        {
            "format" => FormatFilter,
            "zone" => ZoneFilter,
            "name" => NameFilter,
            "description" => DescriptionFilter,
            "uid" => UIDFilter,
            "id" => IDFilter,
            "group" => GroupFilter,
            "craft" => CraftFilter,
            "pack" => PackFilter,
            "type" => TypeFilter,
            "rarity" => RarityFilter,
            "trait" => TraitFilter,
            "keyword" => KeywordFilter,
            "fieldkeyword" => KeywordFilter,
            "cost" => CostFilter,
            "atk" => AtkFilter,
            "hp" => HpFilter,
            "countdown" => CountdownFilter,
            "initcost" => InitCostFilter,
            "initatk" => InitAtkFilter,
            "inithp" => InitHpFilter,
            _ =>  (card, item) => DefaultFilter(),
        };
        return func;
    }

    public bool FilterWithCurrentCard(BattleCard battleCard) {
        if (DefaultFilter(battleCard))
        {
            foreach (var filterItem in filterItems)
            {
                var filterResult = false;
                var filter = GetFilter(filterItem.Key);
                foreach (var item in filterItem.Value)
                {
                    if (filter(battleCard, item))
                    {
                        filterResult = true;
                        break;
                    }
                }
                if (!filterResult) return false;
            }
            return true;
        }
        return false;
    }

    public bool Filter(Card card){
        if (DefaultFilter(card))
        {
            foreach (var filterItem in filterItems)
            {
                var filterResult = false;
                var filter = GetCardFilter(filterItem.Key);
                foreach (var item in filterItem.Value)
                {
                    if (filter(card, item))
                    {
                        filterResult = true;
                        break;
                    }
                }
                if (!filterResult) return false;
            }
            return true;
        }
        return false;
    }

    public virtual bool DefaultFilter() => true;

    public virtual bool DefaultFilter(Card card) => true;
    public virtual bool DefaultFilter(BattleCard battleCard) => true;

    public virtual bool GeneralFilter(int value, string item)
    {
        string op = "=";

        foreach (var condOp in Operator.sortedCondDict)
        {
            if (item.TryTrimStart(condOp, out item))
            {
                op = condOp;
                break;
            }
        }
        if (!int.TryParse(item, out int number))
        {
            return true;
        }

        return Operator.Condition(op, value, number);
    }

    //TODO 考虑增加Effect相关筛选？
    public virtual bool FormatFilter(Card card, string item)
    {
        if (int.TryParse(item, out var format))
            return card.IsFormat((GameFormat)format);
        else
            return true;
    }
    public virtual bool ZoneFilter(Card card, string item)
    {
        if (int.TryParse(item, out var zone))
            return (card.ZoneId == zone) || (card.PackId == 0);
        else
            return true;
    }
    public virtual bool NameFilter(Card card, string item) => string.IsNullOrEmpty(item) || card.name.Contains(item);
    public virtual bool UIDFilter(Card card, string item)
    {
        if (int.TryParse(item, out var uid))
            return card.id == uid;
        else
            return true;
    }
    public virtual bool IDFilter(Card card, string item)
    {
        return GeneralFilter(Card.GetBaseId(card.id), item) || GeneralFilter(Card.GetEvolveId(card.id), item) || GeneralFilter(Card.GetBaseId(card.NameId), item) || GeneralFilter(Card.GetEvolveId(card.NameId), item);
    }
    public virtual bool GroupFilter(Card card, string item) => GeneralFilter(card.GroupId, item);
    public virtual bool CraftFilter(Card card, string item) => GeneralFilter(card.CraftId, item);
    public virtual bool PackFilter(Card card, string item) => GeneralFilter(card.PackId, item);
    public virtual bool TypeFilter(Card card, string item) => GeneralFilter(card.TypeId, item);
    public virtual bool RarityFilter(Card card, string item) => GeneralFilter(card.RarityId, item);
    public virtual bool TraitFilter(Card card, string item)
    {
        var allTraits = card.BaseCard.traits.Concat(card.EvolveCard.traits).Distinct();

        if (item.TryTrimStart("!", out item))
        {
            if (int.TryParse(item, out var number))
                return !allTraits.Contains(CardTrait.All) && allTraits.All(trait => !GeneralFilter((int)trait, item));
            else
                return true;
        }
        else
        {
            if (int.TryParse(item, out var number))
                return allTraits.Contains(CardTrait.All) || allTraits.Any(trait => GeneralFilter((int)trait, item));
            else
                return true;
        }
    }
    public virtual bool KeywordFilter(Card card, string item)
    {
        var allKeywords = card.keywords.Concat(card.effects.SelectMany(effect => effect.keywords)).Distinct();

        if (item.TryTrimStart("!", out item))
        {
            if (int.TryParse(item, out var number))
                return allKeywords.All(keyword => !GeneralFilter((int)keyword, item));
            else
                return true;
        }
        else
        {
            if (int.TryParse(item, out var number))
                return allKeywords.Any(keyword => GeneralFilter((int)keyword, item));
            else
                return true;
        }
    }
    public virtual bool FieldKeywordFilter(Card card, string item)
    {
        var allKeywords = card.BaseCard.keywords.Concat(card.EvolveCard.keywords)
                            .Concat(card.BaseCard.effects.SelectMany(effect => effect.keywords))
                            .Concat(card.EvolveCard.effects.SelectMany(effect => effect.keywords))
                            .Distinct();

        if (item.TryTrimStart("!", out item))
        {
            if (int.TryParse(item, out var number))
                return allKeywords.All(keyword => !GeneralFilter((int)keyword, item));
            else
                return true;
        }
        else
        {
            if (int.TryParse(item, out var number))
                return allKeywords.Any(keyword => GeneralFilter((int)keyword, item));
            else
                return true;
        }
    }
    public virtual bool DescriptionFilter(Card card, string item) => card.BaseCard.description.Contains(item) || card.EvolveCard.description.Contains(item);
    public virtual bool CostFilter(Card card, string item) => GeneralFilter(card.cost, item);
    public virtual bool AtkFilter(Card card, string item) => GeneralFilter(card.atk, item);
    public virtual bool HpFilter(Card card, string item) => GeneralFilter(card.hp, item);
    public virtual bool InitCostFilter(Card card, string item) => GeneralFilter(card.BaseCard.cost, item);
    public virtual bool InitAtkFilter(Card card, string item) => GeneralFilter(card.BaseCard.atk, item);
    public virtual bool InitHpFilter(Card card, string item) => GeneralFilter(card.BaseCard.hp, item);
    public virtual bool CountdownFilter(Card card, string item) => GeneralFilter(card.countdown, item);

    public bool AttackedFilter(BattleCard card, string item)
    {
        if(int.TryParse(item, out var isAttackFinished))
            return isAttackFinished == 1? card.actionController.IsAttackFinished : !card.actionController.IsAttackFinished;
        else
            return true;
    }
}

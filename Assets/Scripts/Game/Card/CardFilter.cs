using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardFilter
{
    public virtual string[] GetSetStringType() => new string[] { "name", "description", "trait", "keyword" };
    public virtual string[] GetSetIntType() => new string[] { "format", "zone" };
    public virtual string[] GetSelectIntType() => new string[] { 
        "uid", "id", "craft", "pack", "type", "rarity",
        "cost", "atk", "hp", "countdown" };
    public virtual string[] GetSetBoolType() => new string[] { "token" };

    public int format, zone;
    public string name, traitName, keywordName, description;
    public List<int> uidList, idList, craftList, packList, typeList, rarityList, 
        costList, atkList, hpList, countdownList;
    public bool isWithToken;

    /// <summary>
    /// Create a filter to search card from database.
    /// </summary>
    /// <param name="formatId">Format id. Set -1 if don't filter format</param>
    public CardFilter(int formatId) {
        format = formatId;
        zone = -1;
        name = traitName = keywordName = description = string.Empty;
        uidList = new List<int>();
        idList = new List<int>();
        craftList = new List<int>();
        packList = new List<int>();
        typeList = new List<int>();
        rarityList = new List<int>();
        costList = new List<int>();
        atkList = new List<int>();
        hpList = new List<int>();
        countdownList = new List<int>();
        isWithToken = false;
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
            case "trait":
                traitName = input;
                return;
            case "keyword":
                keywordName = input;
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
    public virtual bool TraitFilter(Card card) => string.IsNullOrEmpty(traitName) || card.traits.Contains(CardTrait.All) || card.traits.Select(x => x.GetTraitName()).Contains(traitName);
    public virtual bool KeywordFilter(Card card) => string.IsNullOrEmpty(keywordName) || card.keywords.Select(x => x.GetKeywordName()).Contains(keywordName);
    public virtual bool DescriptionFilter(Card card) => string.IsNullOrEmpty(description) || card.description.Contains(description);
    public virtual bool TokenFilter(Card card) => (card.Group == CardGroup.Normal) || (isWithToken && (card.Group == CardGroup.Token));

    public virtual bool CostFilter(Card card) => List.IsNullOrEmpty(costList) || costList.Contains(Mathf.Min(card.cost, 10));
    public virtual bool AtkFilter(Card card) => List.IsNullOrEmpty(atkList) || atkList.Contains(Mathf.Min(card.atk, 10));
    public virtual bool HpFilter(Card card) => List.IsNullOrEmpty(hpList) || hpList.Contains(Mathf.Min(card.hp, 10));
    public virtual bool CountdownFilter(Card card) => List.IsNullOrEmpty(countdownList) || countdownList.Contains(Mathf.Min(card.countdown, 10));
}

public class BattleCardFilter : CardFilter {
    public override string[] GetSetStringType() => new string[] { "name", "description" };
    public override string[] GetSelectIntType() => base.GetSelectIntType().Concat(new string[] { "trait", "keyword" }).ToArray();

    public List<int> traitList, keywordList;
    public BattleCardFilter(int formatId) : base(formatId) {
        traitList = new List<int>();
        keywordList = new List<int>();

        isWithToken = true;
    }

    public static BattleCardFilter Parse(string options) {
        var filter = new BattleCardFilter(-1);
        if (string.IsNullOrEmpty(options) || (options == "none"))
            return filter;

        while (options.TryTrimParentheses(out string trimOptions)) {
            var split = trimOptions.Split(':');
            var type = split[0];
            var items = split[1].Split('|');

            for (int i = 0; i < items.Length; i++) {
                filter.SetParam(type, items[i]);
            }

            options = options.TrimStart("[" + trimOptions + "]");
        }
        return filter;
    }

    public override void SelectInt(string which, int item)
    {
        var list = which switch {
            "trait"     => traitList,
            "keyword"   => keywordList,
            _           => null,
        };

        if (list == null) {
            base.SelectInt(which, item);
            return;
        }

        if (item == -1) {
            list?.Clear();
            return;
        }

        if (Card.StatusNames.Contains(which.ToLower().TrimStart("init")) && (item < 0)) {
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

    public bool FilterWithCurrentCard(BattleCard battleCard) {
        var card = battleCard.CurrentCard;
        return base.Filter(card);
    }

    public override bool TypeFilter(Card card) => List.IsNullOrEmpty(typeList) || typeList.Contains(card.TypeId);
    public override bool TraitFilter(Card card) => List.IsNullOrEmpty(traitList) || card.traits.Contains(CardTrait.All) || traitList.Select(x => (CardTrait)x).Intersect(card.traits).Any();
    public override bool KeywordFilter(Card card) => List.IsNullOrEmpty(keywordList) || keywordList.Select(x => (CardKeyword)x).Intersect(card.keywords).Any();
    

}

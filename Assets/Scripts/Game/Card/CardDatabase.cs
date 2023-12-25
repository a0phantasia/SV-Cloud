using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CardDatabase
{
    public static List<Card> CardMaster => DatabaseManager.instance.cardMaster;
    public static Func<Card, long> Sorter => (card) => {
        return card.cost * (long)1e10 + card.id;
    };

    public static Dictionary<CardPack, string> packNameDict = new Dictionary<CardPack, string>() {
        { CardPack.Basic, "基本卡包" },
    };

    public static Dictionary<CardZone, string> zoneNameDict = new Dictionary<CardZone, string>() {
        { CardZone.Cygames, "官方卡包" },   { CardZone.Engineering, "工程區" },
    };

    public static Dictionary<GameFormat, string> formatNameDict = new Dictionary<GameFormat, string>() {
        { GameFormat.Unlimited, "無限制" },  { GameFormat.Rotation, "指定系列" },
        { GameFormat.TwoPick, "2Pick" },    { GameFormat.AllStarTwoPick, "AS 2Pick"},
    };

    public static Dictionary<BestOf, string> bestOfNameDict = new Dictionary<BestOf, string>() {
        { BestOf.BO1, "BO1" },  { BestOf.BO3_BAN1, "BO3(BAN 1)" },
        { BestOf.BO3, "BO3" },  { BestOf.BO5_BAN1, "BO5(BAN 1)" },
        { BestOf.BO5, "BO5" },  { BestOf.BO8, "八職制霸" },
    };

    public static Dictionary<CardCraft, string> craftNameDict = new Dictionary<CardCraft, string>() {
        { CardCraft.Neutral,"中立" },  
        { CardCraft.Elf,    "精靈" },       { CardCraft.Royal,   "皇家護衛" },    
        { CardCraft.Witch,  "巫師" },       { CardCraft.Dragon,  "龍族" },
        { CardCraft.Necro,  "死靈法師" },   { CardCraft.Vampire,  "吸血鬼" },
        { CardCraft.Bishop, "主教" },       { CardCraft.Nemesis,  "復仇者" },
    };

    public static Dictionary<CardType, string> typeNameDict = new Dictionary<CardType, string>() {
        { CardType.Leader,      "主戰者" },
        { CardType.Follower,    "從者"   },
        { CardType.Spell,       "法術"   },
        { CardType.Amulet,      "護符"   },
        { CardType.Territory,   "領域"   },
        { CardType.Evolved,     "進化後" },
    };

    public static Dictionary<CardRarity, string> rarityNameDict = new Dictionary<CardRarity, string>() {
        { CardRarity.Leader,    "主戰者" },
        { CardRarity.Bronze,    "青銅"   },
        { CardRarity.Silver,    "白銀"   },
        { CardRarity.Gold,      "黃金"   },
        { CardRarity.Legend,    "傳說"   },
    };

    public static Dictionary<CardTrait, string> traitNameDict = new Dictionary<CardTrait, string>() {
        { CardTrait.All,        "全部"      },
        { CardTrait.Soldier,    "士兵"      },
        { CardTrait.Commander,  "指揮官"    },
    };

    public static Dictionary<CardKeyword, string> keywordNameDict = new Dictionary<CardKeyword, string>() {
        { CardKeyword.None,     "-"      },
        { CardKeyword.Storm,    "疾馳"   },
        { CardKeyword.Ward,     "守護"   },
    };

    public static string GetPackName(this CardPack pack) => packNameDict.Get(pack, "-");
    public static string GetZoneName(this CardZone zone) => zoneNameDict.Get(zone, "自訂");
    public static string GetFormatName(this GameFormat format) => formatNameDict.Get(format, "無限制");
    public static string GetBestOfName(this BestOf bestOf) => bestOfNameDict.Get(bestOf, "無限制");
    public static string GetCraftName(this CardCraft craft) => craftNameDict.Get(craft, "中立");
    public static string GetTypeName(this CardType type) => typeNameDict.Get(type, "主戰者");
    public static string GetRarityName(this CardRarity rarity) => rarityNameDict.Get(rarity, "傳說");
    public static string GetTraitName(this CardTrait trait) => traitNameDict.Get(trait, "-");
    public static string GetKeywordName(this CardKeyword keyword) => keywordNameDict.Get(keyword, "-");

    public static int GetMaxCardCountInDeck(this GameFormat format) => format switch {
        GameFormat.TwoPick => 30,
        GameFormat.AllStarTwoPick => 30,
        _ => 40,
    };

    public static string GetGroupNote(this CardGroup group) {
        return group != CardGroup.Normal ? "※這張卡片為特殊卡" : string.Empty;
    }

    public static string GetTraitName(this List<CardTrait> traits) {
        if ((traits == null) || (traits.Count == 0))
            return "-";

        string name = GetTraitName(traits[0]);        
        for (int i = 1; i < traits.Count; i++) {
            name += "‧" + GetTraitName(traits[i]);
        }
        return name;
    }
}

public struct CardFilter
{
    public int format, zone;
    public string name, trait, keyword, description, author;
    public List<int> craftList, packList, typeList, rarityList, costList, atkList, hpList;
    public bool isWithToken;

    /// <summary>
    /// Create a filter to search card from database.
    /// </summary>
    /// <param name="formatId">Format id. Set -1 if don't filter format</param>
    public CardFilter(int formatId) {
        format = formatId;
        zone = 1;
        name = trait = keyword = description = author = string.Empty;
        craftList = new List<int>();
        packList = new List<int>();
        typeList = new List<int>();
        rarityList = new List<int>();
        costList = new List<int>();
        atkList = new List<int>();
        hpList = new List<int>();
        isWithToken = false;
    }

    public void SetString(string which, string input) {
        switch (which) {
            default:
                return;
            case "name": 
                name = input;
                return;
            case "trait":
                trait = input;
                return;
            case "keyword":
                keyword = input;
                return;
            case "description":
                description = input;
                return;
            case "author":
                author = input;
                return;
        }
    }

    public void SetInt(string which, int item) {
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

    public void SetBool(string which, bool item) {
        switch (which) {
            default:
                return;
            case "token":
                isWithToken = item;
                return;
        }
    }

    public void SelectInt(string which, int item) {
        var list = which switch {
            "craft" => craftList,
            "pack"  => packList,
            "type"  => typeList,
            "rarity"=> rarityList,
            "cost"  => costList,
            "atk"   => atkList,
            "hp"    => hpList, 
            _ => null,
        };

        if (item == -1) {
            list?.Clear();
            return;
        }

        if (Card.StatusName.Contains(which) && (item < 0)) {
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
        return FormatFilter(card) && ZoneFilter(card) && NameFilter(card)
            && CostFilter(card) && AtkFilter(card) && HpFilter(card)
            && CraftFilter(card) && PackFilter(card) && TypeFilter(card) 
            && RarityFilter(card) && TraitFilter(card) && KeywordFilter(card) 
            && DescriptionFilter(card) && AuthorFilter(card) && TokenFilter(card);
    }

    public bool FormatFilter(Card card) => (format == -1) || card.IsFormat((GameFormat)format);
    public bool ZoneFilter(Card card) => (card.ZoneId == zone) || (card.PackId == 0);
    public bool NameFilter(Card card) => string.IsNullOrEmpty(name) || card.name.Contains(name);
    public bool CraftFilter(Card card) => craftList.IsNullOrEmpty() || craftList.Contains(card.CraftId);
    public bool PackFilter(Card card) => packList.IsNullOrEmpty() || packList.Contains(card.PackId);
    public bool TypeFilter(Card card) => (card.Type != CardType.Leader) && (card.Type != CardType.Evolved) && (typeList.IsNullOrEmpty() || typeList.Contains(card.TypeId));
    public bool RarityFilter(Card card) => rarityList.IsNullOrEmpty() || rarityList.Contains(card.RarityId);
    public bool TraitFilter(Card card) => string.IsNullOrEmpty(trait) || card.traits.Select(x => x.GetTraitName()).Contains(trait);
    public bool KeywordFilter(Card card) => string.IsNullOrEmpty(keyword) || card.keywords.Select(x => x.GetKeywordName()).Contains(keyword);
    public bool DescriptionFilter(Card card) => string.IsNullOrEmpty(description) || card.description.Contains(description);
    public bool AuthorFilter(Card card) => string.IsNullOrEmpty(author) || (card.author == author);
    public bool TokenFilter(Card card) => (card.Group == CardGroup.Normal) || (isWithToken && (card.Group == CardGroup.Token));

    public bool CostFilter(Card card) => costList.IsNullOrEmpty() || costList.Contains(Mathf.Min(card.cost, 10));
    public bool AtkFilter(Card card) => atkList.IsNullOrEmpty() || atkList.Contains(Mathf.Min(card.atk, 10));
    public bool HpFilter(Card card) => hpList.IsNullOrEmpty() || hpList.Contains(Mathf.Min(card.hp, 10));

}


public enum CardGroup 
{
    Normal = 1, Balanced = 2, Territorize = 6, Accelerate = 7, Crystal = 8, Token = 9
}

public enum GameFormat 
{
    Unlimited = 0,      Rotation = 1, 
    AllStarTwoPick = 8, TwoPick = 9,
}

public enum CardZone 
{
    Cygames = 0,   Engineering = 1,
    Beyond = 8,    Custom = 9
}

public enum BestOf
{
    BO1 = 1,    BO3_BAN1 = 2,   BO3 = 3,
    BO5_BAN1 = 4, BO5 = 5, BO8 = 8,
}

public enum CardPack
{
    Basic = 0,
}

public enum CardCraft 
{
    Neutral = 0,  
    Elf = 1,    Royal = 2,      Witch = 3,  Dragon = 4, 
    Necro = 5,  Vampire = 6,    Bishop = 7, Nemesis = 8,
}

public enum CardType 
{
    Leader = 0, Follower = 1, Spell = 2, Amulet = 3, Territory = 4, 
    Evolved = 9
}

public enum CardRarity
{
    Leader = 0, Bronze = 1, Silver = 2, Gold = 3, Legend = 4,
}

public enum CardTrait 
{
    All = 0,
    Soldier = 1, Commander = 2,
}

public enum CardKeyword 
{
    None = 0,   Storm = 1,  Ward = 2,
}

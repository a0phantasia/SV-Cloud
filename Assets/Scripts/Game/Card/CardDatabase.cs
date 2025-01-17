using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CardDatabase
{
    public static List<Card> CardMaster => DatabaseManager.instance.cardMaster;
    public static Func<Card, long> Sorter => (card) => {
        return card.cost * (long)1e12 + card.CraftId * (long)1e11 
            + card.TypeId * (long)1e10 + card.id;
    };

    public static Dictionary<CardPack, string> packNameDict = new Dictionary<CardPack, string>() {
        { CardPack.Basic, "基本卡包" },
        { CardPack.EngineeringBasic, "基本卡包" },
    };

    public static Dictionary<CardZone, string> zoneNameDict = new Dictionary<CardZone, string>() {
        { CardZone.Cygames, "官方卡包" },   { CardZone.Engineering, "工程區" },
        { CardZone.Art, "藝術園區" },
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

    public static Dictionary<CardType, string> typeEnglishNameDict = new Dictionary<CardType, string>() {
        { CardType.Leader,      "leader"    },
        { CardType.Follower,    "follower"  },
        { CardType.Spell,       "spell"     },
        { CardType.Amulet,      "amulet"    },
        { CardType.Territory,   "territory" },
        { CardType.Evolved,     "evolved"   },
    };

    public static Dictionary<CardRarity, string> rarityNameDict = new Dictionary<CardRarity, string>() {
        { CardRarity.Leader,    "主戰者" },
        { CardRarity.Bronze,    "青銅"   },
        { CardRarity.Silver,    "白銀"   },
        { CardRarity.Gold,      "黃金"   },
        { CardRarity.Legend,    "傳說"   },
    };

    public static Dictionary<BattlePlaceId, string> placeNameDict = new Dictionary<BattlePlaceId, string>() {
        { BattlePlaceId.Deck,         "deck"      },
        { BattlePlaceId.Hand,         "hand"      },
        { BattlePlaceId.Leader,       "leader"    },
        { BattlePlaceId.Territory,    "territory" },
        { BattlePlaceId.Field,        "field"     },
        { BattlePlaceId.Grave,        "grave"     },
    };

    public static string[] PropertyEffects => new string[] { 
        "leaveVanish", "destroyVanish", "returnVanish" 
    };
    public static CardKeyword[] KeywordEffects => new CardKeyword[] { 
        CardKeyword.Storm, CardKeyword.Ward, CardKeyword.Bane,
        CardKeyword.Rush, CardKeyword.Ambush, CardKeyword.Drain,
        CardKeyword.Pressure, CardKeyword.Aura,
    };

    public static string GetPackName(this CardPack pack) => packNameDict.Get(pack, "-");
    public static string GetZoneName(this CardZone zone) => zoneNameDict.Get(zone, "自訂");
    public static string GetFormatName(this GameFormat format) => formatNameDict.Get(format, "無限制");
    public static string GetBestOfName(this BestOf bestOf) => bestOfNameDict.Get(bestOf, "無限制");
    public static string GetCraftName(this CardCraft craft) => craftNameDict.Get(craft, "中立");
    public static string GetTypeName(this CardType type) => typeNameDict.Get(type, "主戰者");
    public static string GetTypeEnglishName(this CardType type) => typeEnglishNameDict.Get(type, "主戰者");
    public static string GetRarityName(this CardRarity rarity) => rarityNameDict.Get(rarity, "傳說");
    public static string GetTraitName(this CardTrait trait) => DatabaseManager.instance.GetTraitName(trait);
    public static string GetKeywordName(this CardKeyword keyword) => DatabaseManager.instance.GetKeywordName(keyword);
    public static string GetKeywordEnglishName(this CardKeyword keyword) => DatabaseManager.instance.GetKeywordEnglishName(keyword);
    public static string GetKeywordInfo(this CardKeyword keyword) => DatabaseManager.instance.GetKeywordInfo(keyword);

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

    public static CardType ToCardTypeWithEnglish(this string type) {
        return typeEnglishNameDict.FirstOrDefault(x => x.Value == type).Key;
    }

    public static BattlePlaceId ToBattlePlace(this string place) {
        if (!placeNameDict.Values.Contains(place))
            return BattlePlaceId.None;

        return placeNameDict.FirstOrDefault(x => x.Value == place).Key;
    }
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
    Cygames = 0,   Engineering = 1, Art = 2,     
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
    EngineeringBasic = 100,
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
    Soldier = 1, Commander = 2, Earth = 3, Artifact = 4, Golem = 5,
}

public enum CardKeyword 
{
    None = 0, Storm = 1, Ward = 2, Bane = 3, Rush = 4, Ambush = 5, Drain = 6,
    Fanfare = 7, Lastword = 8, Attack = 9, Defense = 10, Evolve = 11, 
    Combo = 12, Rally = 13, SpellBoost = 14, Awake = 15, Necromance = 16,
    Venge = 17, Countdown = 18, Reson = 19, EarthRitual = 20, Enhance = 21,
    Pressure = 22, Bury = 23, Reanimate = 24, Aura = 25,
}

public enum BattlePlaceId 
{
    None = 0,   Deck = 1,   Hand = 2,   Leader = 3, 
    Territory = 4,  Field = 5,  Grave = 6,
}

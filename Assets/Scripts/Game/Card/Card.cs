using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Card : IIdentifyHandler
{
    public const int DATA_COL = 8;
    public const int DESC_COL = 2;
    public static string[] StatusName => new string[] { "cost", "atk", "hp" };
    public static Card Get(int id) => DatabaseManager.instance.GetCardInfo(id);
    public static GameObject Prefab => ResourceManager.instance.GetPrefab("Card");

    public int id;
    public int Id => id;
    public int EvolveId => (TypeId == (int)CardType.Evolved) ? 0 : (id - id % 1000 + 900 + id % 100);
    public int GroupId => id / (int)1e8;
    public int ZoneId => id % (int)1e8 / (int)1e7;
    public int PackId => id % (int)1e8 / (int)1e4;
    public int CraftId => id % (int)1e4 / 1000;
    public int TypeId => id % 1000 / 100;
    public int EvolveTypeId => (int)EvolveType;
    public int RarityId => id % 100 / 10;
    public int SerialNum => id % 10;

    public int NameId { get; protected set; }
    public int ArtworkId { get; protected set; }
    public int CountLimit { get; protected set; }
    public int Countdown { get; protected set; }

    public CardGroup Group => (CardGroup)GroupId;
    public CardZone Zone => (CardZone)ZoneId;
    public CardPack Pack => (CardPack)PackId;
    public CardCraft Craft => (CardCraft)CraftId;
    public CardType Type => (CardType)TypeId;
    public CardType EvolveType => Type == CardType.Evolved ? CardType.Follower : CardType.Evolved;
    public CardRarity Rarity => (CardRarity)RarityId;

    public string name;
    public int cost, atk, hp;
    public List<CardTrait> traits = new List<CardTrait>();
    public List<CardKeyword> keywords = new List<CardKeyword>();
    public List<int> tokenIds = new List<int>();
    public List<int> effectIds = new List<int>();
    public List<Effect> effects = new List<Effect>();
    public Dictionary<string, string> options = new Dictionary<string, string>();
    public string description;
    public string author;

    public Task<Texture2D> Artwork => Addressables.LoadAssetAsync<Texture2D>("Artwork/" + ArtworkId).Task;
    public Card EvolveCard => DatabaseManager.instance.GetCardInfo(EvolveId);


    public static Card GetLeaderCard(int craft) => Card.Get(100000000 + craft * 1000);

    public Card(string[] _data, int startIndex) {
        string[] _slicedData = new string[DATA_COL];
        Array.Copy(_data, startIndex, _slicedData, 0, _slicedData.Length);

        id = int.Parse(_slicedData[0]);
        var status = _slicedData[1].ToIntList('/');
        cost = status[0];
        atk = status[1];
        hp = status[2];
        traits = _slicedData[2].ToIntList('/').Select(x => (CardTrait)x).ToList();
        keywords = _slicedData[3].ToIntList('/').Select(x => (CardKeyword)x).ToList();

        options.ParseOptions(_slicedData[4]);
        InitExtraPorperty();

        effectIds = _slicedData[5].ToIntList('/');
        tokenIds = _slicedData[6].ToIntList('/');
        author = _slicedData[7].TrimEnd('\n');
    }

    private void InitExtraPorperty() {
        int ArtId = int.Parse(options.Get("artId", id.ToString()));
        ArtworkId = (Type == CardType.Evolved) ? (ArtId - ArtId % 1000 + 100 + ArtId % 100) : ArtId;

        NameId = int.Parse(options.Get("nameId", id.ToString()));
        CountLimit = int.Parse(options.Get("limit", "3"));
        Countdown = int.Parse(options.Get("countdown", "-1"));
    }

    public Card(Card rhs) {
        id = rhs.id;
        name = rhs.name;
        cost = rhs.cost;
        atk = rhs.atk;
        hp = rhs.hp;
        traits = rhs.traits.Select(x => x).ToList();
        keywords = rhs.keywords.Select(x => x).ToList();
        effectIds = new List<int>(rhs.effectIds);
        tokenIds = new List<int>(rhs.tokenIds);
        options = new Dictionary<string, string>(rhs.options);

        description = rhs.description;
        author = rhs.author;

        ArtworkId = rhs.ArtworkId;
        NameId = rhs.NameId;
        CountLimit = rhs.CountLimit;
        Countdown = rhs.Countdown;
        
        SetEffects(rhs.effectIds);
    }

    public void SetEffects(List<int> effectIds, Func<int, Effect> effectFunc = null) {
        effectFunc ??= (x => Effect.Get(x));
        effects = effectIds.Select(effectFunc).ToList();
        foreach (var e in effects) {
            if (e == null)
                continue;

            e.source = this;
        }
    }

    public void SetDescription(string[] _data) {
        name = _data[0];
        description = _data[1].GetDescription((Type == CardType.Leader) ? string.Empty : "（沒有卡片能力記敘）");
    }

    public float GetIdentifier(string id) 
    {
        return id switch {
            "id" => this.id,
            "name" => NameId,
            "group" => GroupId,
            "zone" => ZoneId,
            "pack" => PackId,
            "craft" => CraftId,
            "type" => TypeId,
            "rarity" => RarityId,
            "serial" => SerialNum,
            _ => float.Parse(options.Get(id, float.MinValue.ToString())),
        };
    }

    public bool TryGetIdenfier(string id, out float value)
    {
        value = GetIdentifier(id);
        return value == float.MinValue;
    }

    public void SetIdentifier(string id, float value)
    {
        
    }

    public bool IsFollower() => (Type == CardType.Follower) || (Type == CardType.Evolved);
    public bool IsFormat(GameFormat format) {
        int newPackId = GameManager.versionData.NewPackIds[ZoneId];
        return format switch {
            GameFormat.Rotation => (PackId == 0) || (PackId == ZoneId * 1000) ||
                (PackId % 1000).IsWithin(newPackId - 4, newPackId),
            _ => true,
        };
    }
}
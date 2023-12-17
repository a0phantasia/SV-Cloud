using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Deck
{
    public string name;
    public int zone, format, craft;
    
    [XmlArray("cards"), XmlArrayItem(typeof(int), ElementName = "id")] 
    public List<int> cardIds;

    [XmlIgnore] public int CardCount => cardIds.Count;
    [XmlIgnore] public int MaxCardCount => ((GameFormat)format).GetMaxCardCountInDeck();
    [XmlIgnore] public List<Card> Cards => cardIds.Select(x => Card.Get(x)).ToList();
    [XmlIgnore] public List<Card> DistinctCards => cardIds.Distinct().Select(x => Card.Get(x)).ToList();
    [XmlIgnore] public Dictionary<int, int> CardIdDistribution => GetCardIdDistribution();
    [XmlIgnore] public List<int> CostDistribution => GetCostDistribution();

    public Deck() {}

    public Deck(CardZone zoneId, GameFormat formatId, CardCraft craftId) {
        zone = (int)zoneId;
        format = (int)formatId;
        craft = (int)craftId;
        cardIds = new List<int>();
    }

    public Deck(Deck rhs) {
        name = rhs.name;
        zone = rhs.zone;
        format = rhs.format;
        craft = rhs.craft;
        cardIds = new List<int>(rhs.cardIds);
    }
    public override string ToString()
    {
        return "【Name】  " + name + " 【Zone】 " + (CardZone)zone + 
            " 【Format】 " + (GameFormat)format + " 【Craft】 " + (CardCraft)craft;
    }

    public bool IsDefault() {
        return string.IsNullOrEmpty(name);
    }

    public bool IsEmpty() {
        return cardIds.IsNullOrEmpty();
    }

    public bool IsBattleAvailable(CardZone battleZone, GameFormat gameFormat) {
        return (!IsDefault()) && (!IsEmpty()) && (CardCount == MaxCardCount)
            && (battleZone == (CardZone)zone) && (gameFormat == (GameFormat)format);
    }

    public Dictionary<int, int> GetCardIdDistribution() {
        Dictionary<int, int> cdf = new Dictionary<int, int>();
        for (int i = 0; i < DistinctCards.Count; i++) {
            Card card = DistinctCards[i];
            cdf.Add(card.id, cardIds.Count(id => card.id == id));
        }
        return cdf;
    }

    public List<int> GetCostDistribution() {
        var cdf = new List<int>() { Cards.Count(x => x.cost <= 1) };
        for (int i = 2; i <= 7; i++) {
            cdf.Add(Cards.Count(x => x.cost == i));
        }
        cdf.Add(Cards.Count(x => x.cost >= 8));
        return cdf;
    }

    public int GetTypeCount(CardType type) {
        return Cards.Count(x => x.Type == type);
    }

    public void Sort() {
        cardIds = Cards.OrderBy(CardDatabase.Sorter).Select(x => x.id).ToList();
    }

    public void AddCard(Card card) {
        cardIds.Add(card.id);
        Sort();
    }

    public void RemoveCard(Card card) {
        cardIds.Remove(card.id);
        Sort();
    }

}
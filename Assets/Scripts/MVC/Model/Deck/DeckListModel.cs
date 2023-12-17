using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckListModel : SelectModel<Deck>
{
    public DeckListMode mode = DeckListMode.Normal;
    public DeckListMode OriginalMode { get; private set; } = DeckListMode.Topic;
    public CardZone Zone { get; private set; } = CardZone.Engineering;
    public GameFormat Format { get; private set; } = GameFormat.Rotation;
    public List<Deck> DefaultDeckList => GetDefaultDeckList();

    public List<Deck> GetDefaultDeckList() {
        var defaultDeckList = new List<Deck>() { new Deck() };
        return (mode switch {
            DeckListMode.Normal => Player.gameData.decks.Concat(defaultDeckList),
            DeckListMode.Topic  => GameManager.versionData.topicDecks,
            DeckListMode.Battle => Player.gameData.decks.Where(x => x.IsBattleAvailable(Zone, Format)),
            _ => defaultDeckList,
        }).ToList();
    }

    public void SetZone(int zoneId) {
        Zone = (CardZone)zoneId;
    }

    public void SetFormat(int formatId) {
        Format = (GameFormat)formatId;
    }

    public void ToggleTopic() {
        var tmp = OriginalMode;
        OriginalMode = mode;
        mode = tmp;
    }
    
}

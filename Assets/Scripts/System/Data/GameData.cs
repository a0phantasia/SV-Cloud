using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XmlRoot("gameData")]
public class GameData
{
    public string nickname;
    public float BGMVolume, SEVolume;

    [XmlArray("deckList"), XmlArrayItem(typeof(Deck), ElementName = "deck")]
    public List<Deck> decks;

    


    public GameData() {
        
    }

    public static GameData GetDefaultData() {
        GameData data = new GameData();
        data.InitGameData();
        return data;
    }

    public void InitGameData() {
        nickname = string.Empty;
        decks = new List<Deck>();
        BGMVolume = SEVolume = 10f;
    }

    public bool IsEmpty() {
        return string.IsNullOrEmpty(nickname);
    }

    public void SetNickname(string name) {
        nickname = name;
    }
}

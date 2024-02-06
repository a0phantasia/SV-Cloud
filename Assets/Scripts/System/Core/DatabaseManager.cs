using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DatabaseManager : Singleton<DatabaseManager>
{
    private ResourceManager RM => ResourceManager.instance;

    public List<Card> cardMaster = new List<Card>();
    public Dictionary<int, Card> cardInfoDict = new Dictionary<int, Card>();
    public Dictionary<int, Effect> effectInfoDict = new Dictionary<int, Effect>();
    public Dictionary<CardTrait, string> traitNameDict = new Dictionary<CardTrait, string>();
    public Dictionary<CardKeyword, string> keywordNameDict = new Dictionary<CardKeyword, string>();
    public Dictionary<CardKeyword, string> keywordEnglishNameDict = new Dictionary<CardKeyword, string>();
    public Dictionary<CardKeyword, string> keywordInfoDict = new Dictionary<CardKeyword, string>();

    // public Dictionary<string, ActivityInfo> activityInfoDict = new Dictionary<string, ActivityInfo>();

    public void Init() {
        RM.LoadCardInfo((x) => { 
            cardInfoDict = x; 
            cardMaster = cardInfoDict.Select(entry => entry.Value).ToList();
        }, (y) => effectInfoDict = y);

        RM.LoadTraitInfo((x) => {
            traitNameDict = x.ToDictionary(entry => (CardTrait)entry.Key, entry => entry.Value[0]);
        });

        RM.LoadKeywordInfo((x) => {
            keywordNameDict = x.ToDictionary(entry => (CardKeyword)entry.Key, entry => entry.Value[0]);
            keywordEnglishNameDict = x.ToDictionary(entry => (CardKeyword)entry.Key, entry => entry.Value[1]);
            keywordInfoDict = x.ToDictionary(entry => (CardKeyword)entry.Key, entry => entry.Value[2].GetDescription("-"));
        });
    }

    public bool VerifyData(out string error) {
        if (keywordInfoDict.Count != GameManager.versionData.keywordCount) {
            error = "獲取關鍵字說明資料失敗 (" + keywordInfoDict.Count + "/" + GameManager.versionData.keywordCount  + ")";
            return false;
        }
        if (traitNameDict.Count != GameManager.versionData.traitCount) {
            error = "獲取類型資料失敗 (" + traitNameDict.Count + "/" + GameManager.versionData.traitCount  + ")";
            return false;
        }
        if (cardInfoDict.Count != GameManager.versionData.cardCount) {
            error = "獲取卡片資料失敗 (" + cardInfoDict.Count + "/" + GameManager.versionData.cardCount + ")";
            return false;
        }
        error = string.Empty;
        return true;
    }

    public Card GetCardInfo(int id) {
        if (id == 0)
            return null;

        return cardInfoDict.Get(id);
    }

    public Effect GetEffectInfo(int id) {
        if (id == 0)
            return null;

        return effectInfoDict.Get(id);
    }
    
    public string GetTraitName(CardTrait trait) {
        return traitNameDict.Get(trait, "-");
    }
    public string GetKeywordName(CardKeyword keyword) {
        return keywordNameDict.Get(keyword, "-");
    }

    public string GetKeywordEnglishName(CardKeyword keyword) {
        return keywordEnglishNameDict.Get(keyword, "-");
    }

    public string GetKeywordInfo(CardKeyword keyword) {
        return keywordInfoDict.Get(keyword, string.Empty);
    }
    
}

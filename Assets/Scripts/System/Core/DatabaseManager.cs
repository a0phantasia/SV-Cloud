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
    public Dictionary<int, string> keywordNameDict = new Dictionary<int, string>();
    public Dictionary<int, string> keywordEnglishNameDict = new Dictionary<int, string>();
    public Dictionary<int, string> keywordInfoDict = new Dictionary<int, string>();

    // public Dictionary<string, ActivityInfo> activityInfoDict = new Dictionary<string, ActivityInfo>();

    public void Init() {
        RM.LoadCardInfo((x) => { 
            cardInfoDict = x; 
            cardMaster = cardInfoDict.Select(entry => entry.Value).ToList();
        }, (y) => effectInfoDict = y);

        RM.LoadKeywordInfo((x) => {
            keywordNameDict = x.ToDictionary(entry => entry.Key, entry => entry.Value[0]);
            keywordEnglishNameDict = x.ToDictionary(entry => entry.Key, entry => entry.Value[1]);
            keywordInfoDict = x.ToDictionary(entry => entry.Key, entry => entry.Value[2].GetDescription("-"));
        });
    }

    public bool VerifyData(out string error) {
        if (keywordInfoDict.Count != GameManager.versionData.keywordCount) {
            error = "獲取關鍵字說明資料失敗 (" + keywordInfoDict.Count + "/" + GameManager.versionData.keywordCount  + ")";
            return false;
        }
        if (cardInfoDict.Count != GameManager.versionData.cardCount) {
            error = "獲取卡片資料失敗 (" + cardInfoDict.Count + "/" + GameManager.versionData.cardCount + ")";
            return false;
        }
        // if (buffInfoDict.Count == 0) {
        //     error = "获取Buff档案失败";
        //     return false;
        // }
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
    
    public string GetKeywordName(int keywordId) {
        return keywordNameDict.Get(keywordId, "-");
    }

    public string GetKeywordEnglishName(int keywordId) {
        return keywordEnglishNameDict.Get(keywordId, "-");
    }

    public string GetKeywordInfo(int keywordId) {
        return keywordInfoDict.Get(keywordId, string.Empty);
    }
    
}

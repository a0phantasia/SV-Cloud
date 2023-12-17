using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class KeywordView : IMonoBehaviour
{

    [SerializeField] private Dropdown dropdown;
    [SerializeField] private IText descriptionText;

    public override void Init()
    {
        base.Init();
        InitDropdownOptions();
    }

    private void InitDropdownOptions() {
        var keywordNames = new List<string>();
        for (int i = 0; i < CardDatabase.keywordNameDict.Count; i++) {
            keywordNames.Add(CardDatabase.keywordNameDict[(CardKeyword)i]);  
        }
        dropdown?.ClearOptions();
        dropdown?.AddOptions(keywordNames);
        dropdown.value = 0;
    }

    public void OnSelectKeyword(int keywordId) {
        descriptionText?.SetText(DatabaseManager.instance.GetKeywordInfo(keywordId));
    }

}

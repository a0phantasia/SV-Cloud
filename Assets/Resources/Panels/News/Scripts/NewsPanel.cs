using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewsPanel : Panel
{
    [SerializeField] private Text newsText;

    public override void Init()
    {
        base.Init();
        newsText?.SetText(GameManager.versionData.News);
    }
}
